using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Add necessary using statements
using IDS.Data; // To access ApplicationDbContext
using IDS.Data.Models; // To access RoleEntry

namespace IDS.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserListModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db; // <-- ADDED FIELD

        // UPDATED CONSTRUCTOR TO INJECT DB CONTEXT
        public UserListModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db; // <-- ASSIGNED FIELD
        }

        public List<UserEntry> Users { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public class UserEntry
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public List<string> Roles { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            // AllRoles property is populated from Identity's RoleManager, not the custom table, but this is fine.
            AllRoles = await _roleManager.Roles
                .Select(r => r.Name ?? string.Empty)
                .Where(n => n != string.Empty)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            foreach (var u in users)
            {
                var roles = (await _userManager.GetRolesAsync(u)).ToList();
                Users.Add(new UserEntry
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? string.Empty,
                    Roles = roles
                });
            }
        }

        // Existing OnPostAddRoleAsync (from dropdown)
        public async Task<IActionResult> OnPostAddRoleAsync(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
            {
                StatusMessage = "Invalid input.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "User not found.";
                return RedirectToPage();
            }

            // Ensure role exists in Identity
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));

                // ADDED LOGIC: Sync to custom RolesList when creating a role from this handler
                if (!await _db.RolesList.AnyAsync(r => r.Name == role))
                {
                    _db.RolesList.Add(new RoleEntry { Name = role });
                    await _db.SaveChangesAsync();
                }
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            StatusMessage = result.Succeeded ? $"Role '{role}' added to {user.Email}." : "Failed to add role.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
            {
                StatusMessage = "Invalid input.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "User not found.";
                return RedirectToPage();
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            StatusMessage = result.Succeeded ? $"Role '{role}' removed from {user.Email}." : "Failed to remove role.";
            return RedirectToPage();
        }

        // NEW HANDLER: OnPostCreateRoleAsync (from text box)
        public async Task<IActionResult> OnPostCreateRoleAsync(string role)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(role))
            {
                StatusMessage = "Invalid input: Role name cannot be empty.";
                return RedirectToPage();
            }

            // 2. Check if the role already exists in Identity
            if (await _roleManager.RoleExistsAsync(role))
            {
                StatusMessage = $"Role '{role}' already exists.";
                return RedirectToPage();
            }

            // 3. Create the role in Identity (AspNetRoles table)
            var identityResult = await _roleManager.CreateAsync(new IdentityRole(role));

            if (identityResult.Succeeded)
            {
                // 4. Create the role in your custom database table (RolesList/RoleEntry)
                _db.RolesList.Add(new RoleEntry { Name = role });
                await _db.SaveChangesAsync(); // Saves to your custom Roles table

                StatusMessage = $"Role '{role}' created successfully.";
            }
            else
            {
                // Handle creation errors
                StatusMessage = $"Failed to create role '{role}'. Errors: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}";
            }

            return RedirectToPage();
        }
    }
}