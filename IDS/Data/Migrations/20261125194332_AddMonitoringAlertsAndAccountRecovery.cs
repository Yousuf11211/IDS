using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitoringAlertsAndAccountRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttackLabel",
                table: "SecurityAlerts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DetectionCount",
                table: "SecurityAlerts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "EventCategory",
                table: "SecurityAlerts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupKey",
                table: "SecurityAlerts",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastObservedAt",
                table: "SecurityAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "SecurityAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RepresentativeAttackId",
                table: "SecurityAlerts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "SecurityAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolvedByUserId",
                table: "SecurityAlerts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "SecurityAlerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AuthenticatorResetRequestId",
                table: "InboxNotifications",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccountSecurity",
                table: "InboxNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SecurityAlertId",
                table: "InboxNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthenticatorResetRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    VerifiedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdentityVerification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticatorResetRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecoveryThrottles",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WindowStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryThrottles", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEmails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProtectedPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Lease = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEmails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_GroupKey",
                table: "SecurityAlerts",
                column: "GroupKey",
                unique: true,
                filter: "[GroupKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_LastObservedAt",
                table: "SecurityAlerts",
                column: "LastObservedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_TicketId",
                table: "SecurityAlerts",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticatorResetRequests_Status_ExpiresAt",
                table: "AuthenticatorResetRequests",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticatorResetRequests_UserId_Status",
                table: "AuthenticatorResetRequests",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RecoveryThrottles_WindowStart",
                table: "RecoveryThrottles",
                column: "WindowStart");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEmails_DueAt",
                table: "SecurityEmails",
                column: "DueAt",
                filter: "[AcceptedAt] IS NULL AND [FailedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEmails_EventKey",
                table: "SecurityEmails",
                column: "EventKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityAlerts_SupportTickets_TicketId",
                table: "SecurityAlerts",
                column: "TicketId",
                principalTable: "SupportTickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityAlerts_SupportTickets_TicketId",
                table: "SecurityAlerts");

            migrationBuilder.DropTable(
                name: "AuthenticatorResetRequests");

            migrationBuilder.DropTable(
                name: "RecoveryThrottles");

            migrationBuilder.DropTable(
                name: "SecurityEmails");

            migrationBuilder.DropIndex(
                name: "IX_SecurityAlerts_GroupKey",
                table: "SecurityAlerts");

            migrationBuilder.DropIndex(
                name: "IX_SecurityAlerts_LastObservedAt",
                table: "SecurityAlerts");

            migrationBuilder.DropIndex(
                name: "IX_SecurityAlerts_TicketId",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "AttackLabel",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "DetectionCount",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "EventCategory",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "GroupKey",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "LastObservedAt",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "RepresentativeAttackId",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "SecurityAlerts");

            migrationBuilder.DropColumn(
                name: "AuthenticatorResetRequestId",
                table: "InboxNotifications");

            migrationBuilder.DropColumn(
                name: "IsAccountSecurity",
                table: "InboxNotifications");

            migrationBuilder.DropColumn(
                name: "SecurityAlertId",
                table: "InboxNotifications");
        }
    }
}
