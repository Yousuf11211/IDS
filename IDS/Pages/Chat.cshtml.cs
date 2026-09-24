using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages;

[Authorize]
public sealed class ChatModel : PageModel
{
    private readonly SecurityPolicyService _policies;
    private readonly UserManager<ApplicationUser> _users;

    public ChatModel(SecurityPolicyService policies, UserManager<ApplicationUser> users)
    {
        _policies = policies;
        _users = users;
    }

    public string CurrentUserId { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!(await _policies.GetAsync()).MessagingEnabled)
            return NotFound();

        var user = await _users.GetUserAsync(User);
        if (user is null || await _users.IsInRoleAsync(user, "Suspended"))
            return Forbid();

        CurrentUserId = user.Id;
        return Page();
    }
}
