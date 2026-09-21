using System.ComponentModel.DataAnnotations;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class GenerateRecoveryCodesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodesModel> _logger;

    public GenerateRecoveryCodesModel(
        UserManager<ApplicationUser> userManager,
        ILogger<GenerateRecoveryCodesModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string[]? RecoveryCodes { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("../Login");
        }

        return await _userManager.GetTwoFactorEnabledAsync(user)
            ? Page()
            : RedirectToPage("./EnableAuthenticator");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("../Login");
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToPage("./EnableAuthenticator");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await _userManager.CheckPasswordAsync(user, Input.Password))
        {
            ModelState.AddModelError(string.Empty, "The current password is incorrect.");
            return Page();
        }

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        if (codes == null)
        {
            ModelState.AddModelError(string.Empty, "Recovery codes could not be generated. Please try again.");
            return Page();
        }

        RecoveryCodes = codes.ToArray();
        _logger.LogInformation("User {UserId} generated new recovery codes.", user.Id);
        StatusMessage = "Your new recovery codes are ready. Save them now; they will only be shown once.";
        return RedirectToPage("./ShowRecoveryCodes");
    }
}
