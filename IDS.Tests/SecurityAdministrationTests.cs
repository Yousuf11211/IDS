using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;

namespace IDS.Tests;

public sealed class SecurityAdministrationTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly ApplicationDbContext _database;
    private readonly UserManager<ApplicationUser> _users;
    private readonly SecurityAdministrationService _administration;
    private readonly SecurityPolicyService _policies;
    private readonly TestAuthentication _authentication = new();
    private readonly TestEnvironment _environment = new();

    public SecurityAdministrationTests()
    {
        _connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<IHostEnvironment>(_environment);
        services.AddAuthentication();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()
            .AddSignInManager<AlwaysChallengeSignInManager>();
        services.AddScoped<AccessControlService>();
        services.AddScoped<SecurityPolicyService>();
        services.AddScoped<SecurityAdministrationService>();
        services.AddScoped<RealtimeSessionValidator>();
        services.AddScoped<AdminVerification>();
        services.AddSingleton<IAuthenticationService>(_authentication);
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _database = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _database.Database.EnsureCreated();
        _users = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _administration = _scope.ServiceProvider.GetRequiredService<SecurityAdministrationService>();
        _policies = _scope.ServiceProvider.GetRequiredService<SecurityPolicyService>();
    }

    [Fact]
    public async Task MessagingRequiresAnIndependentAdminAndAppliesOnlyOnce()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableMessaging, null,
            "Approved internal thesis testing", null);
        Assert.False((await _policies.GetAsync()).MessagingEnabled);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(first.Id, request.Id, true, null));
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(employee.Id, request.Id, true, null));
        Assert.False((await _policies.GetAsync()).MessagingEnabled);

        await _administration.ReviewAsync(second.Id, request.Id, true, null);
        Assert.True((await _policies.GetAsync()).MessagingEnabled);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(second.Id, request.Id, true, null));
        Assert.Single(await _database.AuditLogs.Where(log => log.Action == "Security.Applied").ToListAsync());
    }

    [Fact]
    public async Task DisableCancelsPendingEnableAndCannotBeReversedByStaleApproval()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableMessaging, null,
            "Prepare messaging rollout", null);
        await _administration.DisableFeatureAsync(second.Id, SecurityPolicyService.MessagingKey,
            "Pause rollout during incident investigation", null);
        Assert.False((await _policies.GetAsync()).MessagingEnabled);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(second.Id, request.Id, true, null));
    }

    [Fact]
    public async Task AuthenticatorRecoveryInvalidatesTheOldKeyCodesAndSessions()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        await _users.ResetAuthenticatorKeyAsync(employee);
        await _users.GenerateNewTwoFactorRecoveryCodesAsync(employee, 10);
        var oldKey = await _users.GetAuthenticatorKeyAsync(employee);
        var oldStamp = employee.SecurityStamp;
        var request = await _administration.RequestAsync(first.Id, SecurityActions.ResetAuthenticator,
            employee.Id, "Identity verified against recovery ticket 123", null);
        await _administration.ReviewAsync(second.Id, request.Id, true, null);
        Assert.False(employee.TwoFactorEnabled);
        Assert.NotEqual(oldKey, await _users.GetAuthenticatorKeyAsync(employee));
        Assert.Equal(0, await _users.CountRecoveryCodesAsync(employee));
        Assert.NotEqual(oldStamp, employee.SecurityStamp);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.RequestAsync(first.Id,
            SecurityActions.ResetAuthenticator, second.Id, "Attempt administrator recovery", null));
    }

    [Fact]
    public async Task RevokingRequesterSessionsInvalidatesTheirPendingApprovals()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableMessaging, null,
            "Prepare internal messaging", null);
        await _administration.RevokeSessionsAsync(second.Id, first.Id, "Investigating stolen administrator session", null);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(second.Id, request.Id, true, null));
        Assert.False((await _policies.GetAsync()).MessagingEnabled);
    }

    [Fact]
    public async Task ExpiredRequestAndUnenrolledAdministratorCannotChangePolicy()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableMessaging, null,
            "Prepare internal messaging", null);
        request.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1);
        await _database.SaveChangesAsync();
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(second.Id, request.Id, true, null));
        await _users.SetTwoFactorEnabledAsync(second, false);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.DisableFeatureAsync(second.Id,
            SecurityPolicyService.InvitationsKey, "Try changing security policy", null));
    }

    [Fact]
    public async Task OrdinaryRoleManagementCannotGrantOrRemoveAdminAccess()
    {
        var admin = await AddUserAsync("admin", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var access = _scope.ServiceProvider.GetRequiredService<AccessControlService>();
        await Assert.ThrowsAsync<SecurityChangeException>(() => access.SetExclusiveRoleAsync(_users, employee, AppRoles.Admin));
        await Assert.ThrowsAsync<SecurityChangeException>(() => access.SetExclusiveRoleAsync(_users, admin, AppRoles.Employee));
        Assert.False(await _users.IsInRoleAsync(employee, AppRoles.Admin));
        Assert.True(await _users.IsInRoleAsync(admin, AppRoles.Admin));
    }

    [Fact]
    public async Task PromotionRequiresTwoAdminsAndRevokesExistingEmployeeSessions()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var oldStamp = employee.SecurityStamp;
        var request = await _administration.RequestAsync(first.Id, SecurityActions.GrantAdmin, employee.Id,
            "Approved promotion to security administrator", null);
        Assert.False(await _users.IsInRoleAsync(employee, AppRoles.Admin));
        await _administration.ReviewAsync(second.Id, request.Id, true, null);
        Assert.True(await _users.IsInRoleAsync(employee, AppRoles.Admin));
        Assert.False(await _users.IsInRoleAsync(employee, AppRoles.Employee));
        Assert.NotEqual(oldStamp, employee.SecurityStamp);
    }

    private async Task<ApplicationUser> AddUserAsync(string name, string role)
    {
        await _scope.ServiceProvider.GetRequiredService<AccessControlService>().EnsureRoleExistsAsync(role);
        var user = new ApplicationUser { UserName = name, Email = name + "@example.test", EmailConfirmed = true, TwoFactorEnabled = true };
        Assert.True((await _users.CreateAsync(user, "ExamplePassword1!")).Succeeded);
        Assert.True((await _users.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    [Fact]
    public async Task TestingBypassSkipsEmployeeCodesButAlwaysChallengesAdministrators()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        await _users.ResetAuthenticatorKeyAsync(first);
        await _users.ResetAuthenticatorKeyAsync(employee);
        var key = await _users.GetAuthenticatorKeyAsync(employee);
        var manager = _scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        manager.Context = new DefaultHttpContext { RequestServices = _scope.ServiceProvider };
        Assert.True(await manager.IsTwoFactorEnabledAsync(employee));

        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableEmployeeMfaBypass, null,
            "Exercise employee pages during local testing", null);
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(first.Id, request.Id, true, null));
        Assert.False((await _policies.GetAsync()).EmployeeMfaTestBypass);
        await _administration.ReviewAsync(second.Id, request.Id, true, null);

        Assert.False((await manager.PasswordSignInAsync(employee, "WrongPassword1!", false, true)).Succeeded);
        var result = await manager.PasswordSignInAsync(employee, "ExamplePassword1!", false, true);
        Assert.True(result.Succeeded);
        Assert.True(_authentication.Ticket!.Principal.HasClaim(SecurityPolicyService.BypassedMfaClaim, "true"));
        Assert.True(employee.TwoFactorEnabled);
        Assert.Equal(key, await _users.GetAuthenticatorKeyAsync(employee));
        Assert.False(await _policies.CanBypassTwoFactorAsync(first));
        Assert.True(await manager.IsTwoFactorEnabledAsync(first));
        Assert.True((await manager.PasswordSignInAsync(first, "ExamplePassword1!", false, true)).RequiresTwoFactor);
    }

    [Fact]
    public async Task EndingTestingRejectsExistingBypassedSessionsEvenForEnrolledEmployees()
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableEmployeeMfaBypass, null,
            "Exercise live dashboard during local testing", null);
        await _administration.ReviewAsync(second.Id, request.Id, true, null);
        var principal = PrincipalFor(employee);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(SecurityPolicyService.BypassedMfaClaim, "true"));
        var session = new RealtimeSession(new TestHubContext(principal), false);
        var validator = _scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>();
        Assert.True(await validator.IsAllowedAsync(session));
        await _administration.DisableFeatureAsync(first.Id, SecurityPolicyService.EmployeeMfaBypassKey,
            "Local testing is complete", null);
        Assert.False(await validator.IsAllowedAsync(session));
        Assert.Single(await validator.GetRejectedSessionsAsync(new[] { session }, default));
    }

    [Fact]
    public async Task RefreshingACookieCannotRemoveItsTestingMarkerAfterBypassEnds()
    {
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var principal = PrincipalFor(employee);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(SecurityPolicyService.BypassedMfaClaim, "true"));
        var manager = _scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        manager.Context = new DefaultHttpContext { RequestServices = _scope.ServiceProvider, User = principal };
        // A policy change can race with a profile update that refreshes a login cookie.
        Assert.False((await _policies.GetAsync()).EmployeeMfaTestBypass);
        await manager.SignInWithClaimsAsync(employee, new AuthenticationProperties(), Array.Empty<Claim>());
        Assert.True(_authentication.Ticket!.Principal.HasClaim(SecurityPolicyService.BypassedMfaClaim, "true"));
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task TestingBypassIsIgnoredAndCannotBeApprovedOutsideDevelopment(string environment)
    {
        var first = await AddUserAsync("first", AppRoles.Admin);
        var second = await AddUserAsync("second", AppRoles.Admin);
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var request = await _administration.RequestAsync(first.Id, SecurityActions.EnableEmployeeMfaBypass, null,
            "Exercise employee login during local testing", null);
        _environment.EnvironmentName = environment;
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.ReviewAsync(second.Id, request.Id, true, null));
        await Assert.ThrowsAsync<SecurityChangeException>(() => _administration.RequestAsync(second.Id,
            SecurityActions.EnableEmployeeMfaBypass, null, "Try enabling bypass outside development", null));
        _database.SystemSettings.Add(new SystemSetting { Key = SecurityPolicyService.EmployeeMfaBypassKey, Value = "True" });
        await _database.SaveChangesAsync();
        Assert.False((await _policies.GetAsync()).EmployeeMfaTestBypass);
        Assert.False(await _policies.CanBypassTwoFactorAsync(employee));
        var principal = PrincipalFor(employee);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(SecurityPolicyService.BypassedMfaClaim, "true"));
        var session = new RealtimeSession(new TestHubContext(principal), false);
        Assert.False(await _scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>().IsAllowedAsync(session));
    }

    [Fact]
    public async Task UnenrolledEmployeesCanTestButUnenrolledAdministratorsCannot()
    {
        var employee = await AddUserAsync("employee", AppRoles.Employee);
        var admin = await AddUserAsync("admin", AppRoles.Admin);
        await _users.SetTwoFactorEnabledAsync(employee, false);
        await _users.SetTwoFactorEnabledAsync(admin, false);
        _database.SystemSettings.Add(new SystemSetting { Key = SecurityPolicyService.EmployeeMfaBypassKey, Value = "True" });
        await _database.SaveChangesAsync();
        Assert.True(await _policies.CanBypassTwoFactorAsync(employee));
        Assert.False(await _policies.CanBypassTwoFactorAsync(admin));
        var employeeSession = new RealtimeSession(new TestHubContext(PrincipalFor(employee)), false);
        var adminSession = new RealtimeSession(new TestHubContext(PrincipalFor(admin)), false);
        var validator = _scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>();
        Assert.True(await validator.IsAllowedAsync(employeeSession));
        Assert.False(await validator.IsAllowedAsync(adminSession));
        Assert.Equal(new[] { adminSession }, await validator.GetRejectedSessionsAsync(new[] { employeeSession, adminSession }, default));
    }

    [Fact]
    public async Task RealtimeAccessRejectsRevokedSessionsAndDisabledMessaging()
    {
        var user = await AddUserAsync("employee", AppRoles.Employee);
        var context = new TestHubContext(PrincipalFor(user));
        var dashboard = new RealtimeSession(context, false);
        var chat = new RealtimeSession(context, true);
        var validator = _scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>();
        Assert.True(await validator.IsAllowedAsync(dashboard));
        Assert.False(await validator.IsAllowedAsync(chat));
        Assert.Equal(new[] { chat }, await validator.GetRejectedSessionsAsync(new[] { dashboard, chat }, default));

        await _users.UpdateSecurityStampAsync(user);
        Assert.False(await validator.IsAllowedAsync(dashboard));
        Assert.Equal(2, (await validator.GetRejectedSessionsAsync(new[] { dashboard, chat }, default)).Length);
    }

    [Theory]
    [InlineData("locked")]
    [InlineData("suspended")]
    [InlineData("unenrolled")]
    public async Task IdleConnectionsLoseAccessWhenAnAccountIsRestricted(string restriction)
    {
        var user = await AddUserAsync("employee", AppRoles.Employee);
        if (restriction == "locked") await _users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(10));
        if (restriction == "unenrolled") await _users.SetTwoFactorEnabledAsync(user, false);
        if (restriction == "suspended")
        {
            await _scope.ServiceProvider.GetRequiredService<AccessControlService>().EnsureRoleExistsAsync(AppRoles.Suspended);
            await _users.AddToRoleAsync(user, AppRoles.Suspended);
        }
        var session = new RealtimeSession(new TestHubContext(PrincipalFor(user)), false);
        var validator = _scope.ServiceProvider.GetRequiredService<RealtimeSessionValidator>();
        Assert.False(await validator.IsAllowedAsync(session));
        Assert.Single(await validator.GetRejectedSessionsAsync(new[] { session }, default));
    }

    [Fact]
    public async Task AdminVerificationExpiresAndIsBoundToTheAccountCredentials()
    {
        var user = await AddUserAsync("admin", AppRoles.Admin);
        var context = new DefaultHttpContext { RequestServices = _scope.ServiceProvider, User = PrincipalFor(user) };
        _authentication.Ticket = new AuthenticationTicket(context.User, new AuthenticationProperties(), IdentityConstants.ApplicationScheme);
        var verification = _scope.ServiceProvider.GetRequiredService<AdminVerification>();
        Assert.False(await verification.IsRecentAsync(context));
        await verification.MarkVerifiedAsync(context, user);
        Assert.True(await verification.IsRecentAsync(context));
        _authentication.Ticket.Properties.Items["ids.adminVerifiedAt"] = DateTimeOffset.UtcNow.AddMinutes(-6).ToString("O");
        Assert.False(await verification.IsRecentAsync(context));
        await verification.MarkVerifiedAsync(context, user);
        await _users.UpdateSecurityStampAsync(user);
        Assert.False(await verification.IsRecentAsync(context));
    }

    private ClaimsPrincipal PrincipalFor(ApplicationUser user) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(_scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value.ClaimsIdentity.SecurityStampClaimType,
            user.SecurityStamp!)
    }, IdentityConstants.ApplicationScheme));

    private sealed class TestHubContext(ClaimsPrincipal principal) : HubCallerContext
    {
        public override string ConnectionId { get; } = Guid.NewGuid().ToString();
        public override string? UserIdentifier => principal.FindFirstValue(ClaimTypes.NameIdentifier);
        public override ClaimsPrincipal User => principal;
        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override CancellationToken ConnectionAborted => CancellationToken.None;
        public override void Abort() { }
    }

    private sealed class TestAuthentication : IAuthenticationService
    {
        public AuthenticationTicket? Ticket { get; set; }
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
            Task.FromResult(Ticket == null ? AuthenticateResult.NoResult() : AuthenticateResult.Success(Ticket));
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            Ticket = new AuthenticationTicket(principal, properties!, scheme!);
            return Task.CompletedTask;
        }
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "IDS.Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    public void Dispose() { _scope.Dispose(); _provider.Dispose(); _connection.Dispose(); }
}
