namespace IDS.Data.Models;

// The server stores two encrypted copies so both participants can reopen the conversation.
// No plaintext message or private key is accepted by this model.
public sealed class ChatMessage
{
    public long Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public string ClientMessageId { get; set; } = string.Empty;
    public string SenderCiphertext { get; set; } = string.Empty;
    public string RecipientCiphertext { get; set; } = string.Empty;
    public string SenderNonce { get; set; } = string.Empty;
    public string RecipientNonce { get; set; } = string.Empty;
    public string SenderKeyFingerprint { get; set; } = string.Empty;
    public string RecipientKeyFingerprint { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
