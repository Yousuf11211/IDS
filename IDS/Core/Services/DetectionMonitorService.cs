using Microsoft.EntityFrameworkCore;
using IDS.Data;
using IDS.Data.Models;
using IDS.Hubs;

namespace IDS.Core.Services
{
    /// <summary>
    /// Background service that monitors the Benign_Table and Attack_Table for new entries
    /// and broadcasts updates to connected dashboard clients via SignalR.
    /// 
    /// This service is READ-ONLY - it monitors tables that are written to by an external
    /// detection pipeline (Python/ML model).
    /// </summary>
    public class DetectionMonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DetectionMonitorService> _logger;
        private readonly TimeSpan _pollingInterval;
        private readonly TimeSpan _statsUpdateInterval;
        
        // Track the last ID we've seen to detect new entries
        private long _lastBenignId = 0;
        private long _lastAttackId = 0;
        private DateTime _lastStatsUpdate = DateTime.MinValue;

        public DetectionMonitorService(
            IServiceProvider serviceProvider,
            ILogger<DetectionMonitorService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
  // Read polling interval from config (default 2 seconds)
            var pollSeconds = configuration.GetValue<int>("Detection:PollingIntervalSeconds", 2);
            _pollingInterval = TimeSpan.FromSeconds(pollSeconds);
 
            // Stats update interval (default 10 seconds)
    var statsSeconds = configuration.GetValue<int>("Detection:StatsUpdateIntervalSeconds", 10);
            _statsUpdateInterval = TimeSpan.FromSeconds(statsSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
    "Detection Monitor Service started. Polling every {Interval}s for new data in Benign_Table and Attack_Table", 
       _pollingInterval.TotalSeconds);

 // Wait a bit for the application to fully start
       await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

         // Initialize last IDs
await InitializeLastIdsAsync();

while (!stoppingToken.IsCancellationRequested)
   {
try
 {
      await CheckForNewDetectionsAsync(stoppingToken);
  
       // Periodically broadcast stats update
          if (DateTime.UtcNow - _lastStatsUpdate > _statsUpdateInterval)
    {
     await BroadcastStatsUpdateAsync();
      _lastStatsUpdate = DateTime.UtcNow;
     }
           }
catch (OperationCanceledException)
                {
       // Graceful shutdown
        break;
     }
                catch (Exception ex)
   {
    _logger.LogError(ex, "Error in Detection Monitor Service");
     }

       await Task.Delay(_pollingInterval, stoppingToken);
}

            _logger.LogInformation("Detection Monitor Service stopped");
        }

private async Task InitializeLastIdsAsync()
     {
          try
     {
      using var scope = _serviceProvider.CreateScope();
     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

           // Get the maximum IDs currently in the tables
    try
   {
      _lastBenignId = await db.BenignTraffic.MaxAsync(t => (long?)t.Id) ?? 0;
         _logger.LogInformation("Initialized last Benign ID: {BenignId}", _lastBenignId);
       }
catch (Exception ex)
       {
          _lastBenignId = 0;
     _logger.LogWarning("Could not get last Benign ID (table may not exist): {Message}", ex.Message);
     }

       try
    {
     _lastAttackId = await db.AttackTraffic.MaxAsync(t => (long?)t.Id) ?? 0;
         _logger.LogInformation("Initialized last Attack ID: {AttackId}", _lastAttackId);
    }
   catch (Exception ex)
  {
 _lastAttackId = 0;
      _logger.LogWarning("Could not get last Attack ID (table may not exist): {Message}", ex.Message);
 }
    }
            catch (Exception ex)
 {
         _logger.LogWarning(ex, "Error initializing - will retry on next poll");
    }
   }

        private async Task CheckForNewDetectionsAsync(CancellationToken stoppingToken)
      {
       using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  var notificationService = scope.ServiceProvider.GetRequiredService<IDashboardNotificationService>();

  int newBenignCount = 0;
       int newAttackCount = 0;

  // Check for new benign traffic
    try
   {
     var newBenign = await db.BenignTraffic
.AsNoTracking()
   .Where(t => t.Id > _lastBenignId)
     .OrderBy(t => t.Id)
  .Take(100) // Limit to prevent flooding
       .ToListAsync(stoppingToken);

       foreach (var traffic in newBenign)
       {
      var detectionEvent = new LiveDetectionEvent
 {
       Id = traffic.Id,
       DetectedAt = traffic.DetectedAt,
     SourceIP = traffic.SourceIP,
    DestinationIP = traffic.DestinationIP,
       Protocol = traffic.Protocol,
  IsAttack = false,
      ConfidenceScore = traffic.ConfidenceScore
      };

       await notificationService.BroadcastDetectionEventAsync(detectionEvent);
         _lastBenignId = traffic.Id;
          newBenignCount++;
  }
            }
 catch (Exception ex)
       {
 // Don't log every poll if table doesn't exist
     if (!ex.Message.Contains("Invalid object name"))
      {
     _logger.LogWarning(ex, "Error checking for new benign traffic");
      }
        }

   // Check for new attacks
 try
   {
        var newAttacks = await db.AttackTraffic
       .AsNoTracking()
    .Where(t => t.Id > _lastAttackId)
         .OrderBy(t => t.Id)
   .Take(100) // Limit to prevent flooding
     .ToListAsync(stoppingToken);

   foreach (var attack in newAttacks)
    {
  var detectionEvent = new LiveDetectionEvent
            {
              Id = attack.Id,
 DetectedAt = attack.DetectedAt,
        SourceIP = attack.SourceIP,
  DestinationIP = attack.DestinationIP,
       Protocol = attack.Protocol,
    IsAttack = true,
  AttackType = attack.AttackType,
            Severity = attack.Severity,
      ConfidenceScore = attack.ConfidenceScore
        };

   // Broadcast to all users
      await notificationService.BroadcastDetectionEventAsync(detectionEvent);
        
        // Broadcast attack alert to admins/support
       await notificationService.BroadcastAttackAlertAsync(attack);

         _lastAttackId = attack.Id;
  newAttackCount++;
          }
      }
    catch (Exception ex)
        {
     // Don't log every poll if table doesn't exist
          if (!ex.Message.Contains("Invalid object name"))
     {
        _logger.LogWarning(ex, "Error checking for new attacks");
  }
            }

   // Log if we found new data
        if (newBenignCount > 0 || newAttackCount > 0)
    {
          _logger.LogInformation("Broadcasted {BenignCount} new benign, {AttackCount} new attacks", 
     newBenignCount, newAttackCount);
         }
 }

  private async Task BroadcastStatsUpdateAsync()
      {
      try
       {
            using var scope = _serviceProvider.CreateScope();
     var liveDetectionService = scope.ServiceProvider.GetRequiredService<ILiveDetectionService>();
   var notificationService = scope.ServiceProvider.GetRequiredService<IDashboardNotificationService>();

                var stats = await liveDetectionService.GetCurrentStatsAsync();
     await notificationService.BroadcastStatsUpdateAsync(stats);
     }
 catch (Exception ex)
     {
     _logger.LogWarning(ex, "Error broadcasting stats update");
   }
        }
    }
}
