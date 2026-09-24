using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class TwoFactorAuthenticationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SecurityPolicyService _policies;

    public TwoFactorAuthenticationModel(UserManager<ApplicationUser> userManager, SecurityPolicyService policies)
    {
        _userManager = userManager;
        _policies = policies;
    }

    public bool HasAuthenticator { get; private set; }
    public int RecoveryCodesLeft { get; private set; }
    public bool Is2faEnabled { get; private set; }
    public bool TestingBypass { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("../Login");
        }

        HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
        TestingBypass = await _policies.CanBypassTwoFactorAsync(user);
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
        return Page();
    }
}
