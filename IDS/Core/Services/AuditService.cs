using IDS.Data;
using IDS.Data.Models;

namespace IDS.Core.Services
{
    /// <summary>
    /// Service for logging audit events.
    /// </summary>
    public interface IAuditService
    {
        Task LogAsync(string userId, string userEmail, string action, string entityType,
            string? entityId = null, string? details = null, string? ipAddress = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string userId, string userEmail, string action, string entityType,
            string? entityId = null, string? details = null, string? ipAddress = null)
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = ipAddress
            };

            _db.AuditLogs.Add(auditLog);
            await _db.SaveChangesAsync();
        }
    }
}
