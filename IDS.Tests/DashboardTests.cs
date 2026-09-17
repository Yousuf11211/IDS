using System.Security.Claims;
using IDS.Data;
using IDS.Data.Models;
using IDS.Pages;
using IDS.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IDS.Tests;

public class DashboardTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;

    public DashboardTests()
    {
        // Use a real, isolated database so these tests exercise the EF queries.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();
    }

    [Theory]
    [InlineData(0, 0, "Normal")]
    [InlineData(12, 0, "Normal")]
    [InlineData(0, 8, "Attention")]
    [InlineData(12, 3, "Degraded")]
    [InlineData(4, 3, "Attention")]
    public async Task EmployeeDashboardCountsSavedTraffic(int normalCount, int attackCount, string expectedStatus)
    {
        // Old detections still belong in the lifetime totals. Logs and raw packets do not.
        var timestamp = DateTime.UtcNow.AddDays(-30);
        _db.BenignTraffic.AddRange(Enumerable.Range(0, normalCount)
            .Select(_ => new BenignTraffic { Timestamp = timestamp }));
        _db.AttackTraffic.AddRange(Enumerable.Range(0, attackCount)
            .Select(_ => new AttackTraffic { Timestamp = timestamp, AttackType = "DoS" }));
        _db.RawPackets.Add(new RawPacket());
        _db.LogFiles.AddRange(
            new LogFile { Level = "Warning", Message = "Application warning" },
            new LogFile { Level = "NetworkNormal", Message = "Legacy normal log" },
            new LogFile { Level = "NetworkAttack", Message = "Legacy attack log" });
        await _db.SaveChangesAsync();
        var page = CreatePage(AppRoles.Employee);

        await page.OnGetAsync();

        Assert.Equal(normalCount, page.NormalCount);
        Assert.Equal(attackCount, page.AttackCount);
        Assert.Equal(normalCount + attackCount, page.TotalLogs);
        Assert.Equal(page.NetworkTotal, page.LevelCounts.Values.Sum());
        Assert.Equal(expectedStatus, page.IdsStatus);

        // Revisiting the page records another visit without changing traffic totals.
        await page.OnGetAsync();
        Assert.Equal(normalCount + attackCount, page.TotalLogs);
    }

    [Fact]
    public async Task AdminDashboardKeepsActivityLogsSeparateFromTraffic()
    {
        _db.BenignTraffic.AddRange(new BenignTraffic(), new BenignTraffic());
        _db.AttackTraffic.Add(new AttackTraffic { AttackType = "DoS" });
        _db.LogFiles.Add(new LogFile { Level = "Information", Message = "Signed in" });
        await _db.SaveChangesAsync();
        var page = CreatePage(AppRoles.Admin);

        await page.OnGetAsync();

        Assert.Equal(2, page.TotalLogs); // Includes the dashboard visit.
        Assert.Equal(3, page.NetworkTotal);
        Assert.Equal(2, page.NormalCount);
        Assert.Equal(1, page.AttackCount);
        Assert.IsType<JsonResult>(await page.OnGetLogsAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EmployeeActivityReturnsNewestTenDetections(bool newestAreAttacks)
    {
        var timestamp = DateTime.UtcNow;
        for (var index = 0; index < 12; index++)
        {
            _db.BenignTraffic.Add(new BenignTraffic
            {
                Timestamp = timestamp.AddMinutes(index - (newestAreAttacks ? 30 : 0))
            });
            _db.AttackTraffic.Add(new AttackTraffic
            {
                Timestamp = timestamp.AddMinutes(index - (newestAreAttacks ? 0 : 30)),
                AttackType = "PortScan",
                Severity = "High"
            });
        }
        _db.LogFiles.Add(new LogFile
        {
            Timestamp = timestamp.AddDays(1),
            Level = "Error",
            Message = "Administrative details"
        });
        await _db.SaveChangesAsync();
        var page = CreatePage(AppRoles.Employee);

        var response = await page.OnGetRecentActivityAsync();
        var activity = Assert.IsType<List<DashboardModel.DashboardActivity>>(response.Value);

        Assert.Equal(10, activity.Count);
        Assert.Equal(timestamp.AddMinutes(11), activity.First().Timestamp);
        Assert.Equal(timestamp.AddMinutes(2), activity.Last().Timestamp);
        Assert.All(activity, item => Assert.Equal(newestAreAttacks ? "PortScan" : "Normal Traffic", item.Type));
        Assert.IsType<ForbidResult>(await page.OnGetLogsAsync());
    }

    [Fact]
    public async Task EmptyDatabaseReturnsNoRecentActivity()
    {
        var response = await CreatePage(AppRoles.Employee).OnGetRecentActivityAsync();

        Assert.Empty(Assert.IsType<List<DashboardModel.DashboardActivity>>(response.Value));
    }

    private DashboardModel CreatePage(string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dashboard-test-user"),
            new Claim(ClaimTypes.Role, role)
        }, "Test");

        return new DashboardModel(_db)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
