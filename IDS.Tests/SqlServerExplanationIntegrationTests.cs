using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using IDS.Data.Migrations;
using IDS.Pages;
using IDS.Hubs;
using IDS.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace IDS.Tests;

public sealed class SqlVerificationFactAttribute : FactAttribute
{
    public SqlVerificationFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("IDS_SHAP_SQL_TEST_CONNECTION")))
            Skip = "Opt-in: run pipeline/tests/run_sql_verification.py against a dedicated test database.";
    }
}

public sealed class SqlServerExplanationIntegrationTests(ITestOutputHelper output)
{
    [SqlVerificationFact]
    public async Task RealModelsThroughSqlServerApiRazorAndOptionalBrowser()
    {
        var connection = Environment.GetEnvironmentVariable("IDS_SHAP_SQL_TEST_CONNECTION")!;
        var sql = new SqlConnectionStringBuilder(connection);
        Assert.StartsWith("IDS_Shap_Test_", sql.InitialCatalog);
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "IDS.sln"))) root = root.Parent;
        Assert.NotNull(root);
        var pipeline = Path.Combine(root!.FullName, "pipeline");
        var python = Environment.GetEnvironmentVariable("IDS_SHAP_PYTHON")!;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection).Options;
        await using (var baseline = new InspectionBaselineContext(options))
        {
            Assert.False(await baseline.Database.CanConnectAsync(), "Integration database must be NEW and empty.");
            // A pre-existing duplicate Roles migration prevents blank-database bootstrap.
            // Start with the historical flow schema, seed a legacy record, then apply the two real migrations.
            await baseline.Database.EnsureCreatedAsync();
            var legacy = new AttackTraffic { AttackType = "Attack", ModelVersion = "legacy-test" };
            foreach (var property in typeof(NetworkFlowBase).GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.PropertyType);
                if (type != null && type != typeof(bool))
                    property.SetValue(legacy, Activator.CreateInstance(type));
            }
            baseline.Add(legacy);
            await baseline.SaveChangesAsync();
        }
        await using (var database = new ApplicationDbContext(options))
        {
            var model = database.GetService<IDesignTimeModel>().Model;
            foreach (var migration in new Migration[] { new AddFlowExplanations(), new SupportNullableFlowInputs() })
            {
                var commands = database.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, model);
                foreach (var command in commands) await database.Database.ExecuteSqlRawAsync(command.CommandText);
            }
            var legacy = await database.AttackTraffic.SingleAsync();
            Assert.Equal("Attack", legacy.AttackType);
            Assert.Null(legacy.DeltaStartIncompleteHandshake);
            Assert.Equal(0d, legacy.DeltaStart);
            // UI fixtures cover both new labels even when synthetic traffic predicts other classes.
            database.AddRange(new AttackTraffic { AttackType = "PortScan", Severity = "Medium" },
                new AttackTraffic { AttackType = "DoS_SYN_Flood", Severity = "High" });
            await database.SaveChangesAsync();
        }
        var fixture = new ProcessStartInfo(python) { WorkingDirectory = pipeline, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false };
        fixture.ArgumentList.Add("tests/sql_explanation_fixture.py");
        var fixtureResult = await RunProcess(fixture);
        output.WriteLine(fixtureResult);
        var trace = JsonDocument.Parse(fixtureResult.Trim());
        var attackId = trace.RootElement.GetProperty("attackId").GetString();
        await using (var database = new ApplicationDbContext(options))
        {
            var records = await database.FlowExplanations.Where(e => e.AttackTrafficId == long.Parse(attackId!)).ToListAsync();
            Assert.Equal(2, records.Count);
            Assert.All(records, record => Assert.Equal("completed", record.Status));
            var raw = await database.RawPackets.SingleAsync(r => r.Id == records[0].RawPacketId);
            Assert.Equal(long.Parse(attackId!), raw.ClassifiedRecordId);
            Assert.Equal("Attack", raw.Classification);
            Assert.True(raw.DeltaStartIncompleteHandshake);
            Assert.Null(raw.DeltaStart);
            var existing = records[0];
            database.FlowExplanations.Add(new FlowExplanation { AttackTrafficId = existing.AttackTrafficId,
                RawPacketId = existing.RawPacketId, Stage = existing.Stage, ModelVersion = existing.ModelVersion });
            await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
            database.ChangeTracker.Clear();
            database.FlowExplanations.Add(new FlowExplanation { AttackTrafficId = long.MaxValue,
                Stage = "gatekeeper", ModelVersion = "fk-test" });
            await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        }
        // Test-only HTTP host uses the real controllers, Razor pages, EF queries and
        // role authorization middleware. Header authentication exists only in this test assembly.
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
            ApplicationName = typeof(InspectAttackModel).Assembly.GetName().Name,
            ContentRootPath = Path.Combine(root.FullName, "IDS"), EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddDbContext<ApplicationDbContext>(b => b.UseSqlServer(connection));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        builder.Services.AddAuthentication(authentication => {
            authentication.DefaultAuthenticateScheme = "InspectionFixture";
            authentication.DefaultChallengeScheme = "InspectionFixture";
            authentication.DefaultForbidScheme = "InspectionFixture";
        }).AddScheme<AuthenticationSchemeOptions, InspectionFixtureAuthentication>("InspectionFixture", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddScoped<ILiveDetectionService, LiveDetectionService>();
        builder.Services.AddSingleton<DashboardNotificationService>();
        builder.Services.AddScoped<AttackInspectionService>();
        builder.Services.AddScoped<SecurityPolicyService>();
        await using var app = builder.Build();
        app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
        app.UseMiddleware<PrivateResponseCacheMiddleware>();
        app.MapControllers(); app.MapRazorPages(); app.MapHub<DashboardHub>("/hubs/dashboard");
        await app.StartAsync();
        var baseUrl = app.Urls.Single();
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var apiPath = $"/api/detection/{attackId}/inspection";
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync(apiPath)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync($"/InspectAttack/{attackId}")).StatusCode);
        http.DefaultRequestHeaders.Add("X-Inspection-Test-Role", AppRoles.Employee);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync(apiPath)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync($"/InspectAttack/{attackId}")).StatusCode);
        http.DefaultRequestHeaders.Remove("X-Inspection-Test-Role");
        http.DefaultRequestHeaders.Add("X-Inspection-Test-Role", AppRoles.Support);
        var response = await http.GetAsync(apiPath);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl!.NoStore);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(attackId, json.RootElement.GetProperty("id").GetString());
        Assert.Equal(2, json.RootElement.GetProperty("stages").GetArrayLength());
        var page = await http.GetAsync($"/InspectAttack/{attackId}");
        var html = await page.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("inspectionData", html); Assert.Contains("shapWaterfall", html);
        Assert.Contains($"/EscalateAttack/{attackId}", html);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("/api/detection/9223372036854775807/inspection")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.GetAsync("/api/detection/0/inspection")).StatusCode);
        http.DefaultRequestHeaders.Remove("X-Inspection-Test-Role");
        http.DefaultRequestHeaders.Add("X-Inspection-Test-Role", AppRoles.Admin);
        var stats = JsonDocument.Parse(await http.GetStringAsync("/api/detection/stats/by-type"));
        var dashboard = await http.GetStringAsync("/LiveDashboard");
        foreach (var label in new[] { "PortScan", "DoS_SYN_Flood", "Attack" })
        {
            Assert.True(stats.RootElement.GetProperty(label).GetInt32() >= 1);
            Assert.Contains($"value=\"label:{label}\"", dashboard);
            Assert.Contains($">{label}</span>", dashboard);
        }
        var data = json.RootElement.GetProperty("stages")[1].GetProperty("data");
        Assert.Equal(trace.RootElement.GetProperty("selectedClass").GetString(), data.GetProperty("explainedClass").GetString());
        var features = data.GetProperty("features").EnumerateArray().ToDictionary(f => f.GetProperty("name").GetString()!);
        Assert.Equal(JsonValueKind.Null, features["delta_start"].GetProperty("value").ValueKind);
        Assert.Equal(1, features["delta_start_incomplete_handshake"].GetProperty("value").GetDouble());
        using (var scope = app.Services.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<ILiveDetectionService>();
            var detection = (await service.GetRecentEventsAsync(200)).First(e => e.IsAttack && e.Id.ToString() == attackId);
            await VerifySignalR(baseUrl, app.Services.GetRequiredService<DashboardNotificationService>(), detection);
        }
        if (Environment.GetEnvironmentVariable("IDS_SHAP_BROWSER") == "1")
        {
            var browser = new ProcessStartInfo(python) { WorkingDirectory = pipeline, RedirectStandardOutput = true,
                RedirectStandardError = true, UseShellExecute = false };
            browser.ArgumentList.Add("tests/verify_inspection_browser.py");
            browser.ArgumentList.Add(baseUrl); browser.ArgumentList.Add(attackId!);
            output.WriteLine(await RunProcess(browser));
        }
        else output.WriteLine("Browser rendering was NOT requested.");
        var evidenceDirectory = Path.Combine(root.FullName, "IDS.Tests", "TestResults");
        Directory.CreateDirectory(evidenceDirectory);
        await File.WriteAllTextAsync(Path.Combine(evidenceDirectory, "shap-verification.json"),
            JsonSerializer.Serialize(new { database = sql.InitialCatalog, modelTrace = trace.RootElement,
                httpApiVerified = true, razorReportVerified = true,
                browserVerified = Environment.GetEnvironmentVariable("IDS_SHAP_BROWSER") == "1",
                verifiedAt = DateTimeOffset.UtcNow }, new JsonSerializerOptions { WriteIndented = true }));
        output.WriteLine($"Verified SQL → HTTP API → Razor report for attack {attackId}. Database retained: {sql.InitialCatalog}");
        await app.StopAsync();
    }

    private static async Task VerifySignalR(string baseUrl, DashboardNotificationService service, LiveDetectionEvent detection)
    {
        using var client = new ClientWebSocket();
        client.Options.SetRequestHeader("X-Inspection-Test-Role", AppRoles.Admin);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var token = timeout.Token;
        await client.ConnectAsync(new UriBuilder(baseUrl) { Scheme = "ws", Path = "/hubs/dashboard" }.Uri, token);
        async Task Send(string message) => await client.SendAsync(Encoding.UTF8.GetBytes(message + '\u001e'),
            WebSocketMessageType.Text, true, token);
        async Task<JsonElement> Receive()
        {
            using var buffer = new MemoryStream();
            var chunk = new byte[4096];
            WebSocketReceiveResult result;
            do
            {
                result = await client.ReceiveAsync(new ArraySegment<byte>(chunk), token);
                buffer.Write(chunk, 0, result.Count);
            } while (!result.EndOfMessage);
            return JsonDocument.Parse(Encoding.UTF8.GetString(buffer.ToArray()).TrimEnd('\u001e')).RootElement.Clone();
        }
        await Send("""{"protocol":"json","version":1}""");
        Assert.Equal(JsonValueKind.Object, (await Receive()).ValueKind);
        await Send("""{"type":1,"target":"RequestLatestStats","arguments":[]}""");
        Assert.Equal("StatsRequested", (await Receive()).GetProperty("target").GetString());
        await service.BroadcastDetectionEventAsync(detection);
        var message = await Receive();
        Assert.Equal("NewDetection", message.GetProperty("target").GetString());
        Assert.Equal(detection.AttackType, message.GetProperty("arguments")[0].GetProperty("attackType").GetString());
    }

    private static async Task<string> RunProcess(ProcessStartInfo start)
    {
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(true); throw; }
        Assert.True(process.ExitCode == 0, $"Fixture failed: {await stderr}\n{await stdout}");
        return await stdout;
    }
}

public sealed class InspectionBaselineContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<FlowExplanation>();
        foreach (var type in new[] { typeof(RawPacket), typeof(AttackTraffic), typeof(BenignTraffic) })
        {
            var entity = builder.Entity(type);
            entity.Ignore(nameof(NetworkFlowBase.DeltaStartIncompleteHandshake));
            foreach (var property in typeof(NetworkFlowBase).GetProperties())
            {
                var underlying = Nullable.GetUnderlyingType(property.PropertyType);
                if (underlying != null && underlying != typeof(bool)) entity.Property(property.Name).IsRequired();
            }
        }
    }
}

/// <summary>Never registered by the production application.</summary>
public sealed class InspectionFixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers["X-Inspection-Test-Role"].ToString();
        if (string.IsNullOrEmpty(role)) return Task.FromResult(AuthenticateResult.NoResult());
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, "integration-fixture") }, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
