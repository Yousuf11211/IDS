using IDS.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDS.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260920121000_AddEncryptedEmployeeChat")]
public sealed class AddEncryptedEmployeeChat : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Department", table: "AspNetUsers", type: "nvarchar(80)",
            maxLength: 80, nullable: false, defaultValue: "General");

        migrationBuilder.CreateTable(
            name: "ChatPublicKeys",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                SubjectPublicKeyInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatPublicKeys", key => key.UserId);
                table.ForeignKey("FK_ChatPublicKeys_AspNetUsers_UserId", key => key.UserId,
                    "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChatMessages",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SenderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                RecipientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ClientMessageId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                // SQL Server requires nvarchar(max) for Unicode values longer than 4,000 characters.
                SenderCiphertext = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: false),
                RecipientCiphertext = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: false),
                SenderNonce = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                RecipientNonce = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                SenderKeyFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                RecipientKeyFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatMessages", message => message.Id);
                table.ForeignKey("FK_ChatMessages_AspNetUsers_SenderId", message => message.SenderId,
                    "AspNetUsers", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChatMessages_AspNetUsers_RecipientId", message => message.RecipientId,
                    "AspNetUsers", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_ChatMessages_SenderId_ClientMessageId", "ChatMessages",
            new[] { "SenderId", "ClientMessageId" }, unique: true);
        migrationBuilder.CreateIndex("IX_ChatMessages_RecipientId_Id", "ChatMessages",
            new[] { "RecipientId", "Id" });
        migrationBuilder.CreateIndex("IX_ChatMessages_SenderId_RecipientId_Id", "ChatMessages",
            new[] { "SenderId", "RecipientId", "Id" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ChatMessages");
        migrationBuilder.DropTable("ChatPublicKeys");
        migrationBuilder.DropColumn("Department", "AspNetUsers");
    }
}
