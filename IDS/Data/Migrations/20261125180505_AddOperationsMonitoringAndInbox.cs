using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationsMonitoringAndInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2200)", maxLength: 2200, nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxNotifications_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IncidentEscalations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    InitiatorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedTicket = table.Column<bool>(type: "bit", nullable: false),
                    CancelledById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentEscalations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentEscalations_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkerHeartbeats",
                columns: table => new
                {
                    Worker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastHeartbeatUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerHeartbeats", x => x.Worker);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxNotifications_RecipientId_CreatedAt_Id",
                table: "InboxNotifications",
                columns: new[] { "RecipientId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxNotifications_RecipientId_ReadAt",
                table: "InboxNotifications",
                columns: new[] { "RecipientId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxNotifications_TicketId",
                table: "InboxNotifications",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEscalations_TicketId_Id",
                table: "IncidentEscalations",
                columns: new[] { "TicketId", "Id" });

            // Only untouched legacy handoffs have a known original recipient and can be withdrawn.
            migrationBuilder.Sql("""
                INSERT INTO dbo.IncidentEscalations
                    (TicketId, InitiatorId, RecipientId, Reason, CreatedAt, CreatedTicket)
                SELECT t.Id, t.SubmittedByUserId, t.AssignedToUserId,
                    LEFT(LTRIM(SUBSTRING(t.Description,
                         CHARINDEX('Escalation reason:', t.Description) + LEN('Escalation reason:'), 4000)), 2000),
                    t.CreatedAt, 1
                FROM dbo.SupportTickets t
                WHERE t.SourceAttackId IS NOT NULL AND t.AssignedToUserId IS NOT NULL
                  AND t.Status = 'InProgress' AND t.CreatedAt = t.UpdatedAt
                  AND CHARINDEX('Escalation reason:', t.Description) > 0
                  AND NOT EXISTS (SELECT 1 FROM dbo.TicketComments c WHERE c.TicketId = t.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxNotifications");

            migrationBuilder.DropTable(
                name: "IncidentEscalations");

            migrationBuilder.DropTable(
                name: "WorkerHeartbeats");
        }
    }
}
