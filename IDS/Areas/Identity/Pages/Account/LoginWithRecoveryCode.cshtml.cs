using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

    public LoginWithRecoveryCodeModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginWithRecoveryCodeModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; private set; } = "/";

    public class InputModel
    {
        [Required]
        [Display(Name = "Recovery code")]
        public string RecoveryCode { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        if (await _signInManager.GetTwoFactorAuthenticationUserAsync() == null)
        {
            return RedirectToPage("./Login");
        }

        ReturnUrl = SafeReturnUrl(returnUrl);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToPage("./Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var code = Input.RecoveryCode.Replace(" ", string.Empty);
        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(code);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} signed in with a recovery code.", user.Id);
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid recovery code.");
        return Page();
    }

    private string SafeReturnUrl(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
    }
}
