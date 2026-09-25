using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class VerifyModel(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    AdminVerification verification,
    ApplicationDbContext database,
    SecurityPolicyService policies) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string ReturnUrl { get; private set; } = "/Admin/Security";
    public bool TestingBypass { get; private set; }

    public sealed class InputModel
    {
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        TestingBypass = (await policies.GetAsync()).AdminMfaTestBypass;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        TestingBypass = (await policies.GetAsync()).AdminMfaTestBypass;
        if (!TestingBypass && string.IsNullOrWhiteSpace(Input.Code))
            ModelState.AddModelError("Input.Code", "Enter your current authenticator code.");
        if (!ModelState.IsValid) return Page();
        var user = await users.GetUserAsync(User);
        if (user == null || !user.EmailConfirmed || (!user.TwoFactorEnabled && !TestingBypass) || user.MustChangePassword ||
            !await users.IsInRoleAsync(user, AppRoles.Admin) || await users.IsInRoleAsync(user, AppRoles.Suspended))
            return Forbid();
        if (await users.IsLockedOutAsync(user))
        {
            await signIn.SignOutAsync();
            return RedirectToPage("/Account/Lockout", new { area = "Identity" });
        }

        var code = Input.Code?.Replace(" ", "").Replace("-", "") ?? string.Empty;
        var valid = await users.CheckPasswordAsync(user, Input.Password) &&
            (TestingBypass || await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, code));
        database.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id, UserEmail = user.Email ?? "", EntityType = "Authentication",
            Action = valid ? "Security.AdminVerified" : "Security.AdminVerificationFailed",
            Details = TestingBypass ? "Development administrator authenticator bypass active." : null,
            Timestamp = DateTime.UtcNow, IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await database.SaveChangesAsync();
        // Do not put submitted secrets back into the rendered form after an error.
        ModelState.Remove("Input.Password");
        ModelState.Remove("Input.Code");
        Input = new();
        if (!valid)
        {
            await users.AccessFailedAsync(user);
            ModelState.AddModelError(string.Empty, TestingBypass ? "Verification failed. Check your password." :
                "Verification failed. Check your password and current authenticator code.");
            return Page();
        }
        await users.ResetAccessFailedCountAsync(user);
        await verification.MarkVerifiedAsync(HttpContext, user);
        return LocalRedirect(ReturnUrl);
    }

    private string SafeReturnUrl(string? value) =>
        Url.IsLocalUrl(value) && !value!.Contains("/Admin/Verify", StringComparison.OrdinalIgnoreCase)
            ? value : Url.Page("/Admin/Security")!;
}
