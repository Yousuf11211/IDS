using System.ComponentModel.DataAnnotations;
using System.Text;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IDS.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MailjetEmailSender _emailSender;
    private readonly AccountLinkBuilder _accountLinks;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        MailjetEmailSender emailSender,
        AccountLinkBuilder accountLinks,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _accountLinks = accountLinks;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email.Trim());
        if (user != null && await _userManager.IsEmailConfirmedAsync(user) &&
            _emailSender.IsConfigured)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = _accountLinks.PasswordResetLink(Url, encodedToken);
            if (link != null)
            {
                var name = string.IsNullOrWhiteSpace(user.FirstName) ? "Employee" : user.FirstName;
                var (sent, error) = await _emailSender.SendAccountAccessLinkAsync(
                    user.Email!, name, link, isInvitation: false);
                if (!sent)
                    _logger.LogWarning("Password reset email failed for user {UserId}: {Error}", user.Id, error);
            }
            else
            {
                _logger.LogWarning("Password reset email could not be sent: PUBLIC_BASE_URL is invalid.");
            }
        }

        // The same response for every address prevents account enumeration.
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
