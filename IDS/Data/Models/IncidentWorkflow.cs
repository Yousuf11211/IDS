using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models;

public sealed class InboxNotification
{
    public long Id { get; set; }
    [MaxLength(450)] public string RecipientId { get; set; } = "";
    [MaxLength(200)] public string? SenderName { get; set; }
    [MaxLength(50)] public string Type { get; set; } = "";
    [MaxLength(2200)] public string Message { get; set; } = "";
    public int? TicketId { get; set; }
    public SupportTicket? Ticket { get; set; }
    public int? SecurityAlertId { get; set; }
    public long? AuthenticatorResetRequestId { get; set; }
    public bool IsAccountSecurity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}

public sealed class IncidentEscalation
{
    public long Id { get; set; }
    public int TicketId { get; set; }
    public SupportTicket Ticket { get; set; } = null!;
    [MaxLength(450)] public string InitiatorId { get; set; } = "";
    [MaxLength(450)] public string RecipientId { get; set; } = "";
    [MaxLength(2000)] public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool CreatedTicket { get; set; }
    [MaxLength(450)] public string? CancelledById { get; set; }
    [MaxLength(2000)] public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
}
