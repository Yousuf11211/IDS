using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IDS.Core.Services;
using IDS.Data.Models;

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
        private readonly ILogger<LiveDashboardModel> _logger;

     public LiveDashboardModel(
            ILiveDetectionService liveDetectionService,
          ILogger<LiveDashboardModel> logger)
 {
            _liveDetectionService = liveDetectionService;
        _logger = logger;
     }

        public LiveDashboardStats? InitialStats { get; set; }
        public List<LiveDetectionEvent> InitialEvents { get; set; } = new();
        public bool IsAdmin { get; set; }

      public async Task OnGetAsync()
        {
 IsAdmin = User.IsInRole("Admin");
   
       try
{
        InitialStats = await _liveDetectionService.GetCurrentStatsAsync();
    InitialEvents = await _liveDetectionService.GetRecentEventsAsync(50);
    }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading initial dashboard data");
         InitialStats = new LiveDashboardStats();
            }
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
            var events = await _liveDetectionService.GetRecentEventsAsync(count);
            return new JsonResult(events);
        }

        /// <summary>
        /// API endpoint to get recent attacks (admin only).
   /// </summary>
        [Authorize(Roles = "Admin,Support")]
        public async Task<JsonResult> OnGetAttacksAsync(int count = 20)
        {
            var attacks = await _liveDetectionService.GetRecentAttacksAsync(count);
            return new JsonResult(attacks);
        }
    }
}
