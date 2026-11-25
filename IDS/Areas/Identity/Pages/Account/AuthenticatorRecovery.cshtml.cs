using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class AuthenticatorRecoveryModel(UserManager<ApplicationUser> users, AccountRecovery recovery) : PageModel
{
    [BindProperty, EmailAddress, StringLength(256)] public string? Email { get; set; }
    [BindProperty, StringLength(4096)] public string? Code { get; set; }
    [BindProperty, StringLength(2000)] public string? Reason { get; set; }
    public bool Verified { get; private set; }
    public bool Finished { get; private set; }
    public string? StatusMessage { get; private set; }

    public void OnGet(string? code)
    {
        if (code == null) return;
        Code = code;
        Verified = recovery.Read(AccountRecovery.VerifyEmail, code) != null;
        if (!Verified) StatusMessage = "This verification link is invalid or expired.";
    }

    public async Task<IActionResult> OnPostEmailAsync(CancellationToken token)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Email))
        {
            ModelState.AddModelError(nameof(Email), "Enter a valid registered email address.");
            return Page();
        }
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var email = Email.Trim();
        if (await recovery.AllowAsync(HttpContext, "auth-email", email, true, token) &&
            await recovery.AvailableAsync(RecoverySettings.AuthenticatorEnabled, token))
        {
            var user = await users.FindByEmailAsync(email);
            if (await recovery.EligibleAsync(user, enrolled: true))
                await recovery.SendEmailProofAsync(user!, Url, token);
        }
        await AccountRecovery.PadPublicRequestAsync(started, token);
        Finished = true;
        StatusMessage = "If the account is eligible for this workflow, a verification link will be sent. Check your inbox or contact support.";
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(CancellationToken token)
    {
        var link = recovery.Read(AccountRecovery.VerifyEmail, Code);
        if (link == null)
        {
            StatusMessage = "This verification link is invalid or expired.";
            return Page();
        }
        Verified = true;
        var reason = Reason?.Trim();
        if (!ModelState.IsValid || string.IsNullOrEmpty(reason))
        {
            ModelState.AddModelError(nameof(Reason), "Enter a reason of 1 to 2,000 characters.");
            return Page();
        }
        if (await recovery.AllowAsync(HttpContext, "auth-submit", link.UserId, false, token) &&
            await recovery.AvailableAsync(RecoverySettings.AuthenticatorEnabled, token))
            await recovery.SubmitAsync(link, Code!, reason, token);

        Finished = true;
        StatusMessage = "If eligible, your request was submitted for review. Watch for a separate decision email or contact support.";
        return Page();
    }
}
