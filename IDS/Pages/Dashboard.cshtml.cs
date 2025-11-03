using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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

        public List<LogFile> RecentLogs { get; set; } = new();
        public int TotalLogs { get; set; }
        public Dictionary<string, int> LevelCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var log = new LogFile
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Message = "Dashboard page visited",
                Exception = null,
                UserId = userId
            };

            _db.LogFiles.Add(log);
            await _db.SaveChangesAsync();

            // Load dashboard data
            RecentLogs = await _db.LogFiles
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();

            TotalLogs = await _db.LogFiles.CountAsync();

            var counts = await _db.LogFiles
                .AsNoTracking()
                .GroupBy(l => l.Level)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            LevelCounts = counts.ToDictionary(x => x.Level ?? "Unknown", x => x.Count);
        }
    }
}
