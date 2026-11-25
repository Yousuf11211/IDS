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
public sealed class MonitoringController(ApplicationDbContext database, IMemoryCache cache) : ControllerBase
{
    public sealed record AlertRow(string Id, DateTime Timestamp, string Type, string Severity, string Source, int? Port);
    public sealed record TrafficBucket(DateTime Start, long Benign, long Malicious);
    public sealed record Breakdown(string Label, long Count);
    public sealed record Analytics(List<TrafficBucket> Traffic, List<Breakdown> Types,
        List<Breakdown> Categories, List<Breakdown> Incidents, List<Breakdown> Explanations);
    private sealed record BucketCount(int Bucket, long Count);

    [HttpGet("analytics")]
    public async Task<IActionResult> AnalyticsAsync(string range = "24h", CancellationToken token = default)
    {
        var window = range switch { "1h" => 60, "24h" => 1440, "7d" => 10080, "30d" => 43200, _ => 0 };
        if (window == 0) return BadRequest("Choose a supported time range.");
        var bucketMinutes = range switch { "1h" => 5, "24h" => 60, "7d" => 360, _ => 1440 };
        var global = AppRoles.SupportRoles.Any(User.IsInRole);
        var refresh = await OperationsSettings.NumberAsync(database, OperationsSettings.RefreshKey, 30, 15, 300, token);
        var snapshot = await cache.GetOrCreateAsync(("ids-analytics", range, global), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-window);
            var bucketTicks = TimeSpan.FromMinutes(bucketMinutes).Ticks;
            var bucketStart = new DateTime(cutoff.Ticks - cutoff.Ticks % bucketTicks, DateTimeKind.Utc);
            var benign = await CountBuckets(database.BenignTraffic, cutoff, now, bucketStart, bucketMinutes, token);
            var attackQuery = database.AttackTraffic.Where(a => a.Timestamp >= cutoff && a.Timestamp <= now);
            var attacks = await CountBuckets(database.AttackTraffic, cutoff, now, bucketStart, bucketMinutes, token);
            var traffic = Enumerable.Range(0, window / bucketMinutes + 1)
                .Select(i => new TrafficBucket(bucketStart.AddMinutes(i * bucketMinutes), benign.GetValueOrDefault(i),
                    attacks.GetValueOrDefault(i))).ToList();
            var types = await attackQuery.GroupBy(a => a.AttackType)
                .Select(g => new { Label = g.Key, Count = g.LongCount() }).OrderByDescending(g => g.Count)
                .ThenBy(g => g.Label).Take(10).Select(g => new Breakdown(g.Label, g.Count)).ToListAsync(token);
            var categories = await attackQuery.GroupBy(a => a.AttackCategory ?? "Uncategorized")
                .Select(g => new { Label = g.Key, Count = g.LongCount() }).OrderByDescending(g => g.Count)
                .ThenBy(g => g.Label).Select(g => new Breakdown(g.Label, g.Count)).ToListAsync(token);

            List<Breakdown> incidents = [];
            List<Breakdown> explanations = [];
            if (global)
            {
                incidents = await database.SupportTickets.Where(t => t.Category == "Security Concern" && t.CreatedAt >= cutoff && t.CreatedAt <= now)
                    .GroupBy(t => t.Status).Select(g => new Breakdown(g.Key, g.LongCount())).ToListAsync(token);
                explanations = await database.FlowExplanations.Where(e => e.CreatedAt >= cutoff && e.CreatedAt <= now)
                    .GroupBy(e => e.Status).Select(g => new Breakdown(g.Key, g.LongCount())).ToListAsync(token);
            }
            return new Analytics(traffic, types, categories, incidents, explanations);
        });
        return Ok(new { snapshot, refreshSeconds = refresh });
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

    private static async Task<Dictionary<int, long>> CountBuckets<T>(IQueryable<T> source,
        DateTime cutoff, DateTime now, DateTime start, int bucketMinutes, CancellationToken token)
        where T : NetworkFlowBase =>
        await source.Where(t => t.Timestamp >= cutoff && t.Timestamp <= now)
            .GroupBy(t => EF.Functions.DateDiffMinute(start, t.Timestamp) / bucketMinutes)
            .Select(g => new BucketCount(g.Key, g.LongCount())).ToDictionaryAsync(g => g.Bucket, g => g.Count, token);
}
