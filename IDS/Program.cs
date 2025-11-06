using IDS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using IDS.Security; // added

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register Identity with role support so RoleManager<IdentityRole> is available
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
 .AddEntityFrameworkStores<ApplicationDbContext>()
 .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

// Register access control service
builder.Services.AddScoped<AccessControlService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
 var services = scope.ServiceProvider;
 await SeedRolesAndAdminAsync(services);
}

// Seed default roles and an admin user
async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
 var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
 var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
 var config = services.GetRequiredService<IConfiguration>();
 var dbContext = services.GetRequiredService<IDS.Data.ApplicationDbContext>();

 // Ensure managed roles exist
 foreach (var r in AppRoles.All)
 {
 if (!await roleManager.RoleExistsAsync(r))
 {
 await roleManager.CreateAsync(new IdentityRole(r));
 }
 }

 const string adminRole = AppRoles.Admin;

 var adminEmail = config["AdminUser:Email"] ?? "admin@local";
 var adminPassword = config["AdminUser:Password"] ?? "P@ssw0rd!";

 var adminUser = await userManager.FindByEmailAsync(adminEmail);
 if (adminUser == null)
 {
 adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
 var createResult = await userManager.CreateAsync(adminUser, adminPassword);
 if (createResult.Succeeded)
 {
 await userManager.AddToRoleAsync(adminUser, adminRole);
 }
 }
 else if (!await userManager.IsInRoleAsync(adminUser, adminRole))
 {
 // Ensure exclusive assignment: remove other managed roles first
 var currentRoles = await userManager.GetRolesAsync(adminUser);
 var toRemove = currentRoles.Where(r => AppRoles.IsManagedRole(r) && r != adminRole);
 if (toRemove.Any())
 await userManager.RemoveFromRolesAsync(adminUser, toRemove);
 await userManager.AddToRoleAsync(adminUser, adminRole);
 }

 // One-time safety: ensure admin can sign in
 if (adminUser != null)
 {
 try
 {
 var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
 await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
 if (!adminUser.EmailConfirmed)
 {
 adminUser.EmailConfirmed = true;
 await userManager.UpdateAsync(adminUser);
 }
 await userManager.SetLockoutEndDateAsync(adminUser, null);
 await userManager.ResetAccessFailedCountAsync(adminUser);
 }
 catch { }
 }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 app.UseMigrationsEndPoint();
}
else
{
 app.UseExceptionHandler("/Error");
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
