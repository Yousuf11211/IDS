using System.Security.Claims;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Identity;

namespace IDS.Core.Services;

public static class IncidentWorkflow
{
    public sealed record Responder(string Id, string Name, string Role);

    public static bool CanView(SupportTicket ticket, ClaimsPrincipal user) =>
        ticket.SubmittedByUserId == user.FindFirstValue(ClaimTypes.NameIdentifier) ||
        AppRoles.SupportRoles.Any(user.IsInRole);

    public static string DisplayName(ApplicationUser user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? user.Email ?? "IDS user" : name;
    }

    public static async Task<List<Responder>> RespondersAsync(UserManager<ApplicationUser> users, string? actorId)
    {
        var responders = new List<Responder>();
        foreach (var role in AppRoles.SupportRoles)
        {
            foreach (var user in await users.GetUsersInRoleAsync(role))
            {
                if (user.Id != actorId && user.EmailConfirmed && !user.MustChangePassword && !await users.IsLockedOutAsync(user) &&
                    !await users.IsInRoleAsync(user, AppRoles.Suspended))
                    responders.Add(new(user.Id, DisplayName(user), role));
            }
        }
        return responders.DistinctBy(r => r.Id).OrderBy(r => r.Name).ToList();
    }

    public static void Notify(ApplicationDbContext db, SupportTicket ticket, ApplicationUser actor,
        string? recipientId, string type, string message)
    {
        if (string.IsNullOrEmpty(recipientId) || recipientId == actor.Id) return;
        db.InboxNotifications.Add(new()
        {
            RecipientId = recipientId,
            SenderName = DisplayName(actor),
            Type = type,
            Message = message,
            Ticket = ticket,
            CreatedAt = ticket.UpdatedAt
        });
    }

    public static void Audit(ApplicationDbContext db, SupportTicket ticket, ApplicationUser actor,
        string action, string details) => db.AuditLogs.Add(new()
    {
        Timestamp = ticket.UpdatedAt,
        UserId = actor.Id,
        UserEmail = actor.Email ?? "",
        Action = action,
        EntityType = nameof(SupportTicket),
        EntityId = ticket.Id > 0 ? ticket.Id.ToString() : ticket.TicketNumber,
        Details = details
    });

    public static void RecordAssignment(ApplicationDbContext db, SupportTicket ticket, ApplicationUser actor,
        string? previousId, string? previousName)
    {
        if (previousId == ticket.AssignedToUserId) return;
        var type = previousId == null ? "Incident assigned" : "Incident reassigned";
        var message = $"{ticket.TicketNumber} was assigned to {ticket.AssignedToName}.";
        Audit(db, ticket, actor, "AssignTicket", $"{previousName ?? "Unassigned"} → {ticket.AssignedToName}");
        foreach (var recipient in new[] { ticket.AssignedToUserId, previousId, ticket.SubmittedByUserId }.Distinct())
            Notify(db, ticket, actor, recipient, type, message);
    }

    public static void RecordStatus(ApplicationDbContext db, SupportTicket ticket, ApplicationUser actor, string previous)
    {
        if (previous == ticket.Status) return;
        Audit(db, ticket, actor, "ChangeTicketStatus", $"{previous} → {ticket.Status}");
        foreach (var recipient in new[] { ticket.AssignedToUserId, ticket.SubmittedByUserId }.Distinct())
            Notify(db, ticket, actor, recipient, "Incident status", $"{ticket.TicketNumber} moved from {previous} to {ticket.Status}.");
    }

    public static void Escalate(ApplicationDbContext db, SupportTicket ticket, ApplicationUser actor,
        Responder responder, string reason, bool createdTicket)
    {
        ticket.UpdatedAt = createdTicket ? ticket.CreatedAt : DateTime.UtcNow;
        ticket.AssignedToUserId = responder.Id;
        ticket.AssignedToName = responder.Name;
        ticket.Status = "InProgress";
        db.IncidentEscalations.Add(new()
        {
            Ticket = ticket,
            InitiatorId = actor.Id,
            RecipientId = responder.Id,
            Reason = reason,
            CreatedTicket = createdTicket,
            CreatedAt = ticket.UpdatedAt
        });
        Audit(db, ticket, actor, createdTicket ? "EscalateAttack" : "EscalateIncident",
            $"Escalated to {responder.Name} ({responder.Role}). Reason: {reason}");
        Notify(db, ticket, actor, responder.Id, "Incident escalated", $"{ticket.TicketNumber}: {reason}");
        if (!createdTicket)
            Notify(db, ticket, actor, ticket.SubmittedByUserId, "Incident escalated", $"{ticket.TicketNumber} was escalated to {responder.Name}.");
    }

    public static bool CanCancel(SupportTicket ticket, IncidentEscalation escalation, string actorId) =>
        escalation.InitiatorId == actorId && escalation.CancelledAt == null &&
        ticket.Status == "InProgress" && ticket.AssignedToUserId == escalation.RecipientId &&
        ticket.UpdatedAt == escalation.CreatedAt;
}
