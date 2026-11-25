using System.Security.Claims;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public sealed class AlertDetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> users) : PageModel
{
    public SecurityAlert Alert { get; private set; } = null!;
    [TempData] public string? Result { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken token)
    {
        var alert = await SecurityAlertService.Published(db.SecurityAlerts.AsNoTracking()).Include(a => a.Ticket)
            .SingleOrDefaultAsync(a => a.Id == id, token);
        if (alert == null) return NotFound();
        Alert = alert;
        return Page();
    }

    public async Task<IActionResult> OnPostResolveAsync(int id, string? reason, CancellationToken token)
    {
        reason = reason?.Trim();
        if (reason == null || reason.Length is < 1 or > 1000)
        {
            Result = "Enter a resolution reason of 1 to 1,000 characters.";
            return RedirectToPage(new { id });
        }
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await DatabaseLock.AcquireAsync(db, nameof(SecurityAlertService), token);
        var alert = await SecurityAlertService.Published(db.SecurityAlerts).Include(a => a.Ticket)
            .SingleOrDefaultAsync(a => a.Id == id, token);
        if (alert == null) return NotFound();
        if (alert.ResolvedAt != null) return RedirectToPage(new { id });
        if (alert.Ticket != null && alert.Ticket.Status is not ("Resolved" or "Closed" or "Cancelled"))
        {
            Result = "Resolve or close the linked incident before resolving this alert.";
            return RedirectToPage(new { id });
        }
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        AlertsModel.Audit(db, User, "ResolveSecurityAlert", id.ToString(), reason);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        Result = "Alert resolved.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostIncidentAsync(int id, int? ticketId, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, token);
        await DatabaseLock.AcquireAsync(db, nameof(SecurityAlertService), token);
        var actor = await users.GetUserAsync(User);
        if (actor == null) return Challenge();
        var alert = await SecurityAlertService.Published(db.SecurityAlerts).SingleOrDefaultAsync(a => a.Id == id, token);
        if (alert == null) return NotFound();
        if (alert.TicketId != null || alert.ResolvedAt != null) return RedirectToPage(new { id });
        var ticket = ticketId.HasValue
            ? await db.SupportTickets.SingleOrDefaultAsync(t => t.Id == ticketId && t.Category == "Security Concern", token)
            : alert.RepresentativeAttackId.HasValue
                ? await db.SupportTickets.SingleOrDefaultAsync(t => t.SourceAttackId == alert.RepresentativeAttackId, token)
                : null;
        if (ticketId.HasValue && (ticket == null || !IncidentWorkflow.CanView(ticket, User)))
        {
            Result = "Enter the ID of an accessible security incident.";
            return RedirectToPage(new { id });
        }

        if (ticket == null)
        {
            ticket = new SupportTicket
            {
                TicketNumber = TicketConstants.GenerateTicketNumber(), Subject = alert.AlertType,
                Description = alert.Message, Category = "Security Concern",
                Priority = TicketConstants.Priorities.Contains(alert.Severity) ? alert.Severity : "Medium",
                SubmittedByUserId = actor.Id, SubmittedByEmail = actor.Email ?? "",
                SubmittedByName = IncidentWorkflow.DisplayName(actor), SourceAttackId = alert.RepresentativeAttackId
            };
            db.SupportTickets.Add(ticket);
            IncidentWorkflow.Audit(db, ticket, actor, "CreateSecurityIncident", $"Created from alert {id}.");
        }
        alert.Ticket = ticket;
        AlertsModel.Audit(db, User, "AssociateSecurityAlert", id.ToString(), ticket.TicketNumber);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        Result = "Incident linked.";
        return RedirectToPage(new { id });
    }
}
