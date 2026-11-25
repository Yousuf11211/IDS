using System.Security.Claims;
using IDS.Core.Services;
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
                        : log.Level == "NetworkAttack"
                            ? "Attack" : "Activity"
                })
                .ToListAsync(HttpContext.RequestAborted);

            return new JsonResult(data.Select(log => new
            {
                Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc),
                log.Level, log.Message, log.UserId, log.Classification
            }));
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

                var alertLimit = await OperationsSettings.NumberAsync(_db, OperationsSettings.DashboardAlertLimitKey,
                    10, 0, 20, cancellationToken);
                var alerts = await SecurityAlertService.Published(_db.SecurityAlerts.AsNoTracking())
                    .Where(alert => !alert.IsAcknowledged && alert.ResolvedAt == null)
                    .OrderByDescending(alert => alert.LastObservedAt ?? alert.Timestamp)
                    .Take(alertLimit)
                    .Select(alert => new DashboardActivity
                    {
                        Timestamp = alert.LastObservedAt ?? alert.Timestamp,
                        Message = alert.Message,
                        Severity = alert.Severity.ToLower(),
                        Type = alert.AlertType
                    })
                    .ToListAsync(cancellationToken);
                activity.AddRange(alerts);
            }

            var recent = activity.OrderByDescending(item => item.Timestamp).Take(activityLimit).ToList();
            foreach (var item in recent) item.Timestamp = DateTime.SpecifyKind(item.Timestamp, DateTimeKind.Utc);
            return new JsonResult(recent);
        }

        public class DashboardActivity
        {
            public DateTime Timestamp { get; set; }
            public string Message { get; init; } = string.Empty;
            public string Severity { get; init; } = "low";
            public string Type { get; init; } = string.Empty;
        }
    }
}
