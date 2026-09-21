using System.Security.Cryptography;
using IDS.Core.Services;
using IDS.Data;
using IDS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IDS.Tests;

public sealed class ChatServiceTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ChatService _chat;

    public ChatServiceTests()
    {
        _connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _db.Database.EnsureCreated();
        _users = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _chat = new ChatService(_db, _users);
    }

    [Fact]
    public async Task ConversationIsLimitedToParticipantsAndReadReceiptToRecipient()
    {
        await AddUserAsync("sender", "Security Operations");
        await AddUserAsync("recipient", "IT Support");
        await AddUserAsync("outsider", "IT Support");
        var senderKey = await RegisterKeyAsync("sender");
        var recipientKey = await RegisterKeyAsync("recipient");

        var envelope = new ChatEnvelope(
            "recipient", Guid.NewGuid().ToString(),
            Convert.ToBase64String(new byte[32]), Convert.ToBase64String(new byte[32]),
            Convert.ToBase64String(new byte[12]), Convert.ToBase64String(new byte[12]),
            senderKey.Fingerprint, recipientKey.Fingerprint);
        var sent = await _chat.SendAsync("sender", envelope);

        // The database receives only encrypted bytes and message metadata.
        Assert.Equal(envelope.RecipientCiphertext,
            (await _db.Set<ChatMessage>().SingleAsync()).RecipientCiphertext);
        Assert.Single(await _chat.GetConversationAsync("sender", "recipient", null));
        Assert.Single(await _chat.GetConversationAsync("recipient", "sender", null));
        Assert.Empty(await _chat.GetConversationAsync("outsider", "recipient", null));
        await Assert.ThrowsAsync<ChatValidationException>(() => _chat.MarkReadAsync("sender", sent.Id));
        Assert.Equal(1, await _chat.GetUnreadCountAsync("recipient"));

        var read = await _chat.MarkReadAsync("recipient", sent.Id);
        Assert.NotNull(read.ReadAtUtc);
        Assert.Equal(0, await _chat.GetUnreadCountAsync("recipient"));
        Assert.Equal(sent.Id, (await _chat.SendAsync("sender", envelope)).Id);
    }

    [Fact]
    public async Task DirectoryFiltersByDepartmentAndKeyChangesAreRejected()
    {
        await AddUserAsync("security", "Security Operations");
        await AddUserAsync("support", "IT Support");
        await AddUserAsync("general", "General");
        var firstKey = await RegisterKeyAsync("security");

        var colleagues = await _chat.GetEmployeesAsync("general", "Security Operations");
        Assert.Single(colleagues);
        Assert.Equal("security", colleagues[0].Id);
        Assert.Equal(firstKey.Fingerprint, colleagues[0].Key?.Fingerprint);

        using var replacement = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var newPublicKey = Convert.ToBase64String(replacement.ExportSubjectPublicKeyInfo());
        await Assert.ThrowsAsync<ChatValidationException>(() => _chat.RegisterKeyAsync("security", newPublicKey));
    }

    private async Task AddUserAsync(string id, string department)
    {
        var result = await _users.CreateAsync(new ApplicationUser
        {
            Id = id,
            UserName = id,
            Email = id + "@example.test",
            EmailConfirmed = true,
            Department = department
        });
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
    }

    private async Task<ChatKeyInfo> RegisterKeyAsync(string userId)
    {
        using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        return await _chat.RegisterKeyAsync(userId, Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()));
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        _connection.Dispose();
    }
}
