using IDS.Core.Services;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDS.Api;

[ApiController]
[Route("api/detection")]
[Authorize(Roles = AppRoles.AdminOrSupport)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AttackInspectionController(AttackInspectionService service) : ControllerBase
{
    [HttpGet("{id:long}/inspection")]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        if (!AttackInspectionService.CanInspect(User)) return Forbid();
        if (id <= 0) return BadRequest(new { error = "Incident identifier must be positive." });
        var report = await service.LoadAsync(id, User, cancellationToken);
        return report == null ? NotFound() : Ok(report);
    }
}
