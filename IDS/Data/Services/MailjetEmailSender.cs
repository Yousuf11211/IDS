using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IDS.Data.Services
{
    public class MailjetEmailSender : IEmailSender
    {
   private readonly IConfiguration _configuration;
        private readonly ILogger<MailjetEmailSender> _logger;
        private readonly HttpClient _httpClient;

        public MailjetEmailSender(IConfiguration configuration, ILogger<MailjetEmailSender> logger)
  {
            _configuration = configuration;
    _logger = logger;
     _httpClient = new HttpClient();
    }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
      {
            // Read from environment variables first, fall back to config
        var apiKey = Environment.GetEnvironmentVariable("MAILJET_API_KEY") 
    ?? _configuration["Mailjet:ApiKey"];
         var secretKey = Environment.GetEnvironmentVariable("MAILJET_SECRET_KEY") 
     ?? _configuration["Mailjet:SecretKey"];
       var senderEmail = Environment.GetEnvironmentVariable("MAILJET_SENDER_EMAIL") 
     ?? _configuration["Mailjet:SenderEmail"] 
             ?? "noreply@ids-system.local";
      var senderName = Environment.GetEnvironmentVariable("MAILJET_SENDER_NAME") 
  ?? _configuration["Mailjet:SenderName"] 
          ?? "IDS System";

        // Skip sending if no API keys configured (graceful fallback for local testing)
       if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secretKey))
   {
     _logger.LogWarning("Mailjet API keys not configured. Email to {Email} with subject '{Subject}' was not sent.", email, subject);
   _logger.LogInformation("Email content that would have been sent:\n{Content}", htmlMessage);
   return;
            }

            try
            {
              var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:{secretKey}"));
 _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

       var payload = new
     {
      Messages = new[]
        {
     new
        {
     From = new { Email = senderEmail, Name = senderName },
  To = new[] { new { Email = email } },
  Subject = subject,
  HTMLPart = htmlMessage
        }
  }
                };

        var content = new StringContent(
       JsonSerializer.Serialize(payload),
          Encoding.UTF8,
        "application/json"
             );

var response = await _httpClient.PostAsync("https://api.mailjet.com/v3.1/send", content);

                if (response.IsSuccessStatusCode)
          {
        _logger.LogInformation("Email sent successfully to {Email}", email);
 }
                else
       {
          var responseContent = await response.Content.ReadAsStringAsync();
    _logger.LogError("Failed to send email to {Email}. Status: {Status}, Response: {Response}", 
            email, response.StatusCode, responseContent);
      }
}
   catch (Exception ex)
            {
    _logger.LogError(ex, "Exception occurred while sending email to {Email}", email);
            }
      }
    }
}
