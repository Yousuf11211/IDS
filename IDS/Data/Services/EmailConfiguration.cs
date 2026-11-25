using System.Security.Cryptography;
using IDS.Core.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace IDS.Data.Services;

public sealed record MailjetSettings(bool Enabled, string Host, int Port, string SenderEmail, string SenderName,
    string SupportEmail, string? AdministratorEmail, string ApiKey, string Secret)
{
    public bool Ready => Enabled && !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Secret);

    public static MailjetSettings EnvironmentDefaults()
    {
        static string Read(string key, string fallback = "") => Environment.GetEnvironmentVariable(key) ?? fallback;
        return new(string.Equals(Read("EMAIL_STATUS"), "true", StringComparison.OrdinalIgnoreCase),
            Read("MAILJET_SMTP_SERVER", "in-v3.mailjet.com"),
            int.TryParse(Read("MAILJET_SMTP_PORT"), out var port) ? port : 587,
            Read("MAILJET_FROM_EMAIL", "no-reply@intrusiondetectionsystem.great-site.net"),
            Read("MAILJET_FROM_NAME", "IDS System"),
            Read("SUPPORT_EMAIL", "support@intrusiondetectionsystem.great-site.net"),
            Environment.GetEnvironmentVariable("ADMIN_CONTACT_EMAIL"),
            Read("MAILJET_API_KEY"), Read("MAILJET_SECRET_KEY"));
    }
}

public sealed class EmailConfiguration(ApplicationDbContext db, IDataProtectionProvider protection)
{
    public const string Prefix = "Email.";
    public const string ApiKey = Prefix + "ApiKey";
    public const string Secret = Prefix + "Secret";
    public const string Enabled = Prefix + "Enabled";
    public const string Port = Prefix + "Port";
    public const string SenderEmail = Prefix + "SenderEmail";
    public const string SenderName = Prefix + "SenderName";
    public const string SupportEmail = Prefix + "SupportEmail";
    public const string AdminEmail = Prefix + "AdministratorEmail";
    public const string SupportCopies = Prefix + "SupportCopies";
    private readonly IDataProtector _protector = protection.CreateProtector("IDS.Mailjet.Credentials.v1");

    public string Protect(string value) => _protector.Protect(value);

    public async Task<string?> ContactAsync(CancellationToken token)
    {
        var defaults = MailjetSettings.EnvironmentDefaults();
        var values = await db.SystemSettings.AsNoTracking().Where(s => s.Key == SupportEmail || s.Key == AdminEmail)
            .ToDictionaryAsync(s => s.Key, s => s.Value, token);
        foreach (var address in new[] { values.GetValueOrDefault(SupportEmail) ?? defaults.SupportEmail,
            values.GetValueOrDefault(AdminEmail) ?? defaults.AdministratorEmail })
        {
            if (MimeKit.MailboxAddress.TryParse(address, out var mailbox)) return mailbox.Address;
        }
        return null;
    }

    public async Task<MailjetSettings> ReadAsync(CancellationToken token = default)
    {
        var defaults = MailjetSettings.EnvironmentDefaults();
        var values = await OperationsSettings.ReadAsync(db, Prefix, token);
        string? Get(string key, string? fallback) => values.GetValueOrDefault(key) ?? fallback;
        string SecretValue(string key, string fallback)
        {
            if (!values.TryGetValue(key, out var value)) return fallback;
            try { return _protector.Unprotect(value); }
            catch (CryptographicException) { return ""; }
        }
        return defaults with
        {
            Enabled = values.Enabled(Enabled, defaults.Enabled),
            Port = values.Number(Port, defaults.Port, 1, 65535),
            SenderEmail = Get(SenderEmail, defaults.SenderEmail)!,
            SenderName = Get(SenderName, defaults.SenderName)!,
            SupportEmail = Get(SupportEmail, defaults.SupportEmail)!,
            AdministratorEmail = Get(AdminEmail, defaults.AdministratorEmail),
            ApiKey = SecretValue(ApiKey, defaults.ApiKey),
            Secret = SecretValue(Secret, defaults.Secret)
        };
    }
}
