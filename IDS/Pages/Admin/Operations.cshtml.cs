using System.ComponentModel.DataAnnotations;
using System.Data;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public sealed class OperationsModel(ApplicationDbContext database, UserManager<ApplicationUser> users,
    IConfiguration configuration) : PageModel
{
    [BindProperty] public OperationInput Input { get; set; } = new();
    [BindProperty] public string? Notice { get; set; }
    [BindProperty, DataType(DataType.Password)] public string? Password { get; set; }
    [TempData] public string? StatusMessage { get; set; }

    public sealed class OperationInput
    {
        public bool Assignment { get; set; } = true;
        public bool Escalation { get; set; } = true;
        public bool ShapJobs { get; set; } = true;
        [Range(0, 50)] public int RecentAlerts { get; set; } = 12;
        [Range(15, 300)] public int AnalyticsRefreshSeconds { get; set; } = 30;
        [Range(30, 3600)] public int HeartbeatTimeoutSeconds { get; set; } = 120;

        public Dictionary<string, string> Values() => new()
        {
            [OperationsSettings.AssignmentKey] = Assignment.ToString(),
            [OperationsSettings.EscalationKey] = Escalation.ToString(),
            [OperationsSettings.ShapJobsKey] = ShapJobs.ToString(),
            [OperationsSettings.AlertLimitKey] = RecentAlerts.ToString(),
            [OperationsSettings.RefreshKey] = AnalyticsRefreshSeconds.ToString(),
            [WorkerHealthService.TimeoutKey] = HeartbeatTimeoutSeconds.ToString()
        };
    }

    public async Task OnGetAsync(CancellationToken token)
    {
        Input = new()
        {
            Assignment = await OperationsSettings.IsAssignmentEnabledAsync(database),
            Escalation = await OperationsSettings.IsEscalationEnabledAsync(database),
            ShapJobs = await OperationsSettings.GeneratesShapJobsAsync(database),
            RecentAlerts = await OperationsSettings.NumberAsync(database, OperationsSettings.AlertLimitKey, 12, 0, 50, token),
            AnalyticsRefreshSeconds = await OperationsSettings.NumberAsync(database, OperationsSettings.RefreshKey, 30, 15, 300, token),
            HeartbeatTimeoutSeconds = await OperationsSettings.NumberAsync(database, WorkerHealthService.TimeoutKey,
                DefaultTimeout, 30, 3600, token)
        };
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        var admin = await users.GetUserAsync(User);
        if (admin == null) return Challenge();
        if (!ModelState.IsValid || !await VerifyAsync(admin)) return Page();

        var changes = Input.Values();
        var defaults = new OperationInput { HeartbeatTimeoutSeconds = DefaultTimeout }.Values();
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        var keys = changes.Keys.ToArray();
        var existing = await database.SystemSettings.Where(s => keys.Contains(s.Key)).ToDictionaryAsync(s => s.Key, token);
        var now = DateTime.UtcNow;
        foreach (var (key, value) in changes)
        {
            existing.TryGetValue(key, out var setting);
            var previous = setting?.Value ?? defaults[key];
            if (string.Equals(previous, value, StringComparison.OrdinalIgnoreCase)) continue;
            if (setting == null)
            {
                setting = new SystemSetting { Key = key };
                database.SystemSettings.Add(setting);
            }
            setting.Value = value;
            setting.LastModified = now;
            setting.ModifiedByUserId = admin.Id;
            database.AuditLogs.Add(new AuditLog
            {
                UserId = admin.Id, UserEmail = admin.Email ?? "", Action = "UpdateOperationsSetting",
                EntityType = nameof(SystemSetting), EntityId = key, Details = $"{previous} → {value}", Timestamp = now
            });
        }
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        StatusMessage = "Operating settings saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNoticeAsync(CancellationToken token)
    {
        var admin = await users.GetUserAsync(User);
        if (admin == null) return Challenge();
        var message = Notice?.Trim();
        if (string.IsNullOrEmpty(message) || message.Length > 2000)
            ModelState.AddModelError(nameof(Notice), "Enter a message of 1 to 2,000 characters.");
        if (!ModelState.IsValid || !await VerifyAsync(admin))
        {
            var notice = Notice;
            await OnGetAsync(token);
            Notice = notice;
            return Page();
        }

        var now = DateTime.UtcNow;
        var recipients = await database.Users.Where(u => u.EmailConfirmed && !u.MustChangePassword &&
            (u.LockoutEnd == null || u.LockoutEnd <= now) &&
            (from membership in database.UserRoles
             join role in database.Roles on membership.RoleId equals role.Id
             where membership.UserId == u.Id && role.Name == AppRoles.Suspended
             select membership).Any() == false).Select(u => u.Id).ToListAsync(token);
        database.InboxNotifications.AddRange(recipients.Select(id => new InboxNotification
        {
            RecipientId = id, SenderName = IncidentWorkflow.DisplayName(admin), Type = "System announcement",
            Message = message!, CreatedAt = now
        }));
        database.AuditLogs.Add(new AuditLog
        {
            UserId = admin.Id, UserEmail = admin.Email ?? "", Action = "PublishSystemAnnouncement",
            EntityType = nameof(InboxNotification), Details = $"Recipients: {recipients.Count}. {message}", Timestamp = now
        });
        await database.SaveChangesAsync(token);
        StatusMessage = $"Announcement sent to {recipients.Count} accounts.";
        return RedirectToPage();
    }

    private int DefaultTimeout => Math.Clamp(configuration.GetValue("Detection:WorkerHeartbeatTimeoutSeconds", 120), 30, 3600);

    private async Task<bool> VerifyAsync(ApplicationUser admin)
    {
        var password = Password;
        Password = null;
        ModelState.Remove(nameof(Password));
        if (string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(string.Empty, "Enter your current password.");
            return false;
        }
        if (await users.IsLockedOutAsync(admin)) return false;
        if (!await users.CheckPasswordAsync(admin, password))
        {
            await users.AccessFailedAsync(admin);
            ModelState.AddModelError(string.Empty, "The current password is incorrect.");
            return false;
        }
        await users.ResetAccessFailedCountAsync(admin);
        return true;
    }
}
