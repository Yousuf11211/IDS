using Microsoft.EntityFrameworkCore.Migrations;

namespace IDS.Data.Migrations;

public partial class SupportNullableFlowInputs : Migration
{
    private static readonly string[] Tables = ["RawPackets", "Benign_Table", "Attack_Table"];

    // Freeze the existing extractor columns at this migration. All three tables share this layout.
    private static readonly string[] IntColumns =
    [
        "AckFlagCounts", "BwdAckFlagCounts", "BwdBulkStateCount", "BwdFinFlagCounts", "BwdInitWinBytes",
        "BwdPacketsCount", "BwdRstFlagCounts", "BwdSynFlagCounts", "BwdUrgFlagCounts", "CwrFlagCounts",
        "DstPort", "EceFlagCounts", "FinFlagCounts", "FwdAckFlagCounts", "FwdFinFlagCounts",
        "FwdInitWinBytes", "FwdPacketsCount", "FwdPshFlagCounts", "FwdRstFlagCounts", "FwdSynFlagCounts",
        "FwdUrgFlagCounts", "PacketsCount", "PshFlagCounts", "RstFlagCounts", "SubflowBwdPackets",
        "SubflowFwdPackets", "SynFlagCounts", "UrgFlagCounts"
    ];

    private static readonly string[] LongColumns =
    [
        "BwdTotalHeaderBytes", "BwdTotalPayloadBytes", "FwdTotalHeaderBytes", "FwdTotalPayloadBytes",
        "SubflowBwdBytes", "SubflowFwdBytes", "TotalHeaderBytes", "TotalPayloadBytes"
    ];

    private static readonly string[] DoubleColumns =
    [
        "AckFlagPercentageInTotal", "ActiveCov", "ActiveMax", "ActiveMean", "ActiveMedian", "ActiveMin",
        "ActiveMode", "ActiveSkewness", "ActiveStd", "ActiveVariance", "AvgBwdBulkRate", "AvgSegmentSize",
        "BwdAckFlagPercentageInBwdPackets", "BwdAckFlagPercentageInTotal", "BwdBulkDuration", "BwdBytesRate",
        "BwdMeanHeaderBytes", "BwdPacketsIatMax", "BwdPacketsIatMean", "BwdPacketsIatMedian",
        "BwdPacketsIatMin", "BwdPacketsIatMode", "BwdPacketsIatStd", "BwdPacketsIatTotal",
        "BwdPacketsIatVariance", "BwdPacketsRate", "BwdPayloadBytesMax", "BwdPayloadBytesMedian",
        "BwdPayloadBytesStd", "BytesRate", "CovBwdHeaderBytesDeltaLen", "CovBwdPacketsDeltaLen",
        "CovBwdPacketsDeltaTime", "CovFwdHeaderBytesDeltaLen", "CovFwdPacketsDeltaLen",
        "CovFwdPacketsDeltaTime", "CovHeaderBytesDeltaLen", "CovPacketsDeltaLen", "DeltaStart", "DownUpRate",
        "Duration", "FwdAckFlagPercentageInFwdPackets", "FwdAckFlagPercentageInTotal", "FwdAvgSegmentSize",
        "FwdBytesRate", "FwdMeanHeaderBytes", "FwdPacketsIatCov", "FwdPacketsIatMax", "FwdPacketsIatMean",
        "FwdPacketsIatMedian", "FwdPacketsIatMin", "FwdPacketsIatMode", "FwdPacketsIatSkewness",
        "FwdPacketsIatStd", "FwdPacketsIatTotal", "FwdPacketsIatVariance", "FwdPacketsRate",
        "FwdPayloadBytesCov", "FwdPayloadBytesMax", "FwdPayloadBytesMedian", "FwdPayloadBytesSkewness",
        "FwdPayloadBytesStd", "HandshakeDuration", "IdleCov", "IdleMax", "IdleMean", "IdleMedian", "IdleMin",
        "IdleMode", "IdleSkewness", "IdleStd", "IdleVariance", "MaxBwdPacketsDeltaLen",
        "MaxBwdPacketsDeltaTime", "MaxFwdPacketsDeltaTime", "MaxHeaderBytesDeltaLen",
        "MaxPayloadBytesDeltaLen", "MeanBwdHeaderBytesDeltaLen", "MeanBwdPacketsDeltaLen",
        "MeanBwdPacketsDeltaTime", "MeanFwdHeaderBytesDeltaLen", "MeanFwdPacketsDeltaLen",
        "MeanFwdPacketsDeltaTime", "MeanHeaderBytes", "MeanHeaderBytesDeltaLen", "MeanPacketsDeltaLen",
        "MeanPacketsDeltaTime", "MeanPayloadBytesDeltaLen", "MedianBwdPacketsDeltaLen",
        "MedianFwdPacketsDeltaLen", "MedianFwdPacketsDeltaTime", "MedianPacketsDeltaLen",
        "MinFwdPacketsDeltaTime", "MinPayloadBytesDeltaLen", "ModeBwdPacketsDeltaLen",
        "ModeFwdPacketsDeltaLen", "ModeFwdPacketsDeltaTime", "ModeFwdPayloadBytesDeltaLen",
        "ModePacketsDeltaLen", "ModePacketsDeltaTime", "ModePayloadBytesDeltaLen", "PacketIatMax",
        "PacketIatMin", "PacketIatStd", "PacketIatTotal", "PacketsIatCov", "PacketsIatMean",
        "PacketsIatMedian", "PacketsIatMode", "PacketsIatVariance", "PacketsRate", "PayloadBytesCov",
        "PayloadBytesMax", "PayloadBytesMean", "PayloadBytesMedian", "PayloadBytesSkewness",
        "PayloadBytesStd", "PayloadBytesVariance", "RstFlagPercentageInTotal", "SkewnessBwdPacketsDeltaLen",
        "SkewnessBwdPacketsDeltaTime", "SkewnessFwdPacketsDeltaLen", "SkewnessFwdPacketsDeltaTime",
        "SkewnessHeaderBytes", "SkewnessPacketsDeltaLen", "SkewnessPacketsDeltaTime",
        "StdFwdPacketsDeltaTime", "StdHeaderBytes", "StdPacketsDeltaTime", "SynFlagPercentageInTotal",
        "VarianceHeaderBytes", "VariancePacketsDeltaTime"
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var table in Tables)
        {
            ChangeNullability(migrationBuilder, table, nullable: true);
            migrationBuilder.AddColumn<bool>("DeltaStartIncompleteHandshake", table, type: "bit", nullable: true);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Never erase an observed handshake flag or backfill missing measurements during downgrade.
        foreach (var table in Tables)
        {
            var missing = string.Join(" OR ", IntColumns.Concat(LongColumns).Concat(DoubleColumns)
                .Select(name => $"[{name}] IS NULL"));
            migrationBuilder.Sql($"""
                IF EXISTS (SELECT 1 FROM [{table}] WHERE [DeltaStartIncompleteHandshake] IS NOT NULL OR {missing})
                    THROW 51000, 'Flow data requires the nullable input schema. Keep this schema for model rollback.', 1;
                """);
        }

        foreach (var table in Tables)
        {
            migrationBuilder.DropColumn("DeltaStartIncompleteHandshake", table);
            ChangeNullability(migrationBuilder, table, nullable: false);
        }
    }

    private static void ChangeNullability(MigrationBuilder migration, string table, bool nullable)
    {
        Change<int>(IntColumns, "int");
        Change<long>(LongColumns, "bigint");
        Change<double>(DoubleColumns, "float");

        void Change<T>(string[] columns, string sqlType)
        {
            foreach (var name in columns)
                migration.AlterColumn<T>(name, table, type: sqlType, nullable: nullable,
                    oldClrType: typeof(T), oldType: sqlType, oldNullable: !nullable);
        }
    }
}
