using System.Security.Cryptography;
using System.Text.Json;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IDS.Security;

public static class RecoverySettings
{
    public const string Prefix = "Recovery.";
    public const string PasswordEnabled = Prefix + "PasswordEnabled";
    public const string AuthenticatorEnabled = Prefix + "AuthenticatorEnabled";
    public const string PasswordMinutes = Prefix + "PasswordMinutes";
    public const string RequestHours = Prefix + "RequestHours";
    public const string ApprovalHours = Prefix + "ApprovalHours";
    public const string RequestsPerHour = Prefix + "RequestsPerHour";
    public const string AttemptsPerHour = Prefix + "AttemptsPerHour";
}

public sealed record RecoveryLink(string UserId, string Stamp, string Email, string? IdentityToken = null,
    long? RequestId = null, string? Candidate = null);

public sealed class AccountRecovery(ApplicationDbContext db, UserManager<ApplicationUser> users,
    IDataProtectionProvider protector, AccountEmailQueue queue, MailjetEmailSender smtp, AccountLinkBuilder links, SecurityAlertService alerts)
{
    public const string SelfPassword = "self-password";
    public const string AdminPassword = "admin-password";
    public const string VerifyEmail = "verify-authenticator-email";
    public const string ApproveEnrollment = "replace-authenticator";
    public const string Candidate = "replacement-key";
    internal const string RequestLock = "IDS.AuthenticatorRecovery";
    private const string PasswordLock = "IDS.PasswordReset";
    private static readonly TimeSpan EmailProofLifetime = TimeSpan.FromMinutes(30);

    public string Issue(string purpose, RecoveryLink link, TimeSpan duration) =>
        Protector(purpose).Protect(JsonSerializer.Serialize(link), duration);

    public RecoveryLink? Read(string purpose, string? code)
    {
        if (string.IsNullOrEmpty(code) || code.Length > 4096) return null;
        try { return JsonSerializer.Deserialize<RecoveryLink>(Protector(purpose).Unprotect(code)); }
        catch (Exception error) when (error is CryptographicException or JsonException or FormatException) { return null; }
    }

    public static RecoveryLink For(ApplicationUser user, string? token = null, long? requestId = null) =>
        new(user.Id, user.SecurityStamp ?? "", user.Email ?? "", token, requestId);

    public static bool Matches(RecoveryLink link, ApplicationUser user) =>
        link.UserId == user.Id && link.Stamp == user.SecurityStamp && user.EmailConfirmed &&
        string.Equals(link.Email, user.Email, StringComparison.OrdinalIgnoreCase);

    private ITimeLimitedDataProtector Protector(string purpose) =>
        protector.CreateProtector("IDS.AccountRecovery.v1", purpose).ToTimeLimitedDataProtector();

    public Task<Dictionary<string, string>> SettingsAsync(CancellationToken token) =>
        OperationsSettings.ReadAsync(db, RecoverySettings.Prefix, token);

    public async Task<bool> AvailableAsync(string setting, CancellationToken token)
    {
        var settings = await SettingsAsync(token);
        return settings.Enabled(setting) && links.IsConfigured && (await smtp.LoadAsync(token)).Ready;
    }

    public async Task<bool> AllowAsync(HttpContext context, string operation, string identifier, bool request,
        CancellationToken token)
    {
        var settings = await SettingsAsync(token);
        var limit = request ? settings.Number(RecoverySettings.RequestsPerHour, 4, 1, 20)
            : settings.Number(RecoverySettings.AttemptsPerHour, 10, 3, 30);
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "local";
        var keys = new[]
        {
            operation + ":i:" + AccountEmailQueue.Fingerprint(ip),
            operation + ":a:" + AccountEmailQueue.Fingerprint(identifier.Trim().ToUpperInvariant())
        };
        var now = DateTime.UtcNow;
        var hour = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerHour));
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(db, "IDS.RecoveryThrottle", token);
        var rows = await db.RecoveryThrottles.Where(t => keys.Contains(t.Key)).ToDictionaryAsync(t => t.Key, token);
        if (rows.TryGetValue(keys[0], out var ipRow) && ipRow.WindowStart >= hour && ipRow.Attempts >= limit * 5)
            return false;
        var permitted = true;
        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            if (!rows.TryGetValue(key, out var row))
            {
                row = new RecoveryThrottle { Key = key, WindowStart = hour };
                db.RecoveryThrottles.Add(row);
            }
            if (row.WindowStart < hour)
            {
                row.WindowStart = hour;
                row.Attempts = 0;
            }
            if (row.Attempts >= (i == 0 ? limit * 5 : limit)) permitted = false;
            else row.Attempts++;
        }
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return permitted;
    }

    public static async Task PadPublicRequestAsync(long started, CancellationToken token)
    {
        var floor = TimeSpan.FromMilliseconds(150 + RandomNumberGenerator.GetInt32(30));
        var remaining = floor - System.Diagnostics.Stopwatch.GetElapsedTime(started);
        if (remaining > TimeSpan.Zero) await Task.Delay(remaining, token);
    }

    public async Task<bool> EligibleAsync(ApplicationUser? user, bool enrolled = false) =>
        user != null && user.EmailConfirmed && (!enrolled || user.TwoFactorEnabled) && !user.MustChangePassword &&
        !await users.IsInRoleAsync(user, AppRoles.Suspended) && (!enrolled || !await users.IsLockedOutAsync(user));

    public async Task SendPasswordLinkAsync(ApplicationUser user, IUrlHelper url, bool admin, CancellationToken token)
    {
        var settings = await SettingsAsync(token);
        var validity = TimeSpan.FromMinutes(settings.Number(RecoverySettings.PasswordMinutes, 30, 5, 1440));
        var purpose = admin ? AdminPassword : SelfPassword;
        var signed = Issue(purpose, For(user, await users.GeneratePasswordResetTokenAsync(user)), validity);
        var address = links.Link(url, "/Account/ResetPassword", new { area = "Identity", code = signed, mode = purpose });
        if (address == null || user.Email == null) return;
        var now = DateTime.UtcNow;
        queue.Enqueue("Password reset requested", signed, new(user.Email, IncidentWorkflow.DisplayName(user),
            "Reset your IDS password", $"Use this link within {(int)validity.TotalMinutes} minutes to verify your account and choose a new password.",
            now, "Reset password", address), now.Add(validity));
        Audit(db, user, "Security.PasswordResetRequested");
        await db.SaveChangesAsync(token);
    }

    public async Task SendEmailProofAsync(ApplicationUser user, IUrlHelper url, CancellationToken token)
    {
        var signed = Issue(VerifyEmail, For(user), EmailProofLifetime);
        var address = links.Link(url, "/Account/AuthenticatorRecovery", new { area = "Identity", code = signed });
        if (address == null || user.Email == null) return;
        var now = DateTime.UtcNow;
        queue.Enqueue("Verify authenticator recovery", signed, new(user.Email, IncidentWorkflow.DisplayName(user),
            "Verify your recovery request", "Confirm control of your registered address to submit a request for administrator review. This link expires in 30 minutes.",
            now, "Verify email and continue", address), now.Add(EmailProofLifetime));
        await db.SaveChangesAsync(token);
    }

    public async Task<bool> SubmitAsync(RecoveryLink link, string proof, string reason, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(db, RequestLock, token);
        var now = DateTime.UtcNow;
        var user = await users.FindByIdAsync(link.UserId);
        if (!await EligibleAsync(user, enrolled: true) || !Matches(link, user!)) return false;
        var proofKey = AccountEmailQueue.Fingerprint("Authenticator reset submitted:" + proof);
        if (await db.SecurityEmails.AnyAsync(e => e.EventKey == proofKey, token)) return false;
        var requests = db.AuthenticatorResetRequests.Where(r => r.UserId == user!.Id);
        var active = await requests.Where(r => r.Status == "Pending" || r.Status == "Approved").ToListAsync(token);
        foreach (var request in active)
        {
            if (request.ExpiresAt > now && request.SecurityStamp == user!.SecurityStamp) return false;
            request.Status = request.ExpiresAt <= now ? "Expired" : "Cancelled";
        }
        var settings = await SettingsAsync(token);
        var requestRow = new AuthenticatorResetRequest
        {
            UserId = user!.Id, VerifiedEmail = user.Email!, SecurityStamp = user.SecurityStamp!, Reason = reason,
            CreatedAt = now, ExpiresAt = now.AddHours(settings.Number(RecoverySettings.RequestHours, 48, 1, 168))
        };
        db.AuthenticatorResetRequests.Add(requestRow);
        await db.SaveChangesAsync(token);
        queue.Enqueue("Authenticator reset submitted", proof, new(user.Email!, IncidentWorkflow.DisplayName(user),
            "Authenticator reset request received", $"Request #{requestRow.Id} was submitted for administrator review. Continue using your existing authenticator if you have it.", now));
        NotifyUser(db, user, "Authenticator reset submitted", $"Request #{requestRow.Id} is awaiting administrator review.");
        foreach (var admin in await SecurityAlertService.ActiveAdministratorsAsync(db, token))
        {
            db.InboxNotifications.Add(new()
            {
                RecipientId = admin, Type = "Authenticator reset review",
                Message = $"Authenticator reset request #{requestRow.Id} requires review.",
                AuthenticatorResetRequestId = requestRow.Id, CreatedAt = now
            });
        }

        var mail = await smtp.LoadAsync(token);
        var emailSettings = await OperationsSettings.ReadAsync(db, EmailConfiguration.Prefix, token);
        if (emailSettings.Enabled(EmailConfiguration.SupportCopies))
        {
            var contacts = new[] { mail.AdministratorEmail, mail.SupportEmail }.Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            var contactList = contacts.ToList();
            if (contactList.Count == 0)
            {
                foreach (var admin in await SecurityAlertService.ActiveAdministratorsAsync(db, token))
                {
                    var account = await users.FindByIdAsync(admin);
                    if (!string.IsNullOrWhiteSpace(account?.Email)) contactList.Add(account.Email);
                }
            }
            foreach (var contact in contactList)
            {
                queue.Enqueue("Authenticator reset needs review", requestRow.Id + ":" + contact, new(contact!, "IDS support",
                    "Authenticator reset review required",
                    $"Request #{requestRow.Id} needs identity verification. Sign in to Administration to review it.", now));
            }
        }
        Audit(db, user, "Security.AuthenticatorRecoveryRequested", requestRow.Id.ToString());
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return true;
    }

    public async Task<string> ReviewAsync(ApplicationUser reviewer, long id, bool approve, string reason,
        string? verification, IUrlHelper url, CancellationToken token)
    {
        if (!await users.IsInRoleAsync(reviewer, AppRoles.Admin) ||
            await users.IsInRoleAsync(reviewer, AppRoles.Suspended) || await users.IsLockedOutAsync(reviewer))
            return "Administrator review is required.";
        var now = DateTime.UtcNow;
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(db, RequestLock, token);
        var request = await db.AuthenticatorResetRequests.SingleOrDefaultAsync(r => r.Id == id, token);
        if (request == null || request.Status != "Pending" || request.ExpiresAt <= now) return "The request is no longer pending.";
        if (request.UserId == reviewer.Id) return "A different administrator must review this request.";
        var user = await users.FindByIdAsync(request.UserId);
        if (!await EligibleAsync(user, enrolled: true) ||
            user!.SecurityStamp != request.SecurityStamp || user.Email != request.VerifiedEmail)
            return "The account changed. Ask the user to start a new recovery request.";
        if (approve && !await AvailableAsync(RecoverySettings.AuthenticatorEnabled, token))
            return "Enable account recovery and configure email before approving requests.";

        request.Status = approve ? "Approved" : "Rejected";
        request.ReviewedAt = now;
        request.ReviewerId = reviewer.Id;
        request.ReviewReason = reason;
        request.IdentityVerification = approve ? verification : null;
        var title = approve ? "Authenticator reset approved" : "Authenticator reset rejected";
        string? action = null;
        string? urlText = null;
        var description = $"Your request #{id} was rejected. Contact IDS support if you need help verifying your identity.";
        if (approve)
        {
            var settings = await SettingsAsync(token);
            var duration = TimeSpan.FromHours(settings.Number(RecoverySettings.ApprovalHours, 8, 1, 48));
            request.ExpiresAt = now.Add(duration);
            var code = Issue(ApproveEnrollment, For(user, requestId: id), duration);
            urlText = links.Link(url, "/Account/RecoveryEnrollment", new { area = "Identity", code });
            if (urlText == null) return "The application's public account-link URL is unavailable.";
            action = "Set up replacement authenticator";
            description = $"Your request #{id} was approved. Use the dedicated enrollment link within {(int)duration.TotalHours} hours and verify your current password. After enrollment, sign in with your new authenticator and manage it from Profile Settings.";
        }
        queue.Enqueue(title, id.ToString(), new(user.Email!, IncidentWorkflow.DisplayName(user),
            title, description, now, action, urlText), approve ? request.ExpiresAt : null);
        NotifyUser(db, user, title, approve ? $"Open the email for request #{id} to enroll a new authenticator."
            : $"Request #{id} was rejected. Contact support for help.");
        Audit(db, reviewer, approve ? "Security.AuthenticatorRecoveryApproved" : "Security.AuthenticatorRecoveryRejected", id.ToString());
        await alerts.RecordAccountEventAsync(title, token);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return approve ? "Request approved. Enrollment email queued." : "Request rejected. Outcome email queued.";
    }

    public async Task<ApplicationUser?> EnrollmentUserAsync(RecoveryLink link, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        if (link.RequestId == null || !await db.AuthenticatorResetRequests.AsNoTracking().AnyAsync(r =>
            r.Id == link.RequestId && r.UserId == link.UserId && r.SecurityStamp == link.Stamp &&
            r.Status == "Approved" && r.ExpiresAt > now, token)) return null;
        var user = await users.FindByIdAsync(link.UserId);
        return await EligibleAsync(user, enrolled: true) && Matches(link, user!) ? user : null;
    }

    public async Task<string[]?> ReplaceAsync(RecoveryLink link, string candidate, string password, string code,
        CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(db, RequestLock, token);
        await DatabaseLock.AcquireAsync(db, PasswordLock, token);
        var user = await EnrollmentUserAsync(link, token);
        if (user == null || !await users.CheckPasswordAsync(user, password)) return null;
        // Identity's EF user store stores the key in this internal token slot.
        // Validate via Identity and roll back the staging write if verification fails.
        if (!(await users.SetAuthenticationTokenAsync(user, "[AspNetUserStore]", "AuthenticatorKey", candidate)).Succeeded ||
            !await VerifyFactorAsync(users, user, code, false)) return null;
        var codes = await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        if (codes == null || !(await users.UpdateSecurityStampAsync(user)).Succeeded) return null;
        var request = await db.AuthenticatorResetRequests.SingleAsync(r => r.Id == link.RequestId, token);
        request.Status = "Completed";
        request.CompletedAt = DateTime.UtcNow;
        Audit(db, user, "Security.AuthenticatorRecoveryCompleted", request.Id.ToString());
        queue.Confirm(user, "Authenticator replaced", "Your replacement authenticator was verified. Prior authenticator credentials and recovery codes were revoked.");
        NotifyUser(db, user, "Authenticator replaced", "Your account is now protected by your replacement authenticator.");
        await alerts.RecordAccountEventAsync("Authenticator replaced", token);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return codes.ToArray();
    }

    public static async Task<bool> VerifyFactorAsync(UserManager<ApplicationUser> users, ApplicationUser user,
        string? code, bool recoveryCode)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 64) return false;
        var normalized = code.Trim().Replace(" ", "");
        if (recoveryCode) return (await users.RedeemTwoFactorRecoveryCodeAsync(user, normalized)).Succeeded;
        normalized = normalized.Replace("-", "");
        return normalized.Length == 6 && normalized.All(char.IsAsciiDigit) &&
            await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, normalized);
    }

    public static void Audit(ApplicationDbContext db, ApplicationUser user, string action, string? id = null)
        => db.AuditLogs.Add(new()
        {
            UserId = user.Id, UserEmail = user.Email ?? "", Action = action, EntityType = "Authentication",
            EntityId = id ?? user.Id, Timestamp = DateTime.UtcNow
        });

    public static void NotifyUser(ApplicationDbContext db, ApplicationUser user, string type, string message)
        => db.InboxNotifications.Add(new()
        {
            RecipientId = user.Id, Type = type, Message = message, CreatedAt = DateTime.UtcNow, IsAccountSecurity = true
        });

    public static Task LockPasswordAsync(ApplicationDbContext db, CancellationToken token) =>
        DatabaseLock.AcquireAsync(db, PasswordLock, token);
}
