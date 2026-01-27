using IDS.Data.Services;

namespace IDS.Examples
{
    /// <summary>
 /// Example usage of the MailjetEmailSender.
    /// This file demonstrates how to use the email sender in different scenarios.
    /// </summary>
    public static class EmailSenderExamples
    {
        /// <summary>
        /// Example 1: Send a temporary password email to a new user.
        /// </summary>
   public static async Task SendTempPasswordExample(MailjetEmailSender emailSender)
{
   await emailSender.SendTempPasswordEmailAsync(
           toEmail: "newuser@example.com",
   userName: "John",
     temporaryPassword: "Temp@123!",
    loginUrl: "https://ids.example.com/Identity/Account/Login"
          );
     }

        /// <summary>
   /// Example 2: Send a password reset email.
        /// </summary>
        public static async Task SendPasswordResetExample(MailjetEmailSender emailSender)
        {
            await emailSender.SendPasswordResetEmailAsync(
      toEmail: "user@example.com",
          userName: "John",
    resetUrl: "https://ids.example.com/Identity/Account/ResetPassword?token=abc123",
    expiryHours: 24
            );
    }

        /// <summary>
      /// Example 3: Send an email confirmation.
        /// </summary>
        public static async Task SendEmailConfirmationExample(MailjetEmailSender emailSender)
        {
  await emailSender.SendEmailConfirmationAsync(
     toEmail: "user@example.com",
     userName: "John",
     confirmationUrl: "https://ids.example.com/Identity/Account/ConfirmEmail?token=abc123"
          );
 }

      /// <summary>
  /// Example 4: Send a security alert notification.
     /// </summary>
     public static async Task SendSecurityAlertExample(MailjetEmailSender emailSender)
   {
    await emailSender.SendSecurityAlertEmailAsync(
                toEmail: "admin@example.com",
        userName: "Admin",
           alertId: "ALT-2024-001",
           alertTitle: "Potential DDoS Attack Detected",
       alertDescription: "Multiple connection attempts from a single IP address have been detected, indicating a possible DDoS attack.",
          severityLevel: "HIGH",
             sourceIP: "192.168.1.100",
         attackType: "DDoS",
  detectedAt: DateTime.UtcNow,
      recommendedAction: "Review the traffic patterns and consider blocking the source IP if the attack continues.",
 dashboardUrl: "https://ids.example.com/Admin/Alerts"
     );
        }

        /// <summary>
     /// Example 5: Send a simple custom email using the base IEmailSender interface.
   /// </summary>
 public static async Task SendSimpleEmailExample(MailjetEmailSender emailSender)
        {
            await emailSender.SendEmailAsync(
     email: "user@example.com",
           subject: "Custom Notification",
         htmlMessage: "<h1>Hello!</h1><p>This is a custom notification from IDS.</p>"
    );
    }

        /// <summary>
        /// Example 6: Check if email is configured before sending.
        /// </summary>
      public static async Task ConditionalSendExample(MailjetEmailSender emailSender)
      {
     if (emailSender.IsConfigured)
            {
          // Email is enabled and API keys are set
      await emailSender.SendTempPasswordEmailAsync(
      toEmail: "user@example.com",
         userName: "User",
        temporaryPassword: "Temp@123!"
 );
       Console.WriteLine("Email sent successfully!");
          }
     else
   {
        // Email is disabled or not configured
       Console.WriteLine("Email sending is disabled. Please configure MAILJET credentials and set EMAIL_STATUS=true");
        // Handle alternative notification method (e.g., display credentials on screen)
            }
  }
    }
}

/*
===============================================================================
       MAILJET EMAIL SENDER - USAGE GUIDE
===============================================================================

1. CONFIGURATION
----------------
   All configuration is done via environment variables (or .env file).
   
   Required variables:
   - MAILJET_API_KEY     : Your Mailjet API key
   - MAILJET_SECRET_KEY  : Your Mailjet secret key
   - EMAIL_STATUS : "true" to enable, "false" to disable
   
   Optional variables:
   - MAILJET_SMTP_SERVER : Default: in-v3.mailjet.com
   - MAILJET_SMTP_PORT   : Default: 587
   - MAILJET_FROM_EMAIL  : Sender email (must be verified in Mailjet)
   - MAILJET_FROM_NAME   : Sender display name
   - SUPPORT_EMAIL       : Support email shown in templates

2. DEVELOPMENT MODE
-------------------
   Set EMAIL_STATUS=false during development.
   - Emails will be logged to console instead of sent
   - No API credentials required
   - Safe for testing without sending real emails

3. PRODUCTION MODE
------------------
   Set EMAIL_STATUS=true and provide valid API credentials.
 - Emails will be sent via Mailjet SMTP
   - Ensure MAILJET_FROM_EMAIL is verified in your Mailjet account

4. TEMPLATE CUSTOMIZATION
-------------------------
   Templates are located in: /Templates/Email/
   
   Available templates:
   - TempPasswordEmail.html/.txt     : New user welcome with temp password
   - PasswordResetEmail.html/.txt    : Password reset request
   - EmailConfirmation.html/.txt     : Email address confirmation
   - SecurityAlertEmail.html/.txt    : Security alert notifications
   
   Placeholders use {PlaceholderName} format and are replaced at runtime.

5. DEPENDENCY INJECTION
-----------------------
   The MailjetEmailSender is registered as a singleton in Program.cs:
   
   // Get via DI in a PageModel or Controller:
   public MyPageModel(MailjetEmailSender emailSender) { ... }
   
   // Or via IEmailSender interface:
   public MyPageModel(IEmailSender emailSender) { ... }

6. ERROR HANDLING
-----------------
   The email sender handles all errors gracefully:
   - Failures are logged but don't throw exceptions
   - Your application continues running even if email fails
   - Check logs for email delivery issues

===============================================================================
*/
