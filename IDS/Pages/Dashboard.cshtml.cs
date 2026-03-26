using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace IDS.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DashboardModel(ApplicationDbContext db)
        {
          _db = db;
        }

        public int TotalLogs { get; set; }
        public Dictionary<string, int> LevelCounts { get; set; } = new();
        public int NormalCount { get; set; }
        public int AttackCount { get; set; }
        public int ActivityTotal { get; set; }
        public int NetworkTotal { get; set; }
        public string IdsStatus { get; set; } = "Evaluating";
        public string IdsStatusClass { get; set; } = "off";

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.LogFiles.Add(new LogFile { Timestamp = DateTime.UtcNow, Level = "Information", Message = "Dashboard page visited", UserId = userId });
            await _db.SaveChangesAsync();

         var totalCount = await _db.LogFiles.CountAsync();
   var grouped = await _db.LogFiles.AsNoTracking()
            .GroupBy(l => l.Level)
         .Select(g => new { Level = g.Key, Count = g.Count() })
      .ToListAsync();
        LevelCounts = grouped.ToDictionary(x => x.Level ?? "Unknown", x => x.Count);

       var netNormal = LevelCounts.TryGetValue("NetworkNormal", out var nn) ? nn : 0;
    var netAttack = LevelCounts.TryGetValue("NetworkAttack", out var na) ? na : 0;
            
         if (na == 0)
       {
          netAttack += (LevelCounts.TryGetValue("Error", out var err) ? err : 0) + (LevelCounts.TryGetValue("Warning", out var warn) ? warn : 0);
          }
      
            NormalCount = netNormal;
            AttackCount = netAttack;
  NetworkTotal = NormalCount + AttackCount;
            ActivityTotal = Math.Max(0, totalCount - NetworkTotal);

 if (User.IsInRole(Security.AppRoles.Admin))
  {
  TotalLogs = totalCount;
            }
            else
            {
                TotalLogs = NetworkTotal;
            }

            if (AttackCount > 0 && (AttackCount > (NormalCount / 2))) { IdsStatus = "Attention"; IdsStatusClass = "warn"; }
        else if (AttackCount > 0) { IdsStatus = "Degraded"; IdsStatusClass = "warn"; }
          else { IdsStatus = "Normal"; IdsStatusClass = "on"; }
        }

        public async Task<IActionResult> OnGetLogsAsync()
        {
            if (!User.IsInRole(Security.AppRoles.Admin)) return new ForbidResult();

            var data = await _db.LogFiles.AsNoTracking().OrderByDescending(l => l.Timestamp).Take(200)
                .Select(l => new {
 l.Timestamp,
         l.Level,
            l.Message,
          l.UserId,
         Classification = l.Level == "NetworkNormal" ? "Normal" : (l.Level == "NetworkAttack" || l.Level == "Error" || l.Level == "Warning" ? "Attack" : "Activity")
                }).ToListAsync();
            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetRecentActivityAsync()
  {
          // Get recent security alerts or log entries
         var recentLogs = await _db.LogFiles.AsNoTracking()
  .Where(l => l.Level == "Error" || l.Level == "Warning" || l.Level == "NetworkAttack")
     .OrderByDescending(l => l.Timestamp)
      .Take(10)
     .Select(l => new {
        l.Timestamp,
       l.Message,
    Severity = l.Level == "Error" || l.Level == "NetworkAttack" ? "high" : "medium",
     Type = l.Level
    }).ToListAsync();

            // If user is admin, also check security alerts table
     if (User.IsInRole(Security.AppRoles.Admin))
        {
     try
        {
    var alerts = await _db.SecurityAlerts.AsNoTracking()
   .Where(a => !a.IsAcknowledged)
             .OrderByDescending(a => a.Timestamp)
              .Take(10)
           .Select(a => new {
        a.Timestamp,
               a.Message,
                 Severity = a.Severity.ToLower(),
       Type = a.AlertType
             }).ToListAsync();

 if (alerts.Any())
   {
 var combined = recentLogs.Concat(alerts)
         .OrderByDescending(x => x.Timestamp)
  .Take(10)
            .ToList();
     return new JsonResult(combined);
      }
          }
     catch
 {
        // SecurityAlerts table might not exist yet
}
            }

            return new JsonResult(recentLogs);
        }
    }
}
