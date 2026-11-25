using System.ComponentModel.DataAnnotations;
using System.Text;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ResetPasswordModel(UserManager<ApplicationUser> users, ApplicationDbContext db,
    AccountRecovery recovery, AccountEmailQueue queue, SecurityAlertService alerts) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
        [Required, StringLength(100), DataType(DataType.Password)] public string Password { get; set; } = "";
        [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";
        [Required, StringLength(4096)] public string Code { get; set; } = "";
        [StringLength(20)] public string? Mode { get; set; }
        [StringLength(64), Display(Name = "Authenticator or recovery code")] public string? Factor { get; set; }
        [Display(Name = "Use a recovery code")] public bool UseRecoveryCode { get; set; }
    }

    public IActionResult OnGet(string? code, string? mode = null)
    {
        if (string.IsNullOrEmpty(code) || code.Length > 4096)
            return Expired();
        if (mode is AccountRecovery.SelfPassword or AccountRecovery.AdminPassword)
        {
            var link = recovery.Read(mode, code);
            if (link == null) return Expired();
            Input = new() { Mode = mode, Code = code };
            return Page();
        }
        if (mode != null) return Expired();
        try
        {
            Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            return Page();
        }
        catch (FormatException) { return Expired(); }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token = default)
    {
        var validInput = ModelState.IsValid;
        var factor = Input.Factor;
        Input.Factor = null;
        ModelState.Remove("Input.Factor");
        if (!validInput)
        {
            if (ModelState.IsValid) ModelState.AddModelError(string.Empty, "Enter a valid authenticator or recovery code.");
            return Page();
        }
        var email = Input.Email.Trim();
        if (!await recovery.AllowAsync(HttpContext, "password-verify", email, false, token))
            return Expired("Too many attempts. Wait before trying again or contact support.");

        var legacy = string.IsNullOrEmpty(Input.Mode);
        var self = Input.Mode == AccountRecovery.SelfPassword;
        var link = legacy || (!self && Input.Mode != AccountRecovery.AdminPassword)
            ? null : recovery.Read(Input.Mode!, Input.Code);
        if (!legacy && link == null) return Expired();
        if (self && !await recovery.AvailableAsync(RecoverySettings.PasswordEnabled, token)) return Expired();

        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await AccountRecovery.LockPasswordAsync(db, token);
        var user = await users.FindByEmailAsync(email);
        if (user == null || await users.IsInRoleAsync(user, AppRoles.Suspended)) return Expired();
        // Existing welcome links still work for invited accounts. Established accounts
        // use the purpose-bound envelope and the verified address.
        if (legacy ? user.EmailConfirmed || !user.MustChangePassword : !AccountRecovery.Matches(link!, user))
            return Expired();
        var identityToken = legacy ? Input.Code : link!.IdentityToken;
        if (identityToken == null || !await users.VerifyUserTokenAsync(user, users.Options.Tokens.PasswordResetTokenProvider,
            UserManager<ApplicationUser>.ResetPasswordTokenPurpose, identityToken)) return Expired();

        if (user.TwoFactorEnabled && !await AccountRecovery.VerifyFactorAsync(users, user, factor, Input.UseRecoveryCode))
        {
            ModelState.AddModelError(string.Empty, "Enter a valid authenticator or recovery code. Use authenticator recovery if you have neither.");
            return Page();
        }

        if (legacy)
        {
            user.EmailConfirmed = true;
            user.MustChangePassword = false;
        }
        var result = await users.ResetPasswordAsync(user, identityToken, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors.Where(error => error.Code.StartsWith("Password", StringComparison.Ordinal)))
                ModelState.AddModelError(string.Empty, error.Description);
            if (ModelState.ErrorCount == 0) return Expired();
            return Page();
        }
        AccountRecovery.Audit(db, user, legacy ? "Security.InvitationCompleted" : "Security.PasswordResetCompleted");
        queue.Confirm(user, "Password reset completed", "Your IDS password was reset. Older sessions and password reset links were revoked.");
        AccountRecovery.NotifyUser(db, user, "Password reset completed", "Your password was reset. Contact support if you did not authorize this change.");
        if (!legacy) await alerts.RecordAccountEventAsync("Account password reset completed", token);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return RedirectToPage("./ResetPasswordConfirmation");
    }

    private PageResult Expired(string message = "This link or verification is invalid or expired. Request a new link or contact support.")
    {
        ModelState.AddModelError(string.Empty, message);
        return Page();
    }
}
