using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IDS.Core.Services;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace IDS.Data.Services;

public sealed record AccountEmail(string To, string Name, string Title, string Description, DateTime OccurredAt,
    string? ActionLabel = null, string? ActionUrl = null, bool Confirmation = false);

public sealed class AccountEmailQueue(ApplicationDbContext db, IDataProtectionProvider provider)
{
    private readonly IDataProtector _protection = provider.CreateProtector("IDS.SecurityEmail.v1");
    private const string LockName = "IDS.SecurityEmail";

    public static string Fingerprint(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public void Enqueue(string type, string key, AccountEmail payload, DateTime? expiresAt = null)
    {
        var id = Fingerprint(type + ":" + key);
        var now = DateTime.UtcNow;
        db.SecurityEmails.Add(new SecurityEmail
        {
            EventKey = id, EventType = type, CreatedAt = now, DueAt = now, ExpiresAt = expiresAt,
            ProtectedPayload = _protection.Protect(JsonSerializer.Serialize(payload))
        });
    }

    public void Confirm(ApplicationUser user, string type, string description, string? to = null, string discriminator = "")
    {
        var email = to ?? (user.EmailConfirmed ? user.Email : null);
        if (string.IsNullOrWhiteSpace(email)) return;
        Enqueue(type, user.Id + ":" + user.SecurityStamp + ":" + discriminator,
            new(email, IncidentWorkflow.DisplayName(user), type, description, DateTime.UtcNow, Confirmation: true));
    }

    public async Task<bool> ProcessAsync(MailjetEmailSender smtp, CancellationToken token)
    {
        var settings = await smtp.LoadAsync(token);
        var now = DateTime.UtcNow;
        var claim = Guid.NewGuid();
        SecurityEmail? item;
        await using (var transaction = await db.Database.BeginTransactionAsync(token))
        {
            await DatabaseLock.AcquireAsync(db, LockName, token);
            var ready = db.SecurityEmails.Where(e => e.AcceptedAt == null && e.FailedAt == null && e.DueAt <= now);
            if (!settings.Ready) ready = ready.Where(e => e.ExpiresAt != null && e.ExpiresAt <= now);
            item = await ready.OrderBy(e => e.DueAt).ThenBy(e => e.Id).FirstOrDefaultAsync(token);
            if (item == null) return false;
            item.Lease = claim;
            item.LastAttemptAt = now;
            item.DueAt = now.AddMinutes(2);
            item.Attempts++;
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }

        var accepted = false;
        string? failure = null;
        if (item.ExpiresAt <= now) failure = "Expired";
        else if (item.Attempts > 6) failure = "RetryLimit";
        else
        {
            try
            {
                var payload = JsonSerializer.Deserialize<AccountEmail>(_protection.Unprotect(item.ProtectedPayload!))
                    ?? throw new CryptographicException();
                (accepted, _) = await smtp.SendAccountSecurityAsync(payload, $"ids-{item.EventKey.ToLowerInvariant()}@ids.security", token);
                if (!accepted) failure = "ProviderSubmission";
            }
            catch (Exception error) when (error is CryptographicException or JsonException)
            {
                failure = "ProtectedPayloadUnavailable";
            }
        }
        token.ThrowIfCancellationRequested();
        db.ChangeTracker.Clear();
        await using var resultTransaction = await db.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(db, LockName, token);
        item = await db.SecurityEmails.SingleAsync(e => e.Id == item.Id, token);
        if (item.Lease != claim) return true;
        now = DateTime.UtcNow;
        item.Lease = null;
        item.FailureCode = failure;
        if (accepted)
        {
            item.AcceptedAt = now;
            item.ProtectedPayload = null;
        }
        else if (item.Attempts >= 6 || failure is "Expired" or "ProtectedPayloadUnavailable")
        {
            item.FailedAt = now;
            if (item.ExpiresAt <= now || failure == "ProtectedPayloadUnavailable") item.ProtectedPayload = null;
            foreach (var id in await SecurityAlertService.ActiveAdministratorsAsync(db, token))
                db.InboxNotifications.Add(new()
                {
                    RecipientId = id, Type = "Security email needs attention", IsAccountSecurity = true,
                    Message = $"A {item.EventType} message could not be submitted. Review email delivery in Administration.",
                    CreatedAt = now
                });
        }
        else
        {
            item.DueAt = now.AddSeconds(Math.Min(900, 15 * Math.Pow(3, item.Attempts - 1)));
            if (item.ExpiresAt < item.DueAt) item.DueAt = item.ExpiresAt.Value;
        }
        await db.SaveChangesAsync(token);
        await resultTransaction.CommitAsync(token);
        return true;
    }

    public async Task<bool> RetryAsync(long id, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var changed = await db.SecurityEmails.Where(e => e.Id == id && e.FailedAt != null && e.ProtectedPayload != null &&
                (e.ExpiresAt == null || e.ExpiresAt > now))
            .ExecuteUpdateAsync(update => update.SetProperty(e => e.FailedAt, (DateTime?)null)
                .SetProperty(e => e.Attempts, 0).SetProperty(e => e.DueAt, now), token);
        return changed == 1;
    }
}

public sealed class AccountEmailDispatcher(IServiceScopeFactory scopes, ILogger<AccountEmailDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(8));
        var nextCleanup = DateTime.UtcNow;
        do
        {
            try
            {
                if (DateTime.UtcNow >= nextCleanup)
                {
                    using var cleanup = scopes.CreateScope();
                    var db = cleanup.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;
                    await db.RecoveryThrottles.Where(t => t.WindowStart < now.AddDays(-2)).ExecuteDeleteAsync(stoppingToken);
                    await using (var expiration = await db.Database.BeginTransactionAsync(stoppingToken))
                    {
                        await DatabaseLock.AcquireAsync(db, AccountRecovery.RequestLock, stoppingToken);
                        await db.AuthenticatorResetRequests.Where(r => r.ExpiresAt <= now &&
                            (r.Status == "Pending" || r.Status == "Approved")).ExecuteUpdateAsync(
                                update => update.SetProperty(r => r.Status, "Expired"), stoppingToken);
                        await expiration.CommitAsync(stoppingToken);
                    }
                    // A link can expire after all delivery attempts have already failed.
                    // Retain delivery metadata and drop the expired encrypted payload.
                    await db.SecurityEmails.Where(m => m.FailedAt != null && m.ProtectedPayload != null && m.ExpiresAt <= now)
                        .ExecuteUpdateAsync(update => update.SetProperty(m => m.ProtectedPayload, (string?)null), stoppingToken);
                    nextCleanup = now.AddHours(1);
                }
                for (var i = 0; i < 20; i++)
                {
                    using var scope = scopes.CreateScope();
                    var queue = scope.ServiceProvider.GetRequiredService<AccountEmailQueue>();
                    var smtp = scope.ServiceProvider.GetRequiredService<MailjetEmailSender>();
                    if (!await queue.ProcessAsync(smtp, stoppingToken)) break;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error) { logger.LogWarning("Security email processing will retry after {ErrorType}.", error.GetType().Name); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
