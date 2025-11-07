using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account
{
 [Authorize]
 public class FirstTimeSetupModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly SignInManager<ApplicationUser> _signInManager;

 public FirstTimeSetupModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
 {
 _userManager = userManager;
 _signInManager = signInManager;
 }

 [BindProperty]
 public InputModel Input { get; set; } = new();

 public class InputModel
 {
 [Required]
 [Display(Name = "Username")]
 public string UserName { get; set; } = string.Empty;

 [Required]
 [DataType(DataType.Password)]
 [Display(Name = "New password")]
 [StringLength(100, MinimumLength =6, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
 public string NewPassword { get; set; } = string.Empty;

 [Required]
 [DataType(DataType.Password)]
 [Display(Name = "Confirm new password")]
 [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
 public string ConfirmNewPassword { get; set; } = string.Empty;

 [Display(Name = "First name")]
 public string? FirstName { get; set; }

 [Display(Name = "Last name")]
 public string? LastName { get; set; }
 }

 public async Task<IActionResult> OnGetAsync()
 {
 var user = await _userManager.GetUserAsync(User);
 if (user == null) return RedirectToPage("/Account/Login");

 // If already completed, skip
 if (!user.MustChangePassword)
 {
 return RedirectToPage("/Index", new { area = "" });
 }

 Input = new InputModel
 {
 UserName = user.UserName ?? string.Empty,
 FirstName = user.FirstName,
 LastName = user.LastName
 };
 return Page();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();

 var user = await _userManager.GetUserAsync(User);
 if (user == null) return RedirectToPage("/Account/Login");

 // Update profile fields
 user.UserName = Input.UserName;
 user.FirstName = Input.FirstName ?? string.Empty;
 user.LastName = Input.LastName ?? string.Empty;

 // Reset password without asking for current password
 var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
 var pwdResult = await _userManager.ResetPasswordAsync(user, resetToken, Input.NewPassword);
 if (!pwdResult.Succeeded)
 {
 foreach (var e in pwdResult.Errors)
 {
 ModelState.AddModelError(string.Empty, e.Description);
 }
 return Page();
 }

 user.MustChangePassword = false;
 var updateResult = await _userManager.UpdateAsync(user);
 if (!updateResult.Succeeded)
 {
 foreach (var e in updateResult.Errors)
 {
 ModelState.AddModelError(string.Empty, e.Description);
 }
 return Page();
 }

 // Refresh sign-in so updated username is reflected
 await _signInManager.RefreshSignInAsync(user);
 return RedirectToPage("/Index", new { area = "" });
 }
 }
}
