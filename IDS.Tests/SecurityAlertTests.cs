using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IDS.Tests;

public sealed class SecurityAlertTests
{
    [Fact]
    public async Task GroupedAlertCrossesConfiguredThresholdOnceAndBackfillMergesSafely()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var service = new SecurityAlertService(db, new(db, new ConfigurationBuilder().Build()));
        var start = DateTime.UtcNow.Date.AddHours(-2);
        AttackTraffic Detection(string type, int port, int minute) => new()
        {
            AttackType = type, SrcIp = "192.0.2.10", DstPort = port, Timestamp = start.AddMinutes(minute),
            ConfidenceScore = 0.999, Severity = "Critical"
        };
        db.AttackTraffic.Add(Detection("Example class", 22, 1));
        db.SystemSettings.Add(new() { Key = SecurityAlertSettings.Rules, Value = "Example class | 3 | High" });
        await db.SaveChangesAsync();
        await service.ProcessAsync(default);
        Assert.Empty(await SecurityAlertService.Published(db.SecurityAlerts).ToListAsync());
        db.AttackTraffic.AddRange(Detection("Example class", 22, 2), Detection("Example class", 22, 3),
            Detection("Example class", 80, 3), Detection("Other class", 22, 3));
        await db.SaveChangesAsync();
        await service.ProcessAsync(default);
        Assert.Empty(await SecurityAlertService.Published(db.SecurityAlerts).ToListAsync());
        Assert.True(await service.ScheduleBackfillAsync(7, default));
        Assert.False(await service.ScheduleBackfillAsync(7, default));
        await service.ProcessAsync(default);
        await service.ProcessAsync(default);
        var alert = Assert.Single(await SecurityAlertService.Published(db.SecurityAlerts).ToListAsync());
        Assert.Equal(3, alert.DetectionCount);
        Assert.Equal("High", alert.Severity);
        Assert.Equal("Example class", alert.AttackLabel);
        Assert.Equal(start.AddMinutes(1), alert.Timestamp);
        Assert.Equal(start.AddMinutes(3), alert.LastObservedAt);
    }

    [Fact]
    public async Task WorkerOutageIsReportedOnceAndResolvesAtRecovery()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.SystemSettings.Add(new() { Key = SecurityAlertSettings.WorkerFailures, Value = "true" });
        var detection = new WorkerHeartbeat
        {
            Worker = "detection", IsRunning = true, LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-5)
        };
        db.WorkerHeartbeats.Add(detection);
        await db.SaveChangesAsync();
        var monitor = new SecurityAlertService(db, new(db, new ConfigurationBuilder().Build()));
        await monitor.ProcessAsync(default);
        await monitor.ProcessAsync(default);
        var outage = Assert.Single(await db.SecurityAlerts.ToListAsync());
        Assert.Equal("Detection worker offline", outage.AlertType);
        Assert.Null(outage.ResolvedAt);
        detection.LastHeartbeatUtc = DateTime.UtcNow.AddMilliseconds(-100);
        await db.SaveChangesAsync();
        await monitor.ProcessAsync(default);
        Assert.NotNull(outage.ResolvedAt);
        Assert.Single(await db.SecurityAlerts.ToListAsync());
    }

    [Fact]
    public void MonitoringWindowsValidateBoundariesAndBoundChartSize()
    {
        var now = new DateTime(2026, 11, 25, 15, 0, 0, DateTimeKind.Utc);
        var range = MonitoringWindow.Resolve("custom", now, new(2026, 11, 1), new(2026, 11, 1));
        Assert.NotNull(range);
        Assert.Equal(TimeSpan.FromDays(1), range.To - range.From);
        Assert.Null(MonitoringWindow.Resolve("custom", now, new(2026, 11, 2), new(2026, 11, 1)));
        var all = MonitoringWindow.Resolve("all", now)!;
        Assert.InRange(all.Buckets(new DateTime(1999, 1, 1)).Count, 1, 72);
        Assert.Equal(TimeSpan.FromMinutes(5), now - MonitoringWindow.Resolve("5m", now)!.From);
    }
}
