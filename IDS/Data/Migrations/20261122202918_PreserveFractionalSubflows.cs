using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreserveFractionalSubflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL float(53) represents all int values but only some of the largest bigint values exactly.
            foreach (var table in new[] { "RawPackets", "Benign_Table", "Attack_Table" })
                foreach (var column in new[] { "SubflowFwdBytes", "SubflowBwdBytes" })
                    migrationBuilder.Sql($"""
                        IF EXISTS (SELECT 1 FROM [{table}] WHERE [{column}] IS NOT NULL AND
                            (TRY_CONVERT(bigint, CONVERT(float, [{column}])) IS NULL OR
                             TRY_CONVERT(bigint, CONVERT(float, [{column}])) <> [{column}]))
                            THROW 51001, 'Existing subflow byte count cannot be preserved exactly as float.', 1;
                        """);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowFwdPackets",
                table: "RawPackets",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowFwdBytes",
                table: "RawPackets",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowBwdPackets",
                table: "RawPackets",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowBwdBytes",
                table: "RawPackets",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowFwdPackets",
                table: "Benign_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowFwdBytes",
                table: "Benign_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowBwdPackets",
                table: "Benign_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowBwdBytes",
                table: "Benign_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowFwdPackets",
                table: "Attack_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowFwdBytes",
                table: "Attack_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowBwdPackets",
                table: "Attack_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SubflowBwdBytes",
                table: "Attack_Table",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "RawPackets", "Benign_Table", "Attack_Table" })
                foreach (var (column, type) in new[] { ("SubflowFwdPackets", "int"), ("SubflowBwdPackets", "int"),
                                                       ("SubflowFwdBytes", "bigint"), ("SubflowBwdBytes", "bigint") })
                    migrationBuilder.Sql($"""
                        IF EXISTS (SELECT 1 FROM [{table}] WHERE [{column}] IS NOT NULL AND
                            (TRY_CONVERT({type}, [{column}]) IS NULL OR
                             CONVERT(float, TRY_CONVERT({type}, [{column}])) <> [{column}]))
                            THROW 51002, 'Stored subflow measurements require fractional storage.', 1;
                        """);

            migrationBuilder.AlterColumn<int>(
                name: "SubflowFwdPackets",
                table: "RawPackets",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SubflowFwdBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubflowBwdPackets",
                table: "RawPackets",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SubflowBwdBytes",
                table: "RawPackets",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubflowFwdPackets",
                table: "Benign_Table",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SubflowFwdBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubflowBwdPackets",
                table: "Benign_Table",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SubflowBwdBytes",
                table: "Benign_Table",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubflowFwdPackets",
                table: "Attack_Table",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SubflowFwdBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubflowBwdPackets",
                table: "Attack_Table",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SubflowBwdBytes",
                table: "Attack_Table",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}
