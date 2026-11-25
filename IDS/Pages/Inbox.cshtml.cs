using System.Security.Claims;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages;

[Authorize]
public sealed class InboxModel(ApplicationDbContext database) : PageModel
{
    private const int PageSize = 30;
    private IQueryable<InboxNotification> Mine =>
        database.InboxNotifications.Where(n => n.RecipientId == User.FindFirstValue(ClaimTypes.NameIdentifier));

    public List<InboxNotification> Items { get; private set; } = [];
    public int CurrentPage { get; private set; }
    public bool HasNext { get; private set; }

    public async Task OnGetAsync(int p = 1)
    {
        CurrentPage = Math.Clamp(p, 1, int.MaxValue / PageSize);
        Items = await Newest().Skip((CurrentPage - 1) * PageSize).Take(PageSize + 1).ToListAsync();
        HasNext = Items.Count > PageSize;
        if (HasNext) Items.RemoveAt(PageSize);
    }

    public async Task<JsonResult> OnGetRecentAsync(CancellationToken token)
    {
        var unread = await Mine.CountAsync(n => n.ReadAt == null, token);
        var recent = await Newest().Take(5).Select(n => new
        {
            n.Id, n.Type, n.Message, n.SenderName, n.CreatedAt, IsUnread = n.ReadAt == null
        }).ToListAsync(token);
        return new JsonResult(new { unread, recent });
    }

    public async Task<IActionResult> OnPostOpenAsync(long id)
    {
        var notification = await Mine.Include(n => n.Ticket).SingleOrDefaultAsync(n => n.Id == id);
        if (notification == null) return NotFound();
        if (notification.Ticket != null && !IncidentWorkflow.CanView(notification.Ticket, User)) return Forbid();

        if (notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await database.SaveChangesAsync();
        }
        if (notification.TicketId is int ticketId) return RedirectToPage("/ViewTicket", new { id = ticketId });
        if (User.IsInRole(AppRoles.Admin))
        {
            if (notification.SecurityAlertId is int alertId) return RedirectToPage("/Admin/AlertDetails", new { id = alertId });
            if (notification.AuthenticatorResetRequestId is long requestId)
                return RedirectToPage("/Admin/RecoveryRequests", new { id = requestId });
            if (notification.Type == "Security email needs attention") return RedirectToPage("/Admin/EmailDelivery");
        }
        if (notification.IsAccountSecurity)
        {
            if (notification.Type.StartsWith("Password", StringComparison.Ordinal))
                return RedirectToPage("/Account/Manage/ChangePassword", new { area = "Identity" });
            if (notification.Type.StartsWith("Registered email", StringComparison.Ordinal))
                return RedirectToPage("/Account/Manage/Email", new { area = "Identity" });
            return RedirectToPage("/Account/Manage/TwoFactorAuthentication", pageHandler: null,
                routeValues: new { area = "Identity" }, fragment: "recovery-requests");
        }
        return RedirectToPage("/Inbox", pageHandler: null, routeValues: null, fragment: $"notification-{id}");
    }

    public async Task<IActionResult> OnPostStateAsync(long id, bool unread, int p = 1)
    {
        var notification = await Mine.SingleOrDefaultAsync(n => n.Id == id);
        if (notification == null) return NotFound();
        notification.ReadAt = unread ? null : DateTime.UtcNow;
        await database.SaveChangesAsync();
        return RedirectToPage(new { p });
    }

    public async Task<IActionResult> OnPostReadAllAsync()
    {
        var now = DateTime.UtcNow;
        await Mine.Where(n => n.ReadAt == null).ExecuteUpdateAsync(update => update.SetProperty(n => n.ReadAt, now));
        return RedirectToPage();
    }

    private IOrderedQueryable<InboxNotification> Newest() =>
        Mine.AsNoTracking().OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id);
}
