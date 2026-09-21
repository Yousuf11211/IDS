using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public class AlertDetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public AlertDetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public SecurityAlert Alert { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var alert = await _db.SecurityAlerts.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);
        if (alert == null)
        {
            return NotFound();
        }

        Alert = alert;
        return Page();
    }
}
