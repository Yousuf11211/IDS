using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class RecoveryEnrollmentModel(UserManager<ApplicationUser> users, AccountRecovery recovery) : PageModel
{
    [BindProperty, StringLength(4096)] public string? Link { get; set; }
    [BindProperty, StringLength(4096)] public string? Setup { get; set; }
    [BindProperty, StringLength(100), DataType(DataType.Password)] public string? Password { get; set; }
    [BindProperty, StringLength(16)] public string? Code { get; set; }
    public string? QrImage { get; private set; }
    public string? SharedKey { get; private set; }
    public bool ValidLink { get; private set; }
    public string[]? RecoveryCodes { get; private set; }

    public async Task OnGetAsync(string? code, CancellationToken token)
    {
        Link = code;
        ValidLink = await GetLinkAsync(token) != null;
    }

    public async Task<IActionResult> OnPostPrepareAsync(CancellationToken token)
    {
        var link = await GetLinkAsync(token);
        if (link == null) return Page();
        ValidLink = true;
        var password = ClearPassword();
        if (!ModelState.IsValid || string.IsNullOrEmpty(password) ||
            !await recovery.AllowAsync(HttpContext, "auth-enroll", link.UserId, false, token))
            return VerificationFailed();
        var user = await recovery.EnrollmentUserAsync(link, token);
        if (user == null || !await users.CheckPasswordAsync(user, password)) return VerificationFailed();
        var key = users.GenerateNewAuthenticatorKey();
        Setup = recovery.Issue(AccountRecovery.Candidate, link with { Candidate = key }, TimeSpan.FromMinutes(15));
        ShowQr(link, key);
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync(CancellationToken token)
    {
        var link = await GetLinkAsync(token);
        if (link == null) return Page();
        ValidLink = true;
        var candidate = recovery.Read(AccountRecovery.Candidate, Setup);
        var password = ClearPassword();
        var code = Code;
        Code = null;
        ModelState.Remove(nameof(Code));
        if (!ModelState.IsValid || candidate?.Candidate == null || candidate with { Candidate = null } != link ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(code) ||
            !await recovery.AllowAsync(HttpContext, "auth-enroll", link.UserId, false, token)) return VerificationFailed();
        RecoveryCodes = await recovery.ReplaceAsync(link, candidate.Candidate, password, code, token);
        if (RecoveryCodes == null)
        {
            // Return to password verification before displaying setup material again.
            return VerificationFailed();
        }
        ValidLink = false;
        return Page();
    }

    private string? ClearPassword()
    {
        var password = Password;
        Password = null;
        ModelState.Remove(nameof(Password));
        return password;
    }

    private PageResult VerificationFailed()
    {
        ModelState.AddModelError(string.Empty, "Verification failed. Check your password and the new authenticator code, or contact support.");
        return Page();
    }

    private async Task<RecoveryLink?> GetLinkAsync(CancellationToken token)
    {
        var link = recovery.Read(AccountRecovery.ApproveEnrollment, Link);
        return link != null && await recovery.AvailableAsync(RecoverySettings.AuthenticatorEnabled, token) &&
            await recovery.EnrollmentUserAsync(link, token) != null ? link : null;
    }

    private void ShowQr(RecoveryLink link, string key)
    {
        SharedKey = string.Join(" ", key.Chunk(4).Select(chars => new string(chars)));
        var label = Uri.EscapeDataString(link.Email);
        using var qr = QRCodeGenerator.GenerateQrCode($"otpauth://totp/IDS:{label}?secret={key}&issuer=IDS&digits=6",
            QRCodeGenerator.ECCLevel.M);
        using var png = new PngByteQRCode(qr);
        QrImage = "data:image/png;base64," + Convert.ToBase64String(png.GetGraphic(5));
    }
}
