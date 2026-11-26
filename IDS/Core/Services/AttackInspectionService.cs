using System.Data;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.EntityFrameworkCore;

namespace IDS.Core.Services;

public sealed class AttackInspectionService(ApplicationDbContext db, ILogger<AttackInspectionService> logger,
    ExplanationReferenceCatalog? catalog = null)
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
                    (data.Features is not { Count: > 0 } || data.BaseValue == null || data.RawMargin == null)))
                    throw new JsonException("Incomplete explanation payload");
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Invalid stored explanation {ExplanationId}", e.Id);
                status = "failed";
                error = "Stored explanation could not be read.";
                data = null;
            }
            if (data?.Features is { Count: > 0 }) catalog?.Enrich(data);
            return new InspectionStage(e.Id.ToString(), e.Stage, status, e.ModelVersion,
                e.Attempts, error, e.UpdatedAt, data);
        }).ToList();
        var flow = typeof(NetworkFlowBase).GetProperties()
            .ToDictionary(property => property.Name, property => property.GetValue(attack));
        var rawId = explanations.FirstOrDefault(e => e.RawPacketId != null)?.RawPacketId;
        var timeline = await ReadImportTimelineAsync(rawId, cancellationToken);
        return new InspectionReport(attack.Id.ToString(), attack.AttackType, attack.Severity,
            attack.Timestamp, string.IsNullOrWhiteSpace(attack.SrcIp) ? null : attack.SrcIp,
            attack.DstPort, attack.IsAcknowledged, escalated, attack.ConfidenceScore,
            attack.ModelVersion, stages, flow)
        {
            CsvReceivedAt = timeline.Received,
            ClassificationRecordedAt = timeline.Recorded
        };
    }

    private async Task<(DateTimeOffset? Received, DateTimeOffset? Recorded)> ReadImportTimelineAsync(long? rawId,
        CancellationToken token)
    {
        if (rawId == null || !db.Database.IsSqlServer()) return default;
        var connection = db.Database.GetDbConnection();
        var opened = connection.State == ConnectionState.Closed;
        try
        {
            if (opened) await db.Database.OpenConnectionAsync(token);
            await using var command = connection.CreateCommand();
            command.CommandText = """
                IF OBJECT_ID(N'dbo.PipelineImports', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.PipelineRows', N'U') IS NOT NULL
                SELECT i.CreatedAt, p.ProcessedAt
                FROM dbo.PipelineRows p INNER JOIN dbo.PipelineImports i ON p.FileHash = i.FileHash
                WHERE p.RawId = @rawId
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@rawId";
            parameter.DbType = DbType.Int64;
            parameter.Value = rawId.Value;
            command.Parameters.Add(parameter);
            await using var result = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, token);
            if (!await result.ReadAsync(token)) return default;
            return (new DateTimeOffset(result.GetDateTime(0), TimeSpan.Zero),
                result.IsDBNull(1) ? null : new DateTimeOffset(result.GetDateTime(1), TimeSpan.Zero));
        }
        catch (SqlException error) when (error.Number is 208 or 229)
        {
            logger.LogDebug("Pipeline import timestamps are unavailable for inspection.");
            return default;
        }
        finally
        {
            if (opened) await db.Database.CloseConnectionAsync();
        }
    }
}
