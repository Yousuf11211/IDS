using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogFiles_Timestamp",
                table: "LogFiles",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogFiles");
        }
    }
}
