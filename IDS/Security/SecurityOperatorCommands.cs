using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace IDS.Security;

// These recovery commands require access to the deployment host. No HTTP endpoint calls them.
public static class SecurityOperatorCommands
{
    public static async Task RunAsync(string[] args, IServiceProvider services)
    {
        var prefixes = new[] { "--provision-admin=", "--recover-admin=", "--suspend-admin=" };
        var commands = args.Where(argument => prefixes.Any(prefix => argument.StartsWith(prefix, StringComparison.Ordinal))).ToArray();
        var reason = args.FirstOrDefault(argument => argument.StartsWith("--reason=", StringComparison.Ordinal))?[9..];
        if (commands.Length != 1 || string.IsNullOrWhiteSpace(reason) || reason.Length < 10 || reason.Length > 1000)
            throw new InvalidOperationException("Specify one administrator command and --reason=<10 to 1000 characters>.");
        var command = commands[0];
        var email = command[(command.IndexOf('=') + 1)..];
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var database = services.GetRequiredService<ApplicationDbContext>();
        var access = services.GetRequiredService<AccessControlService>();
        await using var transaction = await database.Database.BeginTransactionAsync();
        var user = await users.FindByEmailAsync(email) ?? throw new InvalidOperationException("The account does not exist.");

        if (command.StartsWith(prefixes[0], StringComparison.Ordinal))
        {
            if (!user.EmailConfirmed || !user.TwoFactorEnabled || user.MustChangePassword ||
                await users.IsLockedOutAsync(user) || await users.IsInRoleAsync(user, AppRoles.Suspended))
                throw new InvalidOperationException("The account must be active and complete email/password/authenticator setup first.");
            await access.SetExclusiveRoleAsync(users, user, AppRoles.Admin, allowAdministratorChange: true);
        }
        else
        {
            if (!await users.IsInRoleAsync(user, AppRoles.Admin))
                throw new InvalidOperationException("The target must be an administrator.");
            if (command.StartsWith(prefixes[1], StringComparison.Ordinal))
            {
                RequireSuccess(await users.SetTwoFactorEnabledAsync(user, false));
                RequireSuccess(await users.ResetAuthenticatorKeyAsync(user));
                await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);
            }
            else
            {
                await access.SetExclusiveRoleAsync(users, user, AppRoles.Suspended, allowAdministratorChange: true);
            }
        }
        RequireSuccess(await users.UpdateSecurityStampAsync(user));
        database.AuditLogs.Add(new AuditLog
        {
            UserId = "deployment-operator", UserEmail = Environment.UserName,
            Action = command.StartsWith(prefixes[0], StringComparison.Ordinal) ? "Security.OperatorProvision" :
                command.StartsWith(prefixes[1], StringComparison.Ordinal) ? "Security.OperatorRecovery" : "Security.OperatorSuspension",
            EntityType = "User", EntityId = user.Id, Details = reason, Timestamp = DateTime.UtcNow
        });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        services.GetRequiredService<ILoggerFactory>().CreateLogger("SecurityOperator")
            .LogWarning("Deployment operator completed an administrator maintenance action for {UserId}", user.Id);
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("The account changed. Reload and retry the maintenance command.");
    }
}
