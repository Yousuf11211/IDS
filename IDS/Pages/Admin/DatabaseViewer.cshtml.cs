using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using IDS.Data;
using IDS.Security;

namespace IDS.Pages.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    public class DatabaseViewerModel : PageModel
    {
     private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private const int PageSize = 50;

        public DatabaseViewerModel(ApplicationDbContext db, IConfiguration configuration)
      {
            _db = db;
          _configuration = configuration;
        }

        public List<string> AvailableTables { get; set; } = new();
        public Dictionary<string, int> TableRowCounts { get; set; } = new();
  public string? SelectedTable { get; set; }
        public List<string> Columns { get; set; } = new();
        public List<Dictionary<string, object?>> TableData { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRows { get; set; } = 0;

        [TempData]
        public string? StatusMessage { get; set; }

        private string GetConnectionString()
{
       return Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? _configuration.GetConnectionString("DefaultConnection")
       ?? throw new InvalidOperationException("Connection string not found.");
        }

     public async Task OnGetAsync(string? table, int page = 1)
     {
       await LoadTableListAsync();

            if (!string.IsNullOrEmpty(table) && AvailableTables.Contains(table))
            {
   SelectedTable = table;
                CurrentPage = Math.Max(1, page);
             await LoadTableDataAsync(table);
            }
        }

        private async Task LoadTableListAsync()
     {
            var tableNames = new List<string>
    {
             "AspNetUsers",
       "AspNetRoles",
     "AspNetUserRoles",
            "AspNetUserClaims",
         "AspNetUserLogins",
                "AspNetUserTokens",
         "AspNetRoleClaims",
     "LogFiles",
          "SecurityAlerts",
     "NetworkEvents",
       "SystemSettings",
 "AuditLogs",
        "DashboardMetrics",
      "SupportTickets",
                "TicketComments",
         // Live Detection Tables (populated by external ML pipeline)
              "Benign_Table",
                "Attack_Table"
            };

            var connectionString = GetConnectionString();

            foreach (var name in tableNames)
            {
       try
     {
          var count = await GetTableRowCountAsync(name, connectionString);
        if (count >= 0)
          {
    AvailableTables.Add(name);
    TableRowCounts[name] = count;
        }
     }
         catch
 {
 // Table might not exist, skip it
       }
   }
        }

     private async Task<int> GetTableRowCountAsync(string tableName, string connectionString)
        {
      try
            {
                using var connection = new SqlConnection(connectionString);
       await connection.OpenAsync();
   
            using var command = connection.CreateCommand();
     command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
     
   var result = await command.ExecuteScalarAsync();
      return Convert.ToInt32(result);
            }
   catch
         {
                return -1;
     }
        }

        private async Task LoadTableDataAsync(string tableName)
        {
  var connectionString = GetConnectionString();
     
    try
            {
   using var connection = new SqlConnection(connectionString);
      await connection.OpenAsync();

                // Get total count
                TotalRows = await GetTableRowCountAsync(tableName, connectionString);
     TotalPages = (int)Math.Ceiling(TotalRows / (double)PageSize);
      CurrentPage = Math.Min(CurrentPage, Math.Max(1, TotalPages));

        var offset = (CurrentPage - 1) * PageSize;

    // Get column names
  using (var columnsCommand = connection.CreateCommand())
        {
            columnsCommand.CommandText = @"
 SELECT COLUMN_NAME 
  FROM INFORMATION_SCHEMA.COLUMNS 
       WHERE TABLE_NAME = @TableName 
           ORDER BY ORDINAL_POSITION";
     columnsCommand.Parameters.AddWithValue("@TableName", tableName);
   
       using var columnsReader = await columnsCommand.ExecuteReaderAsync();
        while (await columnsReader.ReadAsync())
             {
    Columns.Add(columnsReader.GetString(0));
    }
          }

      // Get data with pagination
                using (var dataCommand = connection.CreateCommand())
     {
            dataCommand.CommandText = $@"
         SELECT * FROM [{tableName}] 
        ORDER BY (SELECT NULL) 
            OFFSET @Offset ROWS 
          FETCH NEXT @PageSize ROWS ONLY";
        dataCommand.Parameters.AddWithValue("@Offset", offset);
            dataCommand.Parameters.AddWithValue("@PageSize", PageSize);
   
              using var dataReader = await dataCommand.ExecuteReaderAsync();
  while (await dataReader.ReadAsync())
           {
   var row = new Dictionary<string, object?>();
         for (int i = 0; i < dataReader.FieldCount; i++)
       {
    var columnName = dataReader.GetName(i);
     var value = dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);
          row[columnName] = value;
           }
   TableData.Add(row);
         }
              }
  }
       catch (Exception ex)
   {
    StatusMessage = $"Error loading table data: {ex.Message}";
       }
     }

    public async Task<IActionResult> OnPostAddRowAsync(string tableName, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(tableName) || values == null || !values.Any())
       {
                StatusMessage = "Error: Invalid input data.";
          return RedirectToPage(new { table = tableName });
          }

  var connectionString = GetConnectionString();

            try
            {
          using var connection = new SqlConnection(connectionString);
              await connection.OpenAsync();

      // Build INSERT statement with parameters
      var columns = string.Join(", ", values.Keys.Select(k => $"[{k}]"));
    var paramNames = string.Join(", ", values.Keys.Select((k, i) => $"@p{i}"));
       var sql = $"INSERT INTO [{tableName}] ({columns}) VALUES ({paramNames})";

         using var command = connection.CreateCommand();
    command.CommandText = sql;

        int i = 0;
         foreach (var kvp in values)
         {
           command.Parameters.AddWithValue($"@p{i}", 
              string.IsNullOrEmpty(kvp.Value) ? DBNull.Value : kvp.Value);
        i++;
      }

          await command.ExecuteNonQueryAsync();
     StatusMessage = "Row added successfully.";
            }
            catch (Exception ex)
       {
     StatusMessage = $"Error adding row: {ex.Message}";
   }

            return RedirectToPage(new { table = tableName });
        }

        public async Task<IActionResult> OnPostDeleteRowAsync(string tableName, string rowId)
   {
       if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(rowId))
     {
            StatusMessage = "Error: Invalid input data.";
    return RedirectToPage(new { table = tableName });
     }

            var connectionString = GetConnectionString();

         try
            {
     using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    // Try to find the primary key column
              string pkColumn = "Id";
  using (var pkCommand = connection.CreateCommand())
           {
       pkCommand.CommandText = @"
SELECT COLUMN_NAME 
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
             WHERE TABLE_NAME = @TableName 
          AND CONSTRAINT_NAME LIKE 'PK%'";
       pkCommand.Parameters.AddWithValue("@TableName", tableName);
             
            var result = await pkCommand.ExecuteScalarAsync();
    if (result != null)
    {
      pkColumn = result.ToString()!;
            }
                }

                // Delete the row
     using var deleteCommand = connection.CreateCommand();
      deleteCommand.CommandText = $"DELETE FROM [{tableName}] WHERE [{pkColumn}] = @Id";
     deleteCommand.Parameters.AddWithValue("@Id", rowId);

 var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
      StatusMessage = rowsAffected > 0 ? "Row deleted successfully." : "No rows were deleted.";
            }
            catch (Exception ex)
{
            StatusMessage = $"Error deleting row: {ex.Message}";
  }

            return RedirectToPage(new { table = tableName });
        }
    }
}
