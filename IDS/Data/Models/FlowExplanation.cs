namespace IDS.Data.Models;

/// <summary>Immutable inference snapshot and durable per-stage SHAP work item.</summary>
public sealed class FlowExplanation
{
    public long Id { get; set; }
    public long AttackTrafficId { get; set; }
    public long? RawPacketId { get; set; }
    public string Stage { get; set; } = "";
    public string ModelVersion { get; set; } = "";
    public string Fingerprint { get; set; } = "";
    public string ContextJson { get; set; } = "";
    public string? PayloadJson { get; set; }
    public string Status { get; set; } = "pending";
    public int Attempts { get; set; }
    public double NextAttempt { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
