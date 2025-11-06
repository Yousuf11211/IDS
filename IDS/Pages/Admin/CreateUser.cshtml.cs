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
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IDS.Pages.Admin
{
 [Authorize(Roles = "Admin")]
 public class CreateUserModel : PageModel
 {
 private readonly UserManager<ApplicationUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;
 private readonly ApplicationDbContext _db;
 private readonly AccessControlService _accessControl;
 private readonly IEmailSender _emailSender;

 public CreateUserModel(
 UserManager<ApplicationUser> userManager,
 RoleManager<IdentityRole> roleManager,
 ApplicationDbContext db,
 AccessControlService accessControl,
 IEmailSender emailSender)
 {
 _userManager = userManager;
 _roleManager = roleManager;
 _db = db;
 _accessControl = accessControl;
 _emailSender = emailSender;
 }

 [BindProperty]
 public InputModel Input { get; set; }

 public List<string> AvailableRoles { get; set; } = AppRoles.All.ToList();

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
 if (!ModelState.IsValid) return Page();

 var user = new ApplicationUser {
 UserName = Input.Email,
 Email = Input.Email,
 EmailConfirmed = true,
 FirstName = Input.FirstName,
 LastName = Input.LastName
 };
 var result = await _userManager.CreateAsync(user, Input.Password);
 if (!result.Succeeded)
 {
 foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
 await OnGetAsync();
 return Page();
 }

 if (!string.IsNullOrWhiteSpace(Input.Role))
 {
 await _accessControl.EnsureRoleExistsAsync(Input.Role);
 await _accessControl.SetExclusiveRoleAsync(_userManager, user, Input.Role);
 }

 await _emailSender.SendEmailAsync(
 user.Email,
 "Welcome to IDS",
 $@"<p>A user account was created for you in the IDS system.</p>\n<p>You can log in with your email address and the temporary password below:</p>\n<ul>\n <li><strong>Email:</strong> {user.Email}</li>\n <li><strong>Temporary Password:</strong> {Input.Password}</li>\n</ul>\n<p>For security, please log in and change your password as soon as possible.</p>");

 return RedirectToPage("/Admin/UserList");
 }
 }
}
