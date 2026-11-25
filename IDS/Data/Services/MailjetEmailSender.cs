using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;
using System.Text.Encodings.Web;

namespace IDS.Data.Services
{
    /// <summary>
    /// Professional email sender using Mailjet SMTP with MailKit.
    /// Supports HTML templates, plain text fallback, and development mode (no-send).
    /// </summary>
    public class MailjetEmailSender : IEmailSender
    {
        private readonly ILogger<MailjetEmailSender> _logger;
  
        private readonly EmailConfiguration _configuration;
        private readonly string _templateBasePath;
        private readonly string _logoUrl;
        private readonly bool _ignoreSSLErrors;
        private MailjetSettings? _settings;

        public MailjetEmailSender(ILogger<MailjetEmailSender> logger, IWebHostEnvironment environment,
            EmailConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _templateBasePath = Path.Combine(environment.ContentRootPath, "Templates", "Email");
            _logoUrl = Environment.GetEnvironmentVariable("EMAIL_LOGO_URL") ?? "";
            _ignoreSSLErrors = environment.IsDevelopment() &&
                string.Equals(Environment.GetEnvironmentVariable("MAILJET_IGNORE_CERTIFICATE_ERRORS"), "true",
                    StringComparison.OrdinalIgnoreCase);
        }

        public async Task<MailjetSettings> LoadAsync(CancellationToken token = default) =>
            _settings ??= await _configuration.ReadAsync(token);

        public bool IsConfigured => (_settings ?? MailjetSettings.EnvironmentDefaults()).Ready;

        /// <summary>
        /// Gets the support email address from configuration.
        /// </summary>
        public string SupportEmail => (_settings ?? MailjetSettings.EnvironmentDefaults()).SupportEmail;

        /// <summary>
        /// Gets the logo URL for email branding.
        /// </summary>
        public string LogoUrl => _logoUrl;

      #region IEmailSender Implementation

   /// <summary>
     /// Sends an email (IEmailSender interface implementation).
     /// </summary>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
     {
            await SendEmailAsync(email, null, subject, htmlMessage, null);
}

 #endregion

        #region Core Email Methods

        /// <summary>
        /// Sends an email with both HTML and plain text versions.
        /// Returns a tuple indicating success and any error message.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="toName">Recipient name (optional)</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML body content</param>
        /// <param name="textBody">Plain text body content (optional - auto-generated if not provided)</param>
        public async Task<(bool Success, string? ErrorMessage)> SendEmailAsync(string toEmail, string? toName,
            string subject, string htmlBody, string? textBody = null, string? messageId = null,
            CancellationToken cancellationToken = default)
        {
            var settings = await LoadAsync(cancellationToken);
            if (!settings.Ready) return (false, "Mailjet is disabled or unavailable.");
            if (!MailboxAddress.TryParse(toEmail, out var recipient)) return (false, "Invalid recipient address.");

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
                message.To.Add(new MailboxAddress(toName ?? recipient.Name, recipient.Address));
                message.Subject = subject;
                if (messageId != null) message.MessageId = messageId;
                message.Body = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody ?? StripHtmlTags(htmlBody) }.ToMessageBody();

                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                deadline.CancelAfter(TimeSpan.FromSeconds(20));
                using var client = new SmtpClient { Timeout = 15000 };
                if (_ignoreSSLErrors)
                {
                    client.CheckCertificateRevocation = false;
                    client.ServerCertificateValidationCallback = (_, _, _, _) => true;
                    _logger.LogWarning("SMTP certificate verification is disabled in Development.");
                }

                await client.ConnectAsync(settings.Host, settings.Port,
                    settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, deadline.Token);
                await client.AuthenticateAsync(settings.ApiKey, settings.Secret, deadline.Token);
                await client.SendAsync(message, deadline.Token);
                // The provider has accepted the message. A disconnect error must not cause another send.
                try { await client.DisconnectAsync(true, deadline.Token); }
                catch { }
                return (true, null);
            }
            catch (Exception error) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Mailjet SMTP submission failed ({ErrorType}).", error.GetType().Name);
                return (false, "Mailjet submission failed.");
            }
        }

        public virtual async Task<(bool Success, string? ErrorMessage)> SendAccountSecurityAsync(AccountEmail payload,
            string messageId, CancellationToken token)
        {
            var settings = await LoadAsync(token);
            var contact = string.IsNullOrWhiteSpace(settings.SupportEmail) ? settings.AdministratorEmail ?? "your administrator" : settings.SupportEmail;
            var values = GetBasePlaceholders();
            values["Name"] = payload.Name;
            values["Title"] = payload.Title;
            values["Description"] = payload.Description;
            values["OccurredAt"] = payload.OccurredAt.ToString("yyyy-MM-dd HH:mm 'UTC'");
            values["Contact"] = contact;
            values["ActionLabel"] = payload.ActionLabel ?? "";
            values["ActionUrl"] = payload.ActionUrl ?? "";
            values["Warning"] = payload.Confirmation
                ? "Your IDS account security settings have been updated. If you authorized this change, no further action is required. If you did not make this change, contact your administrator or support immediately."
                : "If you did not expect this message, contact your administrator or support.";
            var (html, text) = await LoadTemplateAsync("AccountSecurity", values, encodeHtml: true);
            return await SendEmailAsync(payload.To, payload.Name, payload.Title + " · IDS Security", html, text, messageId, token);
        }

        /// <summary>
        /// Synchronous wrapper for SendEmailAsync.
        /// </summary>
        public void SendEmail(string toEmail, string? toName, string subject, string htmlBody, string? textBody = null)
        {
            SendEmailAsync(toEmail, toName, subject, htmlBody, textBody).GetAwaiter().GetResult();
        }

        #endregion

        #region Template-Based Email Methods

    /// <summary>
   /// Sends a password reset email.
        /// </summary>
 public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetUrl, int expiryHours = 24)
        {
  var placeholders = GetBasePlaceholders();
     placeholders["UserName"] = userName ?? "User";
     placeholders["UserEmail"] = toEmail;
    placeholders["ResetUrl"] = resetUrl;
            placeholders["ExpiryHours"] = expiryHours.ToString();

  var (htmlBody, textBody) = await LoadTemplateAsync("PasswordResetEmail", placeholders);
   await SendEmailAsync(toEmail, userName, "Reset Your Password - IDS", htmlBody, textBody);
        }

        /// <summary>
        /// Sends an email confirmation email.
     /// </summary>
        public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationUrl)
 {
            var placeholders = GetBasePlaceholders();
         placeholders["UserName"] = userName ?? "User";
    placeholders["UserEmail"] = toEmail;
        placeholders["ConfirmationUrl"] = confirmationUrl;

      var (htmlBody, textBody) = await LoadTemplateAsync("EmailConfirmation", placeholders);
        await SendEmailAsync(toEmail, userName, "Confirm Your Email - IDS", htmlBody, textBody);
        }

        /// <summary>
        /// Sends a security alert notification email.
        /// </summary>
        public async Task SendSecurityAlertEmailAsync(
 string toEmail, 
            string userName,
   string alertId,
            string alertTitle,
    string alertDescription,
    string severityLevel,
            string sourceIP,
     string attackType,
     DateTime detectedAt,
            string? recommendedAction = null,
            string? dashboardUrl = null)
  {
         var placeholders = GetBasePlaceholders();
        placeholders["UserName"] = userName ?? "Admin";
            placeholders["AlertId"] = alertId;
   placeholders["AlertTitle"] = alertTitle;
            placeholders["AlertDescription"] = alertDescription;
        placeholders["SeverityLevel"] = severityLevel;
            placeholders["SourceIP"] = sourceIP ?? "Unknown";
            placeholders["AttackType"] = attackType ?? "Unknown";
            placeholders["DetectedAt"] = detectedAt.ToString("yyyy-MM-dd HH:mm:ss UTC");
    placeholders["RecommendedAction"] = recommendedAction ?? "Review the alert details and take appropriate action based on your security policies.";
        placeholders["DashboardUrl"] = dashboardUrl ?? "/Admin/Alerts";

        var (htmlBody, textBody) = await LoadTemplateAsync("SecurityAlertEmail", placeholders);
            await SendEmailAsync(toEmail, userName, $"Security Alert: {alertTitle} - IDS", htmlBody, textBody);  // Removed emoji
        }

        /// <summary>
 /// Gets base placeholders common to all email templates.
/// </summary>
        private Dictionary<string, string> GetBasePlaceholders()
        {
          return new Dictionary<string, string>
            {
    { "SupportEmail", SupportEmail },
    { "Year", DateTime.Now.Year.ToString() },
     { "LogoUrl", _logoUrl },
         { "HasLogo", string.IsNullOrEmpty(_logoUrl) ? "false" : "true" }
     };
        }

    #endregion

        #region Template Helpers

        /// <summary>
  /// Loads an email template and replaces placeholders.
        /// </summary>
      /// <param name="templateName">Name of the template (without extension)</param>
        /// <param name="placeholders">Dictionary of placeholder values</param>
        /// <returns>Tuple of (HTML body, plain text body)</returns>
        private async Task<(string HtmlBody, string TextBody)> LoadTemplateAsync(
  string templateName, 
     Dictionary<string, string> placeholders, bool encodeHtml = false)
     {
   var htmlPath = Path.Combine(_templateBasePath, $"{templateName}.html");
         var textPath = Path.Combine(_templateBasePath, $"{templateName}.txt");

            var htmlValues = encodeHtml ? placeholders.ToDictionary(item => item.Key,
                item => HtmlEncoder.Default.Encode(item.Value)) : placeholders;
            string htmlBody;
            string textBody;

       // =====================================================
   // Load HTML template
            // =====================================================
    if (File.Exists(htmlPath))
        {
            htmlBody = await File.ReadAllTextAsync(htmlPath);
                htmlBody = ReplacePlaceholders(htmlBody, htmlValues);
    }
   else
  {
  _logger.LogWarning("HTML template not found: {Path}. Using fallback.", htmlPath);
          htmlBody = GenerateFallbackHtml(templateName, htmlValues);
       }

            // =====================================================
            // Load plain text template
        // =====================================================
            if (File.Exists(textPath))
   {
      textBody = await File.ReadAllTextAsync(textPath);
    textBody = ReplacePlaceholders(textBody, placeholders);
            }
          else
  {
    _logger.LogWarning("Text template not found: {Path}. Generating from HTML.", textPath);
         textBody = StripHtmlTags(htmlBody);
          }

         return (htmlBody, textBody);
        }

        /// <summary>
 /// Replaces all placeholders in the format {PlaceholderName} with their values.
  /// </summary>
        private static string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
{
            return System.Text.RegularExpressions.Regex.Replace(template, @"\{([A-Za-z][A-Za-z0-9]*)\}",
                match => placeholders.GetValueOrDefault(match.Groups[1].Value) ?? match.Value);
        }

        /// <summary>
        /// Generates a simple fallback HTML email when template is not found.
        /// </summary>
        private static string GenerateFallbackHtml(string templateName, Dictionary<string, string> placeholders)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><body style='font-family: Arial, sans-serif; padding: 20px;'>");
         sb.AppendLine("<h1>Intrusion Detection System</h1>");
         sb.AppendLine($"<p>Template: {templateName}</p>");
            sb.AppendLine("<hr/>");
 
            foreach (var (key, value) in placeholders)
      {
     if (!string.IsNullOrEmpty(value) && key != "Year")
        {
       sb.AppendLine($"<p><strong>{key}:</strong> {value}</p>");
             }
   }
       
         sb.AppendLine("<hr/>");
       sb.AppendLine("<p style='color: #666; font-size: 12px;'>This is an automated message from IDS.</p>");
            sb.AppendLine("</body></html>");
     
    return sb.ToString();
        }

 /// <summary>
        /// Strips HTML tags from content to create plain text version.
    /// </summary>
        private static string StripHtmlTags(string? html)
 {
     if (string.IsNullOrEmpty(html))
    return string.Empty;

  // Remove script and style blocks
            var result = System.Text.RegularExpressions.Regex.Replace(html, @"<(script|style)[^>]*>.*?</\1>", "", 
    System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    
    // Replace <br> and </p> with newlines
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<br\s*/?>", "\n", 
   System.Text.RegularExpressions.RegexOptions.IgnoreCase);
         result = System.Text.RegularExpressions.Regex.Replace(result, @"</p>", "\n\n", 
   System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"</tr>", "\n", 
          System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"</li>", "\n", 
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
     
    // Remove all remaining HTML tags
         result = System.Text.RegularExpressions.Regex.Replace(result, @"<[^>]+>", "");
        
            // Decode HTML entities
            result = System.Net.WebUtility.HtmlDecode(result);
    
        // Clean up whitespace
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[ \t]+", " ");
    result = System.Text.RegularExpressions.Regex.Replace(result, @"\n\s*\n\s*\n", "\n\n");
        
            return result.Trim();
        }

   #endregion
    }
}
