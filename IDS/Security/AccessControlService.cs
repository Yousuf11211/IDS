using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IDS.Data.Models;
using IDS.Data;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace IDS.Security
{
    /// <summary>
    /// Defines the application roles and their hierarchy.
    /// Role Hierarchy: Admin > Support > Employee > Suspended
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Support = "Support";  // IT Support role - can help users but limited admin access
        public const string Employee = "Employee";
        public const string Suspended = "Suspended";

        /// <summary>
        /// All managed roles in the system.
        /// </summary>
        public static readonly string[] All = new[] { Admin, Support, Employee, Suspended };

        /// <summary>
        /// Roles that have administrative privileges (can access admin pages).
        /// </summary>
        public static readonly string[] AdminRoles = new[] { Admin };

        /// <summary>
        /// Roles that can access the Support Dashboard and manage tickets.
        /// </summary>
        public static readonly string[] SupportRoles = new[] { Admin, Support };

        /// <summary>
        /// Combined role string for authorization attributes (Admin or Support).
        /// </summary>
        public const string AdminOrSupport = Admin + "," + Support;

        /// <summary>
        /// Checks if a role is a managed system role.
        /// </summary>
        public static bool IsManagedRole(string role) => All.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Checks if a role has support-level access (Support or Admin).
        /// </summary>
        public static bool HasSupportAccess(string role) => SupportRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Checks if a role has full admin access.
        /// </summary>
        public static bool HasAdminAccess(string role) => AdminRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }

    public class AccessControlService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _database;

        public AccessControlService(RoleManager<IdentityRole> roleManager, ApplicationDbContext database)
        {
            _roleManager = roleManager;
            _database = database;
        }

        public async Task EnsureRoleExistsAsync(string role)
        {
            if (!AppRoles.IsManagedRole(role)) return;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        /// <summary>
        /// Ensures all application roles exist in the database.
        /// </summary>
        public async Task EnsureAllRolesExistAsync()
        {
            foreach (var role in AppRoles.All)
            {
                await EnsureRoleExistsAsync(role);
            }
        }

        /// <summary>
        /// Enforce exactly one role from AppRoles at any given time.
        /// </summary>
        public async Task SetExclusiveRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string role,
            bool allowAdministratorChange = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (!AppRoles.IsManagedRole(role)) throw new ArgumentException("Unknown role", nameof(role));

            await using var transaction = _database.Database.CurrentTransaction == null
                ? await _database.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
            await EnsureRoleExistsAsync(role);

            var currentRoles = await userManager.GetRolesAsync(user);
            if (!allowAdministratorChange && (string.Equals(role, AppRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                currentRoles.Contains(AppRoles.Admin)))
                throw new SecurityChangeException("Administrator role changes require the approved security workflow.");
            var toRemove = currentRoles.Where(r => AppRoles.IsManagedRole(r) && !string.Equals(r, role, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (toRemove.Length > 0)
            {
                EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, toRemove));
            }

            if (!currentRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
            {
                EnsureSucceeded(await userManager.AddToRoleAsync(user, role));
            }
            EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user));
            if (transaction != null) await transaction.CommitAsync();
        }

        private static void EnsureSucceeded(IdentityResult result)
        {
            if (!result.Succeeded) throw new SecurityChangeException("The account changed. Reload before changing its role.");
        }

        public async Task<bool> IsSuspendedAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (user == null) return false;
            return await userManager.IsInRoleAsync(user, AppRoles.Suspended);
        }

        /// <summary>
        /// Checks if a user has support-level access (Support or Admin role).
        /// </summary>
        public async Task<bool> HasSupportAccessAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (user == null) return false;
            var roles = await userManager.GetRolesAsync(user);
            return roles.Any(r => AppRoles.HasSupportAccess(r));
        }

        /// <summary>
        /// Checks if a user has full admin access.
        /// </summary>
        public async Task<bool> HasAdminAccessAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (user == null) return false;
            return await userManager.IsInRoleAsync(user, AppRoles.Admin);
        }
    }
}
