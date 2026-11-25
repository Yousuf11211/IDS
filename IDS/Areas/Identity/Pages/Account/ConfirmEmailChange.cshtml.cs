using System.Text;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ConfirmEmailChangeModel(UserManager<ApplicationUser> users, ApplicationDbContext db,
    AccountEmailQueue queue) : PageModel
{
    public string? StatusMessage { get; private set; }
    public bool Confirmable { get; private set; }

    public void OnGet(string? userId, string? email, string? code)
        => Confirmable = Valid(userId, email, code);

    public async Task<IActionResult> OnPostAsync(string? userId, string? email, string? code, CancellationToken token)
    {
        if (!Valid(userId, email, code)) return Invalid();
        string decoded;
        try { decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code!)); }
        catch (FormatException) { return Invalid(); }
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await AccountRecovery.LockPasswordAsync(db, token);
        var user = await users.FindByIdAsync(userId!);
        if (user == null || await users.IsInRoleAsync(user, AppRoles.Suspended)) return Invalid();
        var oldEmail = user.EmailConfirmed ? user.Email : null;
        var result = await users.ChangeEmailAsync(user, email!, decoded);
        if (!result.Succeeded || !(await users.SetUserNameAsync(user, email!)).Succeeded ||
            !(await users.UpdateSecurityStampAsync(user)).Succeeded) return Invalid();

        const string message = "The registered email for your IDS account changed. Previously authenticated sessions were revoked.";
        queue.Confirm(user, "Registered email changed", message);
        if (!string.IsNullOrEmpty(oldEmail) && !string.Equals(oldEmail, email, StringComparison.OrdinalIgnoreCase))
            queue.Confirm(user, "Registered email changed", message, oldEmail, "previous-address");
        AccountRecovery.Audit(db, user, "Security.EmailChanged");
        AccountRecovery.NotifyUser(db, user, "Registered email changed", message);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        StatusMessage = "Your email is confirmed. Sign in again with your updated address.";
        return Page();
    }

    private static bool Valid(string? userId, string? email, string? code)
        => userId?.Length is > 0 and <= 450 && email?.Length is > 0 and <= 256 && code?.Length is > 0 and <= 4096;

    private PageResult Invalid()
    {
        StatusMessage = "This email change link is invalid or expired.";
        return Page();
    }
}
