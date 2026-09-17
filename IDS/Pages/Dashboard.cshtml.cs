using System.Security.Claims;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public long TotalLogs { get; private set; }
        public long NormalCount { get; private set; }
        public long AttackCount { get; private set; }
        public long NetworkTotal => NormalCount + AttackCount;
        public Dictionary<string, long> LevelCounts { get; private set; } = new();
        public string IdsStatus { get; private set; } = "Evaluating";
        public string IdsStatusClass { get; private set; } = "off";

        public async Task OnGetAsync()
        {
            var cancellationToken = HttpContext.RequestAborted;

            // The detection pipeline writes classified traffic to these tables.
            // Application logs and raw packets must not inflate the event totals.
            NormalCount = await _db.BenignTraffic.LongCountAsync(cancellationToken);
            AttackCount = await _db.AttackTraffic.LongCountAsync(cancellationToken);
            LevelCounts = new Dictionary<string, long>
            {
                ["Normal Traffic"] = NormalCount,
                ["Attacks Detected"] = AttackCount
            };

            _db.LogFiles.Add(new LogFile
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Message = "Dashboard page visited",
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            await _db.SaveChangesAsync(cancellationToken);

            // Admins have a separate activity-log card; employees see traffic totals.
            TotalLogs = User.IsInRole(AppRoles.Admin)
                ? await _db.LogFiles.LongCountAsync(cancellationToken)
                : NetworkTotal;

            if (AttackCount > 0)
            {
                IdsStatus = AttackCount > NormalCount / 2.0 ? "Attention" : "Degraded";
                IdsStatusClass = "warn";
            }
            else
            {
                IdsStatus = "Normal";
                IdsStatusClass = "on";
            }
        }

        public async Task<IActionResult> OnGetLogsAsync()
        {
            if (!User.IsInRole(AppRoles.Admin))
            {
                return new ForbidResult();
            }

            var data = await _db.LogFiles.AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .Take(200)
                .Select(log => new
                {
                    log.Timestamp,
                    log.Level,
                    log.Message,
                    log.UserId,
                    Classification = log.Level == "NetworkNormal" ? "Normal"
                        : log.Level == "NetworkAttack" || log.Level == "Error" || log.Level == "Warning"
                            ? "Attack" : "Activity"
                })
                .ToListAsync(HttpContext.RequestAborted);

            return new JsonResult(data);
        }

        public async Task<JsonResult> OnGetRecentActivityAsync()
        {
            const int activityLimit = 10;
            var cancellationToken = HttpContext.RequestAborted;

            // Read each source before merging so the newest ten events are shown,
            // even when all recent traffic belongs to the same classification.
            var activity = await _db.AttackTraffic.AsNoTracking()
                .OrderByDescending(traffic => traffic.Timestamp)
                .ThenByDescending(traffic => traffic.Id)
                .Take(activityLimit)
                .Select(traffic => new DashboardActivity
                {
                    Timestamp = traffic.Timestamp,
                    Message = "Detected " + traffic.AttackType + " traffic",
                    Severity = traffic.Severity.ToLower(),
                    Type = traffic.AttackType
                })
                .ToListAsync(cancellationToken);

            var normalTraffic = await _db.BenignTraffic.AsNoTracking()
                .OrderByDescending(traffic => traffic.Timestamp)
                .ThenByDescending(traffic => traffic.Id)
                .Take(activityLimit)
                .Select(traffic => new DashboardActivity
                {
                    Timestamp = traffic.Timestamp,
                    Message = "Normal network traffic detected",
                    Severity = "low",
                    Type = "Normal Traffic"
                })
                .ToListAsync(cancellationToken);
            activity.AddRange(normalTraffic);

            // Administrative log messages stay in the admin dashboard.
            if (User.IsInRole(AppRoles.Admin))
            {
                var logs = await _db.LogFiles.AsNoTracking()
                    .Where(log => log.Level == "Error" || log.Level == "Warning" || log.Level == "NetworkAttack")
                    .OrderByDescending(log => log.Timestamp)
                    .Take(activityLimit)
                    .Select(log => new DashboardActivity
                    {
                        Timestamp = log.Timestamp,
                        Message = log.Message,
                        Severity = log.Level == "Warning" ? "medium" : "high",
                        Type = log.Level
                    })
                    .ToListAsync(cancellationToken);
                activity.AddRange(logs);

                var alerts = await _db.SecurityAlerts.AsNoTracking()
                    .Where(alert => !alert.IsAcknowledged)
                    .OrderByDescending(alert => alert.Timestamp)
                    .Take(activityLimit)
                    .Select(alert => new DashboardActivity
                    {
                        Timestamp = alert.Timestamp,
                        Message = alert.Message,
                        Severity = alert.Severity.ToLower(),
                        Type = alert.AlertType
                    })
                    .ToListAsync(cancellationToken);
                activity.AddRange(alerts);
            }

            return new JsonResult(activity
                .OrderByDescending(item => item.Timestamp)
                .Take(activityLimit)
                .ToList());
        }

        public class DashboardActivity
        {
            public DateTime Timestamp { get; init; }
            public string Message { get; init; } = string.Empty;
            public string Severity { get; init; } = "low";
            public string Type { get; init; } = string.Empty;
        }
    }
}
