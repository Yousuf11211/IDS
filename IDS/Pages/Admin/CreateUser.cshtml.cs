using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace IDS.Pages.Admin
{
 [Authorize(Roles = "Admin")]
 public class CreateUserModel : PageModel
 {
 private readonly UserManager<IdentityUser> _userManager;

 public CreateUserModel(UserManager<IdentityUser> userManager)
 {
 _userManager = userManager;
 }

 [BindProperty]
 public InputModel Input { get; set; }

 public class InputModel
 {
 public string Email { get; set; } = string.Empty;
 public string Password { get; set; } = string.Empty;
 public string Role { get; set; } = string.Empty;
 }

 public void OnGet() { }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();

 var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };
 var result = await _userManager.CreateAsync(user, Input.Password);
 if (!result.Succeeded)
 {
 foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
 return Page();
 }

 if (!string.IsNullOrWhiteSpace(Input.Role))
 {
 await _userManager.AddToRoleAsync(user, Input.Role);
 }

 return RedirectToPage("/Admin/UserList"); // create a list page or redirect where you prefer
 }
 }
}
