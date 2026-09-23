using IDS.Data;
using IDS.Data.Migrations;
using IDS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace IDS.Tests;

public class SqlServerMigrationTests
{
    [Theory]
    [InlineData(nameof(ChatMessage.SenderCiphertext))]
    [InlineData(nameof(ChatMessage.RecipientCiphertext))]
    public void ChatMigrationUsesTheSqlServerTypeSelectedByTheModel(string propertyName)
    {
        // SQLite accepts oversized nvarchar declarations, so check this migration
        // against SQL Server's mapping without opening a database connection.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=MigrationMappingTest;Integrated Security=True")
            .Options;
        using var database = new ApplicationDbContext(options);
        var property = database.Model.FindEntityType(typeof(ChatMessage))!.FindProperty(propertyName)!;
        var table = new AddEncryptedEmployeeChat().UpOperations
            .OfType<CreateTableOperation>()
            .Single(operation => operation.Name == "ChatMessages");
        var column = table.Columns.Single(column => column.Name == propertyName);

        Assert.Equal("nvarchar(max)", property.GetColumnType());
        Assert.Equal(property.GetColumnType(), column.ColumnType);
        Assert.Equal(property.GetMaxLength(), column.MaxLength);
    }
}
