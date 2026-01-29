using Microsoft.EntityFrameworkCore;
using IDS.Data;
using IDS.Data.Models;
using IDS.Hubs;

namespace IDS.Core.Services
{
    /// <summary>
    /// Service for managing live detection data from the external pipeline.
    /// Handles reading from Benign_Table and Attack_Table and notifying connected clients.
    /// </summary>
    public interface ILiveDetectionService
    {
   /// <summary>
        /// Gets the current dashboard statistics.
     /// </summary>
        Task<LiveDashboardStats> GetCurrentStatsAsync();

        /// <summary>
      /// Gets recent detection events for the live feed.
   /// </summary>
   Task<List<LiveDetectionEvent>> GetRecentEventsAsync(int count = 50);

/// <summary>
        /// Gets recent attacks for display.
        /// </summary>
     Task<List<AttackTraffic>> GetRecentAttacksAsync(int count = 20);

        /// <summary>
  /// Gets recent benign traffic for display.
        /// </summary>
   Task<List<BenignTraffic>> GetRecentBenignAsync(int count = 20);

        /// <summary>
        /// Called by the detection pipeline to register a new benign traffic event.
        /// </summary>
        Task<BenignTraffic> RegisterBenignTrafficAsync(BenignTraffic traffic);

        /// <summary>
        /// Called by the detection pipeline to register a new attack event.
        /// </summary>
        Task<AttackTraffic> RegisterAttackAsync(AttackTraffic attack);

     /// <summary>
        /// Gets attack statistics grouped by type.
        /// </summary>
        Task<Dictionary<string, int>> GetAttacksByTypeAsync(DateTime? since = null);

        /// <summary>
        /// Gets attack statistics grouped by severity.
        /// </summary>
        Task<Dictionary<string, int>> GetAttacksBySeverityAsync(DateTime? since = null);
    }

    public class LiveDetectionService : ILiveDetectionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDashboardNotificationService _notificationService;
        private readonly ILogger<LiveDetectionService> _logger;

        public LiveDetectionService(
            ApplicationDbContext db,
            IDashboardNotificationService notificationService,
         ILogger<LiveDetectionService> logger)
        {
          _db = db;
            _notificationService = notificationService;
      _logger = logger;
 }

        public async Task<LiveDashboardStats> GetCurrentStatsAsync()
  {
            var now = DateTime.UtcNow;
          var last24h = now.AddHours(-24);
  var lastHour = now.AddHours(-1);

    // Get counts - use try/catch in case tables don't exist yet
            long totalBenign = 0, totalAttacks = 0;
            long benign24h = 0, attacks24h = 0;
            long benignHour = 0, attacksHour = 0;

     try
   {
totalBenign = await _db.BenignTraffic.LongCountAsync();
  totalAttacks = await _db.AttackTraffic.LongCountAsync();
           
         benign24h = await _db.BenignTraffic.LongCountAsync(t => t.DetectedAt >= last24h);
         attacks24h = await _db.AttackTraffic.LongCountAsync(t => t.DetectedAt >= last24h);
         
    benignHour = await _db.BenignTraffic.LongCountAsync(t => t.DetectedAt >= lastHour);
    attacksHour = await _db.AttackTraffic.LongCountAsync(t => t.DetectedAt >= lastHour);
      }
            catch (Exception ex)
            {
   _logger.LogWarning(ex, "Error getting detection counts - tables may not exist yet");
       }

            var total = totalBenign + totalAttacks;
  var attackPercentage = total > 0 ? (double)totalAttacks / total * 100 : 0;

   // Determine system status
            string status;
          if (attacksHour > 100)
                status = "Critical";
            else if (attacksHour > 50 || attackPercentage > 30)
       status = "Warning";
    else if (attacksHour > 10 || attackPercentage > 10)
      status = "Elevated";
        else
          status = "Normal";

            var attacksByType = await GetAttacksByTypeAsync(last24h);
         var attacksBySeverity = await GetAttacksBySeverityAsync(last24h);

return new LiveDashboardStats
   {
                TotalBenign = totalBenign,
   TotalAttacks = totalAttacks,
   BenignLast24h = benign24h,
        AttacksLast24h = attacks24h,
        BenignLastHour = benignHour,
  AttacksLastHour = attacksHour,
       AttackPercentage = Math.Round(attackPercentage, 2),
         SystemStatus = status,
       LastUpdated = now,
    AttacksByType = attacksByType,
                AttacksBySeverity = attacksBySeverity
    };
        }

        public async Task<List<LiveDetectionEvent>> GetRecentEventsAsync(int count = 50)
        {
    var events = new List<LiveDetectionEvent>();

        try
        {
                // Get recent benign
        var benign = await _db.BenignTraffic
          .AsNoTracking()
   .OrderByDescending(t => t.DetectedAt)
.Take(count / 2)
   .Select(t => new LiveDetectionEvent
       {
            Id = t.Id,
  DetectedAt = t.DetectedAt,
      SourceIP = t.SourceIP,
         DestinationIP = t.DestinationIP,
       Protocol = t.Protocol,
                 IsAttack = false,
  ConfidenceScore = t.ConfidenceScore
   })
       .ToListAsync();

              // Get recent attacks
       var attacks = await _db.AttackTraffic
             .AsNoTracking()
 .OrderByDescending(t => t.DetectedAt)
     .Take(count / 2)
              .Select(t => new LiveDetectionEvent
         {
          Id = t.Id,
          DetectedAt = t.DetectedAt,
      SourceIP = t.SourceIP,
       DestinationIP = t.DestinationIP,
       Protocol = t.Protocol,
              IsAttack = true,
        AttackType = t.AttackType,
   Severity = t.Severity,
        ConfidenceScore = t.ConfidenceScore
        })
       .ToListAsync();

        events.AddRange(benign);
         events.AddRange(attacks);

     // Sort by time descending
 events = events.OrderByDescending(e => e.DetectedAt).Take(count).ToList();
            }
catch (Exception ex)
            {
    _logger.LogWarning(ex, "Error getting recent events - tables may not exist yet");
    }

   return events;
  }

        public async Task<List<AttackTraffic>> GetRecentAttacksAsync(int count = 20)
   {
            try
 {
      return await _db.AttackTraffic
           .AsNoTracking()
     .OrderByDescending(t => t.DetectedAt)
            .Take(count)
   .ToListAsync();
            }
            catch (Exception ex)
     {
 _logger.LogWarning(ex, "Error getting recent attacks");
        return new List<AttackTraffic>();
            }
        }

        public async Task<List<BenignTraffic>> GetRecentBenignAsync(int count = 20)
        {
            try
  {
         return await _db.BenignTraffic
            .AsNoTracking()
           .OrderByDescending(t => t.DetectedAt)
          .Take(count)
       .ToListAsync();
     }
 catch (Exception ex)
         {
     _logger.LogWarning(ex, "Error getting recent benign traffic");
     return new List<BenignTraffic>();
      }
        }

        public async Task<BenignTraffic> RegisterBenignTrafficAsync(BenignTraffic traffic)
        {
   traffic.DetectedAt = DateTime.UtcNow;

       _db.BenignTraffic.Add(traffic);
            await _db.SaveChangesAsync();

         // Notify connected clients
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

      await _notificationService.BroadcastDetectionEventAsync(detectionEvent);

  _logger.LogDebug("Registered benign traffic from {SourceIP} to {DestIP}", 
      traffic.SourceIP, traffic.DestinationIP);

            return traffic;
        }

        public async Task<AttackTraffic> RegisterAttackAsync(AttackTraffic attack)
    {
       attack.DetectedAt = DateTime.UtcNow;
 
            _db.AttackTraffic.Add(attack);
   await _db.SaveChangesAsync();

    // Notify connected clients
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

            // Broadcast to everyone
            await _notificationService.BroadcastDetectionEventAsync(detectionEvent);
    
// Broadcast attack alert to admins
            await _notificationService.BroadcastAttackAlertAsync(attack);

         // Broadcast to attack type subscribers
          await _notificationService.BroadcastToAttackTypeSubscribersAsync(attack.AttackType, detectionEvent);

    _logger.LogInformation("Registered attack: {AttackType} ({Severity}) from {SourceIP}", 
             attack.AttackType, attack.Severity, attack.SourceIP);

    return attack;
        }

        public async Task<Dictionary<string, int>> GetAttacksByTypeAsync(DateTime? since = null)
        {
     try
      {
    var query = _db.AttackTraffic.AsNoTracking();
                
    if (since.HasValue)
         query = query.Where(t => t.DetectedAt >= since.Value);

  return await query
      .GroupBy(t => t.AttackType)
        .Select(g => new { Type = g.Key, Count = g.Count() })
.ToDictionaryAsync(x => x.Type, x => x.Count);
          }
        catch
            {
     return new Dictionary<string, int>();
    }
        }

        public async Task<Dictionary<string, int>> GetAttacksBySeverityAsync(DateTime? since = null)
{
            try
          {
        var query = _db.AttackTraffic.AsNoTracking();
            
    if (since.HasValue)
  query = query.Where(t => t.DetectedAt >= since.Value);

                return await query
               .GroupBy(t => t.Severity)
    .Select(g => new { Severity = g.Key, Count = g.Count() })
   .ToDictionaryAsync(x => x.Severity, x => x.Count);
      }
            catch
   {
     return new Dictionary<string, int>();
       }
        }
    }
}
