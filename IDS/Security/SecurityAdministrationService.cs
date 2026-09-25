using System.Data;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IDS.Security;

public static class SecurityActions
{
    public const string EnableMessaging = "EnableMessaging";
    public const string EnableInvitations = "EnableInvitations";
    public const string ResetAuthenticator = "ResetEmployeeAuthenticator";
    public const string GrantAdmin = "GrantAdministrator";
    public const string EnableEmployeeMfaBypass = "EnableEmployeeMfaTestBypass";

    public static string Label(string action) => action switch
    {
        EnableMessaging => "Enable employee messaging",
        EnableInvitations => "Enable employee invitations",
        ResetAuthenticator => "Reset employee authenticator",
        GrantAdmin => "Grant administrator access",
        EnableEmployeeMfaBypass => "Bypass employee 2FA for development testing",
        _ => action
    };
}

public sealed class SecurityAdministrationService(
    ApplicationDbContext database,
    UserManager<ApplicationUser> users,
    AccessControlService accessControl,
    SecurityPolicyService policies,
    ILogger<SecurityAdministrationService> logger)
{
    public async Task<SecurityChangeRequest> RequestAsync(
        string actorId, string action, string? targetId, string reason, string? ipAddress)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var actor = await RequireAdminAsync(actorId);
        reason = ValidateReason(reason);
        ApplicationUser? target = null;
        if (action is SecurityActions.ResetAuthenticator or SecurityActions.GrantAdmin)
            target = await RequireTargetAsync(action, targetId);
        else if (action is not (SecurityActions.EnableMessaging or SecurityActions.EnableInvitations or SecurityActions.EnableEmployeeMfaBypass))
            throw new SecurityChangeException("Choose a supported security action.");
        if (action == SecurityActions.EnableEmployeeMfaBypass && !policies.AllowsTestingBypass)
            throw new SecurityChangeException("The employee authenticator bypass is available only in Development mode.");

        if (await database.SecurityChangeRequests.AnyAsync(request => request.Status == "Pending" &&
            request.Action == action && request.TargetUserId == (target == null ? null : target.Id) &&
            request.ExpiresAtUtc > DateTime.UtcNow))
            throw new SecurityChangeException("An active request for this change already exists.");

        var request = new SecurityChangeRequest
        {
            Action = action, RequestedById = actor.Id, RequestedByEmail = actor.Email ?? string.Empty,
            RequesterSecurityStamp = actor.SecurityStamp ?? string.Empty,
            TargetUserId = target?.Id, TargetEmail = target?.Email, TargetSecurityStamp = target?.SecurityStamp,
            Reason = reason, CreatedAtUtc = DateTime.UtcNow, ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
        };
        database.SecurityChangeRequests.Add(request);
        AddAudit(actor, "Security.Requested", action, target?.Id, reason, ipAddress);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return request;
    }

    public async Task ReviewAsync(string actorId, long requestId, bool approve, string? ipAddress)
    {
        // The transaction and version token prevent two reviewers from applying the same request.
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var actor = await RequireAdminAsync(actorId);
        var request = await database.SecurityChangeRequests.SingleOrDefaultAsync(item => item.Id == requestId)
            ?? throw new SecurityChangeException("This request no longer exists.");
        if (request.Status != "Pending" || request.ExpiresAtUtc <= DateTime.UtcNow)
            throw new SecurityChangeException("This request is no longer pending. Submit a new request if needed.");
        if (approve && request.RequestedById == actor.Id)
            throw new SecurityChangeException("A different administrator must approve this change.");

        if (approve)
        {
            var requester = await RequireAdminAsync(request.RequestedById);
            if (requester.SecurityStamp != request.RequesterSecurityStamp)
                throw new SecurityChangeException("The requester's credentials changed. Submit a new request.");

            ApplicationUser? target = null;
            if (request.Action is SecurityActions.ResetAuthenticator or SecurityActions.GrantAdmin)
            {
                target = await RequireTargetAsync(request.Action, request.TargetUserId);
                if (target.SecurityStamp != request.TargetSecurityStamp || target.Email != request.TargetEmail)
                    throw new SecurityChangeException("The employee's account changed. Submit a new request.");
            }

            switch (request.Action)
            {
                case SecurityActions.EnableEmployeeMfaBypass:
                    if (!policies.AllowsTestingBypass)
                        throw new SecurityChangeException("The employee authenticator bypass is available only in Development mode.");
                    await SetPolicyAsync(SecurityPolicyService.EmployeeMfaBypassKey, true, actor.Id);
                    break;
                case SecurityActions.EnableMessaging:
                    await SetPolicyAsync(SecurityPolicyService.MessagingKey, true, actor.Id);
                    break;
                case SecurityActions.EnableInvitations:
                    await SetPolicyAsync(SecurityPolicyService.InvitationsKey, true, actor.Id);
                    break;
                case SecurityActions.ResetAuthenticator:
                    EnsureSucceeded(await users.SetTwoFactorEnabledAsync(target!, false));
                    EnsureSucceeded(await users.ResetAuthenticatorKeyAsync(target!));
                    await users.GenerateNewTwoFactorRecoveryCodesAsync(target!, 0);
                    EnsureSucceeded(await users.UpdateSecurityStampAsync(target!));
                    break;
                case SecurityActions.GrantAdmin:
                    // This is the only web workflow allowed to grant the Admin role.
                    await accessControl.SetExclusiveRoleAsync(users, target!, AppRoles.Admin, allowAdministratorChange: true);
                    EnsureSucceeded(await users.UpdateSecurityStampAsync(target!));
                    break;
                default:
                    throw new SecurityChangeException("This request contains an unsupported action.");
            }
        }

        request.Status = approve ? "Approved" : request.RequestedById == actor.Id ? "Cancelled" : "Rejected";
        request.ReviewedById = actor.Id;
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.Version = Guid.NewGuid();
        AddAudit(actor, approve ? "Security.Applied" : "Security.Rejected", request.Action,
            request.TargetUserId, $"Request {request.Id}: {request.Reason}", ipAddress);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogWarning("Security request {RequestId} was {Status} by {ReviewerId}; requester {RequesterId}",
            request.Id, request.Status, actor.Id, request.RequestedById);
    }

    public async Task DisableFeatureAsync(string actorId, string key, string reason, string? ipAddress)
    {
        if (key is not (SecurityPolicyService.MessagingKey or SecurityPolicyService.InvitationsKey or SecurityPolicyService.EmployeeMfaBypassKey))
            throw new SecurityChangeException("Choose a supported feature.");
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var actor = await RequireAdminAsync(actorId);
        reason = ValidateReason(reason);
        await SetPolicyAsync(key, false, actor.Id);

        // Emergency disable also cancels stale requests that could re-enable the feature later.
        var action = key switch
        {
            SecurityPolicyService.MessagingKey => SecurityActions.EnableMessaging,
            SecurityPolicyService.EmployeeMfaBypassKey => SecurityActions.EnableEmployeeMfaBypass,
            _ => SecurityActions.EnableInvitations
        };
        var pending = await database.SecurityChangeRequests
            .Where(request => request.Status == "Pending" && request.Action == action).ToListAsync();
        foreach (var request in pending)
        {
            request.Status = "Cancelled";
            request.ReviewedById = actor.Id;
            request.ReviewedAtUtc = DateTime.UtcNow;
            request.Version = Guid.NewGuid();
        }
        AddAudit(actor, "Security.Disabled", key, null, reason, ipAddress);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogWarning("Security feature {Feature} disabled by {ActorId}", key, actor.Id);
    }

    public async Task RevokeSessionsAsync(string actorId, string targetId, string reason, string? ipAddress)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        var actor = await RequireAdminAsync(actorId);
        var target = await users.FindByIdAsync(targetId)
            ?? throw new SecurityChangeException("The employee no longer exists.");
        reason = ValidateReason(reason);
        EnsureSucceeded(await users.UpdateSecurityStampAsync(target));
        AddAudit(actor, "Security.SessionsRevoked", "User", target.Id, reason, ipAddress);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<ApplicationUser> RequireAdminAsync(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user == null || !user.EmailConfirmed || user.MustChangePassword ||
            (!user.TwoFactorEnabled && !await policies.CanBypassTwoFactorAsync(user)) ||
            await users.IsLockedOutAsync(user) || !await users.IsInRoleAsync(user, AppRoles.Admin) ||
            await users.IsInRoleAsync(user, AppRoles.Suspended))
            throw new SecurityChangeException("An active administrator with verified two-factor authentication is required.");
        return user;
    }

    private async Task<ApplicationUser> RequireTargetAsync(string action, string? id)
    {
        var user = string.IsNullOrWhiteSpace(id) ? null : await users.FindByIdAsync(id);
        if (user == null || await users.IsInRoleAsync(user, AppRoles.Admin) ||
            await users.IsInRoleAsync(user, AppRoles.Suspended))
            throw new SecurityChangeException("Select an active non-admin employee. Administrator recovery requires the deployment operator.");
        if (action == SecurityActions.GrantAdmin &&
            (!user.EmailConfirmed || user.MustChangePassword || !user.TwoFactorEnabled || await users.IsLockedOutAsync(user)))
            throw new SecurityChangeException("The employee must complete account setup and two-factor enrollment before promotion.");
        return user;
    }

    private async Task SetPolicyAsync(string key, bool value, string actorId)
    {
        var setting = await database.SystemSettings.SingleOrDefaultAsync(item => item.Key == key);
        if (setting == null)
        {
            setting = new SystemSetting { Key = key };
            database.SystemSettings.Add(setting);
        }
        setting.Value = value.ToString();
        setting.ModifiedByUserId = actorId;
        setting.LastModified = DateTime.UtcNow;
    }

    private void AddAudit(ApplicationUser actor, string action, string entity, string? id, string reason, string? ip)
        => database.AuditLogs.Add(new AuditLog
        {
            Timestamp = DateTime.UtcNow, UserId = actor.Id, UserEmail = actor.Email ?? string.Empty,
            Action = action, EntityType = entity, EntityId = id, Details = reason, IpAddress = ip
        });

    private static string ValidateReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10 || reason.Length > 1000)
            throw new SecurityChangeException("Provide a reason between 10 and 1,000 characters.");
        return reason.Trim();
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new SecurityChangeException("The account changed during this operation. Reload the page and try again.");
    }
}

public sealed class SecurityChangeException(string message) : Exception(message);
