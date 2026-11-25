using IDS.Data;
using IDS.Data.Models;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _database;

    public sealed record RecoveryStatus(long Id, string Status, DateTime CreatedAt, DateTime ExpiresAt);
    public List<RecoveryStatus> Requests { get; private set; } = [];

    public TwoFactorAuthenticationModel(UserManager<ApplicationUser> userManager, SecurityPolicyService policies, ApplicationDbContext database)
    {
        _userManager = userManager;
        _policies = policies;
        _database = database;
    }

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

        TestingBypass = await _policies.CanBypassTwoFactorAsync(user);
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
        Requests = await _database.AuthenticatorResetRequests.AsNoTracking().Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).Take(10)
            .Select(r => new RecoveryStatus(r.Id, r.Status, r.CreatedAt, r.ExpiresAt))
            .ToListAsync(HttpContext.RequestAborted);
        return Page();
    }
}
