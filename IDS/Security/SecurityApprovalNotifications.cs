using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IDS.Security;

public sealed record ApprovalNotification(long Id, string Action, string RequestedByEmail, string? TargetEmail);
public sealed record ApprovalNotificationList(int Count, IReadOnlyList<ApprovalNotification> Requests);

public sealed class SecurityApprovalNotifications(
    ApplicationDbContext database, UserManager<ApplicationUser> users, SecurityPolicyService policies)
{
    public async Task<ApprovalNotificationList> GetAsync(ApplicationUser user)
    {
        if (!user.EmailConfirmed || user.MustChangePassword || await users.IsLockedOutAsync(user) ||
            !await users.IsInRoleAsync(user, AppRoles.Admin) || await users.IsInRoleAsync(user, AppRoles.Suspended) ||
            (!user.TwoFactorEnabled && !await policies.CanBypassTwoFactorAsync(user)))
            throw new SecurityChangeException("An active administrator is required.");

        var pending = database.SecurityChangeRequests.AsNoTracking().Where(request =>
            request.Status == "Pending" && request.ExpiresAtUtc > DateTime.UtcNow && request.RequestedById != user.Id);
        var count = await pending.CountAsync();
        var requests = await pending.OrderByDescending(request => request.Id).Take(20)
            .Select(request => new { request.Id, request.Action, request.RequestedByEmail, request.TargetEmail }).ToListAsync();
        return new(count, requests.Select(request => new ApprovalNotification(request.Id,
            SecurityActions.Label(request.Action), request.RequestedByEmail, request.TargetEmail)).ToArray());
    }
}
