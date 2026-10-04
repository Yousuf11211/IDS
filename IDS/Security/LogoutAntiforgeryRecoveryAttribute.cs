using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IDS.Security;

/// <summary>Offer a fresh confirmation form when a stale page cannot submit logout.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LogoutAntiforgeryRecoveryAttribute : Attribute, IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is IAntiforgeryValidationFailedResult)
        {
            // Keep CSRF protection: the rejected request must never sign the user out.
            context.Result = new RedirectToPageResult("/Account/Logout", new { area = "Identity", retry = true });
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
