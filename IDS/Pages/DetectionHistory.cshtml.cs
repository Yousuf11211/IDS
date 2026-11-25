using IDS.Api;
using IDS.Core.Services;
using IDS.Data;
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

    public async Task<IActionResult> OnGetAsync(long? id, int p = 1, CancellationToken token = default)
    {
        if (id.HasValue)
        {
            Selected = await MonitoringController.AlertRows(database.AttackTraffic.AsNoTracking().Where(a => a.Id == id))
                .SingleOrDefaultAsync(token);
            if (Selected == null) return NotFound();
        }

        CurrentPage = Math.Clamp(p, 1, int.MaxValue / PageSize);
        Alerts = await MonitoringController.AlertRows(MonitoringController.RecentAttacks(database.AttackTraffic)
            .Skip((CurrentPage - 1) * PageSize).Take(PageSize + 1)).ToListAsync(token);
        HasNext = Alerts.Count > PageSize;
        if (HasNext) Alerts.RemoveAt(PageSize);
        EscalationEnabled = await OperationsSettings.IsEscalationEnabledAsync(database);
        return Page();
    }
}
