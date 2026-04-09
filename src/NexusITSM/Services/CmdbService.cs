using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public class CmdbService
{
    private readonly AppDbContext _db;
    private static int _seq = 100;

    public CmdbService(AppDbContext db) => _db = db;

    public async Task<List<ConfigurationItem>> GetAllAsync() =>
        await _db.ConfigurationItems.OrderBy(c => c.Name).ToListAsync();

    public async Task<List<ConfigurationItem>> SearchAsync(string? search, string? type, CIStatus? status)
    {
        var query = _db.ConfigurationItems.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Name.Contains(search) || (c.IpAddress != null && c.IpAddress.Contains(search)));
        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.Type == type);
        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<ConfigurationItem?> GetByIdAsync(string id) =>
        await _db.ConfigurationItems.FindAsync(id);

    public async Task<ConfigurationItem> CreateAsync(ConfigurationItem ci)
    {
        ci.Id = $"CI-{Interlocked.Increment(ref _seq):D3}";
        ci.CreatedAt = DateTime.UtcNow;
        _db.ConfigurationItems.Add(ci);
        await _db.SaveChangesAsync();
        return ci;
    }

    public async Task UpdateAsync(ConfigurationItem ci)
    {
        _db.ConfigurationItems.Update(ci);
        await _db.SaveChangesAsync();
    }

    public async Task<int> CountByStatusAsync(CIStatus status) =>
        await _db.ConfigurationItems.CountAsync(c => c.Status == status);

    public async Task<int> CountWithAgentAsync() =>
        await _db.ConfigurationItems.CountAsync(c => c.HasAgent);
}
