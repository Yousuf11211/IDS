# Employee chat preview: security and rollout notes

The chat feature is off by default (`Chat:Enabled=false`). It is a thesis preview, not a reviewed company messenger. Enable it only in an approved test environment after applying the `AddEncryptedEmployeeChat` migration.

## Data flow

The browser creates a nonextractable P-256 ECDH private key with Web Crypto and stores it in IndexedDB. The server stores its public key and a SHA-256 fingerprint. Before sending, the user must compare the recipient's fingerprint with that person over a trusted channel and confirm it in the browser. The browser derives message keys with ECDH and HKDF, encrypts with AES-GCM, and uploads separate encrypted copies for sender and recipient. The server stores ciphertext, nonces, sender/recipient IDs, timestamps and read status. SignalR notifications carry message metadata only. Messages are rendered with `textContent`, so message text is never inserted as HTML.

## Important limits

- Browser storage is the only copy of the private key. Clearing it or changing devices makes old messages unreadable. There is no recovery or multiple-device flow.
- Static ECDH keys provide no forward secrecy. A later compromise of a private key can expose old messages.
- Users must verify fingerprints outside this app. A compromised server can substitute a new public key before it is verified. Fingerprint confirmation alone is not a security audit.
- The server sees who talks to whom, message times, ciphertext sizes and read receipts. Encryption covers message contents only.
- An XSS vulnerability or compromised same-origin JavaScript can read plaintext while the user views or writes it. Review all scripts, dependencies, CSP and deployment settings before rollout.
- Read receipts mean the recipient's browser opened a visible conversation. They do not prove the person read or understood a message.
- Notification delivery requires an active connection; the browser notification permission is optional. This preview has no offline push service.

## Before company use

Have an independent security review of the browser cryptography, key verification, identity binding and authentication flow. Add an approved key recovery/device enrollment design, storage and retention policies, rate limits, abuse controls, accessibility testing, and a live SQL Server/browser test of two users exchanging messages and receipts. Keep `Chat:Enabled=false` until those checks pass.
