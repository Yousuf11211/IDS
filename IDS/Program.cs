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

 const string adminRole = "Admin";

 // Ensure Admin role exists
 if (!await roleManager.RoleExistsAsync(adminRole))
 {
 await roleManager.CreateAsync(new IdentityRole(adminRole));
 }

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
 // Optionally log createResult.Errors
 }
 else if (!await userManager.IsInRoleAsync(adminUser, adminRole))
 {
 await userManager.AddToRoleAsync(adminUser, adminRole);
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
