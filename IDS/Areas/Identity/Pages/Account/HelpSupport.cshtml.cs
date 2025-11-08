using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account
{
 [Authorize]
 public class HelpSupportModel : PageModel
 {
 public void OnGet() { }
 }
}
