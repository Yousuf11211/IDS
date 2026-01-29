using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using IDS.Data.Models;

namespace IDS.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time dashboard updates.
    /// Broadcasts detection events and statistics to connected clients.
    /// </summary>
    [Authorize]
    public class DashboardHub : Hub
{
        private readonly ILogger<DashboardHub> _logger;

        public DashboardHub(ILogger<DashboardHub> logger)
   {
     _logger = logger;
     }

        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
 var user = Context.User?.Identity?.Name ?? "Anonymous";
      _logger.LogInformation("Client connected to DashboardHub: {User} ({ConnectionId})", user, Context.ConnectionId);
        
    // Add user to their role-based groups
            if (Context.User?.IsInRole("Admin") == true)
   {
  await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
 if (Context.User?.IsInRole("Support") == true)
    {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Support");
            }
         
 // Everyone goes into the "Users" group
    await Groups.AddToGroupAsync(Context.ConnectionId, "Users");
    
  await base.OnConnectedAsync();
   }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
public override async Task OnDisconnectedAsync(Exception? exception)
      {
    var user = Context.User?.Identity?.Name ?? "Anonymous";
    _logger.LogInformation("Client disconnected from DashboardHub: {User} ({ConnectionId})", user, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
      /// Client can call this to request the latest stats.
        /// </summary>
        public async Task RequestLatestStats()
        {
      // The service will broadcast to this client
         await Clients.Caller.SendAsync("StatsRequested", Context.ConnectionId);
 }

        /// <summary>
        /// Client can subscribe to specific attack types.
        /// </summary>
        public async Task SubscribeToAttackType(string attackType)
        {
     await Groups.AddToGroupAsync(Context.ConnectionId, $"AttackType_{attackType}");
      _logger.LogInformation("Client {ConnectionId} subscribed to attack type: {AttackType}", Context.ConnectionId, attackType);
        }

    /// <summary>
        /// Client can unsubscribe from specific attack types.
    /// </summary>
      public async Task UnsubscribeFromAttackType(string attackType)
   {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"AttackType_{attackType}");
        }
    }

    /// <summary>
    /// Interface for sending dashboard updates from services.
    /// </summary>
    public interface IDashboardNotificationService
    {
  /// <summary>
  /// Broadcast new detection event to all connected clients.
        /// </summary>
        Task BroadcastDetectionEventAsync(LiveDetectionEvent detectionEvent);

        /// <summary>
      /// Broadcast updated statistics to all connected clients.
  /// </summary>
        Task BroadcastStatsUpdateAsync(LiveDashboardStats stats);

 /// <summary>
        /// Broadcast attack alert to admins only.
        /// </summary>
    Task BroadcastAttackAlertAsync(AttackTraffic attack);

        /// <summary>
   /// Broadcast to specific attack type subscribers.
 /// </summary>
 Task BroadcastToAttackTypeSubscribersAsync(string attackType, LiveDetectionEvent detectionEvent);
    }

    /// <summary>
    /// Service for broadcasting dashboard updates via SignalR.
    /// </summary>
    public class DashboardNotificationService : IDashboardNotificationService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly ILogger<DashboardNotificationService> _logger;

        public DashboardNotificationService(
            IHubContext<DashboardHub> hubContext,
    ILogger<DashboardNotificationService> logger)
        {
   _hubContext = hubContext;
         _logger = logger;
        }

        public async Task BroadcastDetectionEventAsync(LiveDetectionEvent detectionEvent)
        {
    try
            {
                // Broadcast to all authenticated users
         await _hubContext.Clients.Group("Users").SendAsync("NewDetection", detectionEvent);
                
     _logger.LogDebug("Broadcasted detection event: {IsAttack} from {SourceIP}", 
         detectionEvent.IsAttack ? "Attack" : "Benign", detectionEvent.SourceIP);
  }
            catch (Exception ex)
            {
   _logger.LogError(ex, "Error broadcasting detection event");
  }
   }

        public async Task BroadcastStatsUpdateAsync(LiveDashboardStats stats)
        {
         try
    {
            await _hubContext.Clients.Group("Users").SendAsync("StatsUpdate", stats);
  _logger.LogDebug("Broadcasted stats update: {Benign} benign, {Attacks} attacks", 
        stats.TotalBenign, stats.TotalAttacks);
 }
            catch (Exception ex)
            {
        _logger.LogError(ex, "Error broadcasting stats update");
  }
        }

      public async Task BroadcastAttackAlertAsync(AttackTraffic attack)
        {
     try
         {
     // Only send to Admins and Support
           var alertData = new
        {
        attack.Id,
       attack.DetectedAt,
           attack.SourceIP,
  attack.DestinationIP,
   attack.Protocol,
    attack.AttackType,
       attack.AttackCategory,
         attack.Severity,
         attack.ConfidenceScore,
            attack.Service
          };

                await _hubContext.Clients.Group("Admins").SendAsync("AttackAlert", alertData);
                await _hubContext.Clients.Group("Support").SendAsync("AttackAlert", alertData);
                
          _logger.LogInformation("Broadcasted attack alert: {AttackType} ({Severity}) from {SourceIP}", 
         attack.AttackType, attack.Severity, attack.SourceIP);
            }
          catch (Exception ex)
            {
       _logger.LogError(ex, "Error broadcasting attack alert");
  }
        }

        public async Task BroadcastToAttackTypeSubscribersAsync(string attackType, LiveDetectionEvent detectionEvent)
        {
      try
            {
     await _hubContext.Clients.Group($"AttackType_{attackType}").SendAsync("AttackTypeEvent", detectionEvent);
      }
            catch (Exception ex)
    {
       _logger.LogError(ex, "Error broadcasting to attack type subscribers");
            }
        }
    }
}
