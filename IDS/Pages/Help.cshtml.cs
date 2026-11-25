using IDS.Data.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDS.Pages;

public sealed class HelpModel(EmailConfiguration email) : PageModel
{
    public string? SupportEmail { get; private set; }

    public async Task OnGetAsync(CancellationToken token) => SupportEmail = await email.ContactAsync(token);
}
