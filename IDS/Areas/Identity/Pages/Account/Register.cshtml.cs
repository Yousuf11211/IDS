using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

// Employee accounts are created through the single admin invitation flow.
[Authorize(Roles = AppRoles.Admin)]
public class RegisterModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Admin/CreateUser", new { area = "" });

    public IActionResult OnPost() => RedirectToPage("/Admin/CreateUser", new { area = "" });
}
