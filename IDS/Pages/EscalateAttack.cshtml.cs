using System.ComponentModel.DataAnnotations;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IDS.Pages;

/// <summary>
/// Turns a severe detection into a security ticket with an accountable responder.
/// The attack ID is checked on the server; users cannot submit arbitrary ticket details.
/// </summary>
[Authorize]
public class EscalateAttackModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EscalateAttackModel> _logger;

    public EscalateAttackModel(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<EscalateAttackModel> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public AttackTraffic? Attack { get; private set; }
    public SupportTicket? ExistingTicket { get; private set; }
    public List<Responder> Responders { get; private set; } = new();

    [BindProperty]
    public EscalationInput Input { get; set; } = new();

    public sealed class EscalationInput
    {
        [Required(ErrorMessage = "Select a responder.")]
        public string ResponderId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Explain why this detection needs review.")]
        [StringLength(2000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;
    }

    public sealed record Responder(string Id, string Name, string Role);

    public async Task<IActionResult> OnGetAsync(long id)
    {
        if (!await LoadAttackAsync(id)) return NotFound();
        await LoadRespondersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!await LoadAttackAsync(id)) return NotFound();
        if (ExistingTicket != null)
            return ExistingTicket.SubmittedByUserId == _userManager.GetUserId(User) ||
                   User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Support)
                ? RedirectToPage("/ViewTicket", new { id = ExistingTicket.Id })
                : RedirectToPage(new { id });

        await LoadRespondersAsync();
        var responder = Responders.FirstOrDefault(r => r.Id == Input.ResponderId);
        if (responder == null)
            ModelState.AddModelError("Input.ResponderId", "Select an active Support or Admin responder.");

        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var now = DateTime.UtcNow;
        var ticket = new SupportTicket
        {
            TicketNumber = TicketConstants.GenerateTicketNumber(),
            SourceAttackId = Attack!.Id,
            SubmittedByUserId = user.Id,
            SubmittedByEmail = user.Email ?? string.Empty,
            SubmittedByName = DisplayName(user),
            AssignedToUserId = responder!.Id,
            AssignedToName = responder.Name,
            Subject = $"{Attack.Severity} {Attack.AttackType} detection",
            Description = $"Detected {Attack.Timestamp:u} from {Attack.SrcIp} to port {Attack.DstPort}.\n\nEscalation reason: {Input.Reason.Trim()}",
            Category = "Security Concern",
            Priority = Attack.Severity,
            Status = "InProgress",
            CreatedAt = now,
            UpdatedAt = now
        };

        // The ticket, acknowledgement and audit entry commit together. A unique
        // SourceAttackId index also guards against a second submission in another tab.
        await using var transaction = await _db.Database.BeginTransactionAsync();
        if (await _db.SupportTickets.AnyAsync(t => t.SourceAttackId == id))
        {
            await transaction.RollbackAsync();
            return RedirectToPage(new { id });
        }

        Attack.IsAcknowledged = true;
        Attack.AcknowledgedBy = user.Id;
        Attack.AcknowledgedAt = now;
        _db.SupportTickets.Add(ticket);
        _db.AuditLogs.Add(new AuditLog
        {
            Timestamp = now,
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            Action = "EscalateAttack",
            EntityType = "AttackTraffic",
            EntityId = id.ToString(),
            Details = $"Escalated to {responder.Name} ({responder.Role}) as {ticket.TicketNumber}."
        });

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            var existingId = await _db.SupportTickets.AsNoTracking()
                .Where(t => t.SourceAttackId == id)
                .Select(t => t.Id)
                .SingleOrDefaultAsync();
            if (existingId == 0) throw;
            _logger.LogInformation(ex, "Detection {AttackId} was already escalated", id);
            return RedirectToPage(new { id });
        }

        _logger.LogInformation("Detection {AttackId} escalated as {TicketNumber} by {UserId} to {ResponderId}",
            id, ticket.TicketNumber, user.Id, responder.Id);
        return RedirectToPage("/ViewTicket", new { id = ticket.Id });
    }

    private async Task<bool> LoadAttackAsync(long id)
    {
        Attack = await _db.AttackTraffic.SingleOrDefaultAsync(a => a.Id == id &&
            (a.Severity == "High" || a.Severity == "Critical"));
        if (Attack == null) return false;
        ExistingTicket = await _db.SupportTickets.AsNoTracking()
            .SingleOrDefaultAsync(t => t.SourceAttackId == id);
        return true;
    }

    private async Task LoadRespondersAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        var support = await _userManager.GetUsersInRoleAsync(AppRoles.Support);
        Responders = admins.Select(u => new Responder(u.Id, DisplayName(u), "Admin"))
            .Concat(support.Select(u => new Responder(u.Id, DisplayName(u), "Support")))
            .DistinctBy(r => r.Id)
            .OrderBy(r => r.Name)
            .ToList();
    }

    private static string DisplayName(ApplicationUser user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? user.Email ?? "Responder" : name;
    }
}
