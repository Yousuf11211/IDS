using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class EnableAuthenticatorModel(UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn, ApplicationDbContext database, AccountEmailQueue email) : PageModel
{
    public string? SharedKey { get; private set; }
    public string? QrImage { get; private set; }
    [BindProperty, DataType(DataType.Password)] public string? Password { get; set; }
    [BindProperty] public string? Code { get; set; }
    [TempData] public string[]? RecoveryCodes { get; set; }
    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        return user.TwoFactorEnabled ? RedirectToPage("./TwoFactorAuthentication") : Page();
    }

    public async Task<IActionResult> OnPostPrepareAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (user.TwoFactorEnabled) return RedirectToPage("./TwoFactorAuthentication");
        if (!await VerifyPasswordAsync(user)) return Page();
        await users.ResetAccessFailedCountAsync(user);
        await LoadKeyAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (user.TwoFactorEnabled) return RedirectToPage("./TwoFactorAuthentication");
        var code = Code?.Replace(" ", "").Replace("-", "");
        ModelState.Remove(nameof(Code));
        Code = null;
        if (!await VerifyPasswordAsync(user)) return Page();

        if (code == null || code.Length != 6 || !code.All(char.IsAsciiDigit) ||
            !await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, code))
        {
            await users.AccessFailedAsync(user);
            ModelState.AddModelError(string.Empty, "Enter a valid six-digit authenticator code.");
            if (!await users.IsLockedOutAsync(user)) await LoadKeyAsync(user);
            return Page();
        }

        await using var transaction = await database.Database.BeginTransactionAsync();
        await users.ResetAccessFailedCountAsync(user);
        var result = await users.SetTwoFactorEnabledAsync(user, true);
        IEnumerable<string>? codes = null;
        if (result.Succeeded && await users.CountRecoveryCodesAsync(user) == 0)
        {
            codes = await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            if (codes == null) result = IdentityResult.Failed();
        }
        if (result.Succeeded) result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Authenticator setup could not be saved. Start enrollment again.");
            return Page();
        }

        email.Confirm(user, "Authenticator enabled", "An authenticator was activated for your IDS account.");
        AccountRecovery.NotifyUser(database, user, "Authenticator enabled", "An authenticator was activated for your IDS account.");
        database.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id, UserEmail = user.Email ?? "", Action = "Security.AuthenticatorEnabled",
            EntityType = "Authentication", EntityId = user.Id, Timestamp = DateTime.UtcNow
        });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        await signIn.RefreshSignInAsync(user);
        StatusMessage = "Your authenticator is enabled.";
        if (codes == null) return RedirectToPage("./TwoFactorAuthentication");
        RecoveryCodes = codes.ToArray();
        return RedirectToPage("./ShowRecoveryCodes");
    }

    private async Task<bool> VerifyPasswordAsync(ApplicationUser user)
    {
        var password = Password;
        Password = null;
        ModelState.Remove(nameof(Password));
        if (!await users.IsLockedOutAsync(user) && !string.IsNullOrEmpty(password))
        {
            if (await users.CheckPasswordAsync(user, password)) return true;
            await users.AccessFailedAsync(user);
        }
        ModelState.AddModelError(string.Empty, "Verify your current password to continue.");
        return false;
    }

    private async Task LoadKeyAsync(ApplicationUser user)
    {
        var key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key) && (await users.ResetAuthenticatorKeyAsync(user)).Succeeded)
            key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            ModelState.AddModelError(string.Empty, "Could not prepare the authenticator key. Try again.");
            return;
        }
        SharedKey = string.Join(" ", key.Chunk(4).Select(chars => new string(chars)));
        var label = Uri.EscapeDataString(user.Email ?? user.Id);
        using var qr = QRCodeGenerator.GenerateQrCode($"otpauth://totp/IDS:{label}?secret={key}&issuer=IDS&digits=6",
            QRCodeGenerator.ECCLevel.M);
        using var png = new PngByteQRCode(qr);
        QrImage = "data:image/png;base64," + Convert.ToBase64String(png.GetGraphic(5));
    }
}
