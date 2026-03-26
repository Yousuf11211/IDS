using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDetectionTablesWithFullFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RawPackets_Classification",
                table: "RawPackets");

            migrationBuilder.DropIndex(
                name: "IX_RawPackets_DestinationIP",
                table: "RawPackets");

            migrationBuilder.DropIndex(
                name: "IX_RawPackets_Protocol",
                table: "RawPackets");

            migrationBuilder.DropIndex(
                name: "IX_RawPackets_SessionId",
                table: "RawPackets");

            migrationBuilder.DropIndex(
                name: "IX_Benign_Table_DestinationIP",
                table: "Benign_Table");

            migrationBuilder.DropIndex(
                name: "IX_Benign_Table_Protocol",
                table: "Benign_Table");

            migrationBuilder.DropIndex(
                name: "IX_Attack_Table_DestinationIP",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "DestinationIP",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FeatureVector",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "NetworkInterface",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadData",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadEncoding",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "Service",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "DestinationIP",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FeatureVector",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "DestinationIP",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FeatureVector",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "Attack_Table");

            migrationBuilder.RenameColumn(
                name: "TcpFlags",
                table: "RawPackets",
                newName: "HandshakeState");

            migrationBuilder.RenameColumn(
                name: "SourcePort",
                table: "RawPackets",
                newName: "UrgFlagCounts");

            migrationBuilder.RenameColumn(
                name: "SourceIP",
                table: "RawPackets",
                newName: "SrcIp");

            migrationBuilder.RenameColumn(
                name: "PacketSize",
                table: "RawPackets",
                newName: "SynFlagCounts");

            migrationBuilder.RenameColumn(
                name: "DestinationPort",
                table: "RawPackets",
                newName: "SubflowFwdPackets");

            migrationBuilder.RenameColumn(
                name: "CapturedAt",
                table: "RawPackets",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "BytesSent",
                table: "RawPackets",
                newName: "TotalPayloadBytes");

            migrationBuilder.RenameColumn(
                name: "BytesReceived",
                table: "RawPackets",
                newName: "TotalHeaderBytes");

            migrationBuilder.RenameIndex(
                name: "IX_RawPackets_SourceIP",
                table: "RawPackets",
                newName: "IX_RawPackets_SrcIp");

            migrationBuilder.RenameIndex(
                name: "IX_RawPackets_CapturedAt",
                table: "RawPackets",
                newName: "IX_RawPackets_Timestamp");

            migrationBuilder.RenameColumn(
                name: "SourcePort",
                table: "Benign_Table",
                newName: "UrgFlagCounts");

            migrationBuilder.RenameColumn(
                name: "SourceIP",
                table: "Benign_Table",
                newName: "SrcIp");

            migrationBuilder.RenameColumn(
                name: "Service",
                table: "Benign_Table",
                newName: "HandshakeState");

            migrationBuilder.RenameColumn(
                name: "PacketSize",
                table: "Benign_Table",
                newName: "SynFlagCounts");

            migrationBuilder.RenameColumn(
                name: "DetectedAt",
                table: "Benign_Table",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "DestinationPort",
                table: "Benign_Table",
                newName: "SubflowFwdPackets");

            migrationBuilder.RenameColumn(
                name: "BytesSent",
                table: "Benign_Table",
                newName: "TotalPayloadBytes");

            migrationBuilder.RenameColumn(
                name: "BytesReceived",
                table: "Benign_Table",
                newName: "TotalHeaderBytes");

            migrationBuilder.RenameIndex(
                name: "IX_Benign_Table_SourceIP",
                table: "Benign_Table",
                newName: "IX_Benign_Table_SrcIp");

            migrationBuilder.RenameIndex(
                name: "IX_Benign_Table_DetectedAt",
                table: "Benign_Table",
                newName: "IX_Benign_Table_Timestamp");

            migrationBuilder.RenameColumn(
                name: "SourcePort",
                table: "Attack_Table",
                newName: "UrgFlagCounts");

            migrationBuilder.RenameColumn(
                name: "SourceIP",
                table: "Attack_Table",
                newName: "SrcIp");

            migrationBuilder.RenameColumn(
                name: "Service",
                table: "Attack_Table",
                newName: "HandshakeState");

            migrationBuilder.RenameColumn(
                name: "PacketSize",
                table: "Attack_Table",
                newName: "SynFlagCounts");

            migrationBuilder.RenameColumn(
                name: "DetectedAt",
                table: "Attack_Table",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "DestinationPort",
                table: "Attack_Table",
                newName: "SubflowFwdPackets");

            migrationBuilder.RenameColumn(
                name: "BytesSent",
                table: "Attack_Table",
                newName: "TotalPayloadBytes");

            migrationBuilder.RenameColumn(
                name: "BytesReceived",
                table: "Attack_Table",
                newName: "TotalHeaderBytes");

            migrationBuilder.RenameIndex(
                name: "IX_Attack_Table_SourceIP",
                table: "Attack_Table",
                newName: "IX_Attack_Table_SrcIp");

            migrationBuilder.RenameIndex(
                name: "IX_Attack_Table_DetectedAt",
                table: "Attack_Table",
                newName: "IX_Attack_Table_Timestamp");

            migrationBuilder.AddColumn<int>(
                name: "AckFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AckFlagPercentageInTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveCov",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMean",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMin",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMode",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveSkewness",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveVariance",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgBwdBulkRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgSegmentSize",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdAckFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdAckFlagPercentageInBwdPackets",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdAckFlagPercentageInTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdBulkDuration",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdBulkStateCount",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdBytesRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdFinFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BwdInitWinBytes",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdMeanHeaderBytes",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdPacketsCount",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMean",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMin",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMode",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatVariance",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdRstFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BwdSynFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "BwdTotalHeaderBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "BwdTotalPayloadBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BwdUrgFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BytesRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "CwrFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DeltaStart",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DownUpRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "DstPort",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EceFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FinFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdAckFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAckFlagPercentageInFwdPackets",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAckFlagPercentageInTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAvgSegmentSize",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdBytesRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdFinFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdInitWinBytes",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdMeanHeaderBytes",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdPacketsCount",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatCov",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMean",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMin",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMode",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatSkewness",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatVariance",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesCov",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesSkewness",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdPshFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdRstFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdSynFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FwdTotalHeaderBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FwdTotalPayloadBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "FwdUrgFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HandshakeDuration",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleCov",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMean",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMin",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMode",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleSkewness",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleVariance",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "RawPackets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "MaxBwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxBwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxPayloadBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanHeaderBytes",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanHeaderBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPayloadBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianBwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianFwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinPayloadBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeBwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPayloadBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePayloadBytesDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatMin",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PacketsCount",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatCov",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMean",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMode",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatVariance",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsRate",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesCov",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMax",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMean",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMedian",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesSkewness",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesStd",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesVariance",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PshFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RstFlagCounts",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RstFlagPercentageInTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessBwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessBwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessFwdPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessHeaderBytes",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessPacketsDeltaLen",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdFwdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdHeaderBytes",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdPacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "SubflowBwdBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubflowBwdPackets",
                table: "RawPackets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SubflowFwdBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "SynFlagPercentageInTotal",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VarianceHeaderBytes",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VariancePacketsDeltaTime",
                table: "RawPackets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "AckFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AckFlagPercentageInTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveCov",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMean",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMin",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMode",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveSkewness",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveVariance",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgBwdBulkRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgSegmentSize",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdAckFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdAckFlagPercentageInBwdPackets",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdAckFlagPercentageInTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdBulkDuration",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdBulkStateCount",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdBytesRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdFinFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BwdInitWinBytes",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdMeanHeaderBytes",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdPacketsCount",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMean",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMin",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMode",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatVariance",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdRstFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BwdSynFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "BwdTotalHeaderBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "BwdTotalPayloadBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BwdUrgFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BytesRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "CwrFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DeltaStart",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DownUpRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "DstPort",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EceFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FinFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdAckFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAckFlagPercentageInFwdPackets",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAckFlagPercentageInTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAvgSegmentSize",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdBytesRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdFinFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdInitWinBytes",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdMeanHeaderBytes",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdPacketsCount",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatCov",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMean",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMin",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMode",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatSkewness",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatVariance",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesCov",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesSkewness",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdPshFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdRstFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdSynFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FwdTotalHeaderBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FwdTotalPayloadBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "FwdUrgFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HandshakeDuration",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleCov",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMean",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMin",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMode",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleSkewness",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleVariance",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Benign_Table",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "MaxBwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxBwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxPayloadBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanHeaderBytes",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanHeaderBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPayloadBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianBwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianFwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinPayloadBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeBwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPayloadBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePayloadBytesDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatMin",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PacketsCount",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatCov",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMean",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMode",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatVariance",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsRate",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesCov",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMax",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMean",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMedian",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesSkewness",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesStd",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesVariance",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PshFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RstFlagCounts",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RstFlagPercentageInTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessBwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessBwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessFwdPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessHeaderBytes",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessPacketsDeltaLen",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdFwdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdHeaderBytes",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdPacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "SubflowBwdBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubflowBwdPackets",
                table: "Benign_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SubflowFwdBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "SynFlagPercentageInTotal",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VarianceHeaderBytes",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VariancePacketsDeltaTime",
                table: "Benign_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "AckFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AckFlagPercentageInTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveCov",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMean",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMin",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveMode",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveSkewness",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ActiveVariance",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgBwdBulkRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AvgSegmentSize",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdAckFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdAckFlagPercentageInBwdPackets",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdAckFlagPercentageInTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdBulkDuration",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdBulkStateCount",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdBytesRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdFinFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BwdInitWinBytes",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdMeanHeaderBytes",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdPacketsCount",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMean",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMin",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatMode",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsIatVariance",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPacketsRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BwdPayloadBytesStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BwdRstFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BwdSynFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "BwdTotalHeaderBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "BwdTotalPayloadBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BwdUrgFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BytesRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovBwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CovPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "CwrFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DeltaStart",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DownUpRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "DstPort",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EceFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FinFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdAckFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAckFlagPercentageInFwdPackets",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAckFlagPercentageInTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdAvgSegmentSize",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdBytesRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdFinFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdInitWinBytes",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdMeanHeaderBytes",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdPacketsCount",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatCov",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMean",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMin",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatMode",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatSkewness",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsIatVariance",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPacketsRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesCov",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesSkewness",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FwdPayloadBytesStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FwdPshFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdRstFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FwdSynFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FwdTotalHeaderBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FwdTotalPayloadBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "FwdUrgFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HandshakeDuration",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleCov",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMean",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMin",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleMode",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleSkewness",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IdleVariance",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Attack_Table",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "MaxBwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxBwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxPayloadBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanBwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanHeaderBytes",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanHeaderBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MeanPayloadBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianBwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianFwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MedianPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinPayloadBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeBwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModeFwdPayloadBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ModePayloadBytesDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatMin",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketIatTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PacketsCount",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatCov",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMean",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatMode",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsIatVariance",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PacketsRate",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesCov",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMax",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMean",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesMedian",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesSkewness",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesStd",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PayloadBytesVariance",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PshFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RstFlagCounts",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RstFlagPercentageInTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessBwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessBwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessFwdPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessHeaderBytes",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessPacketsDeltaLen",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkewnessPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdFwdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdHeaderBytes",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StdPacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "SubflowBwdBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubflowBwdPackets",
                table: "Attack_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SubflowFwdBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "SynFlagPercentageInTotal",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VarianceHeaderBytes",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VariancePacketsDeltaTime",
                table: "Attack_Table",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_DstPort",
                table: "RawPackets",
                column: "DstPort");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_Label",
                table: "RawPackets",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_DstPort",
                table: "Benign_Table",
                column: "DstPort");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_Label",
                table: "Benign_Table",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_DstPort",
                table: "Attack_Table",
                column: "DstPort");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_Label",
                table: "Attack_Table",
                column: "Label");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RawPackets_DstPort",
                table: "RawPackets");

            migrationBuilder.DropIndex(
                name: "IX_RawPackets_Label",
                table: "RawPackets");

            migrationBuilder.DropIndex(
                name: "IX_Benign_Table_DstPort",
                table: "Benign_Table");

            migrationBuilder.DropIndex(
                name: "IX_Benign_Table_Label",
                table: "Benign_Table");

            migrationBuilder.DropIndex(
                name: "IX_Attack_Table_DstPort",
                table: "Attack_Table");

            migrationBuilder.DropIndex(
                name: "IX_Attack_Table_Label",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "AckFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "AckFlagPercentageInTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveCov",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveMean",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveMin",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveMode",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveSkewness",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ActiveVariance",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "AvgBwdBulkRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "AvgSegmentSize",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagPercentageInBwdPackets",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagPercentageInTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdBulkDuration",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdBulkStateCount",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdBytesRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdFinFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdInitWinBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdMeanHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsCount",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMean",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMin",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMode",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatVariance",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPacketsRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdRstFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdSynFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdTotalHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdTotalPayloadBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BwdUrgFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "BytesRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovBwdHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovBwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovBwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovFwdHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovFwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CovPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "CwrFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "DeltaStart",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "DownUpRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "DstPort",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "EceFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FinFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagPercentageInFwdPackets",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagPercentageInTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdAvgSegmentSize",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdBytesRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdFinFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdInitWinBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdMeanHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsCount",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatCov",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMean",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMin",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMode",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatSkewness",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatVariance",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPacketsRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesCov",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesSkewness",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdPshFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdRstFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdSynFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdTotalHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdTotalPayloadBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "FwdUrgFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "HandshakeDuration",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleCov",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleMean",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleMin",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleMode",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleSkewness",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "IdleVariance",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MaxBwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MaxBwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MaxFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MaxHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MaxPayloadBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanBwdHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanBwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanBwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanFwdHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanFwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanHeaderBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MeanPayloadBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MedianBwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MedianFwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MedianFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MedianPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MinFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "MinPayloadBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModeBwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModeFwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModeFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModeFwdPayloadBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModePacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModePacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "ModePayloadBytesDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketIatMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketIatMin",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketIatStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketIatTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsCount",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsIatCov",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsIatMean",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsIatMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsIatMode",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsIatVariance",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PacketsRate",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesCov",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMax",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMean",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMedian",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesSkewness",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesStd",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PayloadBytesVariance",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "PshFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "RstFlagCounts",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "RstFlagPercentageInTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessBwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessBwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessFwdPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessPacketsDeltaLen",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SkewnessPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "StdFwdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "StdHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "StdPacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SubflowBwdBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SubflowBwdPackets",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SubflowFwdBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "SynFlagPercentageInTotal",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "VarianceHeaderBytes",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "VariancePacketsDeltaTime",
                table: "RawPackets");

            migrationBuilder.DropColumn(
                name: "AckFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "AckFlagPercentageInTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveCov",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMean",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMin",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMode",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveSkewness",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ActiveVariance",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "AvgBwdBulkRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "AvgSegmentSize",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagPercentageInBwdPackets",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagPercentageInTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdBulkDuration",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdBulkStateCount",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdBytesRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdFinFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdInitWinBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdMeanHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsCount",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMean",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMin",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMode",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatVariance",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdRstFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdSynFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdTotalHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdTotalPayloadBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BwdUrgFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "BytesRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovBwdHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovBwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovBwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovFwdHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovFwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CovPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "CwrFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "DeltaStart",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "DownUpRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "DstPort",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "EceFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FinFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagPercentageInFwdPackets",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagPercentageInTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdAvgSegmentSize",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdBytesRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdFinFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdInitWinBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdMeanHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsCount",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatCov",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMean",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMin",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMode",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatSkewness",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatVariance",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesCov",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesSkewness",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdPshFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdRstFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdSynFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdTotalHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdTotalPayloadBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "FwdUrgFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "HandshakeDuration",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleCov",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleMean",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleMin",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleMode",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleSkewness",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "IdleVariance",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MaxBwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MaxBwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MaxFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MaxHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MaxPayloadBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanBwdHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanBwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanBwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanFwdHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanFwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanHeaderBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MeanPayloadBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MedianBwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MedianFwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MedianFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MedianPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MinFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "MinPayloadBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModeBwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModeFwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModeFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModeFwdPayloadBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModePacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModePacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "ModePayloadBytesDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatMin",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsCount",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatCov",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatMean",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatMode",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatVariance",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PacketsRate",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesCov",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMax",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMean",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMedian",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesSkewness",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesStd",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesVariance",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "PshFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "RstFlagCounts",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "RstFlagPercentageInTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessBwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessBwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessFwdPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessPacketsDeltaLen",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "StdFwdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "StdHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "StdPacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SubflowBwdBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SubflowBwdPackets",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SubflowFwdBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "SynFlagPercentageInTotal",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "VarianceHeaderBytes",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "VariancePacketsDeltaTime",
                table: "Benign_Table");

            migrationBuilder.DropColumn(
                name: "AckFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "AckFlagPercentageInTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveCov",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMean",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMin",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveMode",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveSkewness",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ActiveVariance",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "AvgBwdBulkRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "AvgSegmentSize",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagPercentageInBwdPackets",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdAckFlagPercentageInTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdBulkDuration",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdBulkStateCount",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdBytesRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdFinFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdInitWinBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdMeanHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsCount",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMean",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMin",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatMode",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsIatVariance",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPacketsRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdPayloadBytesStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdRstFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdSynFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdTotalHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdTotalPayloadBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BwdUrgFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "BytesRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovBwdHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovBwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovBwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovFwdHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovFwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CovPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "CwrFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "DeltaStart",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "DownUpRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "DstPort",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "EceFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FinFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagPercentageInFwdPackets",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdAckFlagPercentageInTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdAvgSegmentSize",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdBytesRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdFinFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdInitWinBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdMeanHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsCount",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatCov",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMean",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMin",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatMode",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatSkewness",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsIatVariance",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPacketsRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesCov",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesSkewness",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPayloadBytesStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdPshFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdRstFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdSynFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdTotalHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdTotalPayloadBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "FwdUrgFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "HandshakeDuration",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleCov",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleMean",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleMin",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleMode",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleSkewness",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "IdleVariance",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MaxBwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MaxBwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MaxFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MaxHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MaxPayloadBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanBwdHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanBwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanBwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanFwdHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanFwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanHeaderBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MeanPayloadBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MedianBwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MedianFwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MedianFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MedianPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MinFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "MinPayloadBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModeBwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModeFwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModeFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModeFwdPayloadBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModePacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModePacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "ModePayloadBytesDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatMin",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketIatTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsCount",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatCov",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatMean",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatMode",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsIatVariance",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PacketsRate",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesCov",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMax",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMean",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesMedian",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesSkewness",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesStd",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PayloadBytesVariance",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "PshFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "RstFlagCounts",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "RstFlagPercentageInTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessBwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessBwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessFwdPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessPacketsDeltaLen",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SkewnessPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "StdFwdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "StdHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "StdPacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SubflowBwdBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SubflowBwdPackets",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SubflowFwdBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "SynFlagPercentageInTotal",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "VarianceHeaderBytes",
                table: "Attack_Table");

            migrationBuilder.DropColumn(
                name: "VariancePacketsDeltaTime",
                table: "Attack_Table");

            migrationBuilder.RenameColumn(
                name: "UrgFlagCounts",
                table: "RawPackets",
                newName: "SourcePort");

            migrationBuilder.RenameColumn(
                name: "TotalPayloadBytes",
                table: "RawPackets",
                newName: "BytesSent");

            migrationBuilder.RenameColumn(
                name: "TotalHeaderBytes",
                table: "RawPackets",
                newName: "BytesReceived");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "RawPackets",
                newName: "CapturedAt");

            migrationBuilder.RenameColumn(
                name: "SynFlagCounts",
                table: "RawPackets",
                newName: "PacketSize");

            migrationBuilder.RenameColumn(
                name: "SubflowFwdPackets",
                table: "RawPackets",
                newName: "DestinationPort");

            migrationBuilder.RenameColumn(
                name: "SrcIp",
                table: "RawPackets",
                newName: "SourceIP");

            migrationBuilder.RenameColumn(
                name: "HandshakeState",
                table: "RawPackets",
                newName: "TcpFlags");

            migrationBuilder.RenameIndex(
                name: "IX_RawPackets_Timestamp",
                table: "RawPackets",
                newName: "IX_RawPackets_CapturedAt");

            migrationBuilder.RenameIndex(
                name: "IX_RawPackets_SrcIp",
                table: "RawPackets",
                newName: "IX_RawPackets_SourceIP");

            migrationBuilder.RenameColumn(
                name: "UrgFlagCounts",
                table: "Benign_Table",
                newName: "SourcePort");

            migrationBuilder.RenameColumn(
                name: "TotalPayloadBytes",
                table: "Benign_Table",
                newName: "BytesSent");

            migrationBuilder.RenameColumn(
                name: "TotalHeaderBytes",
                table: "Benign_Table",
                newName: "BytesReceived");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Benign_Table",
                newName: "DetectedAt");

            migrationBuilder.RenameColumn(
                name: "SynFlagCounts",
                table: "Benign_Table",
                newName: "PacketSize");

            migrationBuilder.RenameColumn(
                name: "SubflowFwdPackets",
                table: "Benign_Table",
                newName: "DestinationPort");

            migrationBuilder.RenameColumn(
                name: "SrcIp",
                table: "Benign_Table",
                newName: "SourceIP");

            migrationBuilder.RenameColumn(
                name: "HandshakeState",
                table: "Benign_Table",
                newName: "Service");

            migrationBuilder.RenameIndex(
                name: "IX_Benign_Table_Timestamp",
                table: "Benign_Table",
                newName: "IX_Benign_Table_DetectedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Benign_Table_SrcIp",
                table: "Benign_Table",
                newName: "IX_Benign_Table_SourceIP");

            migrationBuilder.RenameColumn(
                name: "UrgFlagCounts",
                table: "Attack_Table",
                newName: "SourcePort");

            migrationBuilder.RenameColumn(
                name: "TotalPayloadBytes",
                table: "Attack_Table",
                newName: "BytesSent");

            migrationBuilder.RenameColumn(
                name: "TotalHeaderBytes",
                table: "Attack_Table",
                newName: "BytesReceived");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Attack_Table",
                newName: "DetectedAt");

            migrationBuilder.RenameColumn(
                name: "SynFlagCounts",
                table: "Attack_Table",
                newName: "PacketSize");

            migrationBuilder.RenameColumn(
                name: "SubflowFwdPackets",
                table: "Attack_Table",
                newName: "DestinationPort");

            migrationBuilder.RenameColumn(
                name: "SrcIp",
                table: "Attack_Table",
                newName: "SourceIP");

            migrationBuilder.RenameColumn(
                name: "HandshakeState",
                table: "Attack_Table",
                newName: "Service");

            migrationBuilder.RenameIndex(
                name: "IX_Attack_Table_Timestamp",
                table: "Attack_Table",
                newName: "IX_Attack_Table_DetectedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Attack_Table_SrcIp",
                table: "Attack_Table",
                newName: "IX_Attack_Table_SourceIP");

            migrationBuilder.AddColumn<string>(
                name: "DestinationIP",
                table: "RawPackets",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeatureVector",
                table: "RawPackets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "RawPackets",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetworkInterface",
                table: "RawPackets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadData",
                table: "RawPackets",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadEncoding",
                table: "RawPackets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "RawPackets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Service",
                table: "RawPackets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "RawPackets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationIP",
                table: "Benign_Table",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeatureVector",
                table: "Benign_Table",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "Benign_Table",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DestinationIP",
                table: "Attack_Table",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeatureVector",
                table: "Attack_Table",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "Attack_Table",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_Classification",
                table: "RawPackets",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_DestinationIP",
                table: "RawPackets",
                column: "DestinationIP");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_Protocol",
                table: "RawPackets",
                column: "Protocol");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_SessionId",
                table: "RawPackets",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_DestinationIP",
                table: "Benign_Table",
                column: "DestinationIP");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_Protocol",
                table: "Benign_Table",
                column: "Protocol");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_DestinationIP",
                table: "Attack_Table",
                column: "DestinationIP");
        }
    }
}
