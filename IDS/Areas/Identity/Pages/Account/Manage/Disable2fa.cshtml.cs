using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class Disable2faModel : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    // Two-factor authentication is required for every employee account.
    public IActionResult OnGet() => RedirectToPage("./TwoFactorAuthentication");

    public IActionResult OnPost() => RedirectToPage("./TwoFactorAuthentication");
}
