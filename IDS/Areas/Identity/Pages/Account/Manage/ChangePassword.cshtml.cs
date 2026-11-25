using System.ComponentModel.DataAnnotations;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class ChangePasswordModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    ApplicationDbContext db, AccountEmailQueue email, AccountRecovery recovery) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }
    public bool RequiresAuthenticator { get; private set; }

    public sealed class InputModel
    {
        [Required, StringLength(100), DataType(DataType.Password), Display(Name = "Current password")]
        public string OldPassword { get; set; } = "";

        [Required, StringLength(100), DataType(DataType.Password), Display(Name = "New password")]
        public string NewPassword { get; set; } = "";

        [Required, Compare(nameof(NewPassword)), DataType(DataType.Password), Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = "";

        [StringLength(16), Display(Name = "Authenticator code")] public string? Factor { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        RequiresAuthenticator = user.TwoFactorEnabled;
        return await users.HasPasswordAsync(user) ? Page() : RedirectToPage("./SetPassword");
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        var id = users.GetUserId(User);
        if (id == null) return Challenge();
        if (!await recovery.AllowAsync(HttpContext, "profile-password", id, false, token)) return StatusCode(429);
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await AccountRecovery.LockPasswordAsync(db, token);
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        RequiresAuthenticator = user.TwoFactorEnabled;
        if (!ModelState.IsValid) return Page();

        var factor = Input.Factor;
        Input.Factor = null;
        ModelState.Remove("Input.Factor");
        if (await users.IsLockedOutAsync(user) ||
            (RequiresAuthenticator && !await AccountRecovery.VerifyFactorAsync(users, user, factor, false)))
        {
            ModelState.AddModelError(string.Empty, "Enter a valid authenticator code.");
            return Page();
        }
        var changed = await users.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changed.Succeeded)
        {
            foreach (var error in changed.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        AccountRecovery.Audit(db, user, "Security.ProfilePasswordChanged");
        email.Confirm(user, "Password changed", "Your password was changed from Profile Settings. Other active sessions were revoked.");
        AccountRecovery.NotifyUser(db, user, "Password changed", "Your password was changed from Profile Settings.");
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        await signIn.RefreshSignInAsync(user);
        StatusMessage = "Your password has been changed.";
        return RedirectToPage();
    }
}
