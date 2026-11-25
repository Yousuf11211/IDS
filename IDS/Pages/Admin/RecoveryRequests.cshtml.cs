using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages.Admin;

[Authorize(Roles = AppRoles.Admin)]
public sealed class RecoveryRequestsModel(ApplicationDbContext database, UserManager<ApplicationUser> users,
    AccountRecovery recovery, AdminVerification verification) : PageModel
{
    public static readonly string[] Statuses = ["Pending", "Approved", "Rejected", "Completed", "Expired", "Cancelled"];
    public List<AuthenticatorResetRequest> Requests { get; private set; } = [];
    public AuthenticatorResetRequest? Selected { get; private set; }
    public ApplicationUser? Account { get; private set; }
    public bool HasNext { get; private set; }
    public string? ActorId => users.GetUserId(User);
    [BindProperty(SupportsGet = true)] public string? Status { get; set; } = "Pending";
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int P { get; set; } = 1;
    [TempData] public string? Result { get; set; }
    [BindProperty] public Decision Input { get; set; } = new();

    public sealed class Decision
    {
        [Required, StringLength(1000)] public string? Reason { get; set; }
        [StringLength(1000)] public string? Verification { get; set; }
        [Required, StringLength(100), DataType(DataType.Password)] public string? Password { get; set; }
        [StringLength(16)] public string? Code { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long? id, CancellationToken token)
    {
        const int size = 25;
        P = Math.Clamp(P, 1, 1000000);
        var now = DateTime.UtcNow;
        var query = database.AuthenticatorResetRequests.AsNoTracking();
        if (id.HasValue)
        {
            Selected = await query.SingleOrDefaultAsync(r => r.Id == id, token);
            if (Selected == null) return NotFound();
            Account = await users.FindByIdAsync(Selected.UserId);
        }
        query = Status switch
        {
            "Pending" or "Approved" => query.Where(r => r.Status == Status && r.ExpiresAt > now),
            "Expired" => query.Where(r => r.Status == "Expired" ||
                ((r.Status == "Pending" || r.Status == "Approved") && r.ExpiresAt <= now)),
            "Completed" or "Rejected" or "Cancelled" => query.Where(r => r.Status == Status),
            _ => query
        };
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var value = Search.Trim();
            if (value.Length > 256) return BadRequest();
            var requestId = long.TryParse(value, out var number) ? number : -1;
            query = query.Where(r => r.Id == requestId || r.VerifiedEmail.Contains(value));
        }
        Requests = await query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((P - 1) * size).Take(size + 1).ToListAsync(token);
        HasNext = Requests.Count > size;
        if (HasNext) Requests.RemoveAt(size);
        return Page();
    }

    public async Task<IActionResult> OnPostReviewAsync(long id, bool approve, CancellationToken token)
    {
        var actor = await users.GetUserAsync(User);
        if (actor == null) return Challenge();
        var reason = Input.Reason?.Trim();
        var evidence = Input.Verification?.Trim();
        if (!ModelState.IsValid || string.IsNullOrEmpty(reason) || (approve && string.IsNullOrEmpty(evidence)))
            Result = "Enter a decision reason and record how you confirmed the user's identity before approving.";
        else if (!await verification.CheckCredentialsAsync(actor, Input.Password, Input.Code, requireAuthenticator: true))
            Result = "Administrator password or authenticator verification failed.";
        else Result = await recovery.ReviewAsync(actor, id, approve, reason, evidence, Url, token);
        return RedirectToPage(new { id, status = Status });
    }

    public static string State(AuthenticatorResetRequest item) =>
        item.Status is "Pending" or "Approved" && item.ExpiresAt <= DateTime.UtcNow ? "Expired" : item.Status;
}
