using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models;

public sealed class SecurityEmail
{
    public long Id { get; set; }
    [MaxLength(128)] public string EventKey { get; set; } = "";
    [MaxLength(60)] public string EventType { get; set; } = "";
    public string? ProtectedPayload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    [MaxLength(50)] public string? FailureCode { get; set; }
    public Guid? Lease { get; set; }
    public int Attempts { get; set; }
}

public sealed class AuthenticatorResetRequest
{
    public long Id { get; set; }
    [MaxLength(450)] public string UserId { get; set; } = "";
    [MaxLength(256)] public string VerifiedEmail { get; set; } = "";
    [MaxLength(256)] public string SecurityStamp { get; set; } = "";
    [MaxLength(20)] public string Status { get; set; } = "Pending";
    [MaxLength(2000)] public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    [MaxLength(450)] public string? ReviewerId { get; set; }
    [MaxLength(1000)] public string? ReviewReason { get; set; }
    [MaxLength(1000)] public string? IdentityVerification { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public sealed class RecoveryThrottle
{
    [MaxLength(100)] public string Key { get; set; } = "";
    public DateTime WindowStart { get; set; }
    public int Attempts { get; set; }
}
