using IDS.Data;
using IDS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IDS.Core.Services;

public sealed record WorkerStatus(bool Online, DateTime? LastHeartbeatUtc)
{
    public string Status => Online ? "Live" : "Offline";
}

public sealed record SystemHealth(WorkerStatus Detection, WorkerStatus Explanation)
{
    public string Overall => (Detection.Online, Explanation.Online) switch
    {
        (true, true) => "Operational",
        (false, false) => "Offline",
        _ => "Degraded"
    };
}

public sealed class WorkerHealthService(ApplicationDbContext database, IConfiguration configuration)
{
    public const string TimeoutKey = "Operations.WorkerHeartbeatTimeoutSeconds";

    public async Task<SystemHealth> GetAsync(CancellationToken cancellationToken)
    {
        var seconds = await OperationsSettings.NumberAsync(database, TimeoutKey,
            configuration.GetValue("Detection:WorkerHeartbeatTimeoutSeconds", 120), 30, 3600, cancellationToken);
        var readings = await database.WorkerHeartbeats.AsNoTracking()
            .Select(heartbeat => new { Heartbeat = heartbeat, DatabaseNow = DateTime.UtcNow })
            .ToListAsync(cancellationToken);
        var now = readings.Count == 0 ? DateTime.UtcNow : readings[0].DatabaseNow;
        return Evaluate(readings.Select(reading => reading.Heartbeat), now, TimeSpan.FromSeconds(seconds));
    }

    public static SystemHealth Evaluate(IEnumerable<WorkerHeartbeat> rows, DateTime utcNow, TimeSpan timeout)
    {
        var byWorker = rows.ToDictionary(row => row.Worker, StringComparer.Ordinal);
        WorkerStatus Read(string worker)
        {
            if (!byWorker.TryGetValue(worker, out var row)) return new(false, null);
            var timestamp = DateTime.SpecifyKind(row.LastHeartbeatUtc, DateTimeKind.Utc);
            return new(row.IsRunning && timestamp <= utcNow.AddMilliseconds(5) && utcNow - timestamp < timeout, timestamp);
        }
        return new(Read("detection"), Read("explanation"));
    }
}
