using IDS.Data;
using IDS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IDS.Security;

public sealed record SecurityPolicy(bool MessagingEnabled, bool EmployeeInvitationsEnabled, bool EmployeeMfaTestBypass = false);

public sealed class SecurityPolicyService(ApplicationDbContext database, IConfiguration configuration, IHostEnvironment environment)
{
    public const string MessagingKey = "Security.MessagingEnabled";
    public const string InvitationsKey = "Security.EmployeeInvitationsEnabled";
    public const string EmployeeMfaBypassKey = "Security.EmployeeMfaTestBypass";
    public const string BypassedMfaClaim = "ids.employeeMfaTestBypass";
    public bool AllowsTestingBypass => environment.IsDevelopment();

    public async Task<bool> CanBypassTwoFactorAsync(ApplicationUser user)
    {
        if (!(await GetAsync()).EmployeeMfaTestBypass) return false;
        return !await (from membership in database.UserRoles
                       join role in database.Roles on membership.RoleId equals role.Id
                       where membership.UserId == user.Id && role.Name == AppRoles.Admin
                       select membership.UserId).AnyAsync();
    }

    public async Task<SecurityPolicy> GetAsync()
    {
        var settings = await database.SystemSettings.AsNoTracking()
            .Where(setting => setting.Key == MessagingKey || setting.Key == InvitationsKey || setting.Key == EmployeeMfaBypassKey)
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value);
        // Invalid persisted values deny access instead of silently enabling a feature.
        bool Read(string key, bool fallback) => settings.TryGetValue(key, out var value)
            ? bool.TryParse(value, out var enabled) && enabled : fallback;
        return new SecurityPolicy(
            Read(MessagingKey, configuration.GetValue<bool>("Chat:Enabled")),
            Read(InvitationsKey, true),
            AllowsTestingBypass && Read(EmployeeMfaBypassKey, false));
    }
}
