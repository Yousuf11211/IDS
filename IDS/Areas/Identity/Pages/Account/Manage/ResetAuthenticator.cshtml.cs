using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class ResetAuthenticatorModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ResetAuthenticatorModel> _logger;

    public ResetAuthenticatorModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ResetAuthenticatorModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string Password { get; set; } = string.Empty;
        [Required, StringLength(7, MinimumLength = 6)]
        [Display(Name = "Current authenticator code")]
        public string Code { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return await _userManager.GetUserAsync(User) == null
            ? RedirectToPage("../Login")
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("../Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await _userManager.IsLockedOutAsync(user)) return Forbid();
        if (!await _userManager.CheckPasswordAsync(user, Input.Password) ||
            !await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider,
                Input.Code.Replace(" ", "").Replace("-", "")))
        {
            await _userManager.AccessFailedAsync(user);
            ModelState.AddModelError(string.Empty, "The password or authenticator code is incorrect.");
            return Page();
        }
        await _userManager.ResetAccessFailedCountAsync(user);

        // The enrollment middleware keeps the account on the setup screen until a new code is verified.
        var disableResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disableResult.Succeeded)
        {
            AddErrors(disableResult);
            return Page();
        }

        var resetResult = await _userManager.ResetAuthenticatorKeyAsync(user);
        if (!resetResult.Succeeded)
        {
            AddErrors(resetResult);
            return Page();
        }

        await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);
        await _userManager.UpdateSecurityStampAsync(user);
        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User {UserId} reset their authenticator key.", user.Id);
        StatusMessage = "Set up your authenticator again to complete the reset.";
        return RedirectToPage("./EnableAuthenticator");
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
