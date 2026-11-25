using IDS.Api;
using IDS.Core.Services;
using IDS.Data;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages;

[Authorize]
public sealed class DetectionHistoryModel(ApplicationDbContext database) : PageModel
{
    private const int PageSize = 30;
    public List<MonitoringController.AlertRow> Alerts { get; private set; } = [];
    public MonitoringController.AlertRow? Selected { get; private set; }
    public int CurrentPage { get; private set; }
    public bool HasNext { get; private set; }
    public bool EscalationEnabled { get; private set; }

    public string? Category { get; private set; }
    public string? Explanation { get; private set; }
    public DateTimeOffset? From { get; private set; }
    public DateTimeOffset? To { get; private set; }

    public async Task<IActionResult> OnGetAsync(long? id, int p = 1, string? category = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, string? explanation = null, CancellationToken token = default)
    {
        if (id.HasValue)
        {
            Selected = await MonitoringController.AlertRows(database.AttackTraffic.AsNoTracking().Where(a => a.Id == id))
                .SingleOrDefaultAsync(token);
            if (Selected == null) return NotFound();
        }

        if (category?.Length > 100 || from >= to) return BadRequest();
        Category = category;
        From = from;
        To = to;
        var query = MonitoringController.WithLabel(database.AttackTraffic, category);
        if (explanation == "completed")
        {
            if (!AppRoles.SupportRoles.Any(User.IsInRole)) return Forbid();
            Explanation = explanation;
            var jobsFrom = from?.UtcDateTime ?? DateTime.MinValue;
            var jobsTo = to?.UtcDateTime ?? DateTime.MaxValue;
            query = query.Where(a => database.FlowExplanations.Any(e => e.AttackTrafficId == a.Id &&
                e.Status == "completed" && e.CreatedAt >= jobsFrom && e.CreatedAt < jobsTo));
        }
        if (from.HasValue && Explanation == null) query = query.Where(a => a.Timestamp >= from.Value.UtcDateTime);
        if (to.HasValue && Explanation == null) query = query.Where(a => a.Timestamp < to.Value.UtcDateTime);
        CurrentPage = Math.Clamp(p, 1, int.MaxValue / PageSize);
        Alerts = await MonitoringController.AlertRows(MonitoringController.RecentAttacks(query)
            .Skip((CurrentPage - 1) * PageSize).Take(PageSize + 1)).ToListAsync(token);
        HasNext = Alerts.Count > PageSize;
        if (HasNext) Alerts.RemoveAt(PageSize);
        EscalationEnabled = await OperationsSettings.IsEscalationEnabledAsync(database);
        return Page();
    }
}
