using System;
using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models
{
    /// <summary>
    /// Stores raw packet/flow data before classification.
 /// Inherits all 172 network flow features from NetworkFlowBase.
    /// </summary>
    public class RawPacket : NetworkFlowBase
    {
        public long Id { get; set; }

        /// <summary>
        /// Whether this packet has been processed/classified.
      /// </summary>
        public bool IsProcessed { get; set; } = false;

      /// <summary>
        /// Classification result after processing (Benign, Attack, Unknown).
        /// </summary>
        [MaxLength(20)]
        public string? Classification { get; set; }

        /// <summary>
     /// Reference ID to Benign_Table or Attack_Table after classification.
    /// </summary>
    public long? ClassifiedRecordId { get; set; }
    }

    /// <summary>
/// Stores benign (normal) network traffic detected by the IDS pipeline.
    /// Inherits all 172 network flow features from NetworkFlowBase.
    /// </summary>
    public class BenignTraffic : NetworkFlowBase
    {
        public long Id { get; set; }

        /// <summary>
        /// Model confidence score (0.0 to 1.0) for benign classification.
 /// </summary>
    public double ConfidenceScore { get; set; }

     /// <summary>
  /// Pipeline/model version that classified this traffic.
        /// </summary>
        [MaxLength(50)]
      public string? ModelVersion { get; set; }
    }

    /// <summary>
    /// Stores attack/malicious network traffic detected by the IDS pipeline.
    /// Inherits all 172 network flow features from NetworkFlowBase.
/// </summary>
    public class AttackTraffic : NetworkFlowBase
    {
      public long Id { get; set; }

   /// <summary>
        /// Type of attack detected (DoS, DDoS, PortScan, BruteForce, etc.)
        /// </summary>
   [Required]
     [MaxLength(100)]
    public string AttackType { get; set; } = string.Empty;

        /// <summary>
        /// Attack category (e.g., DoS, Probe, R2L, U2R)
        /// </summary>
        [MaxLength(50)]
        public string? AttackCategory { get; set; }

        /// <summary>
        /// Severity level: Critical, High, Medium, Low
      /// </summary>
        [Required]
 [MaxLength(20)]
        public string Severity { get; set; } = "Medium";

        /// <summary>
      /// Model confidence score (0.0 to 1.0) for attack classification.
 /// </summary>
  public double ConfidenceScore { get; set; }

        /// <summary>
        /// Pipeline/model version that classified this traffic.
        /// </summary>
    [MaxLength(50)]
  public string? ModelVersion { get; set; }

    /// <summary>
        /// Whether this alert has been acknowledged by an admin.
      /// </summary>
        public bool IsAcknowledged { get; set; } = false;

        /// <summary>
        /// User who acknowledged this alert.
        /// </summary>
      [MaxLength(450)]
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// When the alert was acknowledged.
        /// </summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
  /// Notes added by admin.
/// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

/// <summary>
    /// Real-time statistics snapshot for SignalR updates.
    /// </summary>
    public class LiveDashboardStats
    {
        public long TotalBenign { get; set; }
        public long TotalAttacks { get; set; }
        public long BenignLast24h { get; set; }
        public long AttacksLast24h { get; set; }
      public long BenignLastHour { get; set; }
   public long AttacksLastHour { get; set; }
        public double AttackPercentage { get; set; }
        public string SystemStatus { get; set; } = "Normal";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
   public Dictionary<string, int> AttacksByType { get; set; } = new();
        public Dictionary<string, int> AttacksBySeverity { get; set; } = new();
    }

    /// <summary>
    /// Represents a single detection event for real-time feed.
    /// </summary>
    public class LiveDetectionEvent
    {
        public long Id { get; set; }
      public DateTime Timestamp { get; set; }
        public string SrcIp { get; set; } = string.Empty;
   public int DstPort { get; set; }
        public bool IsAttack { get; set; }
    public string? AttackType { get; set; }
        public string? Severity { get; set; }
public double ConfidenceScore { get; set; }
      public string Label { get; set; } = string.Empty;
    }
}
