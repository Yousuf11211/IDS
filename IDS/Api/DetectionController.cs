using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IDS.Data.Models;
using IDS.Core.Services;

namespace IDS.Api
{
    /// <summary>
    /// API Controller for the detection pipeline to submit detection results.
    /// This allows your external detection pipeline to push data to the web app.
    /// 
    /// Security: Uses API key authentication for pipeline access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DetectionController : ControllerBase
    {
        private readonly ILiveDetectionService _liveDetectionService;
     private readonly ILogger<DetectionController> _logger;
        private readonly IConfiguration _configuration;

        public DetectionController(
            ILiveDetectionService liveDetectionService,
            ILogger<DetectionController> logger,
     IConfiguration configuration)
        {
      _liveDetectionService = liveDetectionService;
            _logger = logger;
            _configuration = configuration;
}

        /// <summary>
   /// Validates the API key from request header.
        /// </summary>
        private bool ValidateApiKey()
      {
     var expectedKey = _configuration["Detection:ApiKey"];
            if (string.IsNullOrEmpty(expectedKey))
        {
   // No API key configured - allow in development
    var env = _configuration["ASPNETCORE_ENVIRONMENT"];
      return env == "Development";
     }

   var providedKey = Request.Headers["X-Api-Key"].FirstOrDefault();
       return providedKey == expectedKey;
 }

        /// <summary>
        /// Submit a single benign traffic detection.
        /// POST /api/detection/benign
  /// </summary>
        [HttpPost("benign")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitBenign([FromBody] BenignTrafficDto dto)
        {
            if (!ValidateApiKey())
             return Unauthorized(new { error = "Invalid or missing API key" });

     var traffic = new BenignTraffic
      {
      SourceIP = dto.SourceIP,
    DestinationIP = dto.DestinationIP,
     SourcePort = dto.SourcePort,
                DestinationPort = dto.DestinationPort,
     Protocol = dto.Protocol,
   PacketSize = dto.PacketSize,
    Duration = dto.Duration,
                BytesSent = dto.BytesSent,
    BytesReceived = dto.BytesReceived,
                Service = dto.Service,
       ConfidenceScore = dto.ConfidenceScore,
        FeatureVector = dto.FeatureVector,
           ModelVersion = dto.ModelVersion
            };

            var result = await _liveDetectionService.RegisterBenignTrafficAsync(traffic);
       return Ok(new { id = result.Id, message = "Benign traffic recorded" });
        }

        /// <summary>
        /// Submit a single attack detection.
        /// POST /api/detection/attack
        /// </summary>
        [HttpPost("attack")]
        [AllowAnonymous]
      public async Task<IActionResult> SubmitAttack([FromBody] AttackTrafficDto dto)
        {
if (!ValidateApiKey())
                return Unauthorized(new { error = "Invalid or missing API key" });

            var attack = new AttackTraffic
  {
                SourceIP = dto.SourceIP,
          DestinationIP = dto.DestinationIP,
  SourcePort = dto.SourcePort,
           DestinationPort = dto.DestinationPort,
           Protocol = dto.Protocol,
     PacketSize = dto.PacketSize,
      Duration = dto.Duration,
        BytesSent = dto.BytesSent,
            BytesReceived = dto.BytesReceived,
   Service = dto.Service,
       AttackType = dto.AttackType,
 AttackCategory = dto.AttackCategory,
  Severity = dto.Severity ?? "Medium",
          ConfidenceScore = dto.ConfidenceScore,
           FeatureVector = dto.FeatureVector,
    ModelVersion = dto.ModelVersion
            };

  var result = await _liveDetectionService.RegisterAttackAsync(attack);
      
_logger.LogWarning("Attack detected: {AttackType} from {SourceIP} ({Severity})", 
             dto.AttackType, dto.SourceIP, dto.Severity);

            return Ok(new { id = result.Id, message = "Attack recorded and alert broadcasted" });
        }

        /// <summary>
        /// Submit multiple detections in a batch (for efficiency).
        /// POST /api/detection/batch
        /// </summary>
        [HttpPost("batch")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitBatch([FromBody] BatchDetectionDto batch)
   {
     if (!ValidateApiKey())
         return Unauthorized(new { error = "Invalid or missing API key" });

        int benignCount = 0;
            int attackCount = 0;

      // Process benign traffic
            if (batch.BenignTraffic != null)
            {
foreach (var dto in batch.BenignTraffic)
            {
      var traffic = new BenignTraffic
          {
    SourceIP = dto.SourceIP,
      DestinationIP = dto.DestinationIP,
         SourcePort = dto.SourcePort,
        DestinationPort = dto.DestinationPort,
    Protocol = dto.Protocol,
              PacketSize = dto.PacketSize,
                Duration = dto.Duration,
         BytesSent = dto.BytesSent,
     BytesReceived = dto.BytesReceived,
   Service = dto.Service,
     ConfidenceScore = dto.ConfidenceScore,
   FeatureVector = dto.FeatureVector,
              ModelVersion = dto.ModelVersion
         };
         await _liveDetectionService.RegisterBenignTrafficAsync(traffic);
               benignCount++;
      }
       }

   // Process attacks
         if (batch.Attacks != null)
       {
  foreach (var dto in batch.Attacks)
          {
         var attack = new AttackTraffic
   {
     SourceIP = dto.SourceIP,
              DestinationIP = dto.DestinationIP,
     SourcePort = dto.SourcePort,
        DestinationPort = dto.DestinationPort,
          Protocol = dto.Protocol,
       PacketSize = dto.PacketSize,
  Duration = dto.Duration,
               BytesSent = dto.BytesSent,
   BytesReceived = dto.BytesReceived,
          Service = dto.Service,
           AttackType = dto.AttackType,
            AttackCategory = dto.AttackCategory,
               Severity = dto.Severity ?? "Medium",
          ConfidenceScore = dto.ConfidenceScore,
         FeatureVector = dto.FeatureVector,
ModelVersion = dto.ModelVersion
          };
   await _liveDetectionService.RegisterAttackAsync(attack);
             attackCount++;
          }
            }

            _logger.LogInformation("Batch processed: {Benign} benign, {Attacks} attacks", benignCount, attackCount);
return Ok(new { benignCount, attackCount, message = "Batch processed successfully" });
        }

        /// <summary>
        /// Get current dashboard statistics.
        /// GET /api/detection/stats
        /// </summary>
[HttpGet("stats")]
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> GetRecentAttacks([FromQuery] int count = 20)
   {
      var attacks = await _liveDetectionService.GetRecentAttacksAsync(count);
     return Ok(attacks);
   }

        /// <summary>
      /// Health check for the detection API.
        /// GET /api/detection/health
        /// </summary>
  [HttpGet("health")]
 [AllowAnonymous]
        public IActionResult Health()
     {
       return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
 }
    }

    #region DTOs

    public class BenignTrafficDto
    {
        public string SourceIP { get; set; } = string.Empty;
 public string DestinationIP { get; set; } = string.Empty;
        public int SourcePort { get; set; }
      public int DestinationPort { get; set; }
    public string Protocol { get; set; } = string.Empty;
        public int PacketSize { get; set; }
        public double Duration { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
     public string? Service { get; set; }
public double ConfidenceScore { get; set; }
        public string? FeatureVector { get; set; }
        public string? ModelVersion { get; set; }
    }

    public class AttackTrafficDto
 {
        public string SourceIP { get; set; } = string.Empty;
        public string DestinationIP { get; set; } = string.Empty;
        public int SourcePort { get; set; }
        public int DestinationPort { get; set; }
     public string Protocol { get; set; } = string.Empty;
        public int PacketSize { get; set; }
   public double Duration { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public string? Service { get; set; }
     public string AttackType { get; set; } = string.Empty;
        public string? AttackCategory { get; set; }
        public string? Severity { get; set; }
        public double ConfidenceScore { get; set; }
        public string? FeatureVector { get; set; }
        public string? ModelVersion { get; set; }
    }

    public class BatchDetectionDto
    {
    public List<BenignTrafficDto>? BenignTraffic { get; set; }
    public List<AttackTrafficDto>? Attacks { get; set; }
    }

    #endregion
}
