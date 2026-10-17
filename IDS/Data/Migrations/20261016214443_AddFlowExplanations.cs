using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowExplanations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowExplanations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttackTrafficId = table.Column<long>(type: "bigint", nullable: false),
                    RawPacketId = table.Column<long>(type: "bigint", nullable: true),
                    Stage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContextJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    NextAttempt = table.Column<double>(type: "float", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowExplanations", x => x.Id);
                    table.CheckConstraint("CK_FlowExplanations_Attempts", "[Attempts] >= 0");
                    table.CheckConstraint("CK_FlowExplanations_Stage", "[Stage] IN ('gatekeeper','multiclass')");
                    table.CheckConstraint("CK_FlowExplanations_Status", "[Status] IN ('pending','processing','completed','failed')");
                    table.ForeignKey(
                        name: "FK_FlowExplanations_Attack_Table_AttackTrafficId",
                        column: x => x.AttackTrafficId,
                        principalTable: "Attack_Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlowExplanations_RawPackets_RawPacketId",
                        column: x => x.RawPacketId,
                        principalTable: "RawPackets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowExplanations_AttackTrafficId_Stage_ModelVersion",
                table: "FlowExplanations",
                columns: new[] { "AttackTrafficId", "Stage", "ModelVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowExplanations_RawPacketId",
                table: "FlowExplanations",
                column: "RawPacketId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowExplanations_Status_Fingerprint_NextAttempt_Id",
                table: "FlowExplanations",
                columns: new[] { "Status", "Fingerprint", "NextAttempt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowExplanations");
        }
    }
}
