using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ForgotPasswordModel(UserManager<ApplicationUser> users, AccountRecovery recovery) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        if (!ModelState.IsValid) return Page();
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var email = Input.Email.Trim();
        if (await recovery.AllowAsync(HttpContext, "password-request", email, true, token) &&
            await recovery.AvailableAsync(RecoverySettings.PasswordEnabled, token))
        {
            var user = await users.FindByEmailAsync(email);
            if (await recovery.EligibleAsync(user))
                await recovery.SendPasswordLinkAsync(user!, Url, admin: false, token);
        }
        await AccountRecovery.PadPublicRequestAsync(started, token);
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
