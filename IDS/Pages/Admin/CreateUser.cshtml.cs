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

namespace IDS.Pages.Admin
{
 [Authorize(Roles = "Admin")]
 public class CreateUserModel : PageModel
 {
 private readonly UserManager<IdentityUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;
 private readonly ApplicationDbContext _db;
 private readonly AccessControlService _accessControl;

 public CreateUserModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db, AccessControlService accessControl)
 {
 _userManager = userManager;
 _roleManager = roleManager;
 _db = db;
 _accessControl = accessControl;
 }

 [BindProperty]
 public InputModel Input { get; set; }

 public List<string> AvailableRoles { get; set; } = AppRoles.All.ToList();

 public class InputModel
 {
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 public string Role { get; set; } = string.Empty;
 }

 public async Task OnGetAsync()
 {
 AvailableRoles = AppRoles.All.ToList();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();

 var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };
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

 return RedirectToPage("/Admin/UserList");
 }
 }
}
