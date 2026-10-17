using System.Security.Claims;
using System.Text.Json;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.EntityFrameworkCore;

namespace IDS.Core.Services;

public sealed class AttackInspectionService(ApplicationDbContext db, ILogger<AttackInspectionService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool CanInspect(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true && !user.IsInRole(AppRoles.Suspended) &&
        (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.Support));

    public async Task<InspectionReport?> LoadAsync(long id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        // Defense in depth: inspection contains full network evidence, unlike the summary feed.
        if (!CanInspect(user)) throw new UnauthorizedAccessException();
        if (id <= 0) return null;
        var attack = await db.AttackTraffic.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (attack == null) return null;
        var explanations = await db.FlowExplanations.AsNoTracking().Where(e => e.AttackTrafficId == id)
            .OrderBy(e => e.Stage).ThenByDescending(e => e.Id).ToListAsync(cancellationToken);
        var escalated = await db.SupportTickets.AsNoTracking().AnyAsync(t => t.SourceAttackId == id, cancellationToken);
        var stages = explanations.Select(e =>
        {
            ModelExplanationData? data = null;
            var status = e.Status;
            var error = e.LastError;
            try
            {
                data = JsonSerializer.Deserialize<ModelExplanationData>(
                    status == "completed" ? e.PayloadJson ?? "null" : e.ContextJson, JsonOptions);
                if (data?.SchemaVersion != 1 || (status == "completed" &&
                    (data.Features.Count == 0 || data.BaseValue == null || data.RawMargin == null)))
                    throw new JsonException("Incomplete explanation payload");
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Invalid stored explanation {ExplanationId}", e.Id);
                status = "failed";
                error = "Stored explanation could not be read.";
                data = null;
            }
            return new InspectionStage(e.Id.ToString(), e.Stage, status, e.ModelVersion,
                e.Attempts, error, e.UpdatedAt, data);
        }).ToList();
        var flow = typeof(NetworkFlowBase).GetProperties()
            .ToDictionary(property => property.Name, property => property.GetValue(attack));
        return new InspectionReport(attack.Id.ToString(), attack.AttackType, attack.Severity,
            attack.Timestamp, string.IsNullOrWhiteSpace(attack.SrcIp) ? null : attack.SrcIp,
            attack.DstPort, attack.IsAcknowledged, escalated, attack.ConfidenceScore,
            attack.ModelVersion, stages, flow);
    }
}
