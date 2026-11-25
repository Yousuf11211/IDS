using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IDS.Core.Services;
using IDS.Data.Models;
using IDS.Data;

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
        private readonly ApplicationDbContext _database;

     public LiveDashboardModel(
            ILiveDetectionService liveDetectionService,
          ILogger<LiveDashboardModel> logger, ApplicationDbContext database)
 {
            _liveDetectionService = liveDetectionService;
        _logger = logger;
        _database = database;
     }

        public LiveDashboardStats? InitialStats { get; set; }
        public List<LiveDetectionEvent> InitialEvents { get; set; } = new();
        public bool EscalationEnabled { get; private set; }

      public async Task OnGetAsync()
        {
            EscalationEnabled = await OperationsSettings.IsEscalationEnabledAsync(_database);
   
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
        public async Task<IActionResult> OnGetAttacksAsync(int count = 20)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Support")) return new ForbidResult();

            var attacks = await _liveDetectionService.GetRecentAttacksAsync(count);
            return new JsonResult(attacks);
        }
    }
}
