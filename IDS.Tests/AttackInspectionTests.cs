using System.Security.Claims;
using System.Text.Json;
using IDS.Api;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Pages;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IDS.Tests;

public sealed class AttackInspectionTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly ApplicationDbContext db;
    private readonly AttackInspectionService service;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AttackInspectionTests()
    {
        connection.Open();
        db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        service = new AttackInspectionService(db, NullLogger<AttackInspectionService>.Instance);
    }

    public static ClaimsPrincipal Principal(string? role) => role == null ? new ClaimsPrincipal(new ClaimsIdentity()) :
        new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "test-user"), new Claim(ClaimTypes.Role, role) }, "Test"));

    [Theory]
    [InlineData(null)]
    [InlineData(AppRoles.Employee)]
    [InlineData(AppRoles.Suspended)]
    public async Task UnauthorizedPrincipalsCannotReadPredictableIncidentIds(string? role)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoadAsync(1, Principal(role)));
        var controller = new AttackInspectionController(service)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = Principal(role) } } };
        Assert.IsType<ForbidResult>(await controller.Get(1, default));
        Assert.Equal(AppRoles.AdminOrSupport, typeof(AttackInspectionController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal(AppRoles.AdminOrSupport, typeof(InspectAttackModel).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
    }

    [Fact]
    public async Task CorrectIncidentScoresExactValuesAndMissingReferencesReachApiAndPage()
    {
        var first = new AttackTraffic { AttackType = "Bot", Severity = "High", ConfidenceScore = .72, DstPort = 443 };
        var second = new AttackTraffic { AttackType = "DDoS", Severity = "Critical", ConfidenceScore = .83, DstPort = 80 };
        var raw = new RawPacket { DstPort = 443 };
        db.AddRange(first, second, raw);
        await db.SaveChangesAsync();
        foreach (var stage in new[] { "gatekeeper", "multiclass" })
        {
            var payload = new ModelExplanationData
            {
                SchemaVersion = 1, Probability = stage == "gatekeeper" ? .91 : .72,
                ExplainedClass = stage == "gatekeeper" ? "Attack" : "Bot", ClassIndex = stage == "gatekeeper" ? 1 : 3,
                BaseValue = .5, RawMargin = .75, ReconstructedMargin = .75, OutputUnits = "raw class margin",
                Features = new() { new("packets_rate", 30.125, .25, "model input units", null) },
                TopProbabilities = stage == "multiclass" ? new() { new("Bot", .72), new("DDoS", .20), new("PortScan", .08) } : new()
            };
            db.FlowExplanations.Add(new FlowExplanation { AttackTrafficId = first.Id, RawPacketId = raw.Id,
                Stage = stage, ModelVersion = stage + "-fixture", Fingerprint = "fixture", Status = "completed",
                ContextJson = JsonSerializer.Serialize(payload, JsonOptions), PayloadJson = JsonSerializer.Serialize(payload, JsonOptions) });
        }
        await db.SaveChangesAsync();
        var support = Principal(AppRoles.Support);
        var report = (await service.LoadAsync(first.Id, support))!;
        Assert.Equal("Bot", report.AttackType);
        Assert.Equal(.72, report.CategoryProbability);
        Assert.Equal(.91, report.Stages.Single(s => s.Stage == "gatekeeper").Data!.Probability);
        Assert.Equal(30.125, report.Stages[0].Data!.Features[0].Value);
        Assert.Null(report.Stages[0].Data!.Features[0].Reference);
        Assert.Equal(443, report.StoredFlow["DstPort"]);
        var controller = new AttackInspectionController(service)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = support } } };
        Assert.IsType<OkObjectResult>(await controller.Get(first.Id, default));
        var page = new InspectAttackModel(service)
        { PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = support } } };
        Assert.IsType<PageResult>(await page.OnGetAsync(first.Id, default));
        Assert.Equal(first.Id.ToString(), page.Report.Id);
        var other = (await service.LoadAsync(second.Id, support))!;
        Assert.Equal("DDoS", other.AttackType);
        Assert.Empty(other.Stages);
    }

    [Fact]
    public async Task MissingPendingFailedAndMalformedPayloadsPreserveDetection()
    {
        var attack = new AttackTraffic { AttackType = "Bot" };
        db.Add(attack); await db.SaveChangesAsync();
        Assert.Empty((await service.LoadAsync(attack.Id, Principal(AppRoles.Admin)))!.Stages);
        var explanation = new FlowExplanation { AttackTrafficId = attack.Id, Stage = "gatekeeper", ModelVersion = "v1",
            Status = "pending", ContextJson = "{\"schemaVersion\":1,\"probability\":0.9}" };
        db.Add(explanation); await db.SaveChangesAsync();
        Assert.Equal("pending", (await service.LoadAsync(attack.Id, Principal(AppRoles.Admin)))!.Stages[0].Status);
        explanation.Status = "failed"; explanation.LastError = "Unsupported artifact";
        await db.SaveChangesAsync();
        Assert.Equal("Unsupported artifact", (await service.LoadAsync(attack.Id, Principal(AppRoles.Admin)))!.Stages[0].Error);
        explanation.Status = "completed"; explanation.PayloadJson = "invalid-json";
        await db.SaveChangesAsync();
        Assert.Equal("failed", (await service.LoadAsync(attack.Id, Principal(AppRoles.Admin)))!.Stages[0].Status);
        Assert.Equal(1, await db.AttackTraffic.CountAsync());
        Assert.Null(await service.LoadAsync(0, Principal(AppRoles.Admin)));
        Assert.Null(await service.LoadAsync(999, Principal(AppRoles.Admin)));
    }

    [Fact]
    public async Task DatabaseEnforcesUniqueStagesForeignKeysAndStatus()
    {
        var attack = new AttackTraffic(); db.Add(attack); await db.SaveChangesAsync();
        var entry = new FlowExplanation { AttackTrafficId = attack.Id, Stage = "gatekeeper", ModelVersion = "v1" };
        db.Add(entry); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.Add(new FlowExplanation { AttackTrafficId = attack.Id, Stage = "gatekeeper", ModelVersion = "v1" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        db.Add(new FlowExplanation { AttackTrafficId = 99999, Stage = "multiclass", ModelVersion = "v1" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync()); db.ChangeTracker.Clear();
        db.Add(new FlowExplanation { AttackTrafficId = attack.Id, Stage = "multiclass", ModelVersion = "v1", Status = "unknown" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    public void Dispose() { db.Dispose(); connection.Dispose(); }
}
