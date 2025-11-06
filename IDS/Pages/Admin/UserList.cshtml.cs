using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDS.Security;

namespace IDS.Pages.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UserListModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AccessControlService _accessControl;

        public UserListModel(UserManager<IdentityUser> userManager, AccessControlService accessControl)
        {
            _userManager = userManager;
            _accessControl = accessControl;
        }

        public class UserEntry
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string CurrentRole { get; set; } = string.Empty;
        }

        public List<UserEntry> Users { get; set; } = new();
        public List<string> AllRoles { get; set; } = AppRoles.All.ToList();
        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync(string role)
        {
            var all = await _userManager.Users.ToListAsync();
            foreach (var u in all)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var current = roles.FirstOrDefault(r => AppRoles.IsManagedRole(r)) ?? string.Empty;
                if (string.IsNullOrEmpty(role) || string.Equals(role, current, System.StringComparison.OrdinalIgnoreCase))
                {
                    Users.Add(new UserEntry { Id = u.Id, Email = u.Email ?? u.UserName ?? string.Empty, CurrentRole = current });
                }
            }
        }

        public async Task<IActionResult> OnPostSetRoleAsync(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role) || !AppRoles.IsManagedRole(role))
            {
                StatusMessage = "Invalid input.";
                return RedirectToPage(new { role = string.Empty });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "User not found.";
                return RedirectToPage(new { role = string.Empty });
            }
            await _accessControl.SetExclusiveRoleAsync(_userManager, user, role);
            StatusMessage = $"Updated role for {user.Email} to '{role}'.";
            return RedirectToPage(new { role = string.Empty });
        }
    }
}