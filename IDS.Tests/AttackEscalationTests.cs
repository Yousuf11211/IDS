using System.Security.Claims;
using IDS.Data;
using IDS.Data.Models;
using IDS.Pages;
using IDS.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IDS.Tests;

public sealed class AttackEscalationTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly ServiceProvider _services;

    public AttackEscalationTests()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection).Options;
        _db = new ApplicationDbContext(options);
        _db.Database.EnsureCreated();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(builder => builder.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        _services = services.BuildServiceProvider();
    }

    [Fact]
    public async Task EmployeeCanEscalateSevereAttackOnlyOnceToSupport()
    {
        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = _services.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True((await roles.CreateAsync(new IdentityRole(AppRoles.Support))).Succeeded);

        var employee = new ApplicationUser { UserName = "employee@example.test", Email = "employee@example.test", FirstName = "Alex" };
        var responder = new ApplicationUser { UserName = "responder@example.test", Email = "responder@example.test", FirstName = "Morgan", EmailConfirmed = true };
        Assert.True((await users.CreateAsync(employee)).Succeeded);
        Assert.True((await users.CreateAsync(responder)).Succeeded);
        Assert.True((await users.AddToRoleAsync(responder, AppRoles.Support)).Succeeded);

        var attack = new AttackTraffic { Severity = "Critical", AttackType = "DDoS", SrcIp = "192.0.2.1", DstPort = 443 };
        _db.AttackTraffic.Add(attack);
        await _db.SaveChangesAsync();

        var page = CreatePage(employee);
        page.Input = new EscalateAttackModel.EscalationInput
        {
            ResponderId = responder.Id,
            Reason = "Traffic volume needs immediate investigation."
        };
        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync(attack.Id));

        var ticket = await _db.SupportTickets.SingleAsync();
        Assert.Equal(attack.Id, ticket.SourceAttackId);
        Assert.Equal("Critical", ticket.Priority);
        Assert.Equal(responder.Id, ticket.AssignedToUserId);
        Assert.Equal("InProgress", ticket.Status);
        Assert.True((await _db.AttackTraffic.SingleAsync()).IsAcknowledged);
        Assert.Single(await _db.AuditLogs.ToListAsync());
        Assert.Single(await _db.IncidentEscalations.ToListAsync());
        var notification = await _db.InboxNotifications.SingleAsync();
        Assert.Equal(responder.Id, notification.RecipientId);
        Assert.Equal(ticket.Id, notification.TicketId);
        Assert.Null(notification.ReadAt);

        var inbox = new InboxModel(_db) { PageContext = ContextFor(responder, AppRoles.Support) };
        var view = await inbox.OnPostOpenAsync(notification.Id);
        Assert.Equal("/ViewTicket", Assert.IsType<RedirectToPageResult>(view).PageName);
        Assert.NotNull(notification.ReadAt);
        var anotherInbox = new InboxModel(_db) { PageContext = ContextFor(employee) };
        Assert.IsType<NotFoundResult>(await anotherInbox.OnPostOpenAsync(notification.Id));

        var ticketPage = new ViewTicketModel(_db, users, NullLogger<ViewTicketModel>.Instance)
        {
            PageContext = ContextFor(employee), CancellationReason = "   "
        };
        await ticketPage.OnPostCancelEscalationAsync(ticket.Id);
        Assert.Equal("InProgress", ticket.Status);
        ticketPage.CancellationReason = "Duplicate incident report";
        await ticketPage.OnPostCancelEscalationAsync(ticket.Id);
        Assert.Equal("Cancelled", ticket.Status);
        var escalation = await _db.IncidentEscalations.SingleAsync();
        Assert.Equal(employee.Id, escalation.CancelledById);
        Assert.NotNull(escalation.CancelledAt);
        Assert.Equal("Duplicate incident report", escalation.CancellationReason);
        var cancellation = await _db.InboxNotifications.SingleAsync(n => n.Type == "Escalation cancelled");
        Assert.Equal(responder.Id, cancellation.RecipientId);
        Assert.Contains("Duplicate incident report", cancellation.Message);

        // A second submission returns the existing incident instead of creating another.
        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync(attack.Id));
        Assert.Equal(1, await _db.SupportTickets.CountAsync());
    }

    [Fact]
    public async Task LowSeverityAndNonResponderAssignmentsAreRejected()
    {
        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var employee = new ApplicationUser { UserName = "employee@example.test", Email = "employee@example.test" };
        Assert.True((await users.CreateAsync(employee)).Succeeded);
        var high = new AttackTraffic { Severity = "High", AttackType = "Probe" };
        var low = new AttackTraffic { Severity = "Low", AttackType = "Probe" };
        _db.AttackTraffic.AddRange(high, low);
        await _db.SaveChangesAsync();

        var page = CreatePage(employee);
        page.Input = new EscalateAttackModel.EscalationInput
        {
            ResponderId = employee.Id,
            Reason = "Please review this suspicious network activity."
        };
        Assert.IsType<NotFoundResult>(await page.OnPostAsync(low.Id));
        Assert.IsType<PageResult>(await page.OnPostAsync(high.Id));
        Assert.Equal(0, await _db.SupportTickets.CountAsync());
    }

    [Fact]
    public async Task ResponderWorkBlocksCancellationAndUnauthorizedUsersCannotOpenIncident()
    {
        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = _services.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole(AppRoles.Support));
        var employee = new ApplicationUser { UserName = "initiator" };
        var outsider = new ApplicationUser { UserName = "outsider" };
        var responder = new ApplicationUser { UserName = "responder", EmailConfirmed = true };
        foreach (var account in new[] { employee, outsider, responder }) Assert.True((await users.CreateAsync(account)).Succeeded);
        Assert.True((await users.AddToRoleAsync(responder, AppRoles.Support)).Succeeded);

        var ticket = new SupportTicket
        {
            TicketNumber = "TKT-TEST", SubmittedByUserId = employee.Id, Category = "Security Concern"
        };
        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();

        var origin = new ViewTicketModel(_db, users, NullLogger<ViewTicketModel>.Instance)
        {
            PageContext = ContextFor(employee), ResponderId = responder.Id,
            EscalationReason = "Review failed login activity.", CancellationReason = "Withdraw request."
        };
        await origin.OnPostEscalateAsync(ticket.Id);
        var response = new ViewTicketModel(_db, users, NullLogger<ViewTicketModel>.Instance)
        {
            PageContext = ContextFor(responder, AppRoles.Support), NewComment = "Reviewed the network evidence."
        };
        await response.OnPostAddCommentAsync(ticket.Id);
        await origin.OnPostCancelEscalationAsync(ticket.Id);

        Assert.Equal("InProgress", ticket.Status);
        Assert.NotNull(await _db.TicketComments.SingleAsync());
        Assert.Null((await _db.IncidentEscalations.SingleAsync()).CancelledAt);
        var notification = await _db.InboxNotifications.SingleAsync();
        // The recipient still owns the message after a role change, but loses incident access.
        var inbox = new InboxModel(_db) { PageContext = ContextFor(responder) };
        Assert.IsType<ForbidResult>(await inbox.OnPostOpenAsync(notification.Id));
        var unauthorized = new ViewTicketModel(_db, users, NullLogger<ViewTicketModel>.Instance)
        {
            PageContext = ContextFor(outsider)
        };
        Assert.IsType<ForbidResult>(await unauthorized.OnGetAsync(ticket.Id));
    }

    private EscalateAttackModel CreatePage(ApplicationUser user) =>
        new(_db, _services.GetRequiredService<UserManager<ApplicationUser>>(), NullLogger<EscalateAttackModel>.Instance)
        {
            PageContext = ContextFor(user)
        };

    private static PageContext ContextFor(ApplicationUser user, string role = AppRoles.Employee) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Role, role)], "Test"))
        }
    };

    public void Dispose()
    {
        _services.Dispose();
        _db.Dispose();
        _connection.Dispose();
    }
}
