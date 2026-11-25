using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models;

public sealed class WorkerHeartbeat
{
    [Key, MaxLength(20)]
    public string Worker { get; set; } = string.Empty;
    public DateTime LastHeartbeatUtc { get; set; }
    public bool IsRunning { get; set; }
}
