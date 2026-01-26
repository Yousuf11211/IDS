using IDS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IDS.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file if it exists (for local development)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// Get connection string (environment variable takes precedence)
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register Identity with role support
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie configuration for session timeout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 30 minutes for production
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();

// Register services
builder.Services.AddScoped<AccessControlService>();
builder.Services.AddTransient<IEmailSender, MailjetEmailSender>(); // Use Mailjet instead of SendGrid
builder.Services.AddScoped<IDetectionService, DetectionService>();
builder.Services.AddScoped<IAuditService, AuditService>();

var app = builder.Build();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdminAsync(services);
}

async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var config = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Ensure all managed roles exist
    foreach (var r in AppRoles.All)
    {
        if (!await roleManager.RoleExistsAsync(r))
        {
            await roleManager.CreateAsync(new IdentityRole(r));
            logger.LogInformation("Created role: {Role}", r);
        }
    }

    // Get admin credentials from environment variables (priority) or config
    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
     ?? config["AdminUser:Email"] 
        ?? "admin@local";
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") 
        ?? config["AdminUser:Password"] 
        ?? "P@ssw0rd!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser 
        { 
       UserName = adminEmail, 
            Email = adminEmail, 
      EmailConfirmed = true, 
      MustChangePassword = false,
    FirstName = "System",
            LastName = "Administrator"
   };
        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            logger.LogInformation("Created admin user: {Email}", adminEmail);
   }
    else
        {
 logger.LogError("Failed to create admin user: {Errors}", 
      string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
    {
        // Ensure admin has correct role
        var currentRoles = await userManager.GetRolesAsync(adminUser);
    var toRemove = currentRoles.Where(r => AppRoles.IsManagedRole(r) && r != AppRoles.Admin);
        if (toRemove.Any())
         await userManager.RemoveFromRolesAsync(adminUser, toRemove);
        await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
    }

    // Ensure admin can sign in
    if (adminUser != null)
    {
  try
  {
    if (!adminUser.EmailConfirmed)
{
        adminUser.EmailConfirmed = true;
         await userManager.UpdateAsync(adminUser);
 }
     await userManager.SetLockoutEndDateAsync(adminUser, null);
      await userManager.ResetAccessFailedCountAsync(adminUser);
    }
    catch (Exception ex)
  {
         logger.LogWarning(ex, "Error ensuring admin user state");
  }
    }
}

// Configure the HTTP request pipeline
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

// Auth ping endpoint for session checking
app.MapGet("/auth/ping", () => Results.Ok()).RequireAuthorization();

// Enforce first-time password change redirect
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user != null && user.MustChangePassword)
        {
    var path = context.Request.Path.Value ?? string.Empty;
 var allowedPaths = new[] { "/Identity/Account/FirstTimeSetup", "/Account/Logout", "/Identity/Account/Logout" };
            if (!allowedPaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)))
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
