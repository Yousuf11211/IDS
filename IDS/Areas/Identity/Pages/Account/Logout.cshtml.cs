using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
[LogoutAntiforgeryRecovery]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger) : PageModel
{
    public string ReturnUrl { get; private set; } = string.Empty;
    public bool Retry { get; private set; }

    public void OnGet(string? returnUrl = null, bool retry = false)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        Retry = retry;
    }

    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        return LocalRedirect(SafeReturnUrl(returnUrl));
    }

    public async Task<IActionResult> OnPostInactivityAsync()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out due to inactivity.");
        return RedirectToPage("/Account/Logout", new { area = "Identity" });
    }

    // Old timer URLs remain usable, but a GET must not change authentication state.
    public void OnGetInactivity() => OnGet(retry: true);

    private string SafeReturnUrl(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
}
