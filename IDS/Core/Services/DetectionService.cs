using IDS.Data;
using IDS.Data.Models;

namespace IDS.Core.Services
{
 /// <summary>
 /// Stub detection service ready for ML integration
    /// </summary>
    public interface IDetectionService
    {
        Task<(string Classification, string? AttackType, double Confidence)> ClassifyPacketAsync(NetworkEvent networkEvent);
   Task ProcessNetworkEventAsync(NetworkEvent networkEvent);
    Task GenerateAlertAsync(NetworkEvent networkEvent, string attackType, double confidence);
    }

    public class DetectionService : IDetectionService
    {
 private readonly ApplicationDbContext _db;
        private readonly ILogger<DetectionService> _logger;

        // Attack type thresholds (placeholder for ML model integration)
  private static readonly Dictionary<string, double> AttackThresholds = new()
    {
            { "DoS", 0.75 },
 { "Probe", 0.70 },
            { "R2L", 0.80 },
   { "U2R", 0.85 },
 { "Generic", 0.60 }
     };

        public DetectionService(ApplicationDbContext db, ILogger<DetectionService> logger)
        {
          _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Classifies a network event using placeholder logic.
    /// Replace this with actual ML model inference.
  /// </summary>
        public Task<(string Classification, string? AttackType, double Confidence)> ClassifyPacketAsync(NetworkEvent networkEvent)
     {
         // PLACEHOLDER: This is where ML model integration would go
  // For now, use simple heuristics
       
      var random = new Random();
            var confidence = random.NextDouble();
            
  // Simple heuristics for demo purposes
       string classification = "Normal";
       string? attackType = null;

  // Example: high port scanning detection
            if (networkEvent.DestinationPort > 1024 && networkEvent.SourcePort < 1024)
       {
  if (confidence > 0.7)
      {
          classification = "Suspicious";
     if (confidence > 0.85)
       {
 classification = "Attack";
               attackType = "Probe";
       }
 }
         }

 // Example: large packet size anomaly
    if (networkEvent.PacketSize > 10000)
            {
  confidence = Math.Max(confidence, 0.6);
           if (confidence > 0.75)
       {
         classification = "Attack";
         attackType = "DoS";
     }
  }

   return Task.FromResult((classification, attackType, confidence));
        }

        /// <summary>
    /// Processes a network event through the detection pipeline
        /// </summary>
        public async Task ProcessNetworkEventAsync(NetworkEvent networkEvent)
        {
        try
          {
       var (classification, attackType, confidence) = await ClassifyPacketAsync(networkEvent);
  
            networkEvent.Classification = classification;
        networkEvent.AttackType = attackType;
     networkEvent.ConfidenceScore = confidence;

         _db.NetworkEvents.Add(networkEvent);
         await _db.SaveChangesAsync();

       if (classification == "Attack" && !string.IsNullOrEmpty(attackType))
    {
            await GenerateAlertAsync(networkEvent, attackType, confidence);
     }
     }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error processing network event");
    }
        }

        /// <summary>
        /// Generates a security alert from a detected attack
        /// </summary>
        public async Task GenerateAlertAsync(NetworkEvent networkEvent, string attackType, double confidence)
    {
   var severity = confidence switch
    {
 >= 0.95 => "Critical",
           >= 0.85 => "High",
    >= 0.70 => "Medium",
      >= 0.50 => "Low",
                _ => "Info"
  };

            var alert = new SecurityAlert
            {
  Timestamp = DateTime.UtcNow,
     Severity = severity,
        AlertType = attackType,
   SourceIP = networkEvent.SourceIP,
      DestinationIP = networkEvent.DestinationIP,
     SourcePort = networkEvent.SourcePort,
    DestinationPort = networkEvent.DestinationPort,
  Protocol = networkEvent.Protocol,
 Message = $"Detected {attackType} attack from {networkEvent.SourceIP}:{networkEvent.SourcePort} to {networkEvent.DestinationIP}:{networkEvent.DestinationPort} with {confidence:P1} confidence"
      };

            _db.SecurityAlerts.Add(alert);
   await _db.SaveChangesAsync();

            _logger.LogWarning("Security Alert [{Severity}]: {Message}", severity, alert.Message);
        }
 }

    /// <summary>
    /// Service for logging audit events
    /// </summary>
    public interface IAuditService
    {
    Task LogAsync(string userId, string userEmail, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db)
        {
   _db = db;
        }

        public async Task LogAsync(string userId, string userEmail, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null)
        {
   var auditLog = new AuditLog
            {
    Timestamp = DateTime.UtcNow,
       UserId = userId,
      UserEmail = userEmail,
 Action = action,
         EntityType = entityType,
         EntityId = entityId,
  Details = details,
       IpAddress = ipAddress
       };

        _db.AuditLogs.Add(auditLog);
        await _db.SaveChangesAsync();
        }
    }
}
