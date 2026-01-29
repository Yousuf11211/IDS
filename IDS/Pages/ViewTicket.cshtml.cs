using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
  if (Ticket.SubmittedByUserId != user.Id && !IsSupport)
  {
  return Forbid();
}

      CanManageTicket = IsSupport;

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
            if (ticket.SubmittedByUserId != user.Id && !IsSupport)
   {
        return Forbid();
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
           : user.Email ?? "Unknown",
           IsFromSupport = IsSupport,
      Content = NewComment.Trim(),
       CreatedAt = DateTime.UtcNow
    };

 _context.TicketComments.Add(comment);

    // Update ticket timestamp
    ticket.UpdatedAt = DateTime.UtcNow;
    
await _context.SaveChangesAsync();

   _logger.LogInformation(
     "Comment added to ticket {TicketNumber} by {Email}",
       ticket.TicketNumber, user.Email);

   StatusMessage = "Comment added successfully.";
            return RedirectToPage(new { id });
     }
    }
}
