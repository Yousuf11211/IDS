using System;
using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models
{
    /// <summary>
    /// Represents benign (normal) network traffic detected by the IDS pipeline.
    /// </summary>
    public class BenignTraffic
    {
        public long Id { get; set; }

        /// <summary>
        /// Timestamp when the traffic was detected.
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Source IP address.
        /// </summary>
        [MaxLength(45)]
        public string SourceIP { get; set; } = string.Empty;

      /// <summary>
        /// Destination IP address.
 /// </summary>
        [MaxLength(45)]
        public string DestinationIP { get; set; } = string.Empty;

        /// <summary>
  /// Source port number.
        /// </summary>
        public int SourcePort { get; set; }

        /// <summary>
        /// Destination port number.
   /// </summary>
        public int DestinationPort { get; set; }

 /// <summary>
        /// Network protocol (TCP, UDP, ICMP, etc.)
        /// </summary>
        [MaxLength(20)]
        public string Protocol { get; set; } = string.Empty;

   /// <summary>
        /// Packet size in bytes.
        /// </summary>
        public int PacketSize { get; set; }

        /// <summary>
 /// Duration of the connection in milliseconds.
  /// </summary>
        public double Duration { get; set; }

   /// <summary>
  /// Number of bytes sent from source to destination.
        /// </summary>
        public long BytesSent { get; set; }

        /// <summary>
        /// Number of bytes received from destination.
        /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// Service type (HTTP, HTTPS, FTP, SSH, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? Service { get; set; }

   /// <summary>
      /// Model confidence score (0.0 to 1.0) for benign classification.
   /// </summary>
      public double ConfidenceScore { get; set; }

        /// <summary>
     /// Raw feature vector from the model (JSON serialized).
        /// </summary>
        [MaxLength(4000)]
    public string? FeatureVector { get; set; }

        /// <summary>
        /// Pipeline/model version that classified this traffic.
        /// </summary>
  [MaxLength(50)]
     public string? ModelVersion { get; set; }
  }

    /// <summary>
    /// Represents attack/malicious network traffic detected by the IDS pipeline.
    /// </summary>
    public class AttackTraffic
  {
   public long Id { get; set; }

        /// <summary>
        /// Timestamp when the attack was detected.
        /// </summary>
     public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Source IP address of the attacker.
   /// </summary>
        [MaxLength(45)]
        public string SourceIP { get; set; } = string.Empty;

        /// <summary>
        /// Destination IP address (target).
   /// </summary>
   [MaxLength(45)]
   public string DestinationIP { get; set; } = string.Empty;

   /// <summary>
        /// Source port number.
  /// </summary>
        public int SourcePort { get; set; }

   /// <summary>
   /// Destination port number.
        /// </summary>
   public int DestinationPort { get; set; }

        /// <summary>
     /// Network protocol (TCP, UDP, ICMP, etc.)
        /// </summary>
        [MaxLength(20)]
  public string Protocol { get; set; } = string.Empty;

        /// <summary>
        /// Packet size in bytes.
        /// </summary>
        public int PacketSize { get; set; }

        /// <summary>
     /// Duration of the connection in milliseconds.
     /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Number of bytes sent.
        /// </summary>
        public long BytesSent { get; set; }

  /// <summary>
        /// Number of bytes received.
      /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// Service type targeted (HTTP, HTTPS, FTP, SSH, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? Service { get; set; }

        /// <summary>
        /// Type of attack detected (DoS, DDoS, PortScan, BruteForce, Infiltration, etc.)
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
 /// Raw feature vector from the model (JSON serialized).
        /// </summary>
        [MaxLength(4000)]
        public string? FeatureVector { get; set; }

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
    /// Stores raw packet data captured by the detection pipeline.
    /// This provides a complete record of all network traffic before classification.
    /// </summary>
    public class RawPacket
    {
        public long Id { get; set; }

        /// <summary>
        /// Timestamp when the packet was captured.
  /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

   /// <summary>
      /// Source IP address.
  /// </summary>
        [MaxLength(45)]
     public string SourceIP { get; set; } = string.Empty;

        /// <summary>
        /// Destination IP address.
        /// </summary>
    [MaxLength(45)]
        public string DestinationIP { get; set; } = string.Empty;

   /// <summary>
      /// Source port number.
        /// </summary>
        public int SourcePort { get; set; }

    /// <summary>
        /// Destination port number.
        /// </summary>
        public int DestinationPort { get; set; }

        /// <summary>
        /// Network protocol (TCP, UDP, ICMP, etc.)
        /// </summary>
        [MaxLength(20)]
        public string Protocol { get; set; } = string.Empty;

    /// <summary>
        /// Packet size in bytes.
        /// </summary>
        public int PacketSize { get; set; }

        /// <summary>
        /// Duration of the connection in milliseconds.
        /// </summary>
     public double Duration { get; set; }

        /// <summary>
/// Number of bytes sent from source to destination.
        /// </summary>
 public long BytesSent { get; set; }

     /// <summary>
        /// Number of bytes received from destination.
   /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// Service type (HTTP, HTTPS, FTP, SSH, etc.)
        /// </summary>
      [MaxLength(50)]
        public string? Service { get; set; }

        /// <summary>
  /// TCP flags (SYN, ACK, FIN, RST, PSH, URG).
        /// </summary>
        [MaxLength(50)]
        public string? TcpFlags { get; set; }

        /// <summary>
        /// Raw payload data (hex encoded or base64).
  /// </summary>
        [MaxLength(8000)]
     public string? PayloadData { get; set; }

    /// <summary>
  /// Payload encoding type (hex, base64, none).
        /// </summary>
        [MaxLength(20)]
        public string? PayloadEncoding { get; set; }

     /// <summary>
        /// Raw feature vector extracted for ML model (JSON).
        /// </summary>
        public string? FeatureVector { get; set; }

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

     /// <summary>
      /// Session/flow identifier to group related packets.
 /// </summary>
     [MaxLength(100)]
      public string? SessionId { get; set; }

        /// <summary>
        /// Network interface where packet was captured.
 /// </summary>
        [MaxLength(100)]
      public string? NetworkInterface { get; set; }

        /// <summary>
        /// Additional metadata (JSON format).
        /// </summary>
        [MaxLength(4000)]
        public string? Metadata { get; set; }
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
    public DateTime DetectedAt { get; set; }
        public string SourceIP { get; set; } = string.Empty;
public string DestinationIP { get; set; } = string.Empty;
      public string Protocol { get; set; } = string.Empty;
        public bool IsAttack { get; set; }
        public string? AttackType { get; set; }
     public string? Severity { get; set; }
        public double ConfidenceScore { get; set; }
    }
}
