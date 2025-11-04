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

namespace IDS.Pages.Admin
{
 [Authorize(Roles = "Admin")]
 public class CreateUserModel : PageModel
 {
 private readonly UserManager<IdentityUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;
 private readonly ApplicationDbContext _db;

 public CreateUserModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
 {
 _userManager = userManager;
 _roleManager = roleManager; // fix: variable name
 _db = db;
 }

 [BindProperty]
 public InputModel Input { get; set; }

 public List<string> AvailableRoles { get; set; } = new();

 public class InputModel
 {
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 public string Role { get; set; } = string.Empty;
 }

 public async Task OnGetAsync()
 {
 AvailableRoles = await _db.RolesList.Select(r => r.Name).ToListAsync();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();

 var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };
 var result = await _userManager.CreateAsync(user, Input.Password); // fix: use password overload
 if (!result.Succeeded)
 {
 foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
 await OnGetAsync();
 return Page();
 }

 if (!string.IsNullOrWhiteSpace(Input.Role))
 {
 // Ensure role exists in Identity
 if (!await _roleManager.RoleExistsAsync(Input.Role))
 {
 var roleResult = await _roleManager.CreateAsync(new IdentityRole(Input.Role));
 if (!roleResult.Succeeded)
 {
 foreach (var e in roleResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
 await _userManager.DeleteAsync(user);
 await OnGetAsync();
 return Page();
 }
 }

 // Ensure role exists in Roles table
 if (!await _db.RolesList.AnyAsync(r => r.Name == Input.Role))
 {
 _db.RolesList.Add(new RoleEntry { Name = Input.Role });
 await _db.SaveChangesAsync();
 }

 var addRoleResult = await _userManager.AddToRoleAsync(user, Input.Role);
 if (!addRoleResult.Succeeded)
 {
 foreach (var e in addRoleResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
 await _userManager.DeleteAsync(user);
 await OnGetAsync();
 return Page();
 }
 }

 return RedirectToPage("/Admin/UserList");
 }
 }
}
