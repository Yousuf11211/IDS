using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin
{
 [Authorize(Roles = "Admin")]
 public class UserListModel : PageModel
 {
 private readonly UserManager<IdentityUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;

 public UserListModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
 {
 _userManager = userManager;
 _roleManager = roleManager;
 }

 public List<UserEntry> Users { get; set; } = new();
 public List<string> AllRoles { get; set; } = new();

 [TempData]
 public string StatusMessage { get; set; }

 public class UserEntry
 {
 public string Id { get; set; } = string.Empty;
 public string Email { get; set; } = string.Empty;
 public List<string> Roles { get; set; } = new();
 }

 public async Task OnGetAsync()
 {
 AllRoles = await _roleManager.Roles
 .Select(r => r.Name ?? string.Empty)
 .Where(n => n != string.Empty)
 .ToListAsync();

 var users = await _userManager.Users.ToListAsync();

 foreach (var u in users)
 {
 var roles = (await _userManager.GetRolesAsync(u)).ToList();
 Users.Add(new UserEntry
 {
 Id = u.Id,
 Email = u.Email ?? u.UserName ?? string.Empty,
 Roles = roles
 });
 }
 }

 public async Task<IActionResult> OnPostAddRoleAsync(string userId, string role)
 {
 if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
 {
 StatusMessage = "Invalid input.";
 return RedirectToPage();
 }

 var user = await _userManager.FindByIdAsync(userId);
 if (user == null)
 {
 StatusMessage = "User not found.";
 return RedirectToPage();
 }

 if (!await _roleManager.RoleExistsAsync(role))
 {
 await _roleManager.CreateAsync(new IdentityRole(role));
 }

 var result = await _userManager.AddToRoleAsync(user, role);
 StatusMessage = result.Succeeded ? $"Role '{role}' added to {user.Email}." : "Failed to add role.";
 return RedirectToPage();
 }

 public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
 {
 if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
 {
 StatusMessage = "Invalid input.";
 return RedirectToPage();
 }

 var user = await _userManager.FindByIdAsync(userId);
 if (user == null)
 {
 StatusMessage = "User not found.";
 return RedirectToPage();
 }

 var result = await _userManager.RemoveFromRoleAsync(user, role);
 StatusMessage = result.Succeeded ? $"Role '{role}' removed from {user.Email}." : "Failed to remove role.";
 return RedirectToPage();
 }
 }
}
