using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDetectionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attack_Table",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    DestinationIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    SourcePort = table.Column<int>(type: "int", nullable: false),
                    DestinationPort = table.Column<int>(type: "int", nullable: false),
                    Protocol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PacketSize = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: false),
                    BytesSent = table.Column<long>(type: "bigint", nullable: false),
                    BytesReceived = table.Column<long>(type: "bigint", nullable: false),
                    Service = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AttackType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttackCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    FeatureVector = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attack_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Benign_Table",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    DestinationIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    SourcePort = table.Column<int>(type: "int", nullable: false),
                    DestinationPort = table.Column<int>(type: "int", nullable: false),
                    Protocol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PacketSize = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: false),
                    BytesSent = table.Column<long>(type: "bigint", nullable: false),
                    BytesReceived = table.Column<long>(type: "bigint", nullable: false),
                    Service = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    FeatureVector = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benign_Table", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_AttackType",
                table: "Attack_Table",
                column: "AttackType");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_DestinationIP",
                table: "Attack_Table",
                column: "DestinationIP");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_DetectedAt",
                table: "Attack_Table",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_IsAcknowledged",
                table: "Attack_Table",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_Severity",
                table: "Attack_Table",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Attack_Table_SourceIP",
                table: "Attack_Table",
                column: "SourceIP");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_DestinationIP",
                table: "Benign_Table",
                column: "DestinationIP");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_DetectedAt",
                table: "Benign_Table",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_Protocol",
                table: "Benign_Table",
                column: "Protocol");

            migrationBuilder.CreateIndex(
                name: "IX_Benign_Table_SourceIP",
                table: "Benign_Table",
                column: "SourceIP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attack_Table");

            migrationBuilder.DropTable(
                name: "Benign_Table");
        }
    }
}
