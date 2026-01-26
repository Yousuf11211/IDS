using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;

namespace IDS.Pages.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private const int PageSize = 100;

        public LogsModel(ApplicationDbContext db)
      {
     _db = db;
        }

   public List<LogFile> Logs { get; set; } = new();
        public int TotalLogs { get; set; }
 public int CurrentPage { get; set; } = 1;
  public int TotalPages { get; set; } = 1;
        
        // Counts
     public int ErrorCount { get; set; }
     public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int NetworkCount { get; set; }
        
        // Filters
        public string? LevelFilter { get; set; }
        public string? SearchQuery { get; set; }
      public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public async Task OnGetAsync(string? level, string? search, DateTime? dateFrom, DateTime? dateTo, int page = 1)
        {
    LevelFilter = level;
            SearchQuery = search;
     DateFrom = dateFrom;
    DateTo = dateTo;
     CurrentPage = Math.Max(1, page);

         // Get counts
     ErrorCount = await _db.LogFiles.CountAsync(l => l.Level == "Error");
   WarningCount = await _db.LogFiles.CountAsync(l => l.Level == "Warning");
            InfoCount = await _db.LogFiles.CountAsync(l => l.Level == "Information");
      NetworkCount = await _db.LogFiles.CountAsync(l => l.Level == "NetworkNormal" || l.Level == "NetworkAttack");

            // Build query
  var query = _db.LogFiles.AsNoTracking();

 // Apply filters
      if (!string.IsNullOrEmpty(level))
         {
     query = query.Where(l => l.Level == level);
      }

if (!string.IsNullOrEmpty(search))
       {
    query = query.Where(l => l.Message.Contains(search));
 }

   if (dateFrom.HasValue)
     {
        query = query.Where(l => l.Timestamp >= dateFrom.Value);
            }

      if (dateTo.HasValue)
            {
    var endDate = dateTo.Value.AddDays(1);
   query = query.Where(l => l.Timestamp < endDate);
            }

  // Get total count for pagination
  TotalLogs = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalLogs / (double)PageSize);
         CurrentPage = Math.Min(CurrentPage, Math.Max(1, TotalPages));

      // Get paginated results
    Logs = await query
      .OrderByDescending(l => l.Timestamp)
    .Skip((CurrentPage - 1) * PageSize)
      .Take(PageSize)
         .ToListAsync();
     }

      public string GetQueryString(int page)
        {
      var queryParams = new List<string> { $"page={page}" };
            
        if (!string.IsNullOrEmpty(LevelFilter))
 queryParams.Add($"level={LevelFilter}");
  if (!string.IsNullOrEmpty(SearchQuery))
       queryParams.Add($"search={Uri.EscapeDataString(SearchQuery)}");
            if (DateFrom.HasValue)
                queryParams.Add($"dateFrom={DateFrom.Value:yyyy-MM-dd}");
         if (DateTo.HasValue)
    queryParams.Add($"dateTo={DateTo.Value:yyyy-MM-dd}");
      
            return string.Join("&", queryParams);
        }
    }
}
