using System.Net;
using System.Text.RegularExpressions;
using IDS.Areas.Identity.Pages.Account;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IDS.Tests;

public sealed class LogoutTests : IClassFixture<LogoutTestHost>
{
    private readonly LogoutTestHost _host;
    public LogoutTests(LogoutTestHost host) => _host = host;

    [Fact]
    public async Task ValidLogoutClearsCookieAndProtectedPagesRequireLoginAgain()
    {
        using var client = _host.CreateClient();
        await client.PostAsync("/test/sign-in/admin", null);
        var page = await client.GetAsync("/Identity/Account/Logout?returnUrl=%2F");
        Assert.True(page.Headers.CacheControl?.NoStore);
        var token = Token(await page.Content.ReadAsStringAsync());

        var logout = await client.PostAsync("/Identity/Account/Logout?returnUrl=%2F", Form(token));

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/", logout.Headers.Location?.OriginalString);
        Assert.Contains(logout.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.StartsWith(".AspNetCore.Identity.Application=;") && cookie.Contains("expires="));
        foreach (var path in new[] { "/test/admin", "/Dashboard", "/Admin/Security" })
        {
            var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Identity/Account/Login", response.Headers.Location!.OriginalString);
            Assert.True(response.Headers.CacheControl?.NoStore);
        }
    }

    [Fact]
    public async Task StaleLogoutTokenShowsFreshConfirmationWithoutDisablingCsrfProtection()
    {
        using var client = _host.CreateClient();
        await client.PostAsync("/test/sign-in/admin", null);
        var staleToken = Token(await client.GetStringAsync("/Identity/Account/Logout"));
        // A restored page can still carry the previous login's form token.
        await client.PostAsync("/test/sign-in/other-admin", null);
        var rejected = await client.PostAsync("/Identity/Account/Logout", Form(staleToken));

        Assert.Equal(HttpStatusCode.Redirect, rejected.StatusCode);
        Assert.Contains("retry=True", rejected.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/test/admin")).StatusCode);

        var confirmation = await client.GetStringAsync(rejected.Headers.Location);
        Assert.Contains("You are still signed in", confirmation);
        var completed = await client.PostAsync("/Identity/Account/Logout", Form(Token(confirmation)));
        Assert.Equal(HttpStatusCode.Redirect, completed.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/test/admin")).StatusCode);
    }

    [Fact]
    public async Task LogoutGetDoesNotChangeSessionAndMissingTokenGetsRecoveryPage()
    {
        using var client = _host.CreateClient();
        await client.PostAsync("/test/sign-in/admin", null);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Identity/Account/Logout?handler=Inactivity")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/test/admin")).StatusCode);
        var response = await client.PostAsync("/Identity/Account/Logout", new FormUrlEncodedContent([]));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Logout", response.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/test/admin")).StatusCode);
    }

    [Theory]
    [InlineData("https://example.test/")]
    [InlineData("//example.test/")]
    public async Task UnsafeReturnUrlFallsBackToHomeAfterSuccessfulLogout(string returnUrl)
    {
        using var client = _host.CreateClient();
        await client.PostAsync("/test/sign-in/admin", null);
        var token = Token(await client.GetStringAsync("/Identity/Account/Logout"));
        var response = await client.PostAsync("/Identity/Account/Logout?returnUrl=" + Uri.EscapeDataString(returnUrl), Form(token));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/test/admin")).StatusCode);
    }

    [Fact]
    public async Task InactivityPostSignsOutAndShowsConfirmation()
    {
        using var client = _host.CreateClient();
        await client.PostAsync("/test/sign-in/admin", null);
        var token = Token(await client.GetStringAsync("/Identity/Account/Logout"));
        var response = await client.PostAsync("/Identity/Account/Logout?handler=Inactivity", Form(token));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("You are signed out", await client.GetStringAsync(response.Headers.Location));
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/test/admin")).StatusCode);
    }

    private static string Token(string html) => WebUtility.HtmlDecode(Regex.Match(html,
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    private static FormUrlEncodedContent Form(string token) => new(new Dictionary<string, string>
    {
        ["__RequestVerificationToken"] = token
    });
}

// Real Razor rendering, antiforgery and Identity cookie handling against an isolated database.
// The sign-in shortcuts exist only in this test host; production startup is never run.
public sealed class LogoutTestHost : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private WebApplication _app = null!;
    private Uri _address = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(LogoutModel).Assembly.GetName().Name,
            EnvironmentName = "Testing"
        });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Identity/Account/Login");
        builder.Services.AddScoped<SecurityPolicyService>();
        builder.Services.AddScoped<SecurityApprovalNotifications>();
        builder.Services.AddRazorPages().AddApplicationPart(typeof(LogoutModel).Assembly);
        _app = builder.Build();
        using (var scope = _app.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roles.CreateAsync(new IdentityRole("Admin"));
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var name in new[] { "admin", "other-admin" })
            {
                var user = new ApplicationUser { UserName = name, Email = name + "@example.test", EmailConfirmed = true };
                Assert.True((await users.CreateAsync(user)).Succeeded);
                Assert.True((await users.AddToRoleAsync(user, "Admin")).Succeeded);
            }
        }
        _app.UseRouting();
        _app.UseMiddleware<PrivateResponseCacheMiddleware>();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapPost("/test/sign-in/{name}", async (string name, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) =>
        {
            await signIn.SignInAsync((await users.FindByNameAsync(name))!, false);
        });
        _app.MapGet("/test/admin", () => "Protected admin content").RequireAuthorization(policy => policy.RequireRole("Admin"));
        _app.MapRazorPages();
        await _app.StartAsync();
        _address = new Uri(_app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
    }

    public HttpClient CreateClient() => new(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = _address };

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
