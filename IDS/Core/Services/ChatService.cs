using System.Security.Cryptography;
using IDS.Data;
using IDS.Data.Models;
using IDS.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IDS.Core.Services;

public sealed record ChatKeyInfo(string UserId, string SubjectPublicKeyInfo, string Fingerprint);
public sealed record ChatEmployee(string Id, string Name, string Department, ChatKeyInfo? Key);
public sealed record ChatEnvelope(
    string RecipientId, string ClientMessageId, string SenderCiphertext,
    string RecipientCiphertext, string SenderNonce, string RecipientNonce,
    string SenderKeyFingerprint, string RecipientKeyFingerprint);
public sealed record ChatMessageInfo(
    long Id, string SenderId, string RecipientId, string ClientMessageId,
    string SenderCiphertext, string RecipientCiphertext, string SenderNonce,
    string RecipientNonce, string SenderKeyFingerprint, string RecipientKeyFingerprint,
    DateTime SentAtUtc, DateTime? ReadAtUtc);

public interface IChatService
{
    Task<ChatKeyInfo?> GetKeyAsync(string userId, CancellationToken cancellationToken = default);
    Task<ChatKeyInfo> RegisterKeyAsync(string userId, string publicKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatEmployee>> GetEmployeesAsync(string userId, string? department, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessageInfo>> GetConversationAsync(string userId, string peerId, long? beforeId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task<ChatMessageInfo> SendAsync(string senderId, ChatEnvelope envelope, CancellationToken cancellationToken = default);
    Task<ChatMessageInfo> MarkReadAsync(string recipientId, long messageId, CancellationToken cancellationToken = default);
}

public sealed class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ChatService(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<ChatKeyInfo?> GetKeyAsync(string userId, CancellationToken cancellationToken = default)
    {
        var key = await _db.Set<ChatPublicKey>().AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        return key is null ? null : ToKeyInfo(key);
    }

    public async Task<ChatKeyInfo> RegisterKeyAsync(string userId, string publicKey, CancellationToken cancellationToken = default)
    {
        await RequireActiveUserAsync(userId);
        if (string.IsNullOrWhiteSpace(publicKey) || publicKey.Length > 512)
            throw new ChatValidationException("The public key is invalid.");

        byte[] encoded;
        try
        {
            encoded = Convert.FromBase64String(publicKey);
            using var key = ECDiffieHellman.Create();
            key.ImportSubjectPublicKeyInfo(encoded, out var bytesRead);
            if (bytesRead != encoded.Length || key.KeySize != 256 ||
                key.ExportParameters(false).Curve.Oid.Value != "1.2.840.10045.3.1.7")
                throw new CryptographicException("Expected a P-256 ECDH key.");
        }
        catch (FormatException)
        {
            throw new ChatValidationException("The public key is invalid.");
        }
        catch (CryptographicException)
        {
            throw new ChatValidationException("The public key is invalid.");
        }

        var canonical = Convert.ToBase64String(encoded);
        var existing = await _db.Set<ChatPublicKey>().SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (existing is not null)
        {
            if (existing.SubjectPublicKeyInfo != canonical)
                throw new ChatValidationException("This account already has a chat key. Key replacement requires a reviewed recovery process.");
            return ToKeyInfo(existing);
        }

        var registered = new ChatPublicKey
        {
            UserId = userId,
            SubjectPublicKeyInfo = canonical,
            Fingerprint = Convert.ToHexString(SHA256.HashData(encoded)),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Set<ChatPublicKey>().Add(registered);
        await _db.SaveChangesAsync(cancellationToken);
        return ToKeyInfo(registered);
    }

    public async Task<IReadOnlyList<ChatEmployee>> GetEmployeesAsync(
        string userId, string? department, CancellationToken cancellationToken = default)
    {
        await RequireActiveUserAsync(userId);
        if (department?.Length > 80)
            throw new ChatValidationException("The department filter is too long.");

        var suspended = await _users.GetUsersInRoleAsync(AppRoles.Suspended);
        var suspendedIds = suspended.Select(user => user.Id).ToHashSet();
        var query = _db.Users.AsNoTracking().Where(user => user.Id != userId && user.EmailConfirmed);
        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(user => user.Department == department);

        var employees = await query.OrderBy(user => user.Department)
            .ThenBy(user => user.FirstName).ThenBy(user => user.LastName)
            .Take(300).ToListAsync(cancellationToken);
        var eligible = employees.Where(user => !suspendedIds.Contains(user.Id)).ToList();
        var ids = eligible.Select(user => user.Id).ToList();
        var keys = await _db.Set<ChatPublicKey>().AsNoTracking()
            .Where(key => ids.Contains(key.UserId)).ToDictionaryAsync(key => key.UserId, cancellationToken);

        return eligible.Select(user => new ChatEmployee(
            user.Id,
            string.Join(' ', new[] { user.FirstName, user.LastName }.Where(part => !string.IsNullOrWhiteSpace(part)))
                is { Length: > 0 } name ? name : user.Email ?? "Employee",
            user.Department,
            keys.TryGetValue(user.Id, out var key) ? ToKeyInfo(key) : null)).ToList();
    }

    public async Task<IReadOnlyList<ChatMessageInfo>> GetConversationAsync(
        string userId, string peerId, long? beforeId, CancellationToken cancellationToken = default)
    {
        await RequireActiveUserAsync(userId);
        if (string.IsNullOrWhiteSpace(peerId) || peerId == userId)
            throw new ChatValidationException("Select another employee.");
        if (beforeId is <= 0)
            throw new ChatValidationException("The message cursor is invalid.");

        // Both sides must match the requested conversation. A user cannot request another pair's history.
        var query = _db.Set<ChatMessage>().AsNoTracking().Where(message =>
            (message.SenderId == userId && message.RecipientId == peerId) ||
            (message.SenderId == peerId && message.RecipientId == userId));
        if (beforeId is not null)
            query = query.Where(message => message.Id < beforeId.Value);
        var page = await query.OrderByDescending(message => message.Id).Take(50)
            .ToListAsync(cancellationToken);
        page.Reverse();
        return page.Select(ToMessageInfo).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        await RequireActiveUserAsync(userId);
        return await _db.Set<ChatMessage>().AsNoTracking().CountAsync(message =>
            message.RecipientId == userId && message.ReadAtUtc == null, cancellationToken);
    }

    public async Task<ChatMessageInfo> SendAsync(
        string senderId, ChatEnvelope envelope, CancellationToken cancellationToken = default)
    {
        await RequireActiveUserAsync(senderId);
        if (envelope is null || string.IsNullOrWhiteSpace(envelope.RecipientId) ||
            senderId == envelope.RecipientId || !Guid.TryParse(envelope.ClientMessageId, out _))
            throw new ChatValidationException("The message request is invalid.");

        var recipient = await RequireActiveUserAsync(envelope.RecipientId);
        if (!recipient.EmailConfirmed)
            throw new ChatValidationException("The recipient is unavailable.");

        ValidateCiphertext(envelope.SenderCiphertext, envelope.SenderNonce);
        ValidateCiphertext(envelope.RecipientCiphertext, envelope.RecipientNonce);
        var keys = await _db.Set<ChatPublicKey>().AsNoTracking()
            .Where(key => key.UserId == senderId || key.UserId == envelope.RecipientId)
            .ToListAsync(cancellationToken);
        var senderKey = keys.SingleOrDefault(key => key.UserId == senderId);
        var recipientKey = keys.SingleOrDefault(key => key.UserId == envelope.RecipientId);
        if (senderKey is null || recipientKey is null)
            throw new ChatValidationException("Both employees need chat keys before messaging.");
        if (senderKey.Fingerprint != envelope.SenderKeyFingerprint ||
            recipientKey.Fingerprint != envelope.RecipientKeyFingerprint)
            throw new ChatValidationException("A chat key changed. Verify the employee fingerprint again.");

        // A client-generated ID makes retries safe after a lost acknowledgement.
        var existing = await _db.Set<ChatMessage>().AsNoTracking().SingleOrDefaultAsync(message =>
            message.SenderId == senderId && message.ClientMessageId == envelope.ClientMessageId,
            cancellationToken);
        if (existing is not null)
            return ToMessageInfo(existing);

        var message = new ChatMessage
        {
            SenderId = senderId,
            RecipientId = envelope.RecipientId,
            ClientMessageId = envelope.ClientMessageId,
            SenderCiphertext = envelope.SenderCiphertext,
            RecipientCiphertext = envelope.RecipientCiphertext,
            SenderNonce = envelope.SenderNonce,
            RecipientNonce = envelope.RecipientNonce,
            SenderKeyFingerprint = envelope.SenderKeyFingerprint,
            RecipientKeyFingerprint = envelope.RecipientKeyFingerprint,
            SentAtUtc = DateTime.UtcNow
        };
        _db.Set<ChatMessage>().Add(message);
        await _db.SaveChangesAsync(cancellationToken);
        return ToMessageInfo(message);
    }

    public async Task<ChatMessageInfo> MarkReadAsync(
        string recipientId, long messageId, CancellationToken cancellationToken = default)
    {
        await RequireActiveUserAsync(recipientId);
        var message = await _db.Set<ChatMessage>().SingleOrDefaultAsync(item =>
            item.Id == messageId && item.RecipientId == recipientId, cancellationToken);
        if (message is null)
            throw new ChatValidationException("The message was not found.");
        if (message.ReadAtUtc is null)
        {
            message.ReadAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        return ToMessageInfo(message);
    }

    private async Task<ApplicationUser> RequireActiveUserAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null || await _users.IsInRoleAsync(user, AppRoles.Suspended))
            throw new ChatValidationException("This account cannot use chat.");
        return user;
    }

    private static void ValidateCiphertext(string ciphertext, string nonce)
    {
        if (string.IsNullOrWhiteSpace(ciphertext) || ciphertext.Length > 8192 ||
            string.IsNullOrWhiteSpace(nonce) || nonce.Length > 24)
            throw new ChatValidationException("The encrypted message is invalid.");
        try
        {
            var bytes = Convert.FromBase64String(ciphertext);
            if (bytes.Length is < 17 or > 6144 || Convert.FromBase64String(nonce).Length != 12)
                throw new ChatValidationException("The encrypted message is invalid.");
        }
        catch (FormatException)
        {
            throw new ChatValidationException("The encrypted message is invalid.");
        }
    }

    private static ChatKeyInfo ToKeyInfo(ChatPublicKey key) =>
        new(key.UserId, key.SubjectPublicKeyInfo, key.Fingerprint);

    private static ChatMessageInfo ToMessageInfo(ChatMessage message) =>
        new(message.Id, message.SenderId, message.RecipientId, message.ClientMessageId,
            message.SenderCiphertext, message.RecipientCiphertext, message.SenderNonce,
            message.RecipientNonce, message.SenderKeyFingerprint, message.RecipientKeyFingerprint,
            message.SentAtUtc, message.ReadAtUtc);
}

public sealed class ChatValidationException : Exception
{
    public ChatValidationException(string message) : base(message) { }
}
