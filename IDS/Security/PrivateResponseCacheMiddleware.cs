using Microsoft.AspNetCore.Authorization;

namespace IDS.Security;

/// <summary>Keep account pages and authenticated responses out of HTTP caches.</summary>
public sealed class PrivateResponseCacheMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        // Apply last, including when authentication redirects or a page sets its own cache headers.
        context.Response.OnStarting(() =>
        {
            if (context.User.Identity?.IsAuthenticated == true ||
                context.GetEndpoint()?.Metadata.GetMetadata<IAuthorizeData>() != null ||
                context.Request.Path.StartsWithSegments("/Identity/Account"))
            {
                context.Response.Headers.CacheControl = "no-store, no-cache, private, max-age=0";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "0";
            }
            return Task.CompletedTask;
        });
        return next(context);
    }
}
