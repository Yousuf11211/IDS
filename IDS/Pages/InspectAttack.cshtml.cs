using IDS.Core.Services;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages;

[Authorize(Roles = AppRoles.AdminOrSupport)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class InspectAttackModel(AttackInspectionService service) : PageModel
{
    public InspectionReport Report { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellationToken)
    {
        if (!AttackInspectionService.CanInspect(User)) return Forbid();
        var report = await service.LoadAsync(id, User, cancellationToken);
        if (report == null) return NotFound();
        Report = report;
        return Page();
    }
}
