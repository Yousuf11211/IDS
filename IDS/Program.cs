using IDS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IDS.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using IDS.Data.Models;
using IDS.Data.Services;
using IDS.Core.Services;
using IDS.Hubs;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Load environment variables from .env file
// This should be done early, before other configuration
// =====================================================
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    // Use DotNetEnv for robust .env parsing (handles quotes, multiline, etc.)
    Env.Load(envPath);
    builder.Logging.AddConsole();
    Console.WriteLine("[Startup] Loaded .env file");
}
else
{
    Console.WriteLine("[Startup] No .env file found - using environment variables and appsettings.json");
}

// =====================================================
// Database Configuration
// =====================================================
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =====================================================
// Identity Configuration
// =====================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(24));

// An enrolled account must provide an authenticator code at every login.
builder.Services.AddScoped<SignInManager<ApplicationUser>, AlwaysChallengeSignInManager>();

// Cookie configuration for session timeout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();

// =====================================================
// Add SignalR for real-time dashboard updates
// =====================================================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// =====================================================
// Add API Controllers (read-only detection data access)
// =====================================================
builder.Services.AddControllers();

// =====================================================
// Register Application Services
// =====================================================
builder.Services.AddScoped<AccessControlService>();
builder.Services.AddSingleton<AccountLinkBuilder>();

// Register MailjetEmailSender as both IEmailSender and its concrete type
// This allows injection of either interface or concrete class
builder.Services.AddSingleton<MailjetEmailSender>();
builder.Services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<MailjetEmailSender>());

builder.Services.AddScoped<IDetectionService, DetectionService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// =====================================================
// Live Detection Services (READ-ONLY)
// Monitors Benign_Table and Attack_Table written by external pipeline
// =====================================================
builder.Services.AddScoped<IDashboardNotificationService, DashboardNotificationService>();
builder.Services.AddScoped<ILiveDetectionService, LiveDetectionService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Background service to monitor detection tables and push updates via SignalR
builder.Services.AddHostedService<DetectionMonitorService>();

var app = builder.Build();

// =====================================================
// Seed Roles and Admin User
// =====================================================
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

    // Get admin credentials (environment variables take priority)
    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
        ?? config["AdminUser:Email"] 
        ?? "admin@local";
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Set ADMIN_PASSWORD before starting a database without an admin account.");
        }

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

// =====================================================
// Configure HTTP Pipeline
// =====================================================
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

// =====================================================
// Map SignalR Hub for real-time dashboard
// =====================================================
app.MapHub<DashboardHub>("/hubs/dashboard");
if (builder.Configuration.GetValue<bool>("Chat:Enabled"))
{
    app.MapHub<ChatHub>("/hubs/chat");
}

// =====================================================
// Map API Controllers (read-only detection data)
// =====================================================
app.MapControllers();

// =====================================================
// Complete account setup before any authenticated app page can be used.
// =====================================================
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user != null)
        {
            if (await userManager.IsInRoleAsync(user, AppRoles.Suspended))
            {
                await context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>()
                    .SignOutAsync();
                context.Response.Redirect("/Identity/Account/Login");
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;
            var isLogout = path.Equals("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase);
            var isFirstTimeSetup = path.Equals("/Identity/Account/FirstTimeSetup", StringComparison.OrdinalIgnoreCase);
            var isAuthenticatorSetup = path.Equals("/Identity/Account/Manage/EnableAuthenticator", StringComparison.OrdinalIgnoreCase);

            if (user.MustChangePassword && !isFirstTimeSetup && !isLogout)
            {
                context.Response.Redirect("/Identity/Account/FirstTimeSetup");
                return;
            }

            if (!user.MustChangePassword && !await userManager.GetTwoFactorEnabledAsync(user)
                && !isAuthenticatorSetup && !isLogout)
            {
                context.Response.Redirect("/Identity/Account/Manage/EnableAuthenticator");
                return;
            }
        }
    }
    await next();
});

app.MapRazorPages();

app.Run();
