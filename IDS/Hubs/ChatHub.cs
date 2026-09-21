using IDS.Core.Services;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace IDS.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IChatService _chat;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IConfiguration _configuration;

    public ChatHub(IChatService chat, UserManager<ApplicationUser> users, IConfiguration configuration)
    {
        _chat = chat;
        _users = users;
        _configuration = configuration;
    }

    public override async Task OnConnectedAsync()
    {
        if (!_configuration.GetValue<bool>("Chat:Enabled") || Context.UserIdentifier is null)
        {
            Context.Abort();
            return;
        }
        var user = await _users.FindByIdAsync(Context.UserIdentifier);
        if (user is null || await _users.IsInRoleAsync(user, AppRoles.Suspended))
        {
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync();
    }

    public Task<ChatKeyInfo?> GetMyKey() => InvokeAsync(() => _chat.GetKeyAsync(CurrentUserId));

    public Task<ChatKeyInfo> RegisterKey(string publicKey) =>
        InvokeAsync(() => _chat.RegisterKeyAsync(CurrentUserId, publicKey));

    public Task<IReadOnlyList<ChatEmployee>> GetEmployees(string? department) =>
        InvokeAsync(() => _chat.GetEmployeesAsync(CurrentUserId, department));

    public Task<IReadOnlyList<ChatMessageInfo>> GetConversation(string peerId, long? beforeId) =>
        InvokeAsync(() => _chat.GetConversationAsync(CurrentUserId, peerId, beforeId));

    public Task<int> GetUnreadCount() => InvokeAsync(() => _chat.GetUnreadCountAsync(CurrentUserId));

    public Task<ChatMessageInfo> SendMessage(ChatEnvelope envelope) => InvokeAsync(async () =>
    {
        var saved = await _chat.SendAsync(CurrentUserId, envelope);
        // A notification needs only metadata; ciphertext stays in the conversation endpoint.
        await Clients.User(saved.RecipientId).SendAsync("MessageReceived",
            new { saved.Id, saved.SenderId, saved.SentAtUtc });
        return saved;
    });

    public Task<ChatMessageInfo> MarkRead(long messageId) => InvokeAsync(async () =>
    {
        var updated = await _chat.MarkReadAsync(CurrentUserId, messageId);
        await Clients.User(updated.SenderId).SendAsync("MessageRead", updated.Id, updated.ReadAtUtc);
        return updated;
    });

    private string CurrentUserId => Context.UserIdentifier ??
        throw new HubException("Authentication is required.");

    private async Task<T> InvokeAsync<T>(Func<Task<T>> action)
    {
        if (!_configuration.GetValue<bool>("Chat:Enabled"))
            throw new HubException("Chat is currently unavailable.");
        try
        {
            return await action();
        }
        catch (ChatValidationException exception)
        {
            throw new HubException(exception.Message);
        }
    }
}
