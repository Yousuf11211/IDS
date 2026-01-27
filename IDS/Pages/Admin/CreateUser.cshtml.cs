using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Linq;
using IDS.Data;
using IDS.Data.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IDS.Security;
using IDS.Data.Services;

namespace IDS.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateUserModel : PageModel
  {
        private readonly UserManager<ApplicationUser> _userManager;
     private readonly RoleManager<IdentityRole> _roleManager;
   private readonly ApplicationDbContext _db;
private readonly AccessControlService _accessControl;
   private readonly MailjetEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public CreateUserModel(
            UserManager<ApplicationUser> userManager,
      RoleManager<IdentityRole> roleManager,
     ApplicationDbContext db,
        AccessControlService accessControl,
            MailjetEmailSender emailSender,
            IConfiguration configuration)
   {
    _userManager = userManager;
         _roleManager = roleManager;
            _db = db;
   _accessControl = accessControl;
      _emailSender = emailSender;
            _configuration = configuration;
   }

        [BindProperty]
        public InputModel Input { get; set; } = new();

     public List<string> AvailableRoles { get; set; } = AppRoles.All.ToList();
   
        /// <summary>
     /// Indicates whether email sending is configured and enabled.
    /// </summary>
        public bool EmailEnabled => _emailSender.IsConfigured;

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
       public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
       public string Role { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
      public string LastName { get; set; } = string.Empty;
  }

        public async Task OnGetAsync()
        {
    AvailableRoles = AppRoles.All.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
 {
       if (!ModelState.IsValid)
            {
   await OnGetAsync();
         return Page();
   }

    // Create the user
       var user = new ApplicationUser
       {
       UserName = Input.Email,
           Email = Input.Email,
    EmailConfirmed = true,
    FirstName = Input.FirstName,
      LastName = Input.LastName,
         MustChangePassword = true // Force password change on first login
  };

       var result = await _userManager.CreateAsync(user, Input.Password);
  if (!result.Succeeded)
          {
      foreach (var e in result.Errors)
     {
        ModelState.AddModelError(string.Empty, e.Description);
       }
     await OnGetAsync();
  return Page();
  }

         // Assign role
      if (!string.IsNullOrWhiteSpace(Input.Role))
 {
      await _accessControl.EnsureRoleExistsAsync(Input.Role);
        await _accessControl.SetExclusiveRoleAsync(_userManager, user, Input.Role);
 }

            // Send welcome email with temporary password using professional template
            var loginUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/Login";
            var userName = !string.IsNullOrEmpty(Input.FirstName) ? Input.FirstName : Input.Email.Split('@')[0];
       
            var (emailSuccess, emailError) = await _emailSender.SendTempPasswordEmailAsync(
                toEmail: user.Email!,
                userName: userName,
                temporaryPassword: Input.Password,
                loginUrl: loginUrl
            );

            if (emailSuccess)
            {
                StatusMessage = $"User {user.Email} created successfully. A welcome email has been sent.";
            }
            else
            {
                // If we know email is supposed to be enabled, show the error
                if (EmailEnabled)
                    StatusMessage = $"User created, but email failed: {emailError}. Please send credentials manually.";
                else
                    StatusMessage = $"User created. Email notifications are disabled: {emailError}";
            }

            return RedirectToPage("/Admin/UserList");
  }
}
}
