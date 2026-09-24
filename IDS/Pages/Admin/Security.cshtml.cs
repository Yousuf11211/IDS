using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SecurityModel(
    ApplicationDbContext database,
    UserManager<ApplicationUser> users,
    SecurityPolicyService policies,
    SecurityAdministrationService administration) : PageModel
{
    public SecurityPolicy Policy { get; private set; } = new(false, true);
    public List<SecurityChangeRequest> Requests { get; private set; } = new();
    public List<AuditLog> Audit { get; private set; } = new();
    public List<EmployeeOption> Employees { get; private set; } = new();
    public string ActorId => users.GetUserId(User)!;
    public int EligibleAdministrators { get; private set; }
    public bool AllowsTestingBypass => policies.AllowsTestingBypass;
    [TempData] public string? StatusMessage { get; set; }
    public sealed record EmployeeOption(string Id, string Email);

    public async Task OnGetAsync()
    {
        Policy = await policies.GetAsync();
        Requests = await database.SecurityChangeRequests.AsNoTracking()
            .OrderByDescending(request => request.Id).Take(100).ToListAsync();
        Audit = await database.AuditLogs.AsNoTracking().Where(log => log.Action.StartsWith("Security."))
            .OrderByDescending(log => log.Id).Take(100).ToListAsync();
        Employees = await users.Users.AsNoTracking().OrderBy(user => user.Email)
            .Select(user => new EmployeeOption(user.Id, user.Email ?? user.UserName ?? user.Id)).ToListAsync();
        var admins = await users.GetUsersInRoleAsync(AppRoles.Admin);
        EligibleAdministrators = admins.Count(user => user.EmailConfirmed && user.TwoFactorEnabled &&
            !user.MustChangePassword && (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow));
    }

    public Task<IActionResult> OnPostRequestAsync(string action, string? targetId, string reason, bool acceptPreview = false)
        => ExecuteAsync(async () =>
        {
            if (action == SecurityActions.EnableMessaging && !acceptPreview)
                throw new SecurityChangeException("Acknowledge the messaging preview limitations before requesting access.");
            var request = await administration.RequestAsync(ActorId, action, targetId, reason, IpAddress);
            return $"Request #{request.Id} awaits a different administrator's approval. No change has been applied.";
        });

    public Task<IActionResult> OnPostDisableAsync(string key, string reason) => ExecuteAsync(async () =>
    {
        await administration.DisableFeatureAsync(ActorId, key, reason, IpAddress);
        if (key == SecurityPolicyService.EmployeeMfaBypassKey)
            return "Employee authenticator checks restored. Test sessions end on their next request; live connections are rechecked every five seconds.";
        return "The feature is disabled. Live messaging connections are rechecked every five seconds.";
    });

    public Task<IActionResult> OnPostReviewAsync(long requestId, bool approve) => ExecuteAsync(async () =>
    {
        await administration.ReviewAsync(ActorId, requestId, approve, IpAddress);
        return approve ? "The approved change has been applied." : "The request was closed without applying it.";
    });

    public Task<IActionResult> OnPostRevokeAsync(string targetId, string reason) => ExecuteAsync(async () =>
    {
        await administration.RevokeSessionsAsync(ActorId, targetId, reason, IpAddress);
        return "Sessions revoked. HTTP access ends on the next request; live connections are rechecked every five seconds. The user can sign in again.";
    });

    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

    private async Task<IActionResult> ExecuteAsync(Func<Task<string>> operation)
    {
        try { StatusMessage = await operation(); }
        catch (SecurityChangeException exception) { StatusMessage = "Error: " + exception.Message; }
        catch (DbUpdateConcurrencyException) { StatusMessage = "Error: This request changed. Reload and review its latest status."; }
        return RedirectToPage();
    }
}
