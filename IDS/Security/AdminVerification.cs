using System.Globalization;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IDS.Security;

public sealed class AdminVerification(UserManager<ApplicationUser> users, SecurityPolicyService policies)
{
    private const string VerifiedAtKey = "ids.adminVerifiedAt";
    private const string VerifiedStampKey = "ids.adminVerifiedStamp";
    private const string VerifiedWithBypassKey = "ids.adminVerifiedWithMfaBypass";

    public async Task<bool> IsRecentAsync(HttpContext context)
    {
        var authentication = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authentication.Succeeded || authentication.Properties == null) return false;
        var items = authentication.Properties.Items;
        if (!items.TryGetValue(VerifiedAtKey, out var value) ||
            !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at) ||
            at > DateTimeOffset.UtcNow || DateTimeOffset.UtcNow - at > TimeSpan.FromMinutes(5)) return false;
        var user = await users.GetUserAsync(context.User);
        var bypass = user != null && await policies.CanBypassTwoFactorAsync(user);
        if (items.TryGetValue(VerifiedWithBypassKey, out var usedBypass) && usedBypass == "true" && !bypass) return false;
        return user != null && user.EmailConfirmed && (user.TwoFactorEnabled || bypass) && !user.MustChangePassword &&
            await users.IsInRoleAsync(user, AppRoles.Admin) && !await users.IsInRoleAsync(user, AppRoles.Suspended) &&
            !await users.IsLockedOutAsync(user) &&
            items.TryGetValue(VerifiedStampKey, out var stamp) && stamp == user.SecurityStamp;
    }

    public async Task MarkVerifiedAsync(HttpContext context, ApplicationUser user)
    {
        var authentication = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authentication.Succeeded || authentication.Properties == null || authentication.Principal == null)
            throw new InvalidOperationException("An active login is required.");
        // Authentication properties are encrypted with the cookie and belong to this session only.
        authentication.Properties.Items[VerifiedAtKey] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        authentication.Properties.Items[VerifiedStampKey] = user.SecurityStamp;
        authentication.Properties.Items[VerifiedWithBypassKey] = await policies.CanBypassTwoFactorAsync(user) ? "true" : "false";
        await context.SignInAsync(IdentityConstants.ApplicationScheme,
            authentication.Principal, authentication.Properties);
    }
}

public sealed class AdminVerificationFilter(AdminVerification verification, IServiceScopeFactory scopes) : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var path = context.ActionDescriptor.ViewEnginePath;
        if (!path.StartsWith("/Admin/", StringComparison.OrdinalIgnoreCase) || path == "/Admin/Verify")
        {
            await next();
            return;
        }
        if (!await verification.IsRecentAsync(context.HttpContext))
        {
            var request = context.HttpContext.Request;
            context.Result = new RedirectToPageResult("/Admin/Verify", new
            {
                returnUrl = request.PathBase + request.Path + request.QueryString
            });
            return;
        }

        var result = await next();
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method)) return;

        // Record the route and outcome only; form bodies may contain credentials or tokens.
        using var scope = scopes.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.AuditLogs.Add(new AuditLog
        {
            UserId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
            UserEmail = context.HttpContext.User.Identity?.Name ?? "",
            Action = "Admin.Action", EntityType = "AdminPage", EntityId = path,
            Details = $"Handler: {context.HandlerMethod?.Name}; result: {result.Result?.GetType().Name ?? "Exception"}",
            IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(), Timestamp = DateTime.UtcNow
        });
        await database.SaveChangesAsync();
    }
}
