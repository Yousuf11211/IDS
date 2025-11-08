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

        public int TotalLogs { get; set; } // network total for non-admin; activity+network for admin (we also expose separate totals below)
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

            var netNormal = LevelCounts.TryGetValue("NetworkNormal", out var nn) ? nn :0;
            var netAttack = LevelCounts.TryGetValue("NetworkAttack", out var na) ? na :0;
            // Fallback: include traditional error/warning as attacks if specific network levels are not present
            if (na ==0)
            {
                netAttack += (LevelCounts.TryGetValue("Error", out var err) ? err :0) + (LevelCounts.TryGetValue("Warning", out var warn) ? warn :0);
            }
            // Do not infer normal traffic from generic Information unless ML has produced NetworkNormal entries
            NormalCount = netNormal;
            AttackCount = netAttack;
            NetworkTotal = NormalCount + AttackCount;
            ActivityTotal = Math.Max(0, totalCount - NetworkTotal);

            // Expose TotalLogs for view convenience
            if (User.IsInRole(Security.AppRoles.Admin))
            {
                TotalLogs = totalCount; // admins: total activity logs across system
            }
            else
            {
                TotalLogs = NetworkTotal; // normal users: only network logs
            }

            if (AttackCount >0 && (AttackCount > (NormalCount /2))) { IdsStatus = "Attention"; IdsStatusClass = "warn"; }
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
                    Classification = l.Level == "NetworkNormal" ? "Normal" : (l.Level == "NetworkAttack" || l.Level == "Error" || l.Level == "Warning" ? "Attack" : "Activity")
                }).ToListAsync();
            return new JsonResult(data);
        }
    }
}
