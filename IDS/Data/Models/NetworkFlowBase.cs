using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDS.Data.Models
{
    /// <summary>
    /// Base class containing all network flow features for IDS detection.
    /// These columns match the ML pipeline feature extraction output.
    /// Total: 172 feature columns + Id
    /// </summary>
    public abstract class NetworkFlowBase
    {
        // Column 1: timestamp
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Column 2: src_ip
        [MaxLength(45)]
      public string SrcIp { get; set; } = string.Empty;

        // Column 3: dst_port
        public int DstPort { get; set; }

        // Column 4: duration
        public double Duration { get; set; }

        // Column 5: packets_count
        public int PacketsCount { get; set; }

     // Column 6: fwd_packets_count
        public int FwdPacketsCount { get; set; }

        // Column 7: bwd_packets_count
        public int BwdPacketsCount { get; set; }

        // Column 8: total_payload_bytes
        public long TotalPayloadBytes { get; set; }

     // Column 9: fwd_total_payload_bytes
        public long FwdTotalPayloadBytes { get; set; }

        // Column 10: bwd_total_payload_bytes
        public long BwdTotalPayloadBytes { get; set; }

        // Column 11: payload_bytes_max
        public double PayloadBytesMax { get; set; }

    // Column 12: payload_bytes_mean
        public double PayloadBytesMean { get; set; }

        // Column 13: payload_bytes_std
        public double PayloadBytesStd { get; set; }

    // Column 14: payload_bytes_variance
        public double PayloadBytesVariance { get; set; }

        // Column 15: payload_bytes_median
     public double PayloadBytesMedian { get; set; }

  // Column 16: payload_bytes_skewness
        public double PayloadBytesSkewness { get; set; }

        // Column 17: payload_bytes_cov
        public double PayloadBytesCov { get; set; }

        // Column 18: fwd_payload_bytes_max
        public double FwdPayloadBytesMax { get; set; }

        // Column 19: fwd_payload_bytes_std
        public double FwdPayloadBytesStd { get; set; }

        // Column 20: fwd_payload_bytes_median
     public double FwdPayloadBytesMedian { get; set; }

        // Column 21: fwd_payload_bytes_skewness
      public double FwdPayloadBytesSkewness { get; set; }

        // Column 22: fwd_payload_bytes_cov
        public double FwdPayloadBytesCov { get; set; }

        // Column 23: bwd_payload_bytes_max
        public double BwdPayloadBytesMax { get; set; }

    // Column 24: bwd_payload_bytes_std
        public double BwdPayloadBytesStd { get; set; }

        // Column 25: bwd_payload_bytes_median
   public double BwdPayloadBytesMedian { get; set; }

        // Column 26: total_header_bytes
 public long TotalHeaderBytes { get; set; }

        // Column 27: mean_header_bytes
        public double MeanHeaderBytes { get; set; }

     // Column 28: std_header_bytes
        public double StdHeaderBytes { get; set; }

 // Column 29: skewness_header_bytes
        public double SkewnessHeaderBytes { get; set; }

   // Column 30: variance_header_bytes
        public double VarianceHeaderBytes { get; set; }

   // Column 31: fwd_total_header_bytes
    public long FwdTotalHeaderBytes { get; set; }

        // Column 32: fwd_mean_header_bytes
        public double FwdMeanHeaderBytes { get; set; }

        // Column 33: bwd_total_header_bytes
public long BwdTotalHeaderBytes { get; set; }

        // Column 34: bwd_mean_header_bytes
        public double BwdMeanHeaderBytes { get; set; }

        // Column 35: fwd_avg_segment_size
 public double FwdAvgSegmentSize { get; set; }

   // Column 36: avg_segment_size
 public double AvgSegmentSize { get; set; }

        // Column 37: fwd_init_win_bytes
        public int FwdInitWinBytes { get; set; }

   // Column 38: bwd_init_win_bytes
public int BwdInitWinBytes { get; set; }

     // Column 39: active_min
        public double ActiveMin { get; set; }

      // Column 40: active_max
     public double ActiveMax { get; set; }

        // Column 41: active_mean
   public double ActiveMean { get; set; }

   // Column 42: active_std
        public double ActiveStd { get; set; }

  // Column 43: active_median
        public double ActiveMedian { get; set; }

  // Column 44: active_skewness
        public double ActiveSkewness { get; set; }

        // Column 45: active_cov
        public double ActiveCov { get; set; }

        // Column 46: active_mode
        public double ActiveMode { get; set; }

      // Column 47: active_variance
        public double ActiveVariance { get; set; }

        // Column 48: idle_min
        public double IdleMin { get; set; }

 // Column 49: idle_max
  public double IdleMax { get; set; }

        // Column 50: idle_mean
        public double IdleMean { get; set; }

        // Column 51: idle_std
  public double IdleStd { get; set; }

        // Column 52: idle_median
        public double IdleMedian { get; set; }

      // Column 53: idle_skewness
        public double IdleSkewness { get; set; }

  // Column 54: idle_cov
      public double IdleCov { get; set; }

// Column 55: idle_mode
        public double IdleMode { get; set; }

        // Column 56: idle_variance
   public double IdleVariance { get; set; }

        // Column 57: bytes_rate
        public double BytesRate { get; set; }

 // Column 58: fwd_bytes_rate
        public double FwdBytesRate { get; set; }

        // Column 59: bwd_bytes_rate
        public double BwdBytesRate { get; set; }

        // Column 60: packets_rate
        public double PacketsRate { get; set; }

        // Column 61: bwd_packets_rate
        public double BwdPacketsRate { get; set; }

     // Column 62: fwd_packets_rate
    public double FwdPacketsRate { get; set; }

        // Column 63: down_up_rate
        public double DownUpRate { get; set; }

        // Column 64: avg_bwd_bulk_rate
      public double AvgBwdBulkRate { get; set; }

        // Column 65: bwd_bulk_state_count
public int BwdBulkStateCount { get; set; }

        // Column 66: bwd_bulk_duration
        public double BwdBulkDuration { get; set; }

        // Column 67: fin_flag_counts
        public int FinFlagCounts { get; set; }

        // Column 68: psh_flag_counts
  public int PshFlagCounts { get; set; }

      // Column 69: urg_flag_counts
        public int UrgFlagCounts { get; set; }

        // Column 70: ece_flag_counts
 public int EceFlagCounts { get; set; }

        // Column 71: syn_flag_counts
public int SynFlagCounts { get; set; }

      // Column 72: ack_flag_counts
  public int AckFlagCounts { get; set; }

   // Column 73: cwr_flag_counts
        public int CwrFlagCounts { get; set; }

        // Column 74: rst_flag_counts
 public int RstFlagCounts { get; set; }

    // Column 75: fwd_fin_flag_counts
 public int FwdFinFlagCounts { get; set; }

        // Column 76: fwd_psh_flag_counts
        public int FwdPshFlagCounts { get; set; }

        // Column 77: fwd_urg_flag_counts
        public int FwdUrgFlagCounts { get; set; }

        // Column 78: fwd_syn_flag_counts
        public int FwdSynFlagCounts { get; set; }

        // Column 79: fwd_ack_flag_counts
    public int FwdAckFlagCounts { get; set; }

 // Column 80: fwd_rst_flag_counts
        public int FwdRstFlagCounts { get; set; }

        // Column 81: bwd_fin_flag_counts
        public int BwdFinFlagCounts { get; set; }

    // Column 82: bwd_urg_flag_counts
        public int BwdUrgFlagCounts { get; set; }

        // Column 83: bwd_syn_flag_counts
     public int BwdSynFlagCounts { get; set; }

        // Column 84: bwd_ack_flag_counts
        public int BwdAckFlagCounts { get; set; }

  // Column 85: bwd_rst_flag_counts
        public int BwdRstFlagCounts { get; set; }

   // Column 86: syn_flag_percentage_in_total
    public double SynFlagPercentageInTotal { get; set; }

        // Column 87: ack_flag_percentage_in_total
        public double AckFlagPercentageInTotal { get; set; }

        // Column 88: rst_flag_percentage_in_total
        public double RstFlagPercentageInTotal { get; set; }

        // Column 89: fwd_ack_flag_percentage_in_total
        public double FwdAckFlagPercentageInTotal { get; set; }

        // Column 90: bwd_ack_flag_percentage_in_total
        public double BwdAckFlagPercentageInTotal { get; set; }

        // Column 91: fwd_ack_flag_percentage_in_fwd_packets
        public double FwdAckFlagPercentageInFwdPackets { get; set; }

        // Column 92: bwd_ack_flag_percentage_in_bwd_packets
        public double BwdAckFlagPercentageInBwdPackets { get; set; }

      // Column 93: packets_iat_mean
        public double PacketsIatMean { get; set; }

        // Column 94: packet_iat_std
 public double PacketIatStd { get; set; }

        // Column 95: packet_iat_max
   public double PacketIatMax { get; set; }

        // Column 96: packet_iat_min
        public double PacketIatMin { get; set; }

        // Column 97: packet_iat_total
        public double PacketIatTotal { get; set; }

        // Column 98: packets_iat_median
        public double PacketsIatMedian { get; set; }

  // Column 99: packets_iat_cov
      public double PacketsIatCov { get; set; }

        // Column 100: packets_iat_mode
        public double PacketsIatMode { get; set; }

// Column 101: packets_iat_variance
        public double PacketsIatVariance { get; set; }

     // Column 102: fwd_packets_iat_mean
     public double FwdPacketsIatMean { get; set; }

        // Column 103: fwd_packets_iat_std
      public double FwdPacketsIatStd { get; set; }

   // Column 104: fwd_packets_iat_max
        public double FwdPacketsIatMax { get; set; }

   // Column 105: fwd_packets_iat_min
        public double FwdPacketsIatMin { get; set; }

        // Column 106: fwd_packets_iat_total
        public double FwdPacketsIatTotal { get; set; }

 // Column 107: fwd_packets_iat_median
        public double FwdPacketsIatMedian { get; set; }

        // Column 108: fwd_packets_iat_skewness
        public double FwdPacketsIatSkewness { get; set; }

        // Column 109: fwd_packets_iat_cov
        public double FwdPacketsIatCov { get; set; }

      // Column 110: fwd_packets_iat_mode
      public double FwdPacketsIatMode { get; set; }

        // Column 111: fwd_packets_iat_variance
     public double FwdPacketsIatVariance { get; set; }

        // Column 112: bwd_packets_iat_mean
        public double BwdPacketsIatMean { get; set; }

 // Column 113: bwd_packets_iat_std
        public double BwdPacketsIatStd { get; set; }

    // Column 114: bwd_packets_iat_max
   public double BwdPacketsIatMax { get; set; }

        // Column 115: bwd_packets_iat_min
      public double BwdPacketsIatMin { get; set; }

      // Column 116: bwd_packets_iat_total
        public double BwdPacketsIatTotal { get; set; }

        // Column 117: bwd_packets_iat_median
        public double BwdPacketsIatMedian { get; set; }

        // Column 118: bwd_packets_iat_mode
    public double BwdPacketsIatMode { get; set; }

        // Column 119: bwd_packets_iat_variance
        public double BwdPacketsIatVariance { get; set; }

        // Column 120: subflow_fwd_packets
        public int SubflowFwdPackets { get; set; }

        // Column 121: subflow_bwd_packets
        public int SubflowBwdPackets { get; set; }

 // Column 122: subflow_fwd_bytes
        public long SubflowFwdBytes { get; set; }

        // Column 123: subflow_bwd_bytes
        public long SubflowBwdBytes { get; set; }

        // Column 124: delta_start
        public double DeltaStart { get; set; }

        // Column 125: handshake_duration
        public double HandshakeDuration { get; set; }

        // Column 126: handshake_state
        [MaxLength(50)]
        public string? HandshakeState { get; set; }

        // Column 127: max_bwd_packets_delta_time
  public double MaxBwdPacketsDeltaTime { get; set; }

        // Column 128: mean_packets_delta_time
     public double MeanPacketsDeltaTime { get; set; }

      // Column 129: mode_packets_delta_time
        public double ModePacketsDeltaTime { get; set; }

        // Column 130: variance_packets_delta_time
      public double VariancePacketsDeltaTime { get; set; }

        // Column 131: std_packets_delta_time
        public double StdPacketsDeltaTime { get; set; }

        // Column 132: skewness_packets_delta_time
      public double SkewnessPacketsDeltaTime { get; set; }

      // Column 133: mean_bwd_packets_delta_time
        public double MeanBwdPacketsDeltaTime { get; set; }

// Column 134: skewness_bwd_packets_delta_time
 public double SkewnessBwdPacketsDeltaTime { get; set; }

        // Column 135: cov_bwd_packets_delta_time
public double CovBwdPacketsDeltaTime { get; set; }

        // Column 136: min_fwd_packets_delta_time
        public double MinFwdPacketsDeltaTime { get; set; }

      // Column 137: max_fwd_packets_delta_time
    public double MaxFwdPacketsDeltaTime { get; set; }

   // Column 138: mean_fwd_packets_delta_time
      public double MeanFwdPacketsDeltaTime { get; set; }

        // Column 139: mode_fwd_packets_delta_time
        public double ModeFwdPacketsDeltaTime { get; set; }

  // Column 140: std_fwd_packets_delta_time
        public double StdFwdPacketsDeltaTime { get; set; }

        // Column 141: median_fwd_packets_delta_time
        public double MedianFwdPacketsDeltaTime { get; set; }

        // Column 142: skewness_fwd_packets_delta_time
        public double SkewnessFwdPacketsDeltaTime { get; set; }

        // Column 143: cov_fwd_packets_delta_time
        public double CovFwdPacketsDeltaTime { get; set; }

        // Column 144: mean_packets_delta_len
        public double MeanPacketsDeltaLen { get; set; }

        // Column 145: mode_packets_delta_len
public double ModePacketsDeltaLen { get; set; }

   // Column 146: median_packets_delta_len
    public double MedianPacketsDeltaLen { get; set; }

     // Column 147: skewness_packets_delta_len
     public double SkewnessPacketsDeltaLen { get; set; }

        // Column 148: cov_packets_delta_len
        public double CovPacketsDeltaLen { get; set; }

        // Column 149: max_bwd_packets_delta_len
  public double MaxBwdPacketsDeltaLen { get; set; }

        // Column 150: mean_bwd_packets_delta_len
        public double MeanBwdPacketsDeltaLen { get; set; }

        // Column 151: mode_bwd_packets_delta_len
        public double ModeBwdPacketsDeltaLen { get; set; }

  // Column 152: median_bwd_packets_delta_len
        public double MedianBwdPacketsDeltaLen { get; set; }

    // Column 153: skewness_bwd_packets_delta_len
  public double SkewnessBwdPacketsDeltaLen { get; set; }

 // Column 154: cov_bwd_packets_delta_len
        public double CovBwdPacketsDeltaLen { get; set; }

        // Column 155: mean_fwd_packets_delta_len
        public double MeanFwdPacketsDeltaLen { get; set; }

    // Column 156: mode_fwd_packets_delta_len
        public double ModeFwdPacketsDeltaLen { get; set; }

        // Column 157: median_fwd_packets_delta_len
public double MedianFwdPacketsDeltaLen { get; set; }

   // Column 158: skewness_fwd_packets_delta_len
        public double SkewnessFwdPacketsDeltaLen { get; set; }

        // Column 159: cov_fwd_packets_delta_len
    public double CovFwdPacketsDeltaLen { get; set; }

        // Column 160: max_header_bytes_delta_len
        public double MaxHeaderBytesDeltaLen { get; set; }

        // Column 161: mean_header_bytes_delta_len
        public double MeanHeaderBytesDeltaLen { get; set; }

        // Column 162: cov_header_bytes_delta_len
        public double CovHeaderBytesDeltaLen { get; set; }

   // Column 163: mean_bwd_header_bytes_delta_len
   public double MeanBwdHeaderBytesDeltaLen { get; set; }

    // Column 164: cov_bwd_header_bytes_delta_len
        public double CovBwdHeaderBytesDeltaLen { get; set; }

        // Column 165: mean_fwd_header_bytes_delta_len
        public double MeanFwdHeaderBytesDeltaLen { get; set; }

        // Column 166: cov_fwd_header_bytes_delta_len
        public double CovFwdHeaderBytesDeltaLen { get; set; }

        // Column 167: min_payload_bytes_delta_len
     public double MinPayloadBytesDeltaLen { get; set; }

        // Column 168: max_payload_bytes_delta_len
  public double MaxPayloadBytesDeltaLen { get; set; }

        // Column 169: mean_payload_bytes_delta_len
   public double MeanPayloadBytesDeltaLen { get; set; }

        // Column 170: mode_payload_bytes_delta_len
 public double ModePayloadBytesDeltaLen { get; set; }

        // Column 171: mode_fwd_payload_bytes_delta_len
        public double ModeFwdPayloadBytesDeltaLen { get; set; }

     // Column 172: label
   [MaxLength(100)]
   public string Label { get; set; } = string.Empty;
    }
}
