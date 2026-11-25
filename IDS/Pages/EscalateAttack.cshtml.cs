using System.ComponentModel.DataAnnotations;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
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
    public bool CanViewExistingTicket => ExistingTicket != null && IncidentWorkflow.CanView(ExistingTicket, User);
    public List<IncidentWorkflow.Responder> Responders { get; private set; } = new();

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

    public async Task<IActionResult> OnGetAsync(long id)
    {
        if (!await OperationsSettings.IsEscalationEnabledAsync(_db)) return Forbid();
        if (!await LoadAttackAsync(id)) return NotFound();
        await LoadRespondersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!await OperationsSettings.IsEscalationEnabledAsync(_db)) return Forbid();
        if (!await LoadAttackAsync(id)) return NotFound();
        if (ExistingTicket != null)
            return CanViewExistingTicket
                ? RedirectToPage("/ViewTicket", new { id = ExistingTicket.Id })
                : RedirectToPage(new { id });

        await LoadRespondersAsync();
        var responder = Responders.FirstOrDefault(r => r.Id == Input.ResponderId);
        if (responder == null)
            ModelState.AddModelError("Input.ResponderId", "Select an active Support or Admin responder.");

        if (string.IsNullOrWhiteSpace(Input.Reason) || Input.Reason.Trim().Length is < 10 or > 2000)
            ModelState.AddModelError("Input.Reason", "Enter an escalation reason between 10 and 2,000 characters.");
        if (!ModelState.IsValid || responder == null) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var now = DateTime.UtcNow;
        var ticket = new SupportTicket
        {
            TicketNumber = TicketConstants.GenerateTicketNumber(),
            SourceAttackId = Attack!.Id,
            SubmittedByUserId = user.Id,
            SubmittedByEmail = user.Email ?? string.Empty,
            SubmittedByName = IncidentWorkflow.DisplayName(user),
            Subject = $"{Attack.Severity} {Attack.AttackType} detection",
            Description = $"Detected {Attack.Timestamp:u} from {Attack.SrcIp} to port {Attack.DstPort?.ToString() ?? "Unavailable"}.\n\nEscalation reason: {Input.Reason.Trim()}",
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
        IncidentWorkflow.Escalate(_db, ticket, user, responder, Input.Reason.Trim(), createdTicket: true);

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

    private async Task LoadRespondersAsync() =>
        Responders = await IncidentWorkflow.RespondersAsync(_userManager, _userManager.GetUserId(User));
}
