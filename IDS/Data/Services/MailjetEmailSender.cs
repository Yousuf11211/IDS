using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;

namespace IDS.Data.Services
{
    /// <summary>
    /// Professional email sender using Mailjet SMTP with MailKit.
    /// Supports HTML templates, plain text fallback, and development mode (no-send).
    /// </summary>
    public class MailjetEmailSender : IEmailSender
    {
        private readonly ILogger<MailjetEmailSender> _logger;
        private readonly IWebHostEnvironment _environment;
  
        // Configuration loaded from environment variables
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _supportEmail;
        private readonly bool _emailEnabled;
        private readonly bool _ignoreSSLErrors;
        private readonly string _templateBasePath;

        /// <summary>
        /// Initializes the Mailjet email sender with configuration from environment variables.
        /// </summary>
        public MailjetEmailSender(ILogger<MailjetEmailSender> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;

            // =====================================================
            // Load configuration from environment variables
            // Falls back to defaults for development
            // =====================================================
            _apiKey = Environment.GetEnvironmentVariable("MAILJET_API_KEY") ?? string.Empty;
            _secretKey = Environment.GetEnvironmentVariable("MAILJET_SECRET_KEY") ?? string.Empty;
            _smtpServer = Environment.GetEnvironmentVariable("MAILJET_SMTP_SERVER") ?? "in-v3.mailjet.com";
            _smtpPort = int.TryParse(Environment.GetEnvironmentVariable("MAILJET_SMTP_PORT"), out var port) ? port : 587;
            _fromEmail = Environment.GetEnvironmentVariable("MAILJET_FROM_EMAIL") ?? "no-reply@intrusiondetectionsystem.great-site.net";
            _fromName = Environment.GetEnvironmentVariable("MAILJET_FROM_NAME") ?? "IDS System";
            _supportEmail = Environment.GetEnvironmentVariable("SUPPORT_EMAIL") ?? "support@intrusiondetectionsystem.great-site.net";
            
            // EMAIL_STATUS controls whether emails are actually sent
            // When false, emails are logged but not sent (useful for development)
            var emailStatus = Environment.GetEnvironmentVariable("EMAIL_STATUS") ?? "false";
            _emailEnabled = emailStatus.Equals("true", StringComparison.OrdinalIgnoreCase);

            // SECURITY: Allows bypassing SSL certificate checks (useful for dev/corporate networks but UNSAFE for production)
            var ignoreSSL = Environment.GetEnvironmentVariable("MAILJET_IGNORE_CERTIFICATE_ERRORS") ?? "false";
            _ignoreSSLErrors = ignoreSSL.Equals("true", StringComparison.OrdinalIgnoreCase);
  
            // Template path relative to content root
            _templateBasePath = Path.Combine(_environment.ContentRootPath, "Templates", "Email");
            
            // Log configuration status (without sensitive data)
            _logger.LogInformation("MailjetEmailSender initialized. Email sending is {Status}. SMTP: {Server}:{Port}. SSL Bypass: {SSL}", 
                _emailEnabled ? "ENABLED" : "DISABLED", _smtpServer, _smtpPort, _ignoreSSLErrors);
        }

        /// <summary>
        /// Check if email sending is properly configured.
        /// </summary>
        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_secretKey) && _emailEnabled;

        /// <summary>
        /// Gets the support email address from configuration.
        /// </summary>
        public string SupportEmail => _supportEmail;

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
        public async Task<(bool Success, string? ErrorMessage)> SendEmailAsync(string toEmail, string? toName, string subject, string htmlBody, string? textBody = null)
        {
            // =====================================================
            // Validate inputs
            // =====================================================
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                var msg = "SendEmailAsync called with empty recipient email. Skipping.";
                _logger.LogWarning(msg);
                return (false, msg);
            }

            // =====================================================
            // Check if email sending is enabled
            // =====================================================
            if (!_emailEnabled)
            {
                var msg = $"[EMAIL DISABLED] Would send email to: {toEmail}. Set EMAIL_STATUS=true to enable.";
                _logger.LogInformation(
                    "[EMAIL DISABLED] Would send email to: {ToEmail}\n" +
                    "Subject: {Subject}\n" +
                    "HTML Body Preview: {Preview}...",
                    toEmail, 
                    subject, 
                    htmlBody?.Length > 200 ? htmlBody[..200] : htmlBody);
                return (true, msg); // Return true because it "succeeded" in doing what it was told (not sending)
            }

            // =====================================================
            // Check if API credentials are configured
            // =====================================================
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                var msg = "Mailjet API credentials not set. Set MAILJET_API_KEY and MAILJET_SECRET_KEY.";
                _logger.LogWarning(
                    "[EMAIL NOT CONFIGURED] {Msg} Email to {ToEmail} was NOT sent.",
                    msg, toEmail);
                return (false, msg);
            }

            try
            {
                // =====================================================
                // Build the email message
                // =====================================================
                var message = new MimeMessage();
            
                // Set sender
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        
                // Set recipient
                if (!string.IsNullOrEmpty(toName))
                {
                    message.To.Add(new MailboxAddress(toName, toEmail));
                }
                else
                {
                    message.To.Add(MailboxAddress.Parse(toEmail));
                }
                
                // Set subject
                message.Subject = subject;

                // =====================================================
                // Build multipart body (HTML + plain text)
                // =====================================================
                var builder = new BodyBuilder();
            
                // Plain text version (auto-generate from HTML if not provided)
                builder.TextBody = textBody ?? StripHtmlTags(htmlBody);
        
                // HTML version
                builder.HtmlBody = htmlBody;
                    
                message.Body = builder.ToMessageBody();

                // =====================================================
                // Send via SMTP
                // =====================================================
                using var client = new SmtpClient();

                if (_ignoreSSLErrors)
                {
                    // WARNING: Only use this in development or if strictly necessary due to network/firewall issues
                    // This creates a security risk (Man-in-the-Middle attacks)
                    client.CheckCertificateRevocation = false;
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    _logger.LogWarning("SSL Certificate validation is DISABLED. This is unsafe for production.");
                }
        
                // Connect to Mailjet SMTP server
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                
                // Authenticate with API Key (username) and Secret Key (password)
                await client.AuthenticateAsync(_apiKey, _secretKey);
                
                // Send the message
                await client.SendAsync(message);
            
                // Disconnect cleanly
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'", toEmail, subject);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'", toEmail, subject);
                return (false, ex.Message);
            }
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
        /// Sends a temporary password email to a new user.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> SendTempPasswordEmailAsync(string toEmail, string userName, string temporaryPassword, string? loginUrl = null)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "UserName", userName ?? "User" },
                { "UserEmail", toEmail },
                { "TemporaryPassword", temporaryPassword },
                { "LoginUrl", loginUrl ?? "/Identity/Account/Login" },
                { "SupportEmail", _supportEmail },
                { "Year", DateTime.Now.Year.ToString() }
            };

            var (htmlBody, textBody) = await LoadTemplateAsync("TempPasswordEmail", placeholders);
            return await SendEmailAsync(toEmail, userName, "Your Temporary Password - IDS", htmlBody, textBody);
        }

        /// <summary>
        /// Sends a password reset email.
        /// </summary>
    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetUrl, int expiryHours = 24)
        {
            var placeholders = new Dictionary<string, string>
   {
         { "UserName", userName ?? "User" },
            { "UserEmail", toEmail },
      { "ResetUrl", resetUrl },
       { "ExpiryHours", expiryHours.ToString() },
                { "SupportEmail", _supportEmail },
  { "Year", DateTime.Now.Year.ToString() }
      };

   var (htmlBody, textBody) = await LoadTemplateAsync("PasswordResetEmail", placeholders);
            await SendEmailAsync(toEmail, userName, "Reset Your Password - IDS", htmlBody, textBody);
        }

        /// <summary>
        /// Sends an email confirmation email.
        /// </summary>
        public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationUrl)
   {
            var placeholders = new Dictionary<string, string>
      {
         { "UserName", userName ?? "User" },
                { "UserEmail", toEmail },
      { "ConfirmationUrl", confirmationUrl },
     { "SupportEmail", _supportEmail },
  { "Year", DateTime.Now.Year.ToString() }
            };

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
            var placeholders = new Dictionary<string, string>
      {
        { "UserName", userName ?? "Admin" },
                { "AlertId", alertId },
          { "AlertTitle", alertTitle },
                { "AlertDescription", alertDescription },
          { "SeverityLevel", severityLevel },
   { "SourceIP", sourceIP ?? "Unknown" },
                { "AttackType", attackType ?? "Unknown" },
         { "DetectedAt", detectedAt.ToString("yyyy-MM-dd HH:mm:ss UTC") },
   { "RecommendedAction", recommendedAction ?? "Review the alert details and take appropriate action based on your security policies." },
         { "DashboardUrl", dashboardUrl ?? "/Admin/Alerts" },
      { "SupportEmail", _supportEmail },
            { "Year", DateTime.Now.Year.ToString() }
            };

   var (htmlBody, textBody) = await LoadTemplateAsync("SecurityAlertEmail", placeholders);
       await SendEmailAsync(toEmail, userName, $"?? Security Alert: {alertTitle} - IDS", htmlBody, textBody);
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
     Dictionary<string, string> placeholders)
     {
   var htmlPath = Path.Combine(_templateBasePath, $"{templateName}.html");
         var textPath = Path.Combine(_templateBasePath, $"{templateName}.txt");

            string htmlBody;
            string textBody;

       // =====================================================
   // Load HTML template
            // =====================================================
    if (File.Exists(htmlPath))
        {
            htmlBody = await File.ReadAllTextAsync(htmlPath);
                htmlBody = ReplacePlaceholders(htmlBody, placeholders);
    }
   else
  {
  _logger.LogWarning("HTML template not found: {Path}. Using fallback.", htmlPath);
          htmlBody = GenerateFallbackHtml(templateName, placeholders);
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
            var result = template;
        foreach (var (key, value) in placeholders)
            {
      result = result.Replace($"{{{key}}}", value ?? string.Empty);
  }
         return result;
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
