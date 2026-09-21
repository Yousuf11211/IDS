using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages;

[Authorize]
public sealed class ChatModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _users;

    public ChatModel(IConfiguration configuration, UserManager<ApplicationUser> users)
    {
        _configuration = configuration;
        _users = users;
    }

    public string CurrentUserId { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_configuration.GetValue<bool>("Chat:Enabled"))
            return NotFound();

        var user = await _users.GetUserAsync(User);
        if (user is null || await _users.IsInRoleAsync(user, "Suspended"))
            return Forbid();

        CurrentUserId = user.Id;
        return Page();
    }
}
