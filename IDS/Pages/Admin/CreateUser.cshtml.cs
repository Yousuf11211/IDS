using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using IDS.Data;
using IDS.Core.Services;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public class CreateUserModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AccessControlService _accessControl;
    private readonly MailjetEmailSender _emailSender;
    private readonly AccountLinkBuilder _accountLinks;
    private readonly SecurityPolicyService _policies;
    private readonly ApplicationDbContext _database;
    private readonly AccountEmailQueue _email;

    public CreateUserModel(
        UserManager<ApplicationUser> userManager,
        AccessControlService accessControl,
        MailjetEmailSender emailSender,
        AccountLinkBuilder accountLinks,
        SecurityPolicyService policies, ApplicationDbContext database, AccountEmailQueue email)
    {
        _userManager = userManager;
        _accessControl = accessControl;
        _emailSender = emailSender;
        _accountLinks = accountLinks;
        _policies = policies;
        _database = database;
        _email = email;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<string> AvailableRoles =>
        new[] { AppRoles.Employee, AppRoles.Support };
    public bool InvitationsAllowed { get; private set; }
    public bool EmailConfigured => _emailSender.IsConfigured;
    public bool AccountLinksConfigured => _accountLinks.IsConfigured;
    public bool InvitationsReady => InvitationsAllowed && EmailConfigured && AccountLinksConfigured;
    public string InvitationsUnavailableReason => !InvitationsAllowed
        ? "Employee invitations are paused in Security controls."
        : !EmailConfigured
            ? "Email delivery is disabled or Mailjet credentials are missing. Review email delivery in Administration."
            : !AccountLinksConfigured
                ? "The application URL for account links is missing or invalid. Configure PUBLIC_BASE_URL or Application:PublicBaseUrl and restart the application."
                : string.Empty;

    public async Task OnGetAsync()
    {
        await _emailSender.LoadAsync();
        InvitationsAllowed = (await _policies.GetAsync()).EmployeeInvitationsEnabled;
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Department { get; set; } = Departments.General;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await OnGetAsync();
        if (!Departments.IsKnown(Input.Department))
            ModelState.AddModelError("Input.Department", "Choose a listed department.");

        if (Input.Role is not (AppRoles.Employee or AppRoles.Support))
            ModelState.AddModelError("Input.Role", "Create an employee or support account. Administrator access requires a separate approved request.");

        if (!InvitationsReady)
            ModelState.AddModelError(string.Empty, InvitationsUnavailableReason);

        if (!ModelState.IsValid)
            return Page();

        var email = Input.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim(),
            Department = Input.Department,
            MustChangePassword = true
        };

        // The employee chooses their password from a single-use Identity link.
        // A random initial password keeps the account unusable until then.
        var initialPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) + "aA1!";
        await using var transaction = await _database.Database.BeginTransactionAsync();
        var result = await _userManager.CreateAsync(user, initialPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await _accessControl.EnsureRoleExistsAsync(Input.Role);
        await _accessControl.SetExclusiveRoleAsync(_userManager, user, Input.Role);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = _accountLinks.PasswordResetLink(Url, encodedToken);
        if (link == null)
        {
            ModelState.AddModelError(string.Empty, "The setup link could not be built. Review the configured public application URL.");
            return Page();
        }
        var now = DateTime.UtcNow;
        _email.Enqueue("New user welcome", user.Id, new(email, IncidentWorkflow.DisplayName(user), "Set up your IDS account",
            "An administrator created your IDS account. Choose your password within 24 hours, then sign in and follow the authenticator setup instructions.",
            now, "Set your password", link), now.AddHours(24));
        await _database.SaveChangesAsync();
        await transaction.CommitAsync();
        StatusMessage = $"User {email} created. The welcome email is queued.";
        return RedirectToPage("/Admin/UserList");
    }
}
