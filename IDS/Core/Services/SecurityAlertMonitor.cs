using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.EntityFrameworkCore;

namespace IDS.Core.Services;

public static class SecurityAlertSettings
{
    public const string Prefix = "Alerts.";
    public const string Enabled = Prefix + "Enabled";
    public const string Threshold = Prefix + "Threshold";
    public const string Window = Prefix + "WindowMinutes";
    public const string Severity = Prefix + "Severity";
    public const string Rules = Prefix + "Rules";
    public const string WorkerFailures = Prefix + "WorkerFailures";
    public const string Notify = Prefix + "NotifyAdmins";
    public const string ResolvedDays = Prefix + "ResolvedDays";
    public const string AccountEvents = Prefix + "AccountEvents";
    public static readonly string[] Severities = ["Low", "Medium", "High", "Critical"];
    public static readonly string[] Categories = ["Detection", "Worker health", "Account security"];

    public sealed record Rule(int Threshold, string Severity);

    public static Dictionary<string, Rule>? ParseRules(string? content)
    {
        var rules = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in (content ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var fields = line.Split('|', StringSplitOptions.TrimEntries);
            if (fields.Length != 3 || fields[0].Length is < 1 or > 100 || !int.TryParse(fields[1], out var threshold) ||
                threshold is < 1 or > 10000 || !Severities.Contains(fields[2]) ||
                !rules.TryAdd(fields[0], new(threshold, fields[2]))) return null;
        }
        return rules;
    }
}

public sealed class SecurityAlertService(ApplicationDbContext db, WorkerHealthService workers)
{
    private const string Cursor = "AlertProcessing.Cursor";
    private const string Initial = "AlertProcessing.Initial";
    private const string BackfillCursor = "AlertProcessing.BackfillCursor";
    private const string BackfillFrom = "AlertProcessing.BackfillFrom";
    private const int BatchSize = 500;
    private sealed record Detection(long Id, DateTime At, string Label, string Source, int? Port);
    private sealed record Group(string Key, string Label, string Source, int? Port, List<Detection> Detections);
    private sealed record DraftNotification(SecurityAlert Alert, string Type);

    public static IQueryable<SecurityAlert> Published(IQueryable<SecurityAlert> source) =>
        source.Where(a => a.GroupKey == null || a.PublishedAt != null);

    public async Task<bool> ScheduleBackfillAsync(int days, CancellationToken token)
    {
        if (days is < 1 or > 30) return false;
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, token);
        await DatabaseLock.AcquireAsync(db, nameof(SecurityAlertService), token);
        var states = await ReadStateAsync(token);
        if (states.ContainsKey(BackfillFrom)) return false;
        await InitializeAsync(states, token);
        Set(states, BackfillFrom, DateTime.UtcNow.AddDays(-days).ToString("O"));
        Set(states, BackfillCursor, "0");
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return true;
    }

    public async Task ProcessAsync(CancellationToken token)
    {
        var settings = await OperationsSettings.ReadAsync(db, SecurityAlertSettings.Prefix, token);
        if (!settings.Enabled(SecurityAlertSettings.Enabled)) return;
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, token);
        await DatabaseLock.AcquireAsync(db, nameof(SecurityAlertService), token);
        var states = await ReadStateAsync(token);
        await InitializeAsync(states, token);
        var now = DateTime.UtcNow;
        var published = new List<DraftNotification>();
        var activeCursor = ReadNumber(states, Cursor);
        var live = await ReadDetections(db.AttackTraffic.Where(a => a.Id > activeCursor), token);
        await GroupAsync(live, settings, now, published, token);
        if (live.Count > 0) Set(states, Cursor, live[^1].Id.ToString(CultureInfo.InvariantCulture));

        var historyCursor = ReadNumber(states, BackfillCursor);
        var initial = ReadNumber(states, Initial);
        if (historyCursor < initial && states.TryGetValue(BackfillFrom, out var backfill) &&
            DateTime.TryParse(backfill.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var from))
        {
            var history = await ReadDetections(db.AttackTraffic.Where(a => a.Id > historyCursor && a.Id <= initial &&
                a.Timestamp >= from), token);
            // Track both batches in the same transaction; since their ID ranges
            // are disjoint their counts can be merged safely into the same group.
            await db.SaveChangesAsync(token);
            await GroupAsync(history, settings, now, published, token);
            Set(states, BackfillCursor, (history.Count < BatchSize ? initial : history[^1].Id).ToString(CultureInfo.InvariantCulture));
        }

        if (settings.Enabled(SecurityAlertSettings.WorkerFailures, false))
        {
            var health = await workers.GetAsync(token);
            await ObserveWorkerAsync("detection", health.Detection, now, published, token);
            await ObserveWorkerAsync("explanation", health.Explanation, now, published, token);
        }

        await db.SaveChangesAsync(token);
        if (published.Count > 0 && settings.Enabled(SecurityAlertSettings.Notify, false))
        {
            var admins = await ActiveAdministratorsAsync(db, token);
            foreach (var item in published.Where(p => p.Alert.Severity is "High" or "Critical"))
            {
                foreach (var recipient in admins)
                    db.InboxNotifications.Add(new()
                    {
                        RecipientId = recipient, Type = item.Type, Message = item.Alert.Message,
                        CreatedAt = now, SecurityAlertId = item.Alert.Id
                    });
            }
            await db.SaveChangesAsync(token);
        }
        await transaction.CommitAsync(token);
    }

    public async Task RecordAccountEventAsync(string action, CancellationToken token)
    {
        var settings = await OperationsSettings.ReadAsync(db, SecurityAlertSettings.Prefix, token);
        if (!settings.Enabled(SecurityAlertSettings.Enabled) || !settings.Enabled(SecurityAlertSettings.AccountEvents, false)) return;
        var now = DateTime.UtcNow;
        var alert = new SecurityAlert
        {
            AlertType = action, EventCategory = "Account security", Severity = "Medium",
            Message = $"{action} for an IDS account. Review the account security audit log.",
            Timestamp = now, PublishedAt = now, LastObservedAt = now
        };
        db.SecurityAlerts.Add(alert);
    }

    internal static async Task<List<string>> ActiveAdministratorsAsync(ApplicationDbContext db, CancellationToken token)
    {
        var now = DateTimeOffset.UtcNow;
        var accounts = await (from u in db.Users
                join membership in db.UserRoles on u.Id equals membership.UserId
                join role in db.Roles on membership.RoleId equals role.Id
                where role.Name == AppRoles.Admin && u.EmailConfirmed && !u.MustChangePassword &&
                    !(from blocked in db.UserRoles
                      join blockedRole in db.Roles on blocked.RoleId equals blockedRole.Id
                      where blocked.UserId == u.Id && blockedRole.Name == AppRoles.Suspended
                      select blocked).Any()
                select new { u.Id, u.LockoutEnd }).ToListAsync(token);
        return accounts.Where(a => a.LockoutEnd == null || a.LockoutEnd <= now)
            .Select(a => a.Id).Distinct().ToList();
    }

    private async Task ObserveWorkerAsync(string worker, WorkerStatus status, DateTime now,
        List<DraftNotification> published, CancellationToken token)
    {
        if (status.LastHeartbeatUtc == null) return;
        var alertType = worker == "detection" ? "Detection worker offline" : "Explanation worker offline";
        var active = await db.SecurityAlerts.Include(a => a.Ticket).Where(a => a.EventCategory == "Worker health" &&
            a.AlertType == alertType && a.ResolvedAt == null).OrderByDescending(a => a.Id).FirstOrDefaultAsync(token);
        if (status.Online)
        {
            if (active != null && (active.Ticket == null || active.Ticket.Status is "Resolved" or "Closed" or "Cancelled")) active.ResolvedAt = now;
            return;
        }
        var key = $"worker:{worker}:{status.LastHeartbeatUtc.Value.Ticks}";
        // Preserve the existing alert through changes to the stale observation and
        // through manual resolution of the same outage.
        if (active != null || await db.SecurityAlerts.AnyAsync(a => a.GroupKey == key, token)) return;
        var alert = new SecurityAlert
        {
            GroupKey = key, AlertType = alertType, Severity = "High", EventCategory = "Worker health",
            Message = $"{alertType}. Review the external worker and its SQL Server connection.",
            Timestamp = now, PublishedAt = now, LastObservedAt = status.LastHeartbeatUtc
        };
        db.SecurityAlerts.Add(alert);
        published.Add(new(alert, "Worker health alert"));
    }

    private async Task GroupAsync(List<Detection> batch, IReadOnlyDictionary<string, string> settings, DateTime now,
        List<DraftNotification> published, CancellationToken token)
    {
        if (batch.Count == 0) return;
        var duration = TimeSpan.FromMinutes(settings.Number(SecurityAlertSettings.Window, 10, 1, 120)).Ticks;
        var rules = SecurityAlertSettings.ParseRules(settings.GetValueOrDefault(SecurityAlertSettings.Rules)) ?? [];
        var severity = settings.GetValueOrDefault(SecurityAlertSettings.Severity);
        var fallback = new SecurityAlertSettings.Rule(settings.Number(SecurityAlertSettings.Threshold, 5, 1, 10000),
            SecurityAlertSettings.Severities.Contains(severity) ? severity! : "Medium");
        var groups = batch.GroupBy(d => new { Bucket = d.At.Ticks / duration, Label = d.Label.ToUpperInvariant(),
            Source = d.Source.Trim().ToUpperInvariant(), d.Port })
            .Select(g => new Group("d:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                System.Text.Json.JsonSerializer.Serialize(new { duration, g.Key })))), g.First().Label, g.First().Source,
                g.Key.Port, g.ToList())).ToList();
        var keys = groups.Select(g => g.Key).ToArray();
        var existing = await db.SecurityAlerts.Where(a => a.GroupKey != null && keys.Contains(a.GroupKey))
            .ToDictionaryAsync(a => a.GroupKey!, token);
        foreach (var group in groups)
        {
            var rule = rules.GetValueOrDefault(group.Label) ?? fallback;
            var first = group.Detections.MinBy(d => d.At)!;
            var last = group.Detections.Max(d => d.At);
            if (!existing.TryGetValue(group.Key, out var alert))
            {
                alert = new()
                {
                    GroupKey = group.Key, AttackLabel = group.Label, EventCategory = "Detection",
                    AlertType = group.Label, Message = $"Grouped malicious flows · {group.Label}",
                    SourceIP = group.Source, DestinationPort = group.Port, Timestamp = first.At,
                    RepresentativeAttackId = first.Id
                };
                db.SecurityAlerts.Add(alert);
            }
            alert.Timestamp = alert.Timestamp < first.At ? alert.Timestamp : first.At;
            alert.LastObservedAt = alert.LastObservedAt > last ? alert.LastObservedAt : last;
            alert.DetectionCount += group.Detections.Count;
            if (alert.PublishedAt == null)
            {
                alert.Severity = rule.Severity;
                if (alert.DetectionCount >= rule.Threshold)
                {
                    alert.PublishedAt = now;
                    published.Add(new(alert, "Grouped detection alert"));
                }
            }
        }
    }

    private IQueryable<SystemSetting> State => db.SystemSettings.Where(s => s.Key.StartsWith("AlertProcessing."));
    private Task<Dictionary<string, SystemSetting>> ReadStateAsync(CancellationToken token) => State.ToDictionaryAsync(s => s.Key, token);

    private async Task InitializeAsync(Dictionary<string, SystemSetting> states, CancellationToken token)
    {
        if (states.ContainsKey(Cursor)) return;
        var latest = await db.AttackTraffic.MaxAsync(a => (long?)a.Id, token) ?? 0;
        Set(states, Cursor, latest.ToString(CultureInfo.InvariantCulture));
        Set(states, Initial, latest.ToString(CultureInfo.InvariantCulture));
    }

    private void Set(Dictionary<string, SystemSetting> states, string key, string value)
    {
        if (!states.TryGetValue(key, out var setting))
        {
            setting = new SystemSetting { Key = key };
            states.Add(key, setting);
            db.SystemSettings.Add(setting);
        }
        setting.Value = value;
        setting.LastModified = DateTime.UtcNow;
    }

    private static long ReadNumber(Dictionary<string, SystemSetting> settings, string key) =>
        long.TryParse(settings.GetValueOrDefault(key)?.Value, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private static Task<List<Detection>> ReadDetections(IQueryable<AttackTraffic> query, CancellationToken token) =>
        query.AsNoTracking().OrderBy(a => a.Id).Take(BatchSize).Select(a => new Detection(
            a.Id, a.Timestamp, DetectionLabels.Display(a.AttackType), a.SrcIp, a.DstPort)).ToListAsync(token);
}

public sealed class SecurityAlertMonitor(IServiceScopeFactory services, ILogger<SecurityAlertMonitor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        do
        {
            try
            {
                using var scope = services.CreateScope();
                await scope.ServiceProvider.GetRequiredService<SecurityAlertService>().ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error) { logger.LogWarning("Alert generation will retry after {ErrorType}.", error.GetType().Name); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
