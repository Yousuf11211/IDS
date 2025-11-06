using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IDS.Data.Models;

namespace IDS.Security
{
 public static class AppRoles
 {
 public const string Admin = "Admin";
 public const string Employee = "Employee";
 public const string Suspended = "Suspended";

 public static readonly string[] All = new[] { Admin, Employee, Suspended };

 public static bool IsManagedRole(string role) => All.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
 }

 public class AccessControlService
 {
 private readonly RoleManager<IdentityRole> _roleManager;

 public AccessControlService(RoleManager<IdentityRole> roleManager)
 {
 _roleManager = roleManager;
 }

 public async Task EnsureRoleExistsAsync(string role)
 {
 if (!AppRoles.IsManagedRole(role)) return;
 if (!await _roleManager.RoleExistsAsync(role))
 {
 await _roleManager.CreateAsync(new IdentityRole(role));
 }
 }

 // Enforce exactly one role from AppRoles at any given time
 public async Task SetExclusiveRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
 {
 if (user == null) throw new ArgumentNullException(nameof(user));
 if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
 if (!AppRoles.IsManagedRole(role)) throw new ArgumentException("Unknown role", nameof(role));

 await EnsureRoleExistsAsync(role);

 var currentRoles = await userManager.GetRolesAsync(user);
 var toRemove = currentRoles.Where(r => AppRoles.IsManagedRole(r) && !string.Equals(r, role, StringComparison.OrdinalIgnoreCase)).ToArray();
 if (toRemove.Length >0)
 {
 await userManager.RemoveFromRolesAsync(user, toRemove);
 }

 if (!currentRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
 {
 await userManager.AddToRoleAsync(user, role);
 }
 }

 public async Task<bool> IsSuspendedAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
 {
 if (user == null) return false;
 return await userManager.IsInRoleAsync(user, AppRoles.Suspended);
 }
 }
}
