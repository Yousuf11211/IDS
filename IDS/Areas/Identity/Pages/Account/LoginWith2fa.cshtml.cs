using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

public class LoginWith2faModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginWith2faModel> _logger;

    public LoginWith2faModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginWith2faModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; private set; } = "/";
    public bool RememberMe { get; private set; }

    public class InputModel
    {
        [Required]
        [StringLength(7, MinimumLength = 6)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
    {
        if (await _signInManager.GetTwoFactorAuthenticationUserAsync() == null)
        {
            return RedirectToPage("./Login");
        }

        ReturnUrl = SafeReturnUrl(returnUrl);
        RememberMe = rememberMe;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        RememberMe = rememberMe;

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToPage("./Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var code = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        // Browser trust is disabled so every new sign-in asks for an authenticator code.
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            code, rememberMe, rememberClient: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} signed in with two-factor authentication.", user.Id);
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Page();
    }

    private string SafeReturnUrl(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
    }
}
