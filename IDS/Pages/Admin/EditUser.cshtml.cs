using System.ComponentModel.DataAnnotations;
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
public class EditUserModel(
    UserManager<ApplicationUser> users,
    AccessControlService access,
    MailjetEmailSender emailSender,
    AccountLinkBuilder accountLinks,
    ApplicationDbContext database, AccountRecovery recovery, AccountEmailQueue queue) : PageModel
{
    [BindProperty] public UserEditInput Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string UserFullName { get; private set; } = string.Empty;
    public string CurrentRole { get; private set; } = string.Empty;
    public DateTime? LastLogin { get; private set; }
    public bool IsCurrentUser { get; private set; }
    public bool AccountLinksReady => emailSender.IsConfigured && accountLinks.IsConfigured;
    public string AccountLinksUnavailableReason => !emailSender.IsConfigured
        ? "Email delivery is disabled or Mailjet credentials are missing. Review email delivery in Administration."
        : !accountLinks.IsConfigured
            ? "The application URL for account links is missing or invalid. Configure PUBLIC_BASE_URL or Application:PublicBaseUrl and restart the application."
            : string.Empty;
    public List<string> AllRoles { get; } = new() { AppRoles.Employee, AppRoles.Support, AppRoles.Suspended };

    public class UserEditInput
    {
        [Required] public string UserId { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [StringLength(100)] public string? FirstName { get; set; }
        [StringLength(100)] public string? LastName { get; set; }
        [Required] public string Role { get; set; } = string.Empty;
        [Required, StringLength(80)] public string Department { get; set; } = Departments.General;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await ManageableEmployeeAsync(id);
        if (user == null) return RedirectToPage("/Admin/Security");
        await LoadDisplayAsync(user);
        Input = new UserEditInput
        {
            UserId = user.Id, Email = user.Email ?? "", FirstName = user.FirstName,
            LastName = user.LastName, Role = CurrentRole, Department = user.Department
        };
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        var user = await ManageableEmployeeAsync(Input.UserId);
        if (user == null) return RedirectToPage("/Admin/Security");
        await LoadDisplayAsync(user);
        if (!Departments.IsKnown(Input.Department))
            ModelState.AddModelError("Input.Department", "Choose a listed department.");
        if (Input.Role is not (AppRoles.Employee or AppRoles.Support or AppRoles.Suspended))
            ModelState.AddModelError("Input.Role", "Administrator access requires approval in Security controls.");
        if (Input.Role == AppRoles.Suspended && CurrentRole != AppRoles.Suspended)
            ModelState.AddModelError("Input.Role", "Use Suspend User to suspend an account.");
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim();
        var duplicate = await users.FindByEmailAsync(email);
        if (duplicate != null && duplicate.Id != user.Id)
        {
            ModelState.AddModelError("Input.Email", "This email is already in use.");
            return Page();
        }

        await using var transaction = await database.Database.BeginTransactionAsync();
        var previousEmail = user.EmailConfirmed ? user.Email : null;
        var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
        user.FirstName = Input.FirstName?.Trim() ?? "";
        user.LastName = Input.LastName?.Trim() ?? "";
        user.Department = Input.Department;
        if (emailChanged)
        {
            // Changing the recovery address requires ownership verification again.
            user.Email = email;
            user.UserName = email;
            user.EmailConfirmed = false;
        }
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        if (CurrentRole != Input.Role) await access.SetExclusiveRoleAsync(users, user, Input.Role);
        RequireSuccess(await users.UpdateSecurityStampAsync(user));
        if (emailChanged && !user.MustChangePassword)
        {
            if (previousEmail != null)
                queue.Confirm(user, "Registered email changed", "An administrator changed your sign-in address. Your new address must be verified before use.", previousEmail, "previous-address");
            await emailSender.LoadAsync();
            if (AccountLinksReady) await QueueAddressVerificationAsync(user);
            AccountRecovery.Audit(database, user, "Security.AdminUpdatedEmail");
            await database.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        StatusMessage = $"User {email} updated. Existing sessions were revoked.";
        return RedirectToPage(new { id = user.Id });
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
    {
        var user = await ManageableEmployeeAsync(userId);
        if (user == null) return RedirectToPage("/Admin/Security");
        await emailSender.LoadAsync();
        if (!AccountLinksReady)
        {
            StatusMessage = "Error: " + AccountLinksUnavailableReason;
            return RedirectToPage(new { id = userId });
        }
        if (user.EmailConfirmed)
        {
            if (!await recovery.EligibleAsync(user)) return Forbid();
            await recovery.SendPasswordLinkAsync(user, Url, admin: true, HttpContext.RequestAborted);
            StatusMessage = "Password reset email queued.";
            return RedirectToPage(new { id = userId });
        }
        if (user.Email == null) return Forbid();
        if (!user.MustChangePassword)
        {
            await QueueAddressVerificationAsync(user);
            await database.SaveChangesAsync();
            StatusMessage = "Address verification email queued.";
            return RedirectToPage(new { id = userId });
        }
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var link = accountLinks.PasswordResetLink(Url, WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)));
        if (link == null)
        {
            StatusMessage = "Error: Could not build the account link.";
            return RedirectToPage(new { id = userId });
        }
        var now = DateTime.UtcNow;
        queue.Enqueue("Welcome link reissued", token, new(user.Email, IncidentWorkflow.DisplayName(user),
            "Set up your IDS account", "Choose your password within 24 hours, then sign in to complete authenticator setup.",
            now, "Set your password", link), now.AddHours(24));
        await database.SaveChangesAsync();
        StatusMessage = "Welcome email queued.";
        return RedirectToPage(new { id = userId });
    }

    private async Task QueueAddressVerificationAsync(ApplicationUser user)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var link = accountLinks.Link(Url, "/Account/ConfirmEmail", new
        {
            area = "Identity", userId = user.Id, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token))
        });
        if (link == null || user.Email == null) return;
        var now = DateTime.UtcNow;
        queue.Enqueue("Verify updated email", token, new(user.Email, IncidentWorkflow.DisplayName(user),
            "Verify your updated IDS email", "An administrator updated your email address. Verify the new address before signing in.",
            now, "Verify updated address", link), now.AddHours(24));
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var user = await ManageableEmployeeAsync(userId);
        if (user == null) return RedirectToPage("/Admin/Security");
        // Keep incident history and encrypted conversations attached to their participants.
        // Suspension provides immediate access removal without deleting those records.
        StatusMessage = "Error: Accounts are retained for audit history. Use Suspend User to remove access.";
        return RedirectToPage(new { id = userId });
    }

    public async Task<IActionResult> OnPostSuspendAsync(string userId)
    {
        var user = await ManageableEmployeeAsync(userId);
        if (user == null) return RedirectToPage("/Admin/Security");
        await access.SetExclusiveRoleAsync(users, user, AppRoles.Suspended);
        StatusMessage = $"User {user.Email} suspended and sessions revoked.";
        return RedirectToPage(new { id = userId });
    }

    public async Task<IActionResult> OnPostUnsuspendAsync(string userId, string newRole)
    {
        var user = await ManageableEmployeeAsync(userId);
        if (user == null) return RedirectToPage("/Admin/Security");
        if (newRole is not (AppRoles.Employee or AppRoles.Support))
        {
            StatusMessage = "Error: Select Employee or Support. Administrator access requires a separate approved request.";
            return RedirectToPage(new { id = userId });
        }
        await access.SetExclusiveRoleAsync(users, user, newRole);
        StatusMessage = $"User {user.Email} restored as {newRole}.";
        return RedirectToPage(new { id = userId });
    }

    private async Task<ApplicationUser?> ManageableEmployeeAsync(string? id)
    {
        var user = string.IsNullOrWhiteSpace(id) ? null : await users.FindByIdAsync(id);
        if (user == null) { StatusMessage = "Error: User not found."; return null; }
        if (await users.IsInRoleAsync(user, AppRoles.Admin))
        {
            StatusMessage = "Error: Administrator accounts are protected from employee management. Use your own profile for personal changes or contact the deployment operator for recovery.";
            return null;
        }
        return user;
    }

    private async Task LoadDisplayAsync(ApplicationUser user)
    {
        await emailSender.LoadAsync();
        UserEmail = user.Email ?? "";
        UserFullName = string.IsNullOrWhiteSpace(user.FirstName) ? UserEmail : $"{user.FirstName} {user.LastName}";
        CurrentRole = (await users.GetRolesAsync(user)).FirstOrDefault(AppRoles.IsManagedRole) ?? "None";
        IsCurrentUser = users.GetUserId(User) == user.Id;
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("The employee account changed. Reload before retrying.");
    }
}
