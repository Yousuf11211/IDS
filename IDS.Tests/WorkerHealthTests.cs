using IDS.Core.Services;
using IDS.Data.Models;
using Xunit;

namespace IDS.Tests;

public class WorkerHealthTests
{
    private static readonly DateTime Now = new(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    [Theory]
    [InlineData(true, true, "Operational")]
    [InlineData(true, false, "Degraded")]
    [InlineData(false, true, "Degraded")]
    [InlineData(false, false, "Offline")]
    public void IndependentWorkersDetermineOverallState(bool detection, bool explanation, string overall)
    {
        var state = WorkerHealthService.Evaluate([Pulse("detection", detection), Pulse("explanation", explanation)], Now, Timeout);
        Assert.Equal(overall, state.Overall);
        Assert.Equal(detection ? "Live" : "Offline", state.Detection.Status);
        Assert.Equal(explanation ? "Live" : "Offline", state.Explanation.Status);
    }

    [Fact]
    public void AStaleWorkerIsOfflineUntilItPublishesAgain()
    {
        var detection = Pulse("detection");
        var explanation = Pulse("explanation");
        Assert.Equal("Offline", WorkerHealthService.Evaluate([], Now, Timeout).Overall);
        detection.LastHeartbeatUtc = Now - Timeout;
        var stale = WorkerHealthService.Evaluate([detection, explanation], Now, Timeout);
        Assert.Equal("Degraded", stale.Overall);
        Assert.Equal(Now - Timeout, stale.Detection.LastHeartbeatUtc);
        detection.LastHeartbeatUtc = Now;
        Assert.Equal("Operational", WorkerHealthService.Evaluate([detection, explanation], Now, Timeout).Overall);
    }

    private static WorkerHeartbeat Pulse(string worker, bool running = true) =>
        new() { Worker = worker, LastHeartbeatUtc = Now, IsRunning = running };
}
