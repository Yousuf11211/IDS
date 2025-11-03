using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DashboardModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var log = new LogFile
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Message = "Dashboard page visited",
                Exception = null,
                UserId = userId
            };

            _db.LogFiles.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
