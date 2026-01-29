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
    /// Page for users to submit support tickets.
    /// Accessible by any authenticated user.
    /// </summary>
    [Authorize]
    public class SubmitTicketModel : PageModel
    {
        private readonly ApplicationDbContext _context;
     private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SubmitTicketModel> _logger;

        public SubmitTicketModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SubmitTicketModel> logger)
     {
     _context = context;
    _userManager = userManager;
       _logger = logger;
        }

        [BindProperty]
        public TicketInput Input { get; set; } = new();

        public class TicketInput
     {
         [Required(ErrorMessage = "Subject is required")]
            [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
  [Display(Name = "Subject")]
 public string Subject { get; set; } = string.Empty;

  [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
     public string Category { get; set; } = string.Empty;

          [Required(ErrorMessage = "Priority is required")]
 [Display(Name = "Priority")]
            public string Priority { get; set; } = "Medium";

            [Required(ErrorMessage = "Please describe your issue")]
     [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
     [Display(Name = "Description")]
      public string Description { get; set; } = string.Empty;
      }

        public string[] Categories => TicketConstants.Categories;
        public string[] Priorities => TicketConstants.Priorities;

        [TempData]
  public string? StatusMessage { get; set; }

     public List<SupportTicket> MyTickets { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
      {
   var user = await _userManager.GetUserAsync(User);
            if (user == null)
   {
           return RedirectToPage("/Identity/Account/Login");
     }

    // Load user's existing tickets
            MyTickets = await _context.SupportTickets
          .Where(t => t.SubmittedByUserId == user.Id)
          .OrderByDescending(t => t.CreatedAt)
           .Take(10)
                .ToListAsync();

          return Page();
 }

        public async Task<IActionResult> OnPostAsync()
        {
      var user = await _userManager.GetUserAsync(User);
            if (user == null)
    {
        return RedirectToPage("/Identity/Account/Login");
            }

   if (!ModelState.IsValid)
            {
 MyTickets = await _context.SupportTickets
        .Where(t => t.SubmittedByUserId == user.Id)
     .OrderByDescending(t => t.CreatedAt)
        .Take(10)
    .ToListAsync();
     return Page();
 }

    // Validate category and priority
            if (!TicketConstants.Categories.Contains(Input.Category))
   {
         ModelState.AddModelError("Input.Category", "Invalid category selected");
      return Page();
    }

       if (!TicketConstants.Priorities.Contains(Input.Priority))
            {
      ModelState.AddModelError("Input.Priority", "Invalid priority selected");
       return Page();
    }

            var ticket = new SupportTicket
            {
     TicketNumber = TicketConstants.GenerateTicketNumber(),
    SubmittedByUserId = user.Id,
          SubmittedByEmail = user.Email ?? "",
      SubmittedByName = !string.IsNullOrEmpty(user.FirstName) 
    ? $"{user.FirstName} {user.LastName}" 
 : user.Email ?? "Unknown",
           Subject = Input.Subject.Trim(),
        Description = Input.Description.Trim(),
     Category = Input.Category,
      Priority = Input.Priority,
         Status = "Open",
       CreatedAt = DateTime.UtcNow,
       UpdatedAt = DateTime.UtcNow
         };

  _context.SupportTickets.Add(ticket);
  await _context.SaveChangesAsync();

            _logger.LogInformation(
       "Support ticket {TicketNumber} created by {Email} - {Subject}",
        ticket.TicketNumber, user.Email, ticket.Subject);

StatusMessage = $"Your ticket {ticket.TicketNumber} has been submitted successfully. Our support team will review it shortly.";
   
     return RedirectToPage();
   }
    }
}
