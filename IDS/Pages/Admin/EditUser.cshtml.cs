using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using IDS.Security;
using IDS.Data.Models;
using IDS.Data.Services;

namespace IDS.Pages.Admin
{
  /// <summary>
    /// Edit User page - allows admin to view and modify a specific user's details.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class EditUserModel : PageModel
    {
     private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControl;
        private readonly MailjetEmailSender _emailSender;
        private readonly AccountLinkBuilder _accountLinks;
        private readonly ILogger<EditUserModel> _logger;

        public EditUserModel(
            UserManager<ApplicationUser> userManager,
     AccessControlService accessControl,
  MailjetEmailSender emailSender,
   ILogger<EditUserModel> logger,
   AccountLinkBuilder accountLinks)
      {
 _userManager = userManager;
   _accessControl = accessControl;
_emailSender = emailSender;
_logger = logger;
_accountLinks = accountLinks;
        }

        [BindProperty]
        public UserEditInput Input { get; set; } = new();

 public class UserEditInput
        {
            public string UserId { get; set; } = string.Empty;

       [Required]
   [EmailAddress]
   [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

          [Display(Name = "First Name")]
     [StringLength(100)]
            public string? FirstName { get; set; }

         [Display(Name = "Last Name")]
         [StringLength(100)]
            public string? LastName { get; set; }

        [Required]
            [Display(Name = "Role")]
      public string Role { get; set; } = string.Empty;

            [Required, StringLength(80)]
            public string Department { get; set; } = Departments.General;
   }

     public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
 public string CurrentRole { get; set; } = string.Empty;
     public DateTime? LastLogin { get; set; }
        public bool IsCurrentUser { get; set; }
     public List<string> AllRoles { get; set; } = AppRoles.All.ToList();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
   {
       return RedirectToPage("/Admin/UserList");
            }

            var user = await _userManager.FindByIdAsync(id);
          if (user == null)
            {
                return NotFound();
    }

         var currentUser = await _userManager.GetUserAsync(User);
            IsCurrentUser = currentUser?.Id == id;

   var roles = await _userManager.GetRolesAsync(user);
 CurrentRole = roles.FirstOrDefault(r => AppRoles.IsManagedRole(r)) ?? "None";

        UserEmail = user.Email ?? "";
            UserFullName = !string.IsNullOrEmpty(user.FirstName) 
             ? $"{user.FirstName} {user.LastName}" 
     : user.Email ?? "";

            Input = new UserEditInput
  {
            UserId = user.Id,
         Email = user.Email ?? "",
      FirstName = user.FirstName,
         LastName = user.LastName,
        Role = CurrentRole,
        Department = user.Department
    };

  return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
   if (!Departments.IsKnown(Input.Department))
   {
       ModelState.AddModelError("Input.Department", "Choose a listed department.");
   }
   if (!AppRoles.IsManagedRole(Input.Role))
   {
       ModelState.AddModelError("Input.Role", "Choose a valid role.");
   }
   if (!ModelState.IsValid)
            {
       AllRoles = AppRoles.All.ToList();
       return Page();
            }

          var user = await _userManager.FindByIdAsync(Input.UserId);
    if (user == null)
       {
    StatusMessage = "Error: User not found.";
   return RedirectToPage("/Admin/UserList");
    }

          if (Input.Role == AppRoles.Suspended &&
              !await _userManager.IsInRoleAsync(user, AppRoles.Suspended))
          {
              StatusMessage = "Error: Use Suspend User to suspend an active account.";
              return RedirectToPage(new { id = Input.UserId });
          }

            var currentUser = await _userManager.GetUserAsync(User);
 IsCurrentUser = currentUser?.Id == Input.UserId;

         if (IsCurrentUser && Input.Role != AppRoles.Admin)
         {
             StatusMessage = "Error: You cannot remove your own administrator access.";
             return RedirectToPage(new { id = Input.UserId });
         }

         // Update user details
        user.FirstName = Input.FirstName ?? "";
    user.LastName = Input.LastName ?? "";
    user.Department = Input.Department;

            // Update email if changed
        if (user.Email != Input.Email)
            {
       var emailExists = await _userManager.FindByEmailAsync(Input.Email);
           if (emailExists != null && emailExists.Id != user.Id)
       {
               ModelState.AddModelError("Input.Email", "This email is already in use.");
         AllRoles = AppRoles.All.ToList();
        return Page();
          }
                user.Email = Input.Email;
    user.UserName = Input.Email;
        }

  var updateResult = await _userManager.UpdateAsync(user);
 if (!updateResult.Succeeded)
  {
     foreach (var error in updateResult.Errors)
           {
         ModelState.AddModelError(string.Empty, error.Description);
      }
    AllRoles = AppRoles.All.ToList();
          return Page();
}

        // Update role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
  var currentRole = currentRoles.FirstOrDefault(r => AppRoles.IsManagedRole(r));

            if (currentRole != Input.Role && AppRoles.IsManagedRole(Input.Role))
    {
         // Safety: Prevent admin from removing their own admin role
await _accessControl.SetExclusiveRoleAsync(_userManager, user, Input.Role);
          }

         _logger.LogInformation("User {Email} updated by admin {AdminEmail}", user.Email, currentUser?.Email);
        StatusMessage = $"User {user.Email} has been updated successfully.";

    return RedirectToPage(new { id = Input.UserId });
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Error: User not found.";
                return RedirectToPage("/Admin/UserList");
            }

            if (!_emailSender.IsConfigured || !_accountLinks.IsConfigured)
            {
                StatusMessage = "Error: Configure email and PUBLIC_BASE_URL before sending account links.";
                return RedirectToPage(new { id = userId });
            }

            // Sending a link leaves the current password intact until the user chooses a new one.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = _accountLinks.PasswordResetLink(Url, encodedToken);
            if (link == null)
            {
                StatusMessage = "Error: Could not build the account link. Check PUBLIC_BASE_URL.";
                return RedirectToPage(new { id = userId });
            }

            var name = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email ?? "Employee" : user.FirstName;
            var isInvitation = !user.EmailConfirmed;
            var (sent, error) = await _emailSender.SendAccountAccessLinkAsync(
                user.Email ?? string.Empty, name, link, isInvitation);

            if (sent)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                _logger.LogInformation("Account link sent to {Email} by admin {AdminEmail}", user.Email, currentUser?.Email);
                StatusMessage = isInvitation
                    ? $"Account setup link sent to {user.Email}."
                    : $"Password reset link sent to {user.Email}.";
            }
            else
            {
                _logger.LogWarning("Account email failed for {Email}: {Error}", user.Email, error);
                StatusMessage = $"Error: The account link could not be sent to {user.Email}.";
            }

            return RedirectToPage(new { id = userId });
        }

   public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
   {
    StatusMessage = "Error: User not found.";
        return RedirectToPage("/Admin/UserList");
      }

    var currentUser = await _userManager.GetUserAsync(User);

            // Prevent self-deletion
         if (currentUser?.Id == userId)
            {
    StatusMessage = "Error: You cannot delete your own account.";
  return RedirectToPage(new { id = userId });
          }

            var email = user.Email;
         var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
 _logger.LogInformation("User {Email} deleted by admin {AdminEmail}", email, currentUser?.Email);
    StatusMessage = $"User {email} has been deleted.";
      return RedirectToPage("/Admin/UserList");
         }

 StatusMessage = $"Error: Failed to delete user. {string.Join("; ", result.Errors.Select(e => e.Description))}";
        return RedirectToPage(new { id = userId });
        }

        public async Task<IActionResult> OnPostSuspendAsync(string userId)
        {
 var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
       {
        StatusMessage = "Error: User not found.";
        return RedirectToPage("/Admin/UserList");
       }

   var currentUser = await _userManager.GetUserAsync(User);

     // Prevent self-suspension
  if (currentUser?.Id == userId)
            {
    StatusMessage = "Error: You cannot suspend your own account.";
                return RedirectToPage(new { id = userId });
       }

         await _accessControl.SetExclusiveRoleAsync(_userManager, user, AppRoles.Suspended);

            _logger.LogInformation("User {Email} suspended by admin {AdminEmail}", user.Email, currentUser?.Email);
          StatusMessage = $"User {user.Email} has been suspended.";

  return RedirectToPage(new { id = userId });
        }

    public async Task<IActionResult> OnPostUnsuspendAsync(string userId, string newRole)
        {
   var user = await _userManager.FindByIdAsync(userId);
  if (user == null)
            {
       StatusMessage = "Error: User not found.";
            return RedirectToPage("/Admin/UserList");
   }

            if (string.IsNullOrEmpty(newRole) || !AppRoles.IsManagedRole(newRole) || newRole == AppRoles.Suspended)
            {
   newRole = AppRoles.Employee;
    }

   var currentUser = await _userManager.GetUserAsync(User);
            await _accessControl.SetExclusiveRoleAsync(_userManager, user, newRole);

     _logger.LogInformation("User {Email} unsuspended to {Role} by admin {AdminEmail}", user.Email, newRole, currentUser?.Email);
            StatusMessage = $"User {user.Email} has been unsuspended with role '{newRole}'.";

    return RedirectToPage(new { id = userId });
        }

    }
}
