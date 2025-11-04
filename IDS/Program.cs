using IDS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    var dbContext = services.GetRequiredService<IDS.Data.ApplicationDbContext>(); // <-- ADDED: Get custom DB context

    const string adminRole = "Admin";

    // --- Identity Role Creation ---
    // Ensure Admin role exists in Identity tables (AspNetRoles)
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // --- Custom Role Entry Synchronization ---
    // Check if Admin role is missing from your custom RolesList table
    if (!await dbContext.RolesList.AnyAsync(r => r.Name == adminRole))
    {
        // ADDED LOGIC: Insert the role into your custom Roles table
        dbContext.RolesList.Add(new IDS.Data.Models.RoleEntry { Name = adminRole });
        await dbContext.SaveChangesAsync();
    }

    // ... (rest of the user seeding logic, which remains the same) ...

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
        else
        {
            // If creation failed, optionally log errors to help debugging
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, adminRole))
    {
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }

    // One-time safety: ensure admin can sign in
    if (adminUser != null)
    {
        // Reset password to configured value (useful if seed previously failed or password policy changed)
        try
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
            // ensure email confirmed
            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }

            // clear lockout and failed access count
            await userManager.SetLockoutEndDateAsync(adminUser, null);
            await userManager.ResetAccessFailedCountAsync(adminUser);
        }
        catch
        {
            // swallow exceptions to avoid startup failure; inspect logs if needed
        }
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
 // The default HSTS value is30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ensure authentication middleware runs before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
