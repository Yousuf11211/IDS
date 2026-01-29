using Microsoft.EntityFrameworkCore;
using IDS.Data;
using IDS.Data.Models;
using IDS.Hubs;

namespace IDS.Core.Services
{
    /// <summary>
  /// Service for reading live detection data from the database.
  /// The external detection pipeline writes to Benign_Table and Attack_Table.
  /// This service reads from those tables and provides data for the dashboard.
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
        private readonly ILogger<LiveDetectionService> _logger;

        public LiveDetectionService(
     ApplicationDbContext db,
            ILogger<LiveDetectionService> logger)
     {
       _db = db;
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

    // Determine system status based on attack rate
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
