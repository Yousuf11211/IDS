using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IDS.Data;
using IDS.Security;
using System.Reflection;

namespace IDS.Pages.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    public class DatabaseViewerModel : PageModel
    {
        private readonly ApplicationDbContext _db;
     private const int PageSize = 50;

        public DatabaseViewerModel(ApplicationDbContext db)
        {
            _db = db;
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
     // Get all DbSet properties from the context
            var dbSetProperties = _db.GetType().GetProperties()
      .Where(p => p.PropertyType.IsGenericType && 
         p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
   .ToList();

       // Add custom tables
         var tableNames = new List<string>
            {
     "AspNetUsers",
          "AspNetRoles",
    "AspNetUserRoles",
         "LogFiles",
         "SecurityAlerts",
      "NetworkEvents",
     "SystemSettings",
          "AuditLogs",
          "DashboardMetrics"
        };

            foreach (var name in tableNames)
     {
      try
    {
          var count = await GetTableRowCountAsync(name);
      if (count >= 0)
 {
            AvailableTables.Add(name);
    TableRowCounts[name] = count;
            }
       }
   catch
                {
            // Table might not exist
       }
            }
        }

    private async Task<int> GetTableRowCountAsync(string tableName)
        {
            var sql = $"SELECT COUNT(*) FROM [{tableName}]";
          try
            {
         using var command = _db.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;
     await _db.Database.OpenConnectionAsync();
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
     try
 {
          // Get total count
  TotalRows = await GetTableRowCountAsync(tableName);
       TotalPages = (int)Math.Ceiling(TotalRows / (double)PageSize);
      CurrentPage = Math.Min(CurrentPage, Math.Max(1, TotalPages));

 var offset = (CurrentPage - 1) * PageSize;

     // Get column names
    var columnsSql = $@"
         SELECT COLUMN_NAME 
           FROM INFORMATION_SCHEMA.COLUMNS 
      WHERE TABLE_NAME = '{tableName}' 
      ORDER BY ORDINAL_POSITION";

       using var connection = _db.Database.GetDbConnection();
  await connection.OpenAsync();

  using var columnsCommand = connection.CreateCommand();
       columnsCommand.CommandText = columnsSql;
     using var columnsReader = await columnsCommand.ExecuteReaderAsync();
        
     while (await columnsReader.ReadAsync())
        {
          Columns.Add(columnsReader.GetString(0));
      }
    await columnsReader.CloseAsync();

        // Get data with pagination
        var dataSql = $@"
   SELECT * FROM [{tableName}] 
           ORDER BY (SELECT NULL) 
         OFFSET {offset} ROWS 
               FETCH NEXT {PageSize} ROWS ONLY";

        using var dataCommand = connection.CreateCommand();
       dataCommand.CommandText = dataSql;
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

try
            {
      // Build INSERT statement
         var columns = string.Join(", ", values.Keys.Select(k => $"[{k}]"));
       var parameters = string.Join(", ", values.Keys.Select((k, i) => $"@p{i}"));
           var sql = $"INSERT INTO [{tableName}] ({columns}) VALUES ({parameters})";

   using var connection = _db.Database.GetDbConnection();
      await connection.OpenAsync();
        using var command = connection.CreateCommand();
     command.CommandText = sql;

                int i = 0;
    foreach (var kvp in values)
                {
  var param = command.CreateParameter();
          param.ParameterName = $"@p{i}";
     param.Value = string.IsNullOrEmpty(kvp.Value) ? DBNull.Value : kvp.Value;
  command.Parameters.Add(param);
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

            try
            {
           // Try to find the primary key column
     var pkColumnSql = $@"
       SELECT COLUMN_NAME 
         FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
           WHERE TABLE_NAME = '{tableName}' 
           AND CONSTRAINT_NAME LIKE 'PK%'";

  using var connection = _db.Database.GetDbConnection();
     await connection.OpenAsync();

       string pkColumn = "Id";
       using (var pkCommand = connection.CreateCommand())
    {
        pkCommand.CommandText = pkColumnSql;
      var result = await pkCommand.ExecuteScalarAsync();
          if (result != null)
        {
    pkColumn = result.ToString()!;
          }
  }

                var deleteSql = $"DELETE FROM [{tableName}] WHERE [{pkColumn}] = @id";
      using var deleteCommand = connection.CreateCommand();
           deleteCommand.CommandText = deleteSql;
           var param = deleteCommand.CreateParameter();
 param.ParameterName = "@id";
        param.Value = rowId;
         deleteCommand.Parameters.Add(param);

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
