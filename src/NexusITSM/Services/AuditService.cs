using NexusITSM.Data;
using NexusITSM.Models.Entities;

namespace NexusITSM.Services;

public class AuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db) => _db = db;

    public async Task LogAsync(string action, string entityType, string entityId,
        string userName, string? userId = null, string? oldValues = null,
        string? newValues = null, string? ipAddress = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserName = userName,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
        });
        await _db.SaveChangesAsync();
    }
}
