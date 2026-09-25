using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace IDS.Security;

// Explicit local maintenance command; normal startup never resets passwords.
public sealed class TestAdministratorSetup(
    ApplicationDbContext database, UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles, IConfiguration configuration, IHostEnvironment environment)
{
    public async Task RunAsync()
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("Test administrator setup is available only in Development mode.");
        var accounts = new[] { "First", "Second" }.Select(name => (
            Email: configuration[$"TestAdministrators:{name}:Email"]?.Trim(),
            Password: configuration[$"TestAdministrators:{name}:Password"])).ToArray();
        if (accounts.Any(account => string.IsNullOrWhiteSpace(account.Email) || string.IsNullOrWhiteSpace(account.Password)) ||
            string.Equals(accounts[0].Email, accounts[1].Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure distinct ADMIN_EMAIL and ADMIN2_EMAIL accounts and both ADMIN_PASSWORD and ADMIN2_PASSWORD.");

        await using var transaction = await database.Database.BeginTransactionAsync();
        if (!await roles.RoleExistsAsync(AppRoles.Admin))
            RequireSuccess(await roles.CreateAsync(new IdentityRole(AppRoles.Admin)));
        foreach (var account in accounts)
        {
            var user = await users.FindByEmailAsync(account.Email!);
            var changed = false;
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = account.Email, Email = account.Email, EmailConfirmed = true,
                    FirstName = "Test", LastName = "Administrator"
                };
                RequireSuccess(await users.CreateAsync(user, account.Password!));
                RequireSuccess(await users.AddToRoleAsync(user, AppRoles.Admin));
                changed = true;
            }
            else
            {
                if (!await users.IsInRoleAsync(user, AppRoles.Admin) || await users.IsInRoleAsync(user, AppRoles.Suspended) ||
                    await users.IsLockedOutAsync(user) || !user.EmailConfirmed || user.MustChangePassword)
                    throw new InvalidOperationException("Test setup cannot promote or restore a restricted existing account.");
                if (!await users.CheckPasswordAsync(user, account.Password!))
                {
                    var token = await users.GeneratePasswordResetTokenAsync(user);
                    RequireSuccess(await users.ResetPasswordAsync(user, token, account.Password!));
                    changed = true;
                }
            }
            if (changed)
            {
                database.AuditLogs.Add(new AuditLog
                {
                    UserId = "deployment-operator", UserEmail = Environment.UserName,
                    Action = "Security.TestAdministratorSetup", EntityType = "User", EntityId = user.Id,
                    Details = "Applied locally configured Development test credentials.", Timestamp = DateTime.UtcNow
                });
            }
        }
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Test administrator setup failed: " + string.Join(", ", result.Errors.Select(error => error.Code)));
    }
}
