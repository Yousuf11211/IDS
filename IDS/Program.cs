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
using Microsoft.AspNetCore.SignalR;

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

// Remembered browsers never skip MFA; only the explicit employee testing policy can do so.
builder.Services.AddScoped<SignInManager<ApplicationUser>, AlwaysChallengeSignInManager>();
// Revalidate cookies on every request so suspension and session revocation take effect promptly.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
    options.OnRefreshingPrincipal = context =>
    {
        // Keep the test-session marker when Identity refreshes the user's role claims.
        var marker = context.CurrentPrincipal?.FindFirst(SecurityPolicyService.BypassedMfaClaim);
        if (marker != null && context.NewPrincipal?.Identity is System.Security.Claims.ClaimsIdentity identity)
            identity.AddClaim(marker);
        return Task.CompletedTask;
    };
});

// Cookie configuration for session timeout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages(options => options.Conventions.ConfigureFilter(
    new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(AdminVerificationFilter))));

// =====================================================
// Add SignalR for real-time dashboard updates
// =====================================================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.AddFilter<RealtimeAccessFilter>();
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
builder.Services.AddScoped<SecurityPolicyService>();
builder.Services.AddScoped<SecurityAdministrationService>();
builder.Services.AddScoped<AdminVerification>();
builder.Services.AddScoped<AdminVerificationFilter>();
builder.Services.AddSingleton<RealtimeSessionRegistry>();
builder.Services.AddSingleton<RealtimeAccessFilter>();
builder.Services.AddScoped<RealtimeSessionValidator>();
builder.Services.AddHostedService<RealtimeSessionMonitor>();

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
// Prepare the database before Identity queries mapped user columns.
// =====================================================
var migrateOnly = args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase);
var operatorCommand = args.Any(argument => argument.StartsWith("--provision-admin=", StringComparison.Ordinal) ||
    argument.StartsWith("--recover-admin=", StringComparison.Ordinal) || argument.StartsWith("--suspend-admin=", StringComparison.Ordinal));
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var database = services.GetRequiredService<ApplicationDbContext>().Database;

    // Local development stays in sync with model changes. Deployed databases are
    // upgraded explicitly with --migrate-only before starting application instances.
    if (app.Environment.IsDevelopment() || migrateOnly)
    {
        await database.MigrateAsync();
    }
    else
    {
        var pendingMigrations = (await database.GetPendingMigrationsAsync()).ToArray();
        if (pendingMigrations.Length > 0)
        {
            throw new InvalidOperationException(
                $"The IDS database has pending migrations: {string.Join(", ", pendingMigrations)}. " +
                "Run 'dotnet run --project IDS/IDS.csproj --no-launch-profile -- --migrate-only' " +
                "from the solution directory before starting the application.");
        }
    }

    if (operatorCommand)
    {
        await SecurityOperatorCommands.RunAsync(args, services);
    }
    else if (!migrateOnly)
    {
        await SeedRolesAndAdminAsync(services);
    }
}

if (migrateOnly || operatorCommand)
{
    app.Logger.LogInformation("IDS maintenance command completed successfully.");
    await app.DisposeAsync();
    return;
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

    // Seeding only bootstraps the first administrator. Never restore permissions or
    // clear lockouts on startup; doing so would undo incident-response actions.
    if ((await userManager.GetUsersInRoleAsync(AppRoles.Admin)).Count > 0)
    {
        return;
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
    else
    {
        throw new InvalidOperationException("No administrator is available. A deployment operator must provision an existing, verified account with --provision-admin=<email>.");
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
app.MapHub<DashboardHub>("/hubs/dashboard", options => options.CloseOnAuthenticationExpiration = true);
// Access is checked against the current policy on every connection and hub call.
app.MapHub<ChatHub>("/hubs/chat", options => options.CloseOnAuthenticationExpiration = true);

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
            var policies = context.RequestServices.GetRequiredService<SecurityPolicyService>();
            var bypassTwoFactor = await policies.CanBypassTwoFactorAsync(user);
            var expiredTestSession = context.User.HasClaim(SecurityPolicyService.BypassedMfaClaim, "true") && !bypassTwoFactor;
            if (expiredTestSession || await userManager.IsInRoleAsync(user, AppRoles.Suspended) || await userManager.IsLockedOutAsync(user))
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

            if (!user.MustChangePassword && !bypassTwoFactor && !await userManager.GetTwoFactorEnabledAsync(user)
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
