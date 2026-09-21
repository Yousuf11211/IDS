namespace IDS.Data.Models;

// A public key is registered once. The matching private key exists only in the employee's browser.
public sealed class ChatPublicKey
{
    public string UserId { get; set; } = string.Empty;
    public string SubjectPublicKeyInfo { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
