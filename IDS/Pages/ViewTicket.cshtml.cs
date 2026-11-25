using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;

namespace IDS.Pages
{
    /// <summary>
    /// Page for viewing ticket details and adding comments.
    /// Users can only view their own tickets; Support/Admin can view all.
    /// </summary>
  [Authorize]
    public class ViewTicketModel : PageModel
    {
        private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ViewTicketModel> _logger;

        public ViewTicketModel(
            ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
    ILogger<ViewTicketModel> logger)
        {
            _context = context;
        _userManager = userManager;
     _logger = logger;
        }

        public SupportTicket? Ticket { get; set; }
        public List<TicketComment> Comments { get; set; } = new();
        public bool IsSupport { get; set; }
        public bool CanManageTicket { get; set; }

        [BindProperty]
     public string? NewComment { get; set; }

        public List<IncidentEscalation> Escalations { get; private set; } = [];
        public List<IncidentWorkflow.Responder> Responders { get; private set; } = [];
        public bool CanCancel { get; private set; }

        [BindProperty] public string? ResponderId { get; set; }
        [BindProperty] public string? EscalationReason { get; set; }
        [BindProperty] public string? CancellationReason { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
      {
     var user = await _userManager.GetUserAsync(User);
        if (user == null)
     {
           return RedirectToPage("/Identity/Account/Login");
  }

   // Check if user is support staff
       var roles = await _userManager.GetRolesAsync(user);
      IsSupport = roles.Any(r => AppRoles.HasSupportAccess(r));

         // Load the ticket
       Ticket = await _context.SupportTickets.FindAsync(id);
            if (Ticket == null)
          {
      return NotFound();
            }

     // Check access: owner or support staff
  if (!IncidentWorkflow.CanView(Ticket, User))
  {
  return Forbid();
}

      CanManageTicket = IsSupport;
            Escalations = await _context.IncidentEscalations.AsNoTracking()
                .Where(e => e.TicketId == id).OrderByDescending(e => e.Id).ToListAsync();
            var current = Escalations.FirstOrDefault();
            CanCancel = current != null && IncidentWorkflow.CanCancel(Ticket, current, user.Id);
            if (CanEscalate(Ticket) && await OperationsSettings.IsEscalationEnabledAsync(_context))
                Responders = await IncidentWorkflow.RespondersAsync(_userManager, user.Id);

            // Load comments
            Comments = await _context.TicketComments
       .Where(c => c.TicketId == id)
      .OrderBy(c => c.CreatedAt)
            .ToListAsync();

            return Page();
   }

   public async Task<IActionResult> OnPostAddCommentAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
     if (user == null)
  {
             return RedirectToPage("/Identity/Account/Login");
      }

       var roles = await _userManager.GetRolesAsync(user);
         IsSupport = roles.Any(r => AppRoles.HasSupportAccess(r));

   var ticket = await _context.SupportTickets.FindAsync(id);
    if (ticket == null)
            {
             return NotFound();
    }

        // Check access
            if (!IncidentWorkflow.CanView(ticket, User))
   {
        return Forbid();
     }

            if (ticket.Status is "Cancelled" or "Closed") return StatusCode(409);
            if (string.IsNullOrWhiteSpace(NewComment) || NewComment.Trim().Length > 2000)
      {
      StatusMessage = "Error: Comment cannot be empty.";
        return RedirectToPage(new { id });
         }

            var comment = new TicketComment
            {
   TicketId = id,
       UserId = user.Id,
UserName = !string.IsNullOrEmpty(user.FirstName)
    ? $"{user.FirstName} {user.LastName}"
           : user.Email ?? "Unknown",
           IsFromSupport = IsSupport,
      Content = NewComment.Trim(),
       CreatedAt = DateTime.UtcNow
    };

 _context.TicketComments.Add(comment);

    // Update ticket timestamp
    ticket.UpdatedAt = DateTime.UtcNow;
    
if (!await TrySaveAsync()) return StatusCode(409);

   _logger.LogInformation(
     "Comment added to ticket {TicketNumber} by {Email}",
       ticket.TicketNumber, user.Email);

   StatusMessage = "Comment added successfully.";
            return RedirectToPage(new { id });
     }
        public async Task<IActionResult> OnPostEscalateAsync(int id)
        {
            var actor = await _userManager.GetUserAsync(User);
            if (actor == null) return Challenge();
            if (!await OperationsSettings.IsEscalationEnabledAsync(_context)) return Forbid();

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null) return NotFound();
            if (!IncidentWorkflow.CanView(ticket, User)) return Forbid();
            if (!CanEscalate(ticket)) return StatusCode(409, "Only open, unassigned incidents can be escalated here.");

            var reason = EscalationReason?.Trim();
            var responders = await IncidentWorkflow.RespondersAsync(_userManager, actor.Id);
            var responder = responders.FirstOrDefault(r => r.Id == ResponderId);
            if (reason == null || reason.Length is < 10 or > 2000 || responder == null)
            {
                StatusMessage = "Error: Select an active responder and enter a reason of 10 to 2,000 characters.";
                return RedirectToPage(new { id });
            }

            IncidentWorkflow.Escalate(_context, ticket, actor, responder, reason, createdTicket: false);
            if (!await TrySaveAsync()) return StatusCode(409);
            StatusMessage = "Incident escalated.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCancelEscalationAsync(int id)
        {
            var actor = await _userManager.GetUserAsync(User);
            if (actor == null) return Challenge();

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null) return NotFound();
            if (!IncidentWorkflow.CanView(ticket, User)) return Forbid();

            var escalation = await _context.IncidentEscalations.Where(e => e.TicketId == id)
                .OrderByDescending(e => e.Id).FirstOrDefaultAsync();
            if (escalation == null || escalation.InitiatorId != actor.Id) return Forbid();
            if (!IncidentWorkflow.CanCancel(ticket, escalation, actor.Id))
            {
                StatusMessage = "Error: The incident has changed since escalation and cannot be withdrawn.";
                return RedirectToPage(new { id });
            }

            var reason = CancellationReason?.Trim();
            if (string.IsNullOrWhiteSpace(reason) || reason.Length > 2000)
            {
                StatusMessage = "Error: Enter a cancellation reason of 1 to 2,000 characters.";
                return RedirectToPage(new { id });
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.Status = escalation.CreatedTicket ? "Cancelled" : "Open";
            ticket.AssignedToName = null;
            ticket.AssignedToUserId = null;
            escalation.CancelledAt = ticket.UpdatedAt;
            escalation.CancelledById = actor.Id;
            escalation.CancellationReason = reason;
            IncidentWorkflow.Audit(_context, ticket, actor, "CancelEscalation", reason);
            foreach (var recipient in new[] { escalation.RecipientId, ticket.SubmittedByUserId }.Distinct())
                IncidentWorkflow.Notify(_context, ticket, actor, recipient, "Escalation cancelled", $"{ticket.TicketNumber}: {reason}");
            if (!await TrySaveAsync()) return StatusCode(409);
            StatusMessage = "Escalation cancelled.";
            return RedirectToPage(new { id });
        }

        private static bool CanEscalate(SupportTicket ticket) =>
            ticket.Status == "Open" && ticket.AssignedToUserId == null && ticket.Category == "Security Concern";

        private async Task<bool> TrySaveAsync()
        {
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return false; }
            return true;
        }
    }
}
