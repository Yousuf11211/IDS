using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

[Authorize]
public class FirstTimeSetupModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public FirstTimeSetupModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "The {0} must be between {2} and {1} characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        [Display(Name = "First name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last name")]
        public string? LastName { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("./Login");
        }

        if (!user.MustChangePassword)
        {
            return await NextSetupStepAsync(user);
        }

        Input = new InputModel
        {
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("./Login");
        }

        // A completed account must not be able to reset its password through onboarding.
        if (!user.MustChangePassword)
        {
            return await NextSetupStepAsync(user);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.UserName = Input.UserName.Trim();
        user.FirstName = Input.FirstName?.Trim() ?? string.Empty;
        user.LastName = Input.LastName?.Trim() ?? string.Empty;

        // ResetPasswordAsync saves the password and profile in one Identity update.
        user.MustChangePassword = false;
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, Input.NewPassword);
        if (!passwordResult.Succeeded)
        {
            user.MustChangePassword = true;
            AddErrors(passwordResult);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        return await NextSetupStepAsync(user);
    }

    private async Task<IActionResult> NextSetupStepAsync(ApplicationUser user)
    {
        // The account stays in onboarding until an authenticator code has been verified.
        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToPage("./Manage/EnableAuthenticator");
        }

        return RedirectToPage("/Index", new { area = "" });
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
