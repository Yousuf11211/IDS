using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
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

    public CreateUserModel(
        UserManager<ApplicationUser> userManager,
        AccessControlService accessControl,
        MailjetEmailSender emailSender,
        AccountLinkBuilder accountLinks,
        SecurityPolicyService policies)
    {
        _userManager = userManager;
        _accessControl = accessControl;
        _emailSender = emailSender;
        _accountLinks = accountLinks;
        _policies = policies;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<string> AvailableRoles =>
        new[] { AppRoles.Employee, AppRoles.Support };
    public bool InvitationsAllowed { get; private set; }
    public bool InvitationsReady => InvitationsAllowed && _emailSender.IsConfigured && _accountLinks.IsConfigured;

    public async Task OnGetAsync() => InvitationsAllowed = (await _policies.GetAsync()).EmployeeInvitationsEnabled;

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
            ModelState.AddModelError(string.Empty, InvitationsAllowed
                ? "Configure email and PUBLIC_BASE_URL before inviting employees."
                : "Employee invitations are paused in Security controls.");

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
            StatusMessage = "User created, but the setup link could not be built. Check PUBLIC_BASE_URL and resend the link from the user's page.";
            return RedirectToPage("/Admin/UserList");
        }

        var (sent, emailError) = await _emailSender.SendAccountAccessLinkAsync(
            email, user.FirstName, link, isInvitation: true);
        StatusMessage = sent
            ? $"User {email} created. A password setup link was emailed to them."
            : $"User {email} created, but the setup email failed: {emailError}. Resend the link from the user's page.";
        return RedirectToPage("/Admin/UserList");
    }
}
