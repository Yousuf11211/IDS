using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; private set; } = "/";

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Email or username")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var identifier = Input.Email.Trim();
        var user = await _userManager.FindByNameAsync(identifier)
            ?? await _userManager.FindByEmailAsync(identifier);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        if (await _userManager.IsInRoleAsync(user, AppRoles.Suspended))
        {
            _logger.LogWarning("Suspended user {UserId} attempted to sign in.", user.Id);
            ModelState.AddModelError(string.Empty, "This account is suspended. Contact your administrator.");
            return Page();
        }

        // Identity validates the password, lockout, confirmation and two-factor state in one flow.
        var result = await _signInManager.PasswordSignInAsync(
            user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { returnUrl = ReturnUrl, rememberMe = Input.RememberMe });
        }

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} signed in.", user.Id);
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, result.IsNotAllowed
            ? "Please confirm your account before signing in."
            : "Invalid login attempt.");
        return Page();
    }

    private string SafeReturnUrl(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/");
    }
}
