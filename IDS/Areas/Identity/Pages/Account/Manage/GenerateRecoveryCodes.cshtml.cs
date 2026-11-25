using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class GenerateRecoveryCodesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodesModel> _logger;
    private readonly ApplicationDbContext _database;

    public GenerateRecoveryCodesModel(
        UserManager<ApplicationUser> userManager,
        ILogger<GenerateRecoveryCodesModel> logger,
        ApplicationDbContext database)
    {
        _userManager = userManager;
        _logger = logger;
        _database = database;
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
        [Required, StringLength(7, MinimumLength = 6)]
        [Display(Name = "Current authenticator code")]
        public string Code { get; set; } = string.Empty;
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
        await using var transaction = await _database.Database.BeginTransactionAsync();

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        if (codes == null)
        {
            ModelState.AddModelError(string.Empty, "Recovery codes could not be generated. Please try again.");
            return Page();
        }

        _database.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id, UserEmail = user.Email ?? "", EntityId = user.Id, EntityType = "Authentication",
            Action = "Security.RecoveryCodesReplaced", Timestamp = DateTime.UtcNow
        });
        await _database.SaveChangesAsync();
        await transaction.CommitAsync();
        RecoveryCodes = codes.ToArray();
        _logger.LogInformation("User {UserId} generated new recovery codes.", user.Id);
        StatusMessage = "Your new recovery codes are ready. Save them now; they will only be shown once.";
        return RedirectToPage("./ShowRecoveryCodes");
    }
}
