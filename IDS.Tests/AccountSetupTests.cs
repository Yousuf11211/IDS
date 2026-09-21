using IDS.Areas.Identity.Pages.Account;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    private ResetPasswordModel NewResetPage(string token) => new(_users)
    {
        Input = new ResetPasswordModel.InputModel
        {
            Email = "invite@example.test",
            Code = token,
            Password = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        }
    };

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        _connection.Dispose();
    }
}
