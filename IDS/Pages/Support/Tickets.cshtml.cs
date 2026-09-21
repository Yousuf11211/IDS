using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        public string? SearchQuery { get; set; }
        
// Statistics
      public int TotalOpen { get; set; }
        public int TotalInProgress { get; set; }
    public int TotalResolved { get; set; }
        public int TotalCritical { get; set; }

 [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync(string? status, string? search, string? priority)
        {
  CurrentFilter = status ?? "Open";
        SearchQuery = search;

     // Get statistics
            TotalOpen = await _context.SupportTickets.CountAsync(t => t.Status == "Open");
       TotalInProgress = await _context.SupportTickets.CountAsync(t => t.Status == "InProgress");
 TotalResolved = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved" || t.Status == "Closed");
 TotalCritical = await _context.SupportTickets.CountAsync(t => t.Priority == "Critical" && t.Status != "Closed" && t.Status != "Resolved");

            // Build query
     var query = _context.SupportTickets.AsQueryable();

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

            if (ticket.Status == "Closed" || ticket.Status == "Resolved")
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
     ticket.Status = "InProgress";
      ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

      _logger.LogInformation(
  "Ticket {TicketNumber} assigned to {Email}",
     ticket.TicketNumber, user.Email);

   StatusMessage = $"Ticket {ticket.TicketNumber} assigned to you.";
     return RedirectToPage();
        }
    }
}
