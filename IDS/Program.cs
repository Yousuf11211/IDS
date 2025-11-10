using IDS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using IDS.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using IDS.Data.Models; // <-- Add this for ApplicationUser

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register Identity with role support so RoleManager<IdentityRole> is available
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
 .AddEntityFrameworkStores<ApplicationDbContext>()
 .AddDefaultTokenProviders();

// Inactivity timeout:5 minutes (testing)
builder.Services.ConfigureApplicationCookie(options =>
{
 options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
 options.SlidingExpiration = true; // extend while active, expire after inactivity
 // optional paths keep defaults from Identity UI 
});

builder.Services.AddRazorPages(); 

// Register access control service
builder.Services.AddScoped<AccessControlService>();
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
builder.Services.AddRazorPages();
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
 var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); // <-- Use ApplicationUser
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
 adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, MustChangePassword = false };
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

// small ping endpoint to detect expired auth client-side
app.MapGet("/auth/ping", () => Results.Ok()).RequireAuthorization();

// Enforce first-time setup redirect
app.Use(async (context, next) =>
{
 if (context.User?.Identity?.IsAuthenticated == true)
 {
 var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
 var user = await userManager.GetUserAsync(context.User);
 if (user != null && user.MustChangePassword)
 {
 var path = context.Request.Path.Value ?? string.Empty;
 if (!path.Contains("/Identity/Account/FirstTimeSetup", StringComparison.OrdinalIgnoreCase) && !path.Contains("/Account/Logout", StringComparison.OrdinalIgnoreCase))
 {
 context.Response.Redirect("/Identity/Account/FirstTimeSetup");
 return;
 }
 }
 }
 await next();
});

app.MapRazorPages();

app.Run();
