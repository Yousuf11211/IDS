using IDS.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class HelpSupportModel(EmailConfiguration email) : PageModel
{
    public string? SupportEmail { get; private set; }

    public async Task OnGetAsync(CancellationToken token) => SupportEmail = await email.ContactAsync(token);
}
