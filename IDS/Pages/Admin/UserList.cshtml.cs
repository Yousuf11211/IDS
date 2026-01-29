using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDS.Security;
using IDS.Data.Models;

namespace IDS.Pages.Admin
{
    /// <summary>
    /// User list page - displays all users with filtering.
    /// Actions (edit, delete, etc.) are handled in the EditUser page.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class UserListModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserListModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public class UserEntry
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string CurrentRole { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }

        public List<UserEntry> Users { get; set; } = new();
        public List<string> AllRoles { get; set; } = AppRoles.All.ToList();
        
        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync(string? role, string? search)
        {
            var allUsers = await _userManager.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault(r => AppRoles.IsManagedRole(r)) ?? string.Empty;

                // Apply role filter
                if (!string.IsNullOrEmpty(role) && !string.Equals(role, currentRole, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    var matchesEmail = user.Email?.ToLower().Contains(searchLower) ?? false;
                    var matchesFirstName = user.FirstName?.ToLower().Contains(searchLower) ?? false;
                    var matchesLastName = user.LastName?.ToLower().Contains(searchLower) ?? false;
       
                    if (!matchesEmail && !matchesFirstName && !matchesLastName)
                    {
                        continue;
                    }
                }

                Users.Add(new UserEntry
                {
                    Id = user.Id,
                    Email = user.Email ?? user.UserName ?? string.Empty,
                    CurrentRole = currentRole,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty
                });
            }

            // Sort: Admins first, then Support, then Employees, then Suspended
            Users = Users
                .OrderBy(u => u.CurrentRole == "Admin" ? 0 : u.CurrentRole == "Support" ? 1 : u.CurrentRole == "Employee" ? 2 : 3)
                .ThenBy(u => u.Email)
                .ToList();
        }
    }
}