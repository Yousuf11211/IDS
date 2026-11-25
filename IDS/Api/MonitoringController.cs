using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IDS.Api;

[Authorize]
[ApiController]
[Route("api/monitoring")]
public sealed partial class MonitoringController(ApplicationDbContext database, IMemoryCache cache) : ControllerBase
{
    public sealed record AlertRow(string Id, DateTime Timestamp, string Type, string Severity, string Source, int? Port);
    public sealed record TrafficBucket(DateTime Start, long Benign, long Malicious);
    public sealed record Breakdown(string Label, long Count);
    public sealed record AttackBreakdown(string Label, long Count, DateTime First, DateTime Last, long[] Trend);
    public sealed record FlowTotals(long Benign, long Malicious);
    public sealed record Analytics(DateTime From, DateTime To, int BucketMinutes, List<TrafficBucket> Traffic,
        FlowTotals Totals, FlowTotals? Previous, List<AttackBreakdown> Types, List<Breakdown> Incidents,
        List<Breakdown> IncidentMetrics, List<Breakdown> Explanations, List<Breakdown> Sources, List<Breakdown> Ports);
    private sealed record BucketCount(int Bucket, long Count);

    [HttpGet("analytics")]
    public async Task<IActionResult> AnalyticsAsync(string? range = null, bool live = false,
        DateOnly? from = null, DateOnly? through = null, CancellationToken token = default)
    {
        var settings = await OperationsSettings.ReadAsync(database, "Operations.", token);
        var ranges = live ? MonitoringWindow.LiveRanges : MonitoringWindow.DashboardRanges;
        var defaultRange = settings.GetValueOrDefault(live ? OperationsSettings.LiveRangeKey : OperationsSettings.DashboardRangeKey);
        range ??= defaultRange is not null and not "custom" && ranges.Contains(defaultRange) ? defaultRange : live ? "15m" : "24h";
        var window = ranges.Contains(range) ? MonitoringWindow.Resolve(range, DateTime.UtcNow, from, through) : null;
        if (window == null) return BadRequest("Choose a valid UTC time range.");
        var global = AppRoles.SupportRoles.Any(User.IsInRole);
        var snapshot = await cache.GetOrCreateAsync(("ids-analytics-v2", range, live, from, through, global), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(range is "all" or "custom" ? 60 : 8);
            return await LoadAnalyticsAsync(window, global, live, token);
        });
        return Ok(new
        {
            snapshot, range,
            trafficSeries = settings.GetValueOrDefault(OperationsSettings.TrafficSeriesKey) == "total" ? "total" : "split",
            autoRefresh = live || settings.Enabled(OperationsSettings.DashboardAutoRefreshKey),
            refreshSeconds = live
                ? settings.Number(OperationsSettings.LiveRefreshKey, 10, 5, 300)
                : settings.Number(OperationsSettings.RefreshKey, 30, 15, 300)
        });
    }

    private async Task<Analytics> LoadAnalyticsAsync(MonitoringWindow window, bool global, bool live, CancellationToken token)
    {
        var from = window.From;
        var to = window.To;
        if (from == DateTime.MinValue)
        {
            var firstBenign = await database.BenignTraffic.Where(a => a.Timestamp < to)
                .MinAsync(a => (DateTime?)a.Timestamp, token);
            var firstAttack = await database.AttackTraffic.Where(a => a.Timestamp < to)
                .MinAsync(a => (DateTime?)a.Timestamp, token);
            from = new[] { firstBenign ?? to, firstAttack ?? to }.Min();
        }
        var (start, minutes, bucketCount) = window.Buckets(from);
        var benign = await database.BenignTraffic.Where(a => a.Timestamp >= from && a.Timestamp < to)
            .GroupBy(t => (int)((EF.Functions.DateDiffDay(start, t.Timestamp) * 1440L + t.Timestamp.Hour * 60 + t.Timestamp.Minute - start.Hour * 60 - start.Minute) / minutes))
            .Select(g => new BucketCount(g.Key, g.LongCount())).ToDictionaryAsync(g => g.Bucket, g => g.Count, token);
        var attackQuery = database.AttackTraffic.Where(a => a.Timestamp >= from && a.Timestamp < to);
        var attacks = await attackQuery.GroupBy(t => new
        {
            Bucket = (int)((EF.Functions.DateDiffDay(start, t.Timestamp) * 1440L + t.Timestamp.Hour * 60 + t.Timestamp.Minute - start.Hour * 60 - start.Minute) / minutes),
            Label = t.AttackType
        }).Select(g => new
        {
            g.Key.Bucket, g.Key.Label, Count = g.LongCount(), First = g.Min(a => a.Timestamp), Last = g.Max(a => a.Timestamp)
        }).ToListAsync(token);
        // Bucket boundaries follow UTC clock minutes. Keep the final partial minute in the final bucket.
        int Bucket(int value) => Math.Clamp(value, 0, bucketCount - 1);
        var benignByBucket = benign.GroupBy(k => Bucket(k.Key)).ToDictionary(g => g.Key, g => g.Sum(k => k.Value));
        var attacksByBucket = attacks.GroupBy(a => Bucket(a.Bucket)).ToDictionary(g => g.Key, g => g.Sum(a => a.Count));
        var traffic = Enumerable.Range(0, bucketCount).Select(i => new TrafficBucket(Utc(start.AddMinutes((long)i * minutes)),
            benignByBucket.GetValueOrDefault(i), attacksByBucket.GetValueOrDefault(i))).ToList();
        var types = attacks.GroupBy(a => DetectionLabels.Display(a.Label)).Select(g => new AttackBreakdown(g.Key, g.Sum(a => a.Count),
            Utc(g.Min(a => a.First)), Utc(g.Max(a => a.Last)),
            g.GroupBy(a => Bucket(a.Bucket)).Aggregate(new long[bucketCount], (series, values) =>
            {
                series[values.Key] = values.Sum(a => a.Count);
                return series;
            }))).OrderByDescending(x => x.Count).ThenBy(x => x.Label, StringComparer.Ordinal).ToList();

        FlowTotals? previous = null;
        if (!live && window.Compare)
        {
            var priorStart = from - (to - from);
            var priorBenign = await database.BenignTraffic.LongCountAsync(a => a.Timestamp >= priorStart && a.Timestamp < from, token);
            var priorAttack = await database.AttackTraffic.LongCountAsync(a => a.Timestamp >= priorStart && a.Timestamp < from, token);
            if (priorBenign + priorAttack > 0) previous = new(priorBenign, priorAttack);
        }

        List<Breakdown> incidents = [], incidentMetrics = [], explanations = [], sources = [], ports = [];
        if (!live)
        {
            sources = await attackQuery.Where(a => a.SrcIp.Trim() != "").GroupBy(a => a.SrcIp)
                .Select(g => new { Label = g.Key, Count = g.LongCount() })
                .OrderByDescending(g => g.Count).ThenBy(g => g.Label).Take(5)
                .Select(g => new Breakdown(g.Label, g.Count)).ToListAsync(token);
            ports = await attackQuery.Where(a => a.DstPort >= 0 && a.DstPort <= 65535).GroupBy(a => a.DstPort)
                .Select(g => new { Port = g.Key!.Value, Count = g.LongCount() })
                .OrderByDescending(g => g.Count).ThenBy(g => g.Port).Take(5)
                .Select(g => new Breakdown(g.Port.ToString(), g.Count)).ToListAsync(token);
        }
        if (global || live)
        {
            var jobs = database.FlowExplanations.Where(e => e.CreatedAt >= window.From && e.CreatedAt < to);
            explanations = await jobs.GroupBy(e => e.Status).Select(g => new Breakdown(g.Key, g.LongCount())).ToListAsync(token);
        }
        if (global && !live)
        {
            var tickets = database.SupportTickets.Where(t => t.Category == "Security Concern" && t.CreatedAt >= window.From && t.CreatedAt < to);
            var groups = await tickets.GroupBy(t => new
            {
                t.Status, Assigned = t.AssignedToUserId != null && t.AssignedToUserId != "",
                Priority = t.Priority == "High" || t.Priority == "Critical"
            }).Select(g => new { g.Key, Count = g.LongCount() }).ToListAsync(token);
            incidents = groups.GroupBy(g => g.Key.Status).Select(g => new Breakdown(g.Key, g.Sum(x => x.Count))).ToList();
            var active = groups.Where(g => g.Key.Status is "Open" or "InProgress").ToList();
            var escalated = await database.IncidentEscalations.Where(e => e.CancelledAt == null)
                .Where(e => tickets.Any(t => t.Id == e.TicketId)).Select(e => e.TicketId).Distinct().LongCountAsync(token);
            incidentMetrics =
            [
                new("open", active.Sum(g => g.Count)),
                new("high-priority", active.Where(g => g.Key.Priority).Sum(g => g.Count)),
                new("assigned", active.Where(g => g.Key.Assigned).Sum(g => g.Count)),
                new("unassigned", active.Where(g => !g.Key.Assigned).Sum(g => g.Count)),
                new("escalated", escalated),
                new("InProgress", groups.Where(g => g.Key.Status == "InProgress").Sum(g => g.Count)),
                new("Resolved", groups.Where(g => g.Key.Status == "Resolved").Sum(g => g.Count))
            ];
        }
        return new(Utc(from), Utc(to), minutes, traffic, new(benign.Values.Sum(), types.Sum(a => a.Count)),
            previous, types, incidents, incidentMetrics, explanations, sources, ports);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> AlertsAsync(CancellationToken token)
    {
        var limit = await OperationsSettings.NumberAsync(database, OperationsSettings.AlertLimitKey, 12, 0, 50, token);
        return Ok(new
        {
            enabled = limit > 0,
            alerts = await AlertRows(RecentAttacks(database.AttackTraffic).Take(limit)).ToListAsync(token)
        });
    }

    internal static IQueryable<AttackTraffic> RecentAttacks(IQueryable<AttackTraffic> attacks) =>
        attacks.AsNoTracking().OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id);

    internal static IQueryable<AlertRow> AlertRows(IQueryable<AttackTraffic> attacks) =>
        attacks.Select(a => new AlertRow(a.Id.ToString(), a.Timestamp, a.AttackType, a.Severity, a.SrcIp, a.DstPort));

    internal static DateTime Utc(DateTime date) => DateTime.SpecifyKind(date, DateTimeKind.Utc);

    internal static IQueryable<AttackTraffic> WithLabel(IQueryable<AttackTraffic> query, string? label)
        => string.IsNullOrEmpty(label) ? query : label == DetectionLabels.Unclassified
            ? query.Where(a => a.AttackType.Trim() == "" || DetectionLabels.UnclassifiedValues.Contains(a.AttackType))
            : query.Where(a => a.AttackType == label);
}
