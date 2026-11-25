using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize(Roles = AppRoles.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class Disable2faModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    SecurityPolicyService policy, ApplicationDbContext database, AccountEmailQueue email) : PageModel
{
    public bool CanDisable { get; private set; }
    [BindProperty, DataType(DataType.Password)] public string? Password { get; set; }
    [BindProperty] public string? Code { get; set; }
    [BindProperty] public bool UseRecoveryCode { get; set; }
    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!user.TwoFactorEnabled) return RedirectToPage("./TwoFactorAuthentication");
        CanDisable = await policy.CanBypassTwoFactorAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!user.TwoFactorEnabled) return RedirectToPage("./TwoFactorAuthentication");
        CanDisable = await policy.CanBypassTwoFactorAsync(user);
        if (!CanDisable) return Forbid();

        var password = Password;
        var code = Code?.Trim();
        Password = Code = null;
        ModelState.Remove(nameof(Password));
        ModelState.Remove(nameof(Code));
        if (await users.IsLockedOutAsync(user)) return Forbid();

        await using var transaction = await database.Database.BeginTransactionAsync();
        var valid = !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(code) && code.Length <= 64 &&
            await users.CheckPasswordAsync(user, password) &&
            (UseRecoveryCode
                ? (await users.RedeemTwoFactorRecoveryCodeAsync(user, code.Replace(" ", ""))).Succeeded
                : await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider,
                    code.Replace(" ", "").Replace("-", "")));
        if (!valid)
        {
            await users.AccessFailedAsync(user);
            await transaction.CommitAsync();
            ModelState.AddModelError(string.Empty, "The password or verification code is incorrect.");
            return Page();
        }

        await users.ResetAccessFailedCountAsync(user);
        if (!(await users.SetTwoFactorEnabledAsync(user, false)).Succeeded ||
            !(await users.ResetAuthenticatorKeyAsync(user)).Succeeded ||
            await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 0) == null ||
            !(await users.UpdateSecurityStampAsync(user)).Succeeded)
        {
            ModelState.AddModelError(string.Empty, "The change could not be saved. Try again.");
            return Page();
        }

        email.Confirm(user, "Authenticator disabled", "Your IDS authenticator was deactivated.");
        AccountRecovery.NotifyUser(database, user, "Authenticator disabled", "Your IDS authenticator was deactivated.");
        database.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id, UserEmail = user.Email ?? "", EntityId = user.Id, EntityType = "Authentication",
            Action = "Security.AuthenticatorDisabled", Details = "Development administrator MFA exception applied.",
            Timestamp = DateTime.UtcNow
        });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        await signIn.SignOutAsync();
        return RedirectToPage("../Login");
    }
}
