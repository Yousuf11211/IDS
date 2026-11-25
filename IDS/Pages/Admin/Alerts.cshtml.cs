using System.Security.Claims;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public sealed class AlertsModel(ApplicationDbContext database, SecurityAlertService service) : PageModel
{
    private const int PageSize = 30;
    public List<SecurityAlert> Alerts { get; private set; } = [];
    public List<(string Label, long Count)> Metrics { get; private set; } = [];
    public List<MonitoringBreakdown> SeverityCounts { get; private set; } = [];
    public List<MonitoringBreakdown> DailyCounts { get; private set; } = [];
    public sealed record MonitoringBreakdown(string Label, long Count);
    public bool HasNext { get; private set; }
    public bool BackfillScheduled { get; private set; }
    [BindProperty(SupportsGet = true)] public string? Severity { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? DateRange { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public int P { get; set; } = 1;
    [TempData] public string? Result { get; set; }

    public async Task OnGetAsync(CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var start = DateRange switch
        {
            "today" => now.Date,
            "month" => now.AddDays(-30),
            "all" => DateTime.MinValue,
            _ => now.AddDays(-7)
        };
        var settings = await OperationsSettings.ReadAsync(database, SecurityAlertSettings.Prefix, token);
        var retention = settings.Number(SecurityAlertSettings.ResolvedDays, 90, 0, 3650);
        var query = SecurityAlertService.Published(database.SecurityAlerts.AsNoTracking())
            .Where(a => a.LastObservedAt >= start || (a.LastObservedAt == null && a.Timestamp >= start));
        if (retention > 0 && DateRange != "all")
        {
            var retainedFrom = now.AddDays(-retention);
            query = query.Where(a => a.ResolvedAt == null || a.ResolvedAt >= retainedFrom);
        }
        var groups = await query.GroupBy(a => new { Acknowledged = a.IsAcknowledged, Resolved = a.ResolvedAt != null, a.Severity })
            .Select(g => new { g.Key, Count = g.LongCount() }).ToListAsync(token);
        Metrics =
        [
            ("Total", groups.Sum(g => g.Count)),
            ("Active", groups.Where(g => !g.Key.Resolved).Sum(g => g.Count)),
            ("Unacknowledged", groups.Where(g => !g.Key.Resolved && !g.Key.Acknowledged).Sum(g => g.Count)),
            ("Acknowledged", groups.Where(g => g.Key.Acknowledged).Sum(g => g.Count)),
            ("Critical", groups.Where(g => g.Key.Severity == "Critical").Sum(g => g.Count)),
            ("Resolved", groups.Where(g => g.Key.Resolved).Sum(g => g.Count))
        ];
        SeverityCounts = groups.GroupBy(g => g.Key.Severity).OrderByDescending(g => g.Sum(item => item.Count))
            .Select(g => new MonitoringBreakdown(g.Key, g.Sum(item => item.Count))).ToList();
        var recentStart = start > now.AddDays(-14) ? start : now.AddDays(-14);
        var counts = await query.Where(a => a.Timestamp >= recentStart).GroupBy(a => a.Timestamp.Date)
            .Select(g => new { Day = g.Key, Count = g.LongCount() }).OrderBy(g => g.Day).ToListAsync(token);
        DailyCounts = counts.Select(day => new MonitoringBreakdown(day.Day.ToString("yyyy-MM-dd"), day.Count)).ToList();

        if (!string.IsNullOrEmpty(Severity)) query = query.Where(a => a.Severity == Severity);
        if (!string.IsNullOrEmpty(Category)) query = query.Where(a => a.EventCategory == Category);
        query = Status switch
        {
            "active" => query.Where(a => a.ResolvedAt == null),
            "unacknowledged" => query.Where(a => !a.IsAcknowledged && a.ResolvedAt == null),
            "acknowledged" => query.Where(a => a.IsAcknowledged),
            "resolved" => query.Where(a => a.ResolvedAt != null),
            _ => query
        };
        P = Math.Clamp(P, 1, 1000000);
        Alerts = await query.Include(a => a.Ticket).OrderByDescending(a => a.LastObservedAt ?? a.Timestamp)
            .ThenByDescending(a => a.Id).Skip((P - 1) * PageSize).Take(PageSize + 1).ToListAsync(token);
        HasNext = Alerts.Count > PageSize;
        if (HasNext) Alerts.RemoveAt(PageSize);
        BackfillScheduled = await database.SystemSettings.AnyAsync(s => s.Key == "AlertProcessing.BackfillFrom", token);
    }

    public async Task<IActionResult> OnPostAcknowledgeAsync(int alertId, CancellationToken token)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(database, nameof(SecurityAlertService), token);
        var alert = await SecurityAlertService.Published(database.SecurityAlerts).SingleOrDefaultAsync(a => a.Id == alertId, token);
        if (alert == null) return NotFound();
        if (!alert.IsAcknowledged && alert.ResolvedAt == null)
        {
            alert.IsAcknowledged = true;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.AcknowledgedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Audit(database, User, "AcknowledgeSecurityAlert", alert.Id.ToString());
            await database.SaveChangesAsync(token);
        }
        await transaction.CommitAsync(token);
        return RedirectToPage(new { severity = Severity, status = Status, category = Category, dateRange = DateRange, p = P });
    }

    public async Task<IActionResult> OnPostBackfillAsync(int days, CancellationToken token)
    {
        if (await service.ScheduleBackfillAsync(days, token))
        {
            Audit(database, User, "ScheduleSecurityAlertBackfill", null, $"Historical horizon in days: {days}.");
            await database.SaveChangesAsync(token);
            Result = "Historical processing scheduled.";
        }
        else Result = "Choose up to 30 days. Historical processing can be scheduled once.";
        return RedirectToPage();
    }

    internal static void Audit(ApplicationDbContext db, ClaimsPrincipal actor, string action, string? id, string? details = null)
        => db.AuditLogs.Add(new()
        {
            Action = action, EntityType = nameof(SecurityAlert), EntityId = id, Details = details,
            UserId = actor.FindFirstValue(ClaimTypes.NameIdentifier) ?? "", UserEmail = actor.Identity?.Name ?? ""
        });

    public static string State(SecurityAlert a) => a.ResolvedAt != null ? "Resolved" : a.IsAcknowledged ? "Acknowledged" : "Unacknowledged";
}
