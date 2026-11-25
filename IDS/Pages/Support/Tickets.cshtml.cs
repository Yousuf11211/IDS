using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;

namespace IDS.Pages.Support
{
    /// <summary>
    /// Support Dashboard - View and manage all support tickets.
    /// Accessible by Support and Admin roles only.
    /// </summary>
    [Authorize(Roles = AppRoles.AdminOrSupport)]
    public class TicketsModel : PageModel
 {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TicketsModel> _logger;

        public TicketsModel(
            ApplicationDbContext context,
      UserManager<ApplicationUser> userManager,
   ILogger<TicketsModel> logger)
        {
            _context = context;
     _userManager = userManager;
            _logger = logger;
  }

  public List<SupportTicket> Tickets { get; set; } = new();
        public string CurrentFilter { get; set; } = "Open";
        public bool AssignmentEnabled { get; private set; }
        public string? SearchQuery { get; set; }
        
// Statistics
      public int TotalOpen { get; set; }
        public int TotalInProgress { get; set; }
    public int TotalResolved { get; set; }
        public int TotalCritical { get; set; }

 [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync(string? status, string? search, string? priority, string? incident = null, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
  CurrentFilter = status ?? "All";
        AssignmentEnabled = await OperationsSettings.IsAssignmentEnabledAsync(_context);
        SearchQuery = search;

     // Get statistics
            TotalOpen = await _context.SupportTickets.CountAsync(t => t.Status == "Open");
       TotalInProgress = await _context.SupportTickets.CountAsync(t => t.Status == "InProgress");
 TotalResolved = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved" || t.Status == "Closed");
 TotalCritical = await _context.SupportTickets.CountAsync(t => t.Priority == "Critical" && t.Status != "Closed" && t.Status != "Resolved" && t.Status != "Cancelled");

            // Build query
     var query = _context.SupportTickets.AsQueryable();
            if (incident != null)
            {
                query = query.Where(t => t.Category == "Security Concern");
                if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value.UtcDateTime);
                if (to.HasValue) query = query.Where(t => t.CreatedAt < to.Value.UtcDateTime);
                var active = query.Where(t => t.Status == "Open" || t.Status == "InProgress");
                query = incident switch
                {
                    "open" => active,
                    "high-priority" => active.Where(t => t.Priority == "High" || t.Priority == "Critical"),
                    "assigned" => active.Where(t => t.AssignedToUserId != null && t.AssignedToUserId != ""),
                    "unassigned" => active.Where(t => t.AssignedToUserId == null || t.AssignedToUserId == ""),
                    "escalated" => query.Where(t => _context.IncidentEscalations.Any(e => e.TicketId == t.Id && e.CancelledAt == null)),
                    "InProgress" or "Resolved" => query.Where(t => t.Status == incident),
                    _ => query
                };
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
 query = query.Where(t => t.Status == status);
          }

  // Apply priority filter
  if (!string.IsNullOrEmpty(priority) && priority != "All")
       {
          query = query.Where(t => t.Priority == priority);
     }

          // Apply search
       if (!string.IsNullOrEmpty(search))
            {
      search = search.ToLower();
    query = query.Where(t =>
        t.TicketNumber.ToLower().Contains(search) ||
         t.Subject.ToLower().Contains(search) ||
      t.SubmittedByEmail.ToLower().Contains(search) ||
 t.SubmittedByName.ToLower().Contains(search));
            }

    // Order by priority (Critical first), then by date
         Tickets = await query
           .OrderBy(t => t.Status == "Closed" ? 1 : 0)
   .ThenBy(t => t.Priority == "Critical" ? 0 : t.Priority == "High" ? 1 : t.Priority == "Medium" ? 2 : 3)
                .ThenByDescending(t => t.CreatedAt)
          .Take(100)
   .ToListAsync();
        }

        public async Task<IActionResult> OnPostAssignToMeAsync(int ticketId)
        {
            if (!await OperationsSettings.IsAssignmentEnabledAsync(_context)) return Forbid();
 var user = await _userManager.GetUserAsync(User);
         if (user == null)
      {
       return RedirectToPage("/Identity/Account/Login");
        }

            var ticket = await _context.SupportTickets.FindAsync(ticketId);
    if (ticket == null)
            {
   StatusMessage = "Error: Ticket not found.";
       return RedirectToPage();
            }

            if (ticket.Status is "Closed" or "Resolved" or "Cancelled")
            {
                StatusMessage = "Error: Reopen this ticket before assigning it.";
                return RedirectToPage();
            }

            if (!string.IsNullOrEmpty(ticket.AssignedToUserId))
            {
                StatusMessage = "Error: This ticket is already assigned. Open it to reassign.";
                return RedirectToPage();
            }

            ticket.AssignedToUserId = user.Id;
     ticket.AssignedToName = !string.IsNullOrEmpty(user.FirstName)
      ? $"{user.FirstName} {user.LastName}"
 : user.Email ?? "Support";
     var oldStatus = ticket.Status;
     ticket.Status = "InProgress";
      ticket.UpdatedAt = DateTime.UtcNow;

            IncidentWorkflow.RecordAssignment(_context, ticket, user, null, null);
            IncidentWorkflow.RecordStatus(_context, ticket, user, oldStatus);
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return StatusCode(409, "The incident changed. Reload the queue."); }

      _logger.LogInformation(
  "Ticket {TicketNumber} assigned to {Email}",
     ticket.TicketNumber, user.Email);

   StatusMessage = $"Ticket {ticket.TicketNumber} assigned to you.";
     return RedirectToPage();
        }
    }
}
