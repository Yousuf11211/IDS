using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IDS.Security;

// These recovery commands require access to the deployment host. No HTTP endpoint calls them.
public static class SecurityOperatorCommands
{
    public static async Task RunAsync(string[] args, IServiceProvider services)
    {
        var prefixes = new[] { "--provision-admin=", "--recover-admin=", "--suspend-admin=", "--recover-account=" };
        var commands = args.Where(argument => prefixes.Any(prefix => argument.StartsWith(prefix, StringComparison.Ordinal))).ToArray();
        var reason = args.FirstOrDefault(argument => argument.StartsWith("--reason=", StringComparison.Ordinal))?[9..];
        if (commands.Length != 1 || string.IsNullOrWhiteSpace(reason) || reason.Length < 10 || reason.Length > 1000)
            throw new InvalidOperationException("Specify one account maintenance command and --reason=<10 to 1000 characters>.");
        var command = commands[0];
        var email = command[(command.IndexOf('=') + 1)..];
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var database = services.GetRequiredService<ApplicationDbContext>();
        var access = services.GetRequiredService<AccessControlService>();
        await using var transaction = await database.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var recoveringAccount = command.StartsWith(prefixes[3], StringComparison.Ordinal);
        if (recoveringAccount)
        {
            await DatabaseLock.AcquireAsync(database, AccountRecovery.RequestLock, CancellationToken.None);
            await AccountRecovery.LockPasswordAsync(database, CancellationToken.None);
        }
        var user = await users.FindByEmailAsync(email) ?? throw new InvalidOperationException("The account does not exist.");

        if (recoveringAccount)
        {
            await RecoverAccountAsync(args, services, database, users, user, reason);
            await transaction.CommitAsync();
            return;
        }

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

    private static async Task RecoverAccountAsync(string[] args, IServiceProvider services,
        ApplicationDbContext database, UserManager<ApplicationUser> users, ApplicationUser user, string reason)
    {
        if (!long.TryParse(args.FirstOrDefault(value => value.StartsWith("--request=", StringComparison.Ordinal))?[10..],
                out var requestId) || requestId <= 0)
            throw new InvalidOperationException("Provide the ID of an approved recovery request with --request=<id>.");
        if (await users.IsInRoleAsync(user, AppRoles.Admin) || await users.IsInRoleAsync(user, AppRoles.Suspended) ||
            !(await users.IsInRoleAsync(user, AppRoles.Employee) || await users.IsInRoleAsync(user, AppRoles.Support)))
            throw new InvalidOperationException("This command is for active employees and support accounts.");
        var request = await database.AuthenticatorResetRequests.SingleOrDefaultAsync(item => item.Id == requestId && item.UserId == user.Id);
        if (request == null || request.Status != "Approved" || request.ExpiresAt <= DateTime.UtcNow ||
            request.SecurityStamp != user.SecurityStamp || request.VerifiedEmail != user.Email ||
            request.ReviewerId == null || request.ReviewerId == user.Id || string.IsNullOrWhiteSpace(request.IdentityVerification) ||
            !user.EmailConfirmed || !user.TwoFactorEnabled || user.MustChangePassword)
            throw new InvalidOperationException("The approved request and account must still match with current verification.");

        // Revoke both credentials. The verified setup link must be reissued by an
        // administrator through the existing invitation workflow.
        var unusable = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)) + "aA1!";
        var token = await users.GeneratePasswordResetTokenAsync(user);
        user.MustChangePassword = true;
        user.EmailConfirmed = false;
        RequireSuccess(await users.ResetPasswordAsync(user, token, unusable));
        RequireSuccess(await users.SetTwoFactorEnabledAsync(user, false));
        RequireSuccess(await users.ResetAuthenticatorKeyAsync(user));
        if (await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 0) == null)
            throw new InvalidOperationException("The old recovery codes could not be revoked.");
        RequireSuccess(await users.UpdateSecurityStampAsync(user));
        request.Status = "Cancelled";
        services.GetRequiredService<AccountEmailQueue>().Confirm(user, "Operator account recovery",
            "A verified support case revoked your old credentials. An administrator will reissue the account setup link.",
            request.VerifiedEmail, request.Id.ToString());
        database.AuditLogs.Add(new AuditLog
        {
            UserId = "deployment-operator", UserEmail = Environment.UserName,
            Action = "Security.OperatorAccountRecovery", EntityType = nameof(AuthenticatorResetRequest),
            EntityId = request.Id.ToString(), Details = reason
        });
        await database.SaveChangesAsync();
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("The account changed. Reload and retry the maintenance command.");
    }
}
