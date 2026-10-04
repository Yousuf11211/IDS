using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages;

[Authorize]
public class SettingsModel : PageModel
{
    public void OnGet() { }
}
