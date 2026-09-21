using IDS.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations;

/// <summary>Connects an incident ticket to the detection that raised it.</summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260920120000_LinkEscalatedAttackTickets")]
public sealed class LinkEscalatedAttackTickets : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "SourceAttackId",
            table: "SupportTickets",
            type: "bigint",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_SupportTickets_SourceAttackId",
            table: "SupportTickets",
            column: "SourceAttackId",
            unique: true,
            filter: "[SourceAttackId] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SupportTickets_SourceAttackId",
            table: "SupportTickets");

        migrationBuilder.DropColumn(
            name: "SourceAttackId",
            table: "SupportTickets");
    }
}
