using IDS.Areas.Identity.Pages.Account;
using IDS.Data;
using IDS.Core.Services;
using IDS.Data.Services;
using IDS.Security;
using IDS.Pages.Admin;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IDS.Tests;

public sealed class AccountSetupTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly UserManager<ApplicationUser> _users;

    public AccountSetupTests()
    {
        _connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Application:PublicBaseUrl"] = "https://ids.example.test" }).Build());
        services.AddSingleton<IWebHostEnvironment>(new TestWebEnvironment());
        services.AddSingleton<AccountLinkBuilder>();
        services.AddScoped<EmailConfiguration>();
        services.AddScoped<MailjetEmailSender>();
        services.AddScoped<AccountEmailQueue>();
        services.AddScoped<WorkerHealthService>();
        services.AddScoped<SecurityAlertService>();
        services.AddScoped<AccountRecovery>();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
        _users = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }

    [Fact]
    public async Task ValidSetupLinkConfirmsEmailAndCompletesPasswordStep()
    {
        var user = await AddInvitedUserAsync();
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var page = NewResetPage(token);

        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());

        var updated = await _users.FindByIdAsync(user.Id);
        Assert.True(updated!.EmailConfirmed);
        Assert.False(updated.MustChangePassword);
        Assert.True(await _users.CheckPasswordAsync(updated, "NewPassword1!"));
    }

    [Fact]
    public async Task InvalidSetupLinkLeavesAccountPending()
    {
        var user = await AddInvitedUserAsync();
        var page = NewResetPage("invalid-token");

        Assert.IsType<PageResult>(await page.OnPostAsync());

        var updated = await _users.FindByIdAsync(user.Id);
        Assert.False(updated!.EmailConfirmed);
        Assert.True(updated.MustChangePassword);
        Assert.False(await _users.CheckPasswordAsync(updated, "NewPassword1!"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SelfServiceResetValidatesFactorAndInvalidatesOtherLinks(bool enrolled)
    {
        await ConfigureEmailAsync();
        var user = await AddActiveUserAsync("reset", enrolled);
        var service = _scope.ServiceProvider.GetRequiredService<AccountRecovery>();
        var code = service.Issue(AccountRecovery.SelfPassword,
            AccountRecovery.For(user, await _users.GeneratePasswordResetTokenAsync(user)), TimeSpan.FromMinutes(10));
        var originalStamp = user.SecurityStamp;
        var model = NewResetPage(code);
        model.Input.Mode = AccountRecovery.SelfPassword;
        model.Input.Email = user.Email!;
        if (enrolled)
        {
            model.Input.Factor = "invalid";
            Assert.IsType<PageResult>(await model.OnPostAsync());
            Assert.Equal(originalStamp, user.SecurityStamp);
            model.ModelState.Clear();
            model.Input.Factor = SecurityAdministrationTests.CurrentTotp((await _users.GetAuthenticatorKeyAsync(user))!);
        }
        Assert.IsType<RedirectToPageResult>(await model.OnPostAsync());
        Assert.NotEqual(originalStamp, user.SecurityStamp);
        Assert.True(await _users.CheckPasswordAsync(user, "NewPassword1!"));
        model.ModelState.Clear();
        Assert.IsType<PageResult>(await model.OnPostAsync());
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var message = Assert.Single(await db.SecurityEmails.ToListAsync());
        Assert.Equal("Password reset completed", message.EventType);
        Assert.NotNull(message.ProtectedPayload);
        Assert.DoesNotContain(user.Email!, message.ProtectedPayload!);
    }

    [Fact]
    public async Task EmailVerificationApprovalAndReplacementAreSingleUse()
    {
        await ConfigureEmailAsync();
        var user = await AddActiveUserAsync("recover", true, AppRoles.Admin);
        var admin = await AddActiveUserAsync("review", true, AppRoles.Admin);
        var employee = await AddActiveUserAsync("employee", true, AppRoles.Employee);
        var service = _scope.ServiceProvider.GetRequiredService<AccountRecovery>();
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proof = service.Issue(AccountRecovery.VerifyEmail, AccountRecovery.For(user), TimeSpan.FromMinutes(10));
        var verified = service.Read(AccountRecovery.VerifyEmail, proof)!;
        Assert.Null(service.Read(AccountRecovery.ApproveEnrollment, proof));
        Assert.Null(service.Read(AccountRecovery.VerifyEmail,
            service.Issue(AccountRecovery.VerifyEmail, verified, TimeSpan.FromSeconds(-5))));
        Assert.True(await service.SubmitAsync(verified, proof, "Authenticator phone was lost.", default));
        Assert.False(await service.SubmitAsync(verified, proof, "Repeat request.", default));
        var request = await db.AuthenticatorResetRequests.SingleAsync();
        Assert.Contains(await db.InboxNotifications.ToListAsync(), n => n.RecipientId == admin.Id && n.AuthenticatorResetRequestId == request.Id);
        Assert.Contains("different administrator", await service.ReviewAsync(user, request.Id, true, "Verified", "In person", new TestUrl(), default));
        Assert.Contains("required", await service.ReviewAsync(employee, request.Id, true, "Verified", "In person", new TestUrl(), default));
        var oldKey = await _users.GetAuthenticatorKeyAsync(user);
        var oldCodes = (await _users.GenerateNewTwoFactorRecoveryCodesAsync(user, 2))!.ToArray();
        var oldStamp = user.SecurityStamp;
        var url = new TestUrl();
        Assert.Contains("approved", await service.ReviewAsync(admin, request.Id, true, "Verified", "In person", url, default));
        Assert.Contains("no longer pending", await service.ReviewAsync(admin, request.Id, true, "Repeat", "In person", url, default));
        Assert.Equal(oldStamp, user.SecurityStamp);
        Assert.Equal(oldKey, await _users.GetAuthenticatorKeyAsync(user));
        var approval = service.Read(AccountRecovery.ApproveEnrollment, url.Code)!;
        var candidate = _users.GenerateNewAuthenticatorKey();
        Assert.Null(await service.ReplaceAsync(approval, candidate, "WrongPassword1!", "123456", default));
        Assert.Null(await service.ReplaceAsync(approval, candidate, "InitialPassword1!", "invalid", default));
        db.ChangeTracker.Clear();
        Assert.Equal(oldKey, await _users.GetAuthenticatorKeyAsync((await _users.FindByIdAsync(user.Id))!));
        db.ChangeTracker.Clear();
        var codes = await service.ReplaceAsync(approval, candidate, "InitialPassword1!", SecurityAdministrationTests.CurrentTotp(candidate), default);
        Assert.Equal(10, codes!.Length);
        var changed = (await _users.FindByIdAsync(user.Id))!;
        Assert.True(changed.TwoFactorEnabled);
        Assert.Equal(candidate, await _users.GetAuthenticatorKeyAsync(changed));
        Assert.NotEqual(oldStamp, changed.SecurityStamp);
        Assert.False((await _users.RedeemTwoFactorRecoveryCodeAsync(changed, oldCodes[0])).Succeeded);
        Assert.Null(await service.EnrollmentUserAsync(approval, default));
        Assert.Equal("Completed", (await db.AuthenticatorResetRequests.SingleAsync()).Status);
        Assert.Equal(1, await db.SecurityEmails.CountAsync(e => e.EventType == "Authenticator replaced"));
    }

    [Fact]
    public async Task RejectionPreservesAuthenticatorAndQueuesOutcome()
    {
        await ConfigureEmailAsync();
        var user = await AddActiveUserAsync("rejected", true);
        var admin = await AddActiveUserAsync("reject-admin", true, AppRoles.Admin);
        var originalStamp = user.SecurityStamp;
        var originalKey = await _users.GetAuthenticatorKeyAsync(user);
        var service = _scope.ServiceProvider.GetRequiredService<AccountRecovery>();
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var link = AccountRecovery.For(user);
        var proof = service.Issue(AccountRecovery.VerifyEmail, link, TimeSpan.FromMinutes(12));
        Assert.True(await service.SubmitAsync(link, proof, "Changed mobile phone.", default));
        var request = await db.AuthenticatorResetRequests.SingleAsync();
        var url = new TestUrl();
        Assert.Contains("rejected", await service.ReviewAsync(admin, request.Id, false, "Identity verification incomplete", null, url, default));
        Assert.Equal("Rejected", request.Status);
        Assert.Equal(admin.Id, request.ReviewerId);
        Assert.Null(url.Code);
        Assert.Null(await service.EnrollmentUserAsync(AccountRecovery.For(user, requestId: request.Id), default));
        Assert.Equal(originalStamp, user.SecurityStamp);
        Assert.Equal(originalKey, await _users.GetAuthenticatorKeyAsync(user));
        Assert.Contains(await db.SecurityEmails.ToListAsync(), e => e.EventType == "Authenticator reset rejected");
    }

    [Fact]
    public async Task AcknowledgingLinkingAndResolvingAlertPreservesIncidentWorkflow()
    {
        var admin = await AddActiveUserAsync("alert-admin", true, AppRoles.Admin);
        var service = _scope.ServiceProvider.GetRequiredService<SecurityAlertService>();
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var actor = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, admin.Id), new Claim(ClaimTypes.Name, admin.UserName!),
            new Claim(ClaimTypes.Role, AppRoles.Admin)
        ], "fixture"));
        var context = new PageContext { HttpContext = new DefaultHttpContext { User = actor } };
        var alert = new SecurityAlert { AlertType = "Test event", Message = "Alert workflow fixture.", Severity = "High" };
        db.SecurityAlerts.Add(alert);
        await db.SaveChangesAsync();
        var page = new AlertsModel(db, service) { PageContext = context };
        Assert.IsType<RedirectToPageResult>(await page.OnPostAcknowledgeAsync(alert.Id, default));
        Assert.True(alert.IsAcknowledged);
        Assert.IsType<RedirectToPageResult>(await page.OnPostAcknowledgeAsync(alert.Id, default));
        Assert.Equal(1, await db.AuditLogs.CountAsync(log => log.Action == "AcknowledgeSecurityAlert"));

        var details = new AlertDetailsModel(db, _users) { PageContext = context };
        Assert.IsType<RedirectToPageResult>(await details.OnPostIncidentAsync(alert.Id, null, default));
        var ticket = Assert.Single(await db.SupportTickets.ToListAsync());
        Assert.Equal(ticket.Id, alert.TicketId);
        Assert.Equal("High", ticket.Priority);
        Assert.IsType<RedirectToPageResult>(await details.OnPostResolveAsync(alert.Id, "Review complete", default));
        Assert.Null(alert.ResolvedAt);
        ticket.Status = "Resolved";
        await db.SaveChangesAsync();
        Assert.IsType<RedirectToPageResult>(await details.OnPostResolveAsync(alert.Id, "Linked incident resolved", default));
        Assert.NotNull(alert.ResolvedAt);
        Assert.Equal(admin.Id, alert.ResolvedByUserId);
    }

    [Fact]
    public async Task RecoveryThrottleUsesPersistedAccountLimit()
    {
        var service = _scope.ServiceProvider.GetRequiredService<AccountRecovery>();
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.SystemSettings.Add(new() { Key = RecoverySettings.RequestsPerHour, Value = "1" });
        await db.SaveChangesAsync();
        var context = new DefaultHttpContext();
        Assert.True(await service.AllowAsync(context, "password-request", "Test@example.test", true, default));
        Assert.False(await service.AllowAsync(context, "password-request", "test@example.test", true, default));
        Assert.True(await service.AllowAsync(context, "password-request", "other@example.test", true, default));
        Assert.Equal(3, await db.RecoveryThrottles.CountAsync());
    }

    [Fact]
    public async Task OutboxRetriesWithStableIdentifierAndNotifiesAdministrators()
    {
        await ConfigureEmailAsync();
        var admin = await AddActiveUserAsync("delivery-admin", true, AppRoles.Admin);
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var queue = _scope.ServiceProvider.GetRequiredService<AccountEmailQueue>();
        var smtp = ActivatorUtilities.CreateInstance<SimulatedMailjet>(_scope.ServiceProvider);
        queue.Confirm(admin, "Test security change", "Test queue persistence.");
        await db.SaveChangesAsync();
        for (var attempt = 1; attempt <= 6; attempt++)
        {
            Assert.True(await queue.ProcessAsync(smtp, default));
            var record = await db.SecurityEmails.SingleAsync();
            Assert.Equal(attempt, record.Attempts);
            Assert.NotNull(record.LastAttemptAt);
            Assert.Null(record.AcceptedAt);
            if (attempt < 6)
            {
                Assert.Null(record.FailedAt);
                Assert.True(record.DueAt > DateTime.UtcNow);
                record.DueAt = DateTime.UtcNow.AddMinutes(-1);
                await db.SaveChangesAsync();
            }
        }
        var failed = await db.SecurityEmails.AsNoTracking().SingleAsync();
        Assert.NotNull(failed.FailedAt);
        Assert.Single(await db.InboxNotifications.Where(n => n.RecipientId == admin.Id).ToListAsync());
        Assert.True(await queue.RetryAsync(failed.Id, default));
        db.ChangeTracker.Clear();
        smtp.Accept = true;
        Assert.True(await queue.ProcessAsync(smtp, default));
        Assert.Single(smtp.MessageIds.Distinct());
        var accepted = await db.SecurityEmails.SingleAsync();
        Assert.NotNull(accepted.AcceptedAt);
        Assert.Null(accepted.ProtectedPayload);
        Assert.False(await queue.ProcessAsync(smtp, default));
    }

    public sealed class SimulatedMailjet(Microsoft.Extensions.Logging.ILogger<MailjetEmailSender> logger,
        IWebHostEnvironment environment, EmailConfiguration configuration)
        : MailjetEmailSender(logger, environment, configuration)
    {
        public bool Accept { get; set; }
        public List<string> MessageIds { get; } = [];

        public override Task<(bool Success, string? ErrorMessage)> SendAccountSecurityAsync(AccountEmail payload,
            string messageId, CancellationToken token)
        {
            MessageIds.Add(messageId);
            return Task.FromResult<(bool, string?)>((Accept, Accept ? null : "Simulated provider failure"));
        }
    }

    private async Task ConfigureEmailAsync()
    {
        var settings = _scope.ServiceProvider.GetRequiredService<EmailConfiguration>();
        var db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.SystemSettings.AddRange(new SystemSetting { Key = EmailConfiguration.Enabled, Value = "true" },
            new SystemSetting { Key = EmailConfiguration.ApiKey, Value = settings.Protect("test-only") },
            new SystemSetting { Key = EmailConfiguration.Secret, Value = settings.Protect("test-only") },
            new SystemSetting { Key = EmailConfiguration.SupportEmail, Value = "support@example.test" },
            new SystemSetting { Key = EmailConfiguration.AdminEmail, Value = "admin@example.test" });
        await db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> AddActiveUserAsync(string name, bool enrolled, string? role = null)
    {
        var user = new ApplicationUser { UserName = name + "@example.test", Email = name + "@example.test", EmailConfirmed = true };
        Assert.True((await _users.CreateAsync(user, "InitialPassword1!")).Succeeded);
        if (enrolled)
        {
            Assert.True((await _users.ResetAuthenticatorKeyAsync(user)).Succeeded);
            Assert.True((await _users.SetTwoFactorEnabledAsync(user, true)).Succeeded);
        }
        if (role != null)
        {
            var roles = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roles.RoleExistsAsync(role)) Assert.True((await roles.CreateAsync(new(role))).Succeeded);
            Assert.True((await _users.AddToRoleAsync(user, role)).Succeeded);
        }
        return user;
    }

    private sealed class TestUrl : IUrlHelper
    {
        public string? Code { get; private set; }
        public ActionContext ActionContext { get; } =
            new(new DefaultHttpContext(), new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        public string? Action(UrlActionContext actionContext) => null;
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => true;
        public string? Link(string? routeName, object? values) => null;
        public string? RouteUrl(UrlRouteContext routeContext)
        {
            Code = new RouteValueDictionary(routeContext.Values)["code"]?.ToString();
            return "/Identity/Account/RecoveryEnrollment";
        }
    }

    private async Task<ApplicationUser> AddInvitedUserAsync()
    {
        var user = new ApplicationUser
        {
            UserName = "invite@example.test",
            Email = "invite@example.test",
            EmailConfirmed = false,
            MustChangePassword = true
        };
        var result = await _users.CreateAsync(user, "InitialPassword1!");
        Assert.True(result.Succeeded);
        return user;
    }

    private ResetPasswordModel NewResetPage(string token)
        => WithInput(ActivatorUtilities.CreateInstance<ResetPasswordModel>(_scope.ServiceProvider), token);

    private sealed class TestWebEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "IDS";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = ".";
        public string WebRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static ResetPasswordModel WithInput(ResetPasswordModel page, string token)
    {
        page.PageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        page.Input = new ResetPasswordModel.InputModel
        {
            Email = "invite@example.test",
            Code = token,
            Password = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };
        return page;
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        _connection.Dispose();
    }
}
