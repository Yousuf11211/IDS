using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IDS.Api;

[ApiController]
[Route("api/admin/approvals")]
[Authorize(Roles = AppRoles.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AdminApprovalsController(
    UserManager<ApplicationUser> users, SecurityApprovalNotifications notifications) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        try { return Ok(await notifications.GetAsync(user)); }
        catch (SecurityChangeException) { return Forbid(); }
    }
}
