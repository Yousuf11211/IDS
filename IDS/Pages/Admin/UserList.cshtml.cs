using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDS.Security;
using IDS.Data.Models;
using IDS.Data.Services;

namespace IDS.Pages.Admin
{
  /// <summary>
    /// Admin page for managing users - view, change roles, suspend/unsuspend, reset passwords, delete.
    /// Only accessible by Admin role users.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class UserListModel : PageModel
  {
      private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccessControlService _accessControl;
        private readonly MailjetEmailSender _emailSender;
        private readonly ILogger<UserListModel> _logger;

        public UserListModel(
            UserManager<ApplicationUser> userManager, 
   AccessControlService accessControl,
            MailjetEmailSender emailSender,
      ILogger<UserListModel> logger)
   {
     _userManager = userManager;
          _accessControl = accessControl;
            _emailSender = emailSender;
            _logger = logger;
        }

    /// <summary>
        /// Represents a user entry for display in the user list.
        /// </summary>
        public class UserEntry
        {
            public string Id { get; set; } = string.Empty;
     public string Email { get; set; } = string.Empty;
            public string CurrentRole { get; set; } = string.Empty;
     public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
     }

      // ============================================
        // Page Properties
      // ============================================
        
        /// <summary>List of users matching current filters.</summary>
        public List<UserEntry> Users { get; set; } = new();

        /// <summary>All available roles for the dropdown.</summary>
        public List<string> AllRoles { get; set; } = AppRoles.All.ToList();
        
        /// <summary>Status message displayed after actions.</summary>
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        // ============================================
        // GET Handler - Load User List
     // ============================================
   
        /// <summary>
        /// Loads the user list with optional role and search filters.
        /// </summary>
     public async Task OnGetAsync(string? role, string? search)
        {
            var allUsers = await _userManager.Users.ToListAsync();
            
       foreach (var user in allUsers)
            {
            var roles = await _userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault(r => AppRoles.IsManagedRole(r)) ?? string.Empty;
         
  // Apply role filter
       if (!string.IsNullOrEmpty(role) && !string.Equals(role, currentRole, StringComparison.OrdinalIgnoreCase))
                {
         continue;
       }
     
     // Apply search filter (email)
            if (!string.IsNullOrEmpty(search) && 
  !(user.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
           {
          continue;
       }
 
 Users.Add(new UserEntry 
         { 
         Id = user.Id, 
             Email = user.Email ?? user.UserName ?? string.Empty, 
     CurrentRole = currentRole, 
             FirstName = user.FirstName ?? string.Empty, 
         LastName = user.LastName ?? string.Empty 
            });
        }
            
   // Sort: Admins first, then Employees, then Suspended, alphabetically within each group
         Users = Users
           .OrderBy(u => u.CurrentRole == "Admin" ? 0 : u.CurrentRole == "Employee" ? 1 : 2)
        .ThenBy(u => u.Email)
              .ToList();
        }

   // ============================================
        // POST Handler - Set Role
        // ============================================
  
        /// <summary>
        /// Changes a user's role. Includes safety checks to prevent self-demotion without confirmation.
        /// </summary>
        public async Task<IActionResult> OnPostSetRoleAsync(string userId, string role)
        {
   // Validate inputs
     if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role) || !AppRoles.IsManagedRole(role))
            {
    StatusMessage = "Error: Invalid input provided.";
      return RedirectToPage();
            }
   
      var targetUser = await _userManager.FindByIdAsync(userId);
   if (targetUser == null)
            {
StatusMessage = "Error: User not found.";
         return RedirectToPage();
            }
  
   var currentUser = await _userManager.GetUserAsync(User);
            var isCurrentUser = currentUser?.Id == userId;
          
   // Safety: Prevent admins from suspending themselves
     if (isCurrentUser && role == "Suspended")
            {
    StatusMessage = "Error: You cannot suspend your own account.";
      _logger.LogWarning("Admin {AdminEmail} attempted to suspend their own account", currentUser?.Email);
         return RedirectToPage();
         }
            
// Log the role change
    var previousRoles = await _userManager.GetRolesAsync(targetUser);
            var previousRole = previousRoles.FirstOrDefault(r => AppRoles.IsManagedRole(r)) ?? "None";
            
     _logger.LogInformation(
   "Role change: {AdminEmail} changed {TargetEmail} from '{OldRole}' to '{NewRole}'",
 currentUser?.Email, targetUser.Email, previousRole, role);
   
  // Apply the role change
      await _accessControl.SetExclusiveRoleAsync(_userManager, targetUser, role);
      
            StatusMessage = $"Successfully updated {targetUser.Email} role to '{role}'.";
       
       // Add extra message if admin changed their own role
            if (isCurrentUser && role != "Admin")
         {
                StatusMessage += " Warning: You have removed your own Admin privileges.";
   }
 
      return RedirectToPage();
    }

        // ============================================
        // POST Handler - Reset Password
        // ============================================
        
        /// <summary>
        /// Resets a user's password and sends them a temporary password via email.
        /// </summary>
        public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
        {
   if (string.IsNullOrWhiteSpace(userId))
          {
    StatusMessage = "Error: Invalid user ID.";
   return RedirectToPage();
            }
   
     var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
            {
             StatusMessage = "Error: User not found.";
         return RedirectToPage();
            }
            
            var currentUser = await _userManager.GetUserAsync(User);
   
            try
          {
        // Generate a secure temporary password
  var tempPassword = GenerateSecurePassword();
   
        // Reset the user's password
     var token = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
                var result = await _userManager.ResetPasswordAsync(targetUser, token, tempPassword);
       
          if (!result.Succeeded)
    {
        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
         StatusMessage = $"Error: Failed to reset password. {errors}";
 _logger.LogError("Password reset failed for {Email}: {Errors}", targetUser.Email, errors);
         return RedirectToPage();
    }
    
              // Mark that user needs to change password on next login
                targetUser.MustChangePassword = true;
   await _userManager.UpdateAsync(targetUser);
       
    // Send the temporary password via email
var userName = !string.IsNullOrEmpty(targetUser.FirstName) 
             ? targetUser.FirstName 
          : targetUser.Email?.Split('@')[0] ?? "User";
          
  var (success, errorMsg) = await _emailSender.SendTempPasswordEmailAsync(
         targetUser.Email ?? string.Empty,
       userName,
 tempPassword
                );
      
      if (success)
           {
            StatusMessage = $"Password reset for {targetUser.Email}. Temporary password sent via email.";
      _logger.LogInformation(
         "Password reset: {AdminEmail} reset password for {TargetEmail}",
              currentUser?.Email, targetUser.Email);
        }
         else
     {
          // Password was reset but email failed - still inform admin
   StatusMessage = $"Password reset for {targetUser.Email}. Note: Email delivery issue - {errorMsg ?? "check email settings"}";
      _logger.LogWarning(
            "Password reset email failed for {Email}: {Error}",
             targetUser.Email, errorMsg);
     }
          }
        catch (Exception ex)
       {
            StatusMessage = $"Error: An unexpected error occurred while resetting password.";
        _logger.LogError(ex, "Unexpected error during password reset for {Email}", targetUser.Email);
            }
    
            return RedirectToPage();
        }

        // ============================================
        // POST Handler - Delete User
        // ============================================
      
        /// <summary>
        /// Permanently deletes a user account. Cannot delete own account.
   /// </summary>
        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
   {
            if (string.IsNullOrWhiteSpace(userId))
       {
                StatusMessage = "Error: Invalid user ID.";
           return RedirectToPage();
          }
  
            var targetUser = await _userManager.FindByIdAsync(userId);
     if (targetUser == null)
   {
   StatusMessage = "Error: User not found.";
    return RedirectToPage();
}
            
            var currentUser = await _userManager.GetUserAsync(User);
            
          // Safety: Prevent admins from deleting themselves
 if (currentUser?.Id == userId)
         {
    StatusMessage = "Error: You cannot delete your own account.";
                _logger.LogWarning("Admin {AdminEmail} attempted to delete their own account", currentUser?.Email);
        return RedirectToPage();
         }
       
  var deletedEmail = targetUser.Email;
            var result = await _userManager.DeleteAsync(targetUser);
            
       if (result.Succeeded)
         {
        StatusMessage = $"Successfully deleted user {deletedEmail}.";
     _logger.LogInformation(
        "User deleted: {AdminEmail} deleted user {DeletedEmail}",
         currentUser?.Email, deletedEmail);
       }
   else
            {
        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            StatusMessage = $"Error: Failed to delete user. {errors}";
       _logger.LogError("User deletion failed for {Email}: {Errors}", deletedEmail, errors);
  }
      
  return RedirectToPage();
        }

        // ============================================
        // Helper Methods
  // ============================================
      
        /// <summary>
   /// Generates a secure temporary password that meets complexity requirements.
        /// </summary>
     private static string GenerateSecurePassword()
     {
 const string upperChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
      const string lowerChars = "abcdefghjkmnpqrstuvwxyz";
            const string digitChars = "23456789";
            const string specialChars = "!@#$%&*";
  
          var random = new Random();
  var password = new char[12];
            
            // Ensure at least one of each required type
            password[0] = upperChars[random.Next(upperChars.Length)];
            password[1] = lowerChars[random.Next(lowerChars.Length)];
            password[2] = digitChars[random.Next(digitChars.Length)];
 password[3] = specialChars[random.Next(specialChars.Length)];
            
  // Fill remaining with random mix
            var allChars = upperChars + lowerChars + digitChars + specialChars;
       for (int i = 4; i < password.Length; i++)
            {
   password[i] = allChars[random.Next(allChars.Length)];
  }
            
  // Shuffle the password
            return new string(password.OrderBy(_ => random.Next()).ToArray());
      }
    }
}