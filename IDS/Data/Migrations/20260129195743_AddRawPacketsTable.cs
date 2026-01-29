using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRawPacketsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawPackets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    TcpFlags = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PayloadData = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    PayloadEncoding = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FeatureVector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ClassifiedRecordId = table.Column<long>(type: "bigint", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NetworkInterface = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawPackets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_CapturedAt",
                table: "RawPackets",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_Classification",
                table: "RawPackets",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_DestinationIP",
                table: "RawPackets",
                column: "DestinationIP");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_IsProcessed",
                table: "RawPackets",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_Protocol",
                table: "RawPackets",
                column: "Protocol");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_SessionId",
                table: "RawPackets",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RawPackets_SourceIP",
                table: "RawPackets",
                column: "SourceIP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawPackets");
        }
    }
}
