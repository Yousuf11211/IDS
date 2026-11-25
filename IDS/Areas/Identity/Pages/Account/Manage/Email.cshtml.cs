using System.ComponentModel.DataAnnotations;
using System.Text;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class EmailModel(UserManager<ApplicationUser> users, AccountRecovery recovery, MailjetEmailSender smtp,
    AccountLinkBuilder links, AccountEmailQueue email, ApplicationDbContext db) : PageModel
{
    public string? Email { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public bool RequiresAuthenticator { get; private set; }
    [TempData] public string? StatusMessage { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [EmailAddress, StringLength(256), Display(Name = "New email")] public string? NewEmail { get; set; }
        [StringLength(100), DataType(DataType.Password)] public string? Password { get; set; }
        [StringLength(16)] public string? Factor { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        Load(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync(CancellationToken token)
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        Load(user);
        var newAddress = Input.NewEmail?.Trim();
        var password = Input.Password;
        var factor = Input.Factor;
        var valid = ModelState.IsValid;
        Input.Password = Input.Factor = null;
        ModelState.Remove("Input.Password");
        ModelState.Remove("Input.Factor");
        if (!valid || string.IsNullOrEmpty(newAddress) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(string.Empty, "Enter a new email address and verify your credentials.");
            return Page();
        }
        if (string.Equals(newAddress, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Your email address is already current.";
            return RedirectToPage();
        }
        if (!await recovery.AllowAsync(HttpContext, "profile-email", user.Id, false, token))
            return StatusCode(429);
        if (await users.IsLockedOutAsync(user) || !await users.CheckPasswordAsync(user, password) ||
            (user.TwoFactorEnabled && !await AccountRecovery.VerifyFactorAsync(users, user, factor, false)))
        {
            ModelState.AddModelError(string.Empty, "Verify your current password and authenticator.");
            return Page();
        }
        var code = await users.GenerateChangeEmailTokenAsync(user, newAddress);
        var path = links.Link(Url, "/Account/ConfirmEmailChange", new
        {
            area = "Identity", userId = user.Id, email = newAddress,
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))
        });
        return await QueueAsync(user, newAddress, path, code, change: true, token);
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync(CancellationToken token)
    {
        var user = await users.GetUserAsync(User);
        if (user?.Email == null) return Challenge();
        if (user.EmailConfirmed) return RedirectToPage();
        if (!await recovery.AllowAsync(HttpContext, "verify-email", user.Id, true, token)) return StatusCode(429);
        var code = await users.GenerateEmailConfirmationTokenAsync(user);
        var path = links.Link(Url, "/Account/ConfirmEmail", new
        {
            area = "Identity", userId = user.Id, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))
        });
        return await QueueAsync(user, user.Email, path, code, change: false, token);
    }

    private void Load(ApplicationUser user)
    {
        Email = user.Email;
        IsEmailConfirmed = user.EmailConfirmed;
        RequiresAuthenticator = user.TwoFactorEnabled;
    }

    private async Task<IActionResult> QueueAsync(ApplicationUser user, string address, string? link, string code,
        bool change, CancellationToken token)
    {
        if (link == null || !(await smtp.LoadAsync(token)).Ready)
        {
            StatusMessage = "Email verification is currently unavailable. Contact your administrator.";
            return RedirectToPage();
        }
        var title = change ? "Confirm your new IDS email address" : "Verify your IDS email address";
        var now = DateTime.UtcNow;
        email.Enqueue(title, code, new(address, IncidentWorkflow.DisplayName(user), title,
            "Verify the address using this secure link within 24 hours.", now, "Verify email address", link),
            now.AddHours(24));
        await db.SaveChangesAsync(token);
        StatusMessage = "Verification email queued. Check your inbox for the link.";
        return RedirectToPage();
    }
}
