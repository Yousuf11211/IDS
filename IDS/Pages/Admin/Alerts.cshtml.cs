using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using System.Security.Claims;

namespace IDS.Pages.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AlertsModel : PageModel
{
        private readonly ApplicationDbContext _db;

        public AlertsModel(ApplicationDbContext db)
        {
   _db = db;
        }

  public List<SecurityAlert> Alerts { get; set; } = new();
   public int CriticalCount { get; set; }
   public int HighCount { get; set; }
        public int OtherCount { get; set; }
        public int AcknowledgedCount { get; set; }
        
        public string? SeverityFilter { get; set; }
        public string? StatusFilter { get; set; }
 public string? DateRange { get; set; }

    public async Task OnGetAsync(string? severity, string? status, string? dateRange)
        {
            SeverityFilter = severity;
   StatusFilter = status;
     DateRange = dateRange;

         // Get counts
  CriticalCount = await _db.SecurityAlerts.CountAsync(a => a.Severity == "Critical" && !a.IsAcknowledged);
            HighCount = await _db.SecurityAlerts.CountAsync(a => a.Severity == "High" && !a.IsAcknowledged);
       OtherCount = await _db.SecurityAlerts.CountAsync(a => 
  (a.Severity == "Medium" || a.Severity == "Low" || a.Severity == "Info") && !a.IsAcknowledged);
      AcknowledgedCount = await _db.SecurityAlerts.CountAsync(a => a.IsAcknowledged);

        // Build query
            var query = _db.SecurityAlerts.AsNoTracking();

   // Apply filters
     if (!string.IsNullOrEmpty(severity))
         {
       query = query.Where(a => a.Severity == severity);
 }

            if (!string.IsNullOrEmpty(status))
{
      var isAcknowledged = status == "acknowledged";
    query = query.Where(a => a.IsAcknowledged == isAcknowledged);
    }

   if (!string.IsNullOrEmpty(dateRange) && dateRange != "all")
            {
   var startDate = dateRange switch
    {
         "today" => DateTime.UtcNow.Date,
  "week" => DateTime.UtcNow.AddDays(-7),
        "month" => DateTime.UtcNow.AddDays(-30),
       _ => DateTime.MinValue
   };
      query = query.Where(a => a.Timestamp >= startDate);
  }

            Alerts = await query
       .OrderByDescending(a => a.Severity == "Critical" ? 0 : a.Severity == "High" ? 1 : 2)
        .ThenByDescending(a => a.Timestamp)
       .Take(200)
         .ToListAsync();
        }

        public async Task<IActionResult> OnPostAcknowledgeAsync(int alertId)
   {
     var alert = await _db.SecurityAlerts.FindAsync(alertId);
            if (alert != null)
     {
    alert.IsAcknowledged = true;
    alert.AcknowledgedAt = DateTime.UtcNow;
     alert.AcknowledgedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
       await _db.SaveChangesAsync();
    }
   return RedirectToPage();
        }
    }
}
