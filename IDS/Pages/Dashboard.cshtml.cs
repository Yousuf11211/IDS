using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IDS.Pages
{
    [Authorize]                                         // Require authentication to view the dashboard
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<LogFile> RecentLogs { get; set; } = new();
        public int TotalLogs { get; set; }
        public Dictionary<string, int> LevelCounts { get; set; } = new();

        [BindProperty]
        public AdminCreateUserInput CreateInput { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new();

        [TempData]
        public string CreateUserStatusMessage { get; set; }

        public class AdminCreateUserInput
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public string Role { get; set; } = string.Empty;
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

            // Load dashboard data
            RecentLogs = await _db.LogFiles
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();

            TotalLogs = await _db.LogFiles.CountAsync();

            var counts = await _db.LogFiles
                .AsNoTracking()
                .GroupBy(l => l.Level)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            LevelCounts = counts.ToDictionary(x => x.Level ?? "Unknown", x => x.Count);

            // Load available roles for admin create user form
            AvailableRoles = await _roleManager.Roles
                .Select(r => r.Name ?? string.Empty)
                .Where(n => n != string.Empty)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                // reload roles for redisplay
                AvailableRoles = await _roleManager.Roles
                    .Select(r => r.Name ?? string.Empty)
                    .Where(n => n != string.Empty)
                    .ToListAsync();
                await OnGetAsync();
                return Page();
            }

            var user = new IdentityUser { UserName = CreateInput.Email, Email = CreateInput.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, CreateInput.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                }
                AvailableRoles = await _roleManager.Roles
                    .Select(r => r.Name ?? string.Empty)
                    .Where(n => n != string.Empty)
                    .ToListAsync();
                await OnGetAsync();
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(CreateInput.Role))
            {
                // ensure role exists
                if (!await _roleManager.RoleExistsAsync(CreateInput.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(CreateInput.Role));
                }
                await _userManager.AddToRoleAsync(user, CreateInput.Role);
            }

            CreateUserStatusMessage = $"User {CreateInput.Email} created.";

            return RedirectToPage();
        }
    }
}
