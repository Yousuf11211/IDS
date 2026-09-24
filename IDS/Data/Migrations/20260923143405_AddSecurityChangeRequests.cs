using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityChangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityChangeRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RequestedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestedByEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RequesterSecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TargetEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TargetSecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityChangeRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityChangeRequests");
        }
    }
}
