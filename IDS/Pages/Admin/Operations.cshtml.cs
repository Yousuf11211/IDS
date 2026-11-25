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
    [BindProperty, StringLength(16)] public string? Code { get; set; }
    [TempData] public string? StatusMessage { get; set; }

    public sealed class OperationInput
    {
        public bool Assignment { get; set; } = true;
        public bool Escalation { get; set; } = true;
        public bool ShapJobs { get; set; } = true;
        [Range(0, 50)] public int RecentAlerts { get; set; } = 12;
        [Range(15, 300)] public int AnalyticsRefreshSeconds { get; set; } = 30;
        [Range(30, 3600)] public int HeartbeatTimeoutSeconds { get; set; } = 120;

        public bool DashboardRefresh { get; set; } = true;
        [Range(5, 300)] public int LiveRefreshSeconds { get; set; } = 10;
        [Required] public string DefaultTrafficSeries { get; set; } = "split";
        [Range(0, 20)] public int DashboardAlertLimit { get; set; } = 10;
        [Range(10, 100)] public int RecentDetections { get; set; } = 25;
        [Required] public string DashboardRange { get; set; } = "24h";
        [Required] public string LiveRange { get; set; } = "15m";
        public bool AlertsEnabled { get; set; } = true;
        public bool WorkerAlerts { get; set; }
        public bool AlertNotifications { get; set; }
        public bool AccountAlerts { get; set; }
        [Range(1, 10000)] public int AlertThreshold { get; set; } = 5;
        [Range(1, 120)] public int AlertWindow { get; set; } = 10;
        [Required] public string AlertSeverity { get; set; } = "Medium";
        [StringLength(3500)] public string? AlertRules { get; set; }
        [Range(0, 3650)] public int ResolvedAlertDays { get; set; } = 90;
        public bool PasswordRecovery { get; set; } = true;
        public bool AuthenticatorRecovery { get; set; } = true;
        [Range(5, 1440)] public int PasswordMinutes { get; set; } = 30;
        [Range(1, 168)] public int RequestHours { get; set; } = 48;
        [Range(1, 48)] public int ApprovalHours { get; set; } = 8;
        [Range(1, 20)] public int RequestLimit { get; set; } = 4;
        [Range(3, 30)] public int AttemptLimit { get; set; } = 10;

        public Dictionary<string, string> Values() => new()
        {
            [OperationsSettings.AssignmentKey] = Assignment.ToString(),
            [OperationsSettings.EscalationKey] = Escalation.ToString(),
            [OperationsSettings.ShapJobsKey] = ShapJobs.ToString(),
            [OperationsSettings.AlertLimitKey] = RecentAlerts.ToString(),
            [OperationsSettings.RefreshKey] = AnalyticsRefreshSeconds.ToString(),
            [WorkerHealthService.TimeoutKey] = HeartbeatTimeoutSeconds.ToString(),
            [OperationsSettings.DashboardAutoRefreshKey] = DashboardRefresh.ToString(),
            [OperationsSettings.LiveRefreshKey] = LiveRefreshSeconds.ToString(),
            [OperationsSettings.TrafficSeriesKey] = DefaultTrafficSeries,
            [OperationsSettings.DashboardAlertLimitKey] = DashboardAlertLimit.ToString(),
            [OperationsSettings.RecentDetectionsKey] = RecentDetections.ToString(),
            [OperationsSettings.DashboardRangeKey] = DashboardRange,
            [OperationsSettings.LiveRangeKey] = LiveRange,
            [SecurityAlertSettings.Enabled] = AlertsEnabled.ToString(),
            [SecurityAlertSettings.WorkerFailures] = WorkerAlerts.ToString(),
            [SecurityAlertSettings.Notify] = AlertNotifications.ToString(),
            [SecurityAlertSettings.AccountEvents] = AccountAlerts.ToString(),
            [SecurityAlertSettings.Threshold] = AlertThreshold.ToString(),
            [SecurityAlertSettings.Window] = AlertWindow.ToString(),
            [SecurityAlertSettings.Severity] = AlertSeverity,
            [SecurityAlertSettings.Rules] = AlertRules?.Trim() ?? "",
            [SecurityAlertSettings.ResolvedDays] = ResolvedAlertDays.ToString(),
            [RecoverySettings.PasswordEnabled] = PasswordRecovery.ToString(),
            [RecoverySettings.AuthenticatorEnabled] = AuthenticatorRecovery.ToString(),
            [RecoverySettings.PasswordMinutes] = PasswordMinutes.ToString(),
            [RecoverySettings.RequestHours] = RequestHours.ToString(),
            [RecoverySettings.ApprovalHours] = ApprovalHours.ToString(),
            [RecoverySettings.RequestsPerHour] = RequestLimit.ToString(),
            [RecoverySettings.AttemptsPerHour] = AttemptLimit.ToString()
        };
    }

    public async Task OnGetAsync(CancellationToken token)
    {
        var options = await OperationsSettings.ReadAsync(database, "Operations.", token);
        var alerts = await OperationsSettings.ReadAsync(database, SecurityAlertSettings.Prefix, token);
        var recovery = await OperationsSettings.ReadAsync(database, RecoverySettings.Prefix, token);
        Input = new()
        {
            Assignment = options.Enabled(OperationsSettings.AssignmentKey),
            Escalation = options.Enabled(OperationsSettings.EscalationKey),
            ShapJobs = options.Enabled(OperationsSettings.ShapJobsKey),
            RecentAlerts = options.Number(OperationsSettings.AlertLimitKey, 12, 0, 50),
            AnalyticsRefreshSeconds = options.Number(OperationsSettings.RefreshKey, 30, 15, 300),
            HeartbeatTimeoutSeconds = options.Number(WorkerHealthService.TimeoutKey, DefaultTimeout, 30, 3600),
            DashboardRefresh = options.Enabled(OperationsSettings.DashboardAutoRefreshKey),
            LiveRefreshSeconds = options.Number(OperationsSettings.LiveRefreshKey, 10, 5, 300),
            DefaultTrafficSeries = options.GetValueOrDefault(OperationsSettings.TrafficSeriesKey) ?? "split",
            DashboardAlertLimit = options.Number(OperationsSettings.DashboardAlertLimitKey, 10, 0, 20),
            RecentDetections = options.Number(OperationsSettings.RecentDetectionsKey, 25, 10, 100),
            DashboardRange = options.GetValueOrDefault(OperationsSettings.DashboardRangeKey) ?? "24h",
            LiveRange = options.GetValueOrDefault(OperationsSettings.LiveRangeKey) ?? "15m",
            AlertsEnabled = alerts.Enabled(SecurityAlertSettings.Enabled),
            WorkerAlerts = alerts.Enabled(SecurityAlertSettings.WorkerFailures, false),
            AlertNotifications = alerts.Enabled(SecurityAlertSettings.Notify, false),
            AccountAlerts = alerts.Enabled(SecurityAlertSettings.AccountEvents, false),
            AlertThreshold = alerts.Number(SecurityAlertSettings.Threshold, 5, 1, 10000),
            AlertWindow = alerts.Number(SecurityAlertSettings.Window, 10, 1, 120),
            AlertSeverity = alerts.GetValueOrDefault(SecurityAlertSettings.Severity) ?? "Medium",
            AlertRules = alerts.GetValueOrDefault(SecurityAlertSettings.Rules),
            ResolvedAlertDays = alerts.Number(SecurityAlertSettings.ResolvedDays, 90, 0, 3650),
            PasswordRecovery = recovery.Enabled(RecoverySettings.PasswordEnabled),
            AuthenticatorRecovery = recovery.Enabled(RecoverySettings.AuthenticatorEnabled),
            PasswordMinutes = recovery.Number(RecoverySettings.PasswordMinutes, 30, 5, 1440),
            RequestHours = recovery.Number(RecoverySettings.RequestHours, 48, 1, 168),
            ApprovalHours = recovery.Number(RecoverySettings.ApprovalHours, 8, 1, 48),
            RequestLimit = recovery.Number(RecoverySettings.RequestsPerHour, 4, 1, 20),
            AttemptLimit = recovery.Number(RecoverySettings.AttemptsPerHour, 10, 3, 30)
        };
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        var admin = await users.GetUserAsync(User);
        if (admin == null) return Challenge();
        if (!MonitoringWindow.DashboardRanges.Contains(Input.DashboardRange) || Input.DashboardRange == "custom" ||
            !MonitoringWindow.LiveRanges.Contains(Input.LiveRange))
            ModelState.AddModelError(string.Empty, "Choose a listed default time window.");
        if (Input.DefaultTrafficSeries is not ("total" or "split"))
            ModelState.AddModelError(string.Empty, "Choose a supported traffic visualization.");
        if (!SecurityAlertSettings.Severities.Contains(Input.AlertSeverity) || SecurityAlertSettings.ParseRules(Input.AlertRules) == null)
            ModelState.AddModelError(string.Empty, "Enter valid alert severity and category rules.");
        if (!ModelState.IsValid || !await VerifyAsync(admin)) return Page();

        var changes = Input.Values();
        var defaults = new OperationInput { HeartbeatTimeoutSeconds = DefaultTimeout }.Values();
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        var keys = changes.Keys.ToArray();
        var existing = await database.SystemSettings.Where(s => keys.Contains(s.Key)).ToDictionaryAsync(s => s.Key, token);
        var sensitive = changes.Any(pair => (pair.Key.StartsWith(SecurityAlertSettings.Prefix) ||
            pair.Key.StartsWith(RecoverySettings.Prefix)) && !string.Equals(pair.Value,
            existing.GetValueOrDefault(pair.Key)?.Value ?? defaults[pair.Key], StringComparison.OrdinalIgnoreCase));
        var code = Code;
        Code = null;
        ModelState.Remove(nameof(Code));
        if (sensitive && admin.TwoFactorEnabled && !await AccountRecovery.VerifyFactorAsync(users, admin, code, false))
        {
            ModelState.AddModelError(string.Empty, "Verify your authenticator to change alert or account recovery settings.");
            return Page();
        }
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
                EntityType = nameof(SystemSetting), EntityId = key, Details = key == SecurityAlertSettings.Rules ? "Updated attack category alert rules." : $"{previous} → {value}", Timestamp = now
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
