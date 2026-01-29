using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IDS.Data.Models;
using IDS.Core.Services;

namespace IDS.Api
{
    /// <summary>
    /// API Controller for reading detection data (read-only).
    /// The detection pipeline writes directly to the database tables.
    /// This API only provides read access for the dashboard.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DetectionController : ControllerBase
    {
        private readonly ILiveDetectionService _liveDetectionService;
        private readonly ILogger<DetectionController> _logger;

        public DetectionController(
            ILiveDetectionService liveDetectionService,
            ILogger<DetectionController> logger)
        {
    _liveDetectionService = liveDetectionService;
            _logger = logger;
      }

      /// <summary>
/// Get current dashboard statistics.
      /// GET /api/detection/stats
   /// </summary>
      [HttpGet("stats")]
     public async Task<IActionResult> GetStats()
     {
        var stats = await _liveDetectionService.GetCurrentStatsAsync();
      return Ok(stats);
        }

        /// <summary>
/// Get recent detection events for the live feed.
        /// GET /api/detection/recent?count=50
        /// </summary>
  [HttpGet("recent")]
   public async Task<IActionResult> GetRecentEvents([FromQuery] int count = 50)
   {
       var events = await _liveDetectionService.GetRecentEventsAsync(count);
       return Ok(events);
    }

      /// <summary>
     /// Get recent attacks only.
        /// GET /api/detection/attacks?count=20
    /// </summary>
     [HttpGet("attacks")]
    public async Task<IActionResult> GetRecentAttacks([FromQuery] int count = 20)
        {
            var attacks = await _liveDetectionService.GetRecentAttacksAsync(count);
  return Ok(attacks);
        }

        /// <summary>
        /// Get recent benign traffic.
        /// GET /api/detection/benign?count=20
     /// </summary>
 [HttpGet("benign")]
        public async Task<IActionResult> GetRecentBenign([FromQuery] int count = 20)
    {
            var benign = await _liveDetectionService.GetRecentBenignAsync(count);
            return Ok(benign);
        }

      /// <summary>
    /// Get attack statistics grouped by type.
     /// GET /api/detection/stats/by-type?hours=24
 /// </summary>
        [HttpGet("stats/by-type")]
        public async Task<IActionResult> GetAttacksByType([FromQuery] int hours = 24)
        {
    var since = DateTime.UtcNow.AddHours(-hours);
var stats = await _liveDetectionService.GetAttacksByTypeAsync(since);
         return Ok(stats);
      }

  /// <summary>
        /// Get attack statistics grouped by severity.
        /// GET /api/detection/stats/by-severity?hours=24
        /// </summary>
[HttpGet("stats/by-severity")]
        public async Task<IActionResult> GetAttacksBySeverity([FromQuery] int hours = 24)
        {
      var since = DateTime.UtcNow.AddHours(-hours);
      var stats = await _liveDetectionService.GetAttacksBySeverityAsync(since);
      return Ok(stats);
    }

        /// <summary>
        /// Health check endpoint.
        /// GET /api/detection/health
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
  public IActionResult Health()
 {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
      }
    }
}
