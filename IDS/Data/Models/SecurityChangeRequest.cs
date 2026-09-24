using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models;

// A request records the exact action and account state reviewed by the second admin.
public sealed class SecurityChangeRequest
{
    public long Id { get; set; }
    [MaxLength(80)] public string Action { get; set; } = string.Empty;
    [MaxLength(450)] public string RequestedById { get; set; } = string.Empty;
    [MaxLength(256)] public string RequestedByEmail { get; set; } = string.Empty;
    [MaxLength(256)] public string RequesterSecurityStamp { get; set; } = string.Empty;
    [MaxLength(450)] public string? TargetUserId { get; set; }
    [MaxLength(256)] public string? TargetEmail { get; set; }
    [MaxLength(256)] public string? TargetSecurityStamp { get; set; }
    [MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [MaxLength(20)] public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    [MaxLength(450)] public string? ReviewedById { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    [ConcurrencyCheck] public Guid Version { get; set; } = Guid.NewGuid();
}
