using IDS.Core.Services;
using IDS.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IDS.Api;

public sealed partial class MonitoringController
{
    private sealed class FeedRow
    {
        public long Id { get; init; }
        public DateTime Timestamp { get; init; }
        public bool IsAttack { get; init; }
        public string Source { get; init; } = "";
        public int? Port { get; init; }
        public string? Category { get; init; }
        public string? Severity { get; init; }
        public double Confidence { get; init; }
        public string? Explanation { get; init; }
    }

    [HttpGet("feed")]
    public async Task<IActionResult> FeedAsync(string range = "15m", string? search = null, string? category = null,
        string? traffic = null, string? explanation = null, string sort = "latest", int page = 1,
        DateTimeOffset? asOf = null, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;
        var until = asOf?.UtcDateTime ?? now;
        if (until > now) until = now;
        var window = MonitoringWindow.LiveRanges.Contains(range) && until >= now.AddDays(-2)
            ? MonitoringWindow.Resolve(range, until) : null;
        if (window == null || search?.Length > 100 || category?.Length > 100 || page is < 1 or > 1000000)
            return BadRequest("Choose valid detection filters.");
        var settings = await OperationsSettings.ReadAsync(database, "Operations.", token);
        var size = settings.Number(OperationsSettings.RecentDetectionsKey, 25, 10, 100);
        var recentLimit = settings.Number(OperationsSettings.AlertLimitKey, 12, 0, 50);
        var attacks = database.AttackTraffic.AsNoTracking().Where(a => a.Timestamp >= window.From && a.Timestamp < window.To);
        var benign = database.BenignTraffic.AsNoTracking().Where(a => a.Timestamp >= window.From && a.Timestamp < window.To);
        IQueryable<FeedRow> ProjectAttacks(IQueryable<IDS.Data.Models.AttackTraffic> query) => query.Select(a => new FeedRow
        {
            Id = a.Id, Timestamp = a.Timestamp, IsAttack = true, Source = a.SrcIp, Port = a.DstPort,
            Category = a.AttackType, Severity = a.Severity, Confidence = a.ConfidenceScore,
            Explanation = database.FlowExplanations.Any(e => e.AttackTrafficId == a.Id && (e.Status == "pending" || e.Status == "processing")) ? "Pending" :
                database.FlowExplanations.Any(e => e.AttackTrafficId == a.Id && e.Status == "failed") ? "Failed" :
                database.FlowExplanations.Any(e => e.AttackTrafficId == a.Id && e.Status == "completed") ? "Available" : "No job"
        });
        var attackRows = ProjectAttacks(WithLabel(attacks, category));
        var benignRows = benign.Select(a => new FeedRow
        {
            Id = a.Id, Timestamp = a.Timestamp, IsAttack = false, Source = a.SrcIp, Port = a.DstPort,
            Category = null, Severity = null, Confidence = a.ConfidenceScore, Explanation = null
        });
        var rows = traffic == "benign" || explanation == "not-applicable" ? benignRows
            : traffic == "malicious" || !string.IsNullOrEmpty(category) || !string.IsNullOrEmpty(explanation) ? attackRows
            : attackRows.Concat(benignRows);
        if (traffic == "benign" && (!string.IsNullOrEmpty(category) ||
                (!string.IsNullOrEmpty(explanation) && explanation != "not-applicable")) ||
            traffic == "malicious" && explanation == "not-applicable")
            rows = rows.Where(a => false);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            var port = int.TryParse(search, out var parsed) ? parsed : -1;
            rows = rows.Where(a => a.Source.Contains(search) || a.Category!.Contains(search) || a.Port == port);
        }
        if (explanation is "Pending" or "Failed" or "Available" or "No job")
            rows = rows.Where(a => a.Explanation == explanation);
        var ordered = sort switch
        {
            "oldest" => rows.OrderBy(a => a.Timestamp),
            "confidence" => rows.OrderByDescending(a => a.Confidence).ThenByDescending(a => a.Timestamp),
            "source" => rows.OrderBy(a => a.Source).ThenByDescending(a => a.Timestamp),
            _ => rows.OrderByDescending(a => a.Timestamp)
        };
        var detections = await ordered.ThenByDescending(a => a.IsAttack).ThenByDescending(a => a.Id)
            .Skip((page - 1) * size).Take(size + 1).ToListAsync(token);
        var hasNext = detections.Count > size;
        if (hasNext) detections.RemoveAt(size);
        var recent = recentLimit == 0 ? new List<FeedRow>() : await ProjectAttacks(attacks)
            .OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id).Take(recentLimit).ToListAsync(token);

        var ids = detections.Concat(recent).Where(a => a.IsAttack).Select(a => a.Id).Distinct().ToArray();
        var tickets = database.SupportTickets.AsNoTracking().Where(t => t.SourceAttackId != null && ids.Contains(t.SourceAttackId.Value));
        if (!AppRoles.SupportRoles.Any(User.IsInRole))
        {
            var actor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            tickets = tickets.Where(t => t.SubmittedByUserId == actor);
        }
        var byAttack = await tickets.ToDictionaryAsync(t => t.SourceAttackId!.Value, t => t.Id, token);
        object View(FeedRow row) => new
        {
            id = row.Id.ToString(), timestamp = Utc(row.Timestamp), row.IsAttack, row.Source, row.Port,
            category = row.IsAttack ? DetectionLabels.Display(row.Category) : null, row.Severity,
            confidence = double.IsFinite(row.Confidence) && row.Confidence >= 0 && row.Confidence <= 1 ? (double?)row.Confidence : null,
            row.Explanation, ticketId = row.IsAttack && byAttack.TryGetValue(row.Id, out var ticket) ? (int?)ticket : null
        };
        return Ok(new
        {
            rows = detections.Select(View), recent = recent.Select(View), recentEnabled = recentLimit > 0,
            page, hasNext, asOf = Utc(window.To), escalation = settings.Enabled(OperationsSettings.EscalationKey)
        });

    }
}
