using System.ComponentModel.DataAnnotations;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public sealed class EmailDeliveryModel(ApplicationDbContext database, EmailConfiguration configuration, MailjetEmailSender smtp,
    UserManager<ApplicationUser> users, AdminVerification verification, AccountEmailQueue queue, AccountLinkBuilder links) : PageModel
{
    public sealed class SettingsInput
    {
        public bool Enabled { get; set; }
        public bool SupportCopies { get; set; }
        [Range(1, 65535)] public int Port { get; set; }
        [Required, EmailAddress, StringLength(256)] public string? SenderEmail { get; set; }
        [Required, StringLength(100)] public string? SenderName { get; set; }
        [EmailAddress, StringLength(256)] public string? AdministratorEmail { get; set; }
        [EmailAddress, StringLength(256)] public string? SupportEmail { get; set; }
        [StringLength(250), DataType(DataType.Password)] public string? ApiKey { get; set; }
        [StringLength(250), DataType(DataType.Password)] public string? Secret { get; set; }
    }

    [BindProperty] public SettingsInput Input { get; set; } = new();
    [BindProperty, StringLength(100), DataType(DataType.Password)] public string? Password { get; set; }
    [BindProperty, StringLength(16)] public string? Code { get; set; }
    [TempData] public string? StatusMessage { get; set; }
    public string Host { get; private set; } = "";
    public bool Ready { get; private set; }
    public bool TrustedOrigin { get; private set; }
    public List<DeliveryRow> Recent { get; private set; } = [];
    public sealed record DeliveryRow(long Id, string EventType, DateTime CreatedAt, DateTime? LastAttemptAt,
        DateTime? AcceptedAt, DateTime? FailedAt, string? FailureCode, int Attempts, bool CanRetry);

    public async Task OnGetAsync(CancellationToken token)
    {
        var settings = await smtp.LoadAsync(token);
        var stored = await OperationsSettings.ReadAsync(database, EmailConfiguration.Prefix, token);
        Input = new()
        {
            Enabled = settings.Enabled, Port = settings.Port, SenderEmail = settings.SenderEmail,
            SenderName = settings.SenderName, AdministratorEmail = settings.AdministratorEmail,
            SupportEmail = settings.SupportEmail, SupportCopies = stored.Enabled(EmailConfiguration.SupportCopies)
        };
        await LoadStatusAsync(settings, token);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        var actor = await users.GetUserAsync(User);
        if (actor == null) return Challenge();
        if (!ModelState.IsValid || !await verification.CheckCredentialsAsync(actor, Password, Code, requireAuthenticator: true))
        {
            ModelState.AddModelError(string.Empty, "Review your settings and verify your administrator password and authenticator.");
            ModelState.Remove("Input.ApiKey");
            ModelState.Remove("Input.Secret");
            ModelState.Remove(nameof(Password));
            ModelState.Remove(nameof(Code));
            Input.ApiKey = Input.Secret = Password = Code = null;
            await LoadStatusAsync(await smtp.LoadAsync(token), token);
            return Page();
        }
        var pair = new Dictionary<string, string>
        {
            [EmailConfiguration.Enabled] = Input.Enabled.ToString(),
            [EmailConfiguration.Port] = Input.Port.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [EmailConfiguration.SenderEmail] = Input.SenderEmail!.Trim(),
            [EmailConfiguration.SenderName] = Input.SenderName!.Trim(),
            [EmailConfiguration.SupportEmail] = Input.SupportEmail?.Trim() ?? "",
            [EmailConfiguration.AdminEmail] = Input.AdministratorEmail?.Trim() ?? "",
            [EmailConfiguration.SupportCopies] = Input.SupportCopies.ToString()
        };
        if (!string.IsNullOrWhiteSpace(Input.ApiKey)) pair[EmailConfiguration.ApiKey] = configuration.Protect(Input.ApiKey.Trim());
        if (!string.IsNullOrWhiteSpace(Input.Secret)) pair[EmailConfiguration.Secret] = configuration.Protect(Input.Secret.Trim());
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(database, "IDS.AdminSettings", token);
        var existing = await database.SystemSettings.Where(s => s.Key.StartsWith(EmailConfiguration.Prefix))
            .ToDictionaryAsync(s => s.Key, token);
        foreach (var (key, value) in pair)
        {
            if (!existing.TryGetValue(key, out var setting))
            {
                setting = new SystemSetting { Key = key };
                database.SystemSettings.Add(setting);
            }
            if (setting.Value == value) continue;
            setting.Value = value;
            setting.LastModified = DateTime.UtcNow;
            setting.ModifiedByUserId = actor.Id;
            AccountRecovery.Audit(database, actor, "Security.EmailSettingChanged", key);
        }
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        StatusMessage = "Email settings saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestAsync(CancellationToken token)
    {
        var actor = await users.GetUserAsync(User);
        if (actor == null) return Challenge();
        var settings = await smtp.LoadAsync(token);
        if (!settings.Ready || !actor.EmailConfirmed || actor.Email == null)
        {
            StatusMessage = "Email must be configured and enabled, and your address must be verified.";
            return RedirectToPage();
        }
        if (!await verification.CheckCredentialsAsync(actor, Password, Code, requireAuthenticator: true))
        {
            StatusMessage = "Administrator password or authenticator verification failed.";
            return RedirectToPage();
        }
        var now = DateTime.UtcNow;
        queue.Enqueue("Mailjet test", Guid.NewGuid().ToString("N"), new(actor.Email, IncidentWorkflow.DisplayName(actor),
            "IDS Security email test", "This message was requested from the Email Delivery page.", now), now.AddHours(1));
        AccountRecovery.Audit(database, actor, "Security.EmailTestQueued");
        await database.SaveChangesAsync(token);
        StatusMessage = "Test email queued for your verified address. Check the submission status below.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRetryAsync(long id, CancellationToken token)
    {
        var actor = await users.GetUserAsync(User);
        if (actor == null) return Challenge();
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        if (await queue.RetryAsync(id, token))
        {
            AccountRecovery.Audit(database, actor, "Security.EmailRetryQueued", id.ToString());
            await database.SaveChangesAsync(token);
            StatusMessage = "Email queued for retry.";
        }
        else StatusMessage = "The email is no longer eligible for retry.";
        await transaction.CommitAsync(token);
        return RedirectToPage();
    }

    private async Task LoadStatusAsync(MailjetSettings settings, CancellationToken token)
    {
        Ready = settings.Ready;
        Host = settings.Host;
        TrustedOrigin = links.IsConfigured;
        var now = DateTime.UtcNow;
        Recent = await database.SecurityEmails.AsNoTracking().OrderByDescending(e => e.Id).Take(30)
            .Select(e => new DeliveryRow(e.Id, e.EventType, e.CreatedAt, e.LastAttemptAt, e.AcceptedAt,
                e.FailedAt, e.FailureCode, e.Attempts, e.FailedAt != null && e.ProtectedPayload != null &&
                (e.ExpiresAt == null || e.ExpiresAt > now))).ToListAsync(token);
    }
}
