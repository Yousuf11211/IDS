using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IDS.Core.Services;

namespace IDS.Pages
{
    /// <summary>
    /// Live Dashboard with real-time updates via SignalR.
 /// Shows live feed of detection events from the IDS pipeline.
    /// </summary>
    [Authorize]
    public class LiveDashboardModel : PageModel
    {
    private readonly ILiveDetectionService _liveDetectionService;

     public LiveDashboardModel(
            ILiveDetectionService liveDetectionService)
 {
            _liveDetectionService = liveDetectionService;
     }

      /// <summary>
  /// API endpoint to get current stats (for manual refresh).
        /// </summary>
      public async Task<JsonResult> OnGetStatsAsync()
   {
       var stats = await _liveDetectionService.GetCurrentStatsAsync();
       return new JsonResult(stats);
        }

        /// <summary>
     /// API endpoint to get recent events.
        /// </summary>
 public async Task<JsonResult> OnGetEventsAsync(int count = 50)
   {
            var events = await _liveDetectionService.GetRecentEventsAsync(Math.Clamp(count, 1, 100));
            return new JsonResult(events);
        }

        /// <summary>
        /// API endpoint to get recent attacks (admin only).
        /// </summary>
        public async Task<IActionResult> OnGetAttacksAsync(int count = 20)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Support")) return new ForbidResult();

            var attacks = await _liveDetectionService.GetRecentAttacksAsync(Math.Clamp(count, 1, 100));
            return new JsonResult(attacks);
        }
    }
}
