using System.Collections.Concurrent;
using System.Security.Claims;
using IDS.Data;
using IDS.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IDS.Security;

public sealed record RealtimeSession(HubCallerContext Context, bool IsChat);

public sealed class RealtimeSessionRegistry
{
    private readonly ConcurrentDictionary<string, RealtimeSession> _sessions = new();
    public RealtimeSession[] Snapshot() => _sessions.Values.ToArray();
    public void Add(HubCallerContext context, bool isChat) => _sessions[context.ConnectionId] = new(context, isChat);
    public void Remove(string id) => _sessions.TryRemove(id, out _);
    public void Abort(RealtimeSession session)
    {
        Remove(session.Context.ConnectionId);
        session.Context.Abort();
    }
}

public sealed class RealtimeSessionValidator(
    ApplicationDbContext database, SecurityPolicyService policies, IOptions<IdentityOptions> identity)
{
    public async Task<RealtimeSession[]> GetRejectedSessionsAsync(RealtimeSession[] sessions, CancellationToken cancellationToken)
    {
        var policy = await policies.GetAsync();
        var accounts = new Dictionary<string, IDS.Data.Models.ApplicationUser>();
        var suspendedIds = new HashSet<string>();
        var adminIds = new HashSet<string>();
        // Read each connected account once, even when it has several browser tabs open.
        // Batching also keeps the query below SQL Server's parameter limit.
        foreach (var ids in sessions.Select(session => session.Context.UserIdentifier)
                     .OfType<string>().Distinct().Chunk(500))
        {
            foreach (var user in await database.Users.AsNoTracking()
                         .Where(user => ids.Contains(user.Id)).ToListAsync(cancellationToken))
                accounts[user.Id] = user;
            var roles = await (from role in database.Roles
                                   join membership in database.UserRoles on role.Id equals membership.RoleId
                                   where ids.Contains(membership.UserId) && (role.Name == AppRoles.Suspended || role.Name == AppRoles.Admin)
                                   select new { membership.UserId, role.Name }).ToListAsync(cancellationToken);
            suspendedIds.UnionWith(roles.Where(role => role.Name == AppRoles.Suspended).Select(role => role.UserId));
            adminIds.UnionWith(roles.Where(role => role.Name == AppRoles.Admin).Select(role => role.UserId));
        }
        return sessions.Where(session =>
        {
            var id = session.Context.UserIdentifier;
            return (session.IsChat && !policy.MessagingEnabled) || id == null ||
                !accounts.TryGetValue(id, out var user) || suspendedIds.Contains(id) ||
                !HasValidCredentials(session, user, policy.EmployeeMfaTestBypass && !adminIds.Contains(id));
        }).ToArray();
    }

    public async Task<bool> IsAllowedAsync(RealtimeSession session)
    {
        var id = session.Context.UserIdentifier;
        var policy = await policies.GetAsync();
        if (id == null || (session.IsChat && !policy.MessagingEnabled)) return false;
        var user = await database.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == id);
        if (user == null) return false;
        var roles = await (from role in database.Roles
                       join membership in database.UserRoles on role.Id equals membership.RoleId
                       where membership.UserId == id
                       select role.Name).ToListAsync();
        return !roles.Contains(AppRoles.Suspended) &&
            HasValidCredentials(session, user, policy.EmployeeMfaTestBypass && !roles.Contains(AppRoles.Admin));
    }

    private bool HasValidCredentials(RealtimeSession session, IDS.Data.Models.ApplicationUser user, bool bypassTwoFactor)
    {
        var stamp = session.Context.User?.FindFirstValue(identity.Value.ClaimsIdentity.SecurityStampClaimType);
        var expiredTestSession = session.Context.User?.HasClaim(SecurityPolicyService.BypassedMfaClaim, "true") == true && !bypassTwoFactor;
        return !expiredTestSession && user.EmailConfirmed && (user.TwoFactorEnabled || bypassTwoFactor) && !user.MustChangePassword &&
            (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow) &&
            !string.IsNullOrEmpty(stamp) && stamp == user.SecurityStamp;
    }
}

public sealed class RealtimeAccessFilter(RealtimeSessionRegistry registry, IServiceScopeFactory scopes) : IHubFilter
{
    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        var session = new RealtimeSession(context.Context, context.Hub is ChatHub);
        if (!await IsAllowedAsync(session)) { context.Context.Abort(); return; }
        registry.Add(context.Context, session.IsChat);
        try { await next(context); }
        catch { registry.Remove(context.Context.ConnectionId); throw; }
    }

    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var session = new RealtimeSession(context.Context, context.Hub is ChatHub);
        if (!await IsAllowedAsync(session))
        {
            registry.Abort(session);
            throw new HubException("This session or feature is no longer available. Sign in again.");
        }
        return await next(context);
    }

    public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception,
        Func<HubLifetimeContext, Exception?, Task> next)
    {
        registry.Remove(context.Context.ConnectionId);
        await next(context, exception);
    }

    private async Task<bool> IsAllowedAsync(RealtimeSession session)
    {
        using var scope = scopes.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>().IsAllowedAsync(session);
    }
}

// Idle WebSockets do not pass through cookie authentication again. Check them too,
// including on other app instances, so revoked sessions stop receiving broadcasts.
public sealed class RealtimeSessionMonitor(
    RealtimeSessionRegistry registry, IServiceScopeFactory scopes, ILogger<RealtimeSessionMonitor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var sessions = registry.Snapshot();
                if (sessions.Length == 0) continue;
                try
                {
                    using var scope = scopes.CreateScope();
                    var validator = scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>();
                    foreach (var session in await validator.GetRejectedSessionsAsync(sessions, stoppingToken))
                        registry.Abort(session);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning(exception, "Could not validate live sessions; closing connections until access can be verified.");
                    foreach (var session in sessions) registry.Abort(session);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
