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
        public string IdsStatus { get; set; } = "Evaluating";
        public string IdsStatusClass { get; set; } = "off";

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.LogFiles.Add(new LogFile { Timestamp = DateTime.UtcNow, Level = "Information", Message = "Dashboard page visited", UserId = userId });
            await _db.SaveChangesAsync();

            TotalLogs = await _db.LogFiles.CountAsync();
            var counts = await _db.LogFiles.AsNoTracking().GroupBy(l => l.Level).Select(g => new { Level = g.Key, Count = g.Count() }).ToListAsync();
            LevelCounts = counts.ToDictionary(x => x.Level ?? "Unknown", x => x.Count);

            // Derive traffic classification (placeholder logic):
            NormalCount = LevelCounts.TryGetValue("Information", out var info) ? info :0;
            AttackCount = (LevelCounts.TryGetValue("Error", out var err) ? err :0) + (LevelCounts.TryGetValue("Warning", out var warn) ? warn :0);

            if (AttackCount >0 && (AttackCount > NormalCount /2)) { IdsStatus = "Attention"; IdsStatusClass = "warn"; }
            else if (AttackCount >0) { IdsStatus = "Degraded"; IdsStatusClass = "warn"; }
            else { IdsStatus = "Normal"; IdsStatusClass = "on"; }
        }

        [Authorize(Roles = Security.AppRoles.Admin)]
        public async Task<JsonResult> OnGetLogsAsync()
        {
            var data = await _db.LogFiles.AsNoTracking().OrderByDescending(l => l.Timestamp).Take(200)
                .Select(l => new {
                    l.Timestamp,
                    l.Level,
                    l.Message,
                    l.UserId,
                    Classification = l.Level == "Information" ? "Normal" : (l.Level == "Error" || l.Level == "Warning" ? "Attack" : "Other")
                }).ToListAsync();
            return new JsonResult(data);
        }
    }
}
