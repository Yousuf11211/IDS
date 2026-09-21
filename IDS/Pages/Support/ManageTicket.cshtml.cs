using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;

namespace IDS.Pages.Support
{
    /// <summary>
    /// Page for Support/Admin to manage a specific ticket.
    /// </summary>
    [Authorize(Roles = AppRoles.AdminOrSupport)]
    public class ManageTicketModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageTicketModel> _logger;

        public ManageTicketModel(
ApplicationDbContext context,
UserManager<ApplicationUser> userManager,
     ILogger<ManageTicketModel> logger)
        {
         _context = context;
          _userManager = userManager;
        _logger = logger;
        }

     public SupportTicket? Ticket { get; set; }
        public List<TicketComment> Comments { get; set; } = new();
        public List<Responder> Responders { get; set; } = new();
        public sealed record Responder(string Id, string Name, string Role);
        public string[] Statuses => TicketConstants.Statuses;
 public string[] Priorities => TicketConstants.Priorities;

   [BindProperty]
  public string? NewStatus { get; set; }

        [BindProperty]
     public string? NewPriority { get; set; }

        [BindProperty]
        public string? ResolutionNotes { get; set; }

        [BindProperty]
  public string? NewComment { get; set; }

        [BindProperty]
        public string? ResponderId { get; set; }

        [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
     {
      Ticket = await _context.SupportTickets.FindAsync(id);
            if (Ticket == null)
            {
  return NotFound();
 }

            Comments = await _context.TicketComments
         .Where(c => c.TicketId == id)
            .OrderBy(c => c.CreatedAt)
   .ToListAsync();

            await LoadRespondersAsync();

    return Page();
        }

      public async Task<IActionResult> OnPostUpdateStatusAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
   return RedirectToPage("/Identity/Account/Login");
            }

            var ticket = await _context.SupportTickets.FindAsync(id);
  if (ticket == null)
        {
        return NotFound();
   }

      var oldStatus = ticket.Status;

            if (!string.IsNullOrEmpty(NewStatus) && TicketConstants.Statuses.Contains(NewStatus))
         {
           ticket.Status = NewStatus;
             ticket.UpdatedAt = DateTime.UtcNow;

     // Auto-assign if not assigned and moving to InProgress
                if (NewStatus == "InProgress" && string.IsNullOrEmpty(ticket.AssignedToUserId))
    {
           ticket.AssignedToUserId = user.Id;
        ticket.AssignedToName = !string.IsNullOrEmpty(user.FirstName)
? $"{user.FirstName} {user.LastName}"
         : user.Email ?? "Support";
     }

    // Set resolved date
       if (NewStatus == "Resolved" || NewStatus == "Closed")
     {
      ticket.ResolvedAt = DateTime.UtcNow;
                }

    await _context.SaveChangesAsync();

                _logger.LogInformation(
            "Ticket {TicketNumber} status changed from {OldStatus} to {NewStatus} by {Email}",
        ticket.TicketNumber, oldStatus, NewStatus, user.Email);

  StatusMessage = $"Status updated to {NewStatus}.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUpdatePriorityAsync(int id)
        {
      var user = await _userManager.GetUserAsync(User);
     if (user == null)
   {
             return RedirectToPage("/Identity/Account/Login");
         }

         var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
  {
    return NotFound();
            }

            if (!string.IsNullOrEmpty(NewPriority) && TicketConstants.Priorities.Contains(NewPriority))
     {
            var oldPriority = ticket.Priority;
         ticket.Priority = NewPriority;
  ticket.UpdatedAt = DateTime.UtcNow;

          await _context.SaveChangesAsync();

       _logger.LogInformation(
         "Ticket {TicketNumber} priority changed from {OldPriority} to {NewPriority} by {Email}",
    ticket.TicketNumber, oldPriority, NewPriority, user.Email);

 StatusMessage = $"Priority updated to {NewPriority}.";
          }

            return RedirectToPage(new { id });
    }

        public async Task<IActionResult> OnPostResolveAsync(int id)
     {
            var user = await _userManager.GetUserAsync(User);
  if (user == null)
         {
      return RedirectToPage("/Identity/Account/Login");
    }

      var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
return NotFound();
        }

      ticket.Status = "Resolved";
    ticket.ResolvedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
         ticket.ResolutionNotes = ResolutionNotes?.Trim();

    if (string.IsNullOrEmpty(ticket.AssignedToUserId))
        {
    ticket.AssignedToUserId = user.Id;
                ticket.AssignedToName = !string.IsNullOrEmpty(user.FirstName)
         ? $"{user.FirstName} {user.LastName}"
                  : user.Email ?? "Support";
}

 await _context.SaveChangesAsync();

         _logger.LogInformation(
       "Ticket {TicketNumber} resolved by {Email}",
     ticket.TicketNumber, user.Email);

     StatusMessage = $"Ticket {ticket.TicketNumber} has been resolved.";
    return RedirectToPage(new { id });
  }

        public async Task<IActionResult> OnPostAddCommentAsync(int id)
        {
         var user = await _userManager.GetUserAsync(User);
if (user == null)
            {
   return RedirectToPage("/Identity/Account/Login");
            }

            var ticket = await _context.SupportTickets.FindAsync(id);
   if (ticket == null)
     {
   return NotFound();
            }

     if (string.IsNullOrWhiteSpace(NewComment))
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
  : user.Email ?? "Support",
     IsFromSupport = true,
        Content = NewComment.Trim(),
  CreatedAt = DateTime.UtcNow
            };

       _context.TicketComments.Add(comment);
  ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

     _logger.LogInformation(
        "Support comment added to ticket {TicketNumber} by {Email}",
                ticket.TicketNumber, user.Email);

            StatusMessage = "Comment added.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAssignToMeAsync(int id)
        {
       var user = await _userManager.GetUserAsync(User);
      if (user == null)
   {
        return RedirectToPage("/Identity/Account/Login");
  }

  var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
        {
 return NotFound();
            }

            if (ticket.Status == "Closed" || ticket.Status == "Resolved")
            {
                StatusMessage = "Error: Reopen this ticket before assigning it.";
                return RedirectToPage(new { id });
            }

            if (!string.IsNullOrEmpty(ticket.AssignedToUserId) && ticket.AssignedToUserId != user.Id)
            {
                StatusMessage = "Error: This ticket is already assigned. Use Reassign to transfer it.";
                return RedirectToPage(new { id });
            }

         ticket.AssignedToUserId = user.Id;
   ticket.AssignedToName = !string.IsNullOrEmpty(user.FirstName)
    ? $"{user.FirstName} {user.LastName}"
                : user.Email ?? "Support";
            ticket.UpdatedAt = DateTime.UtcNow;

       if (ticket.Status == "Open")
            {
         ticket.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

      _logger.LogInformation(
     "Ticket {TicketNumber} assigned to {Email}",
  ticket.TicketNumber, user.Email);

            StatusMessage = "Ticket assigned to you.";
   return RedirectToPage(new { id });
   }

        public async Task<IActionResult> OnPostAssignAsync(int id)
        {
            var actor = await _userManager.GetUserAsync(User);
            if (actor == null) return Challenge();

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null) return NotFound();
            if (ticket.Status == "Closed" || ticket.Status == "Resolved")
            {
                StatusMessage = "Error: Reopen this ticket before reassigning it.";
                return RedirectToPage(new { id });
            }

            await LoadRespondersAsync();
            var responder = Responders.FirstOrDefault(r => r.Id == ResponderId);
            if (responder == null)
            {
                StatusMessage = "Error: Select a Support or Admin responder.";
                return RedirectToPage(new { id });
            }

            if (ticket.AssignedToUserId == responder.Id)
            {
                StatusMessage = "This ticket is already assigned to that responder.";
                return RedirectToPage(new { id });
            }

            var previousAssignee = ticket.AssignedToName ?? "Unassigned";
            ticket.AssignedToUserId = responder.Id;
            ticket.AssignedToName = responder.Name;
            ticket.Status = "InProgress";
            ticket.UpdatedAt = DateTime.UtcNow;

            // Keep the handoff visible in the ticket and in the audit log.
            _context.TicketComments.Add(new TicketComment
            {
                TicketId = ticket.Id,
                UserId = actor.Id,
                UserName = DisplayName(actor),
                IsFromSupport = true,
                Content = $"Assigned from {previousAssignee} to {responder.Name} ({responder.Role}).",
                CreatedAt = ticket.UpdatedAt
            });
            _context.AuditLogs.Add(new AuditLog
            {
                Timestamp = ticket.UpdatedAt,
                UserId = actor.Id,
                UserEmail = actor.Email ?? string.Empty,
                Action = "AssignTicket",
                EntityType = "SupportTicket",
                EntityId = ticket.Id.ToString(),
                Details = $"{previousAssignee} -> {responder.Name} ({responder.Role})"
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketNumber} assigned to {ResponderId} by {ActorId}",
                ticket.TicketNumber, responder.Id, actor.Id);
            StatusMessage = $"Ticket assigned to {responder.Name}.";
            return RedirectToPage(new { id });
        }

        private async Task LoadRespondersAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            var support = await _userManager.GetUsersInRoleAsync(AppRoles.Support);
            Responders = admins.Select(u => new Responder(u.Id, DisplayName(u), "Admin"))
                .Concat(support.Select(u => new Responder(u.Id, DisplayName(u), "Support")))
                .DistinctBy(r => r.Id)
                .OrderBy(r => r.Name)
                .ToList();
        }

        private static string DisplayName(ApplicationUser user)
        {
            var name = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrEmpty(name) ? user.Email ?? "Responder" : name;
        }
    }
}
