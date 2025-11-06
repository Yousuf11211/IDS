using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class SendGridEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SendGridEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var apiKey = _config["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(_config["SendGrid:FromEmail"], _config["SendGrid:FromName"]);
        var to = new EmailAddress(email);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlMessage);
        await client.SendEmailAsync(msg);
    }
}
