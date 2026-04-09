using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;

namespace NexusITSM.Services;

public class ChangeService
{
    private readonly AppDbContext _db;
    private static int _seq = 10;

    public ChangeService(AppDbContext db) => _db = db;

    public async Task<List<Change>> GetAllAsync() =>
        await _db.Changes.Include(c => c.Assignee).OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<Change?> GetByIdAsync(string id) =>
        await _db.Changes.Include(c => c.Assignee).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Change> CreateAsync(Change change)
    {
        change.Id = $"CHG-{Interlocked.Increment(ref _seq):D3}";
        change.CreatedAt = DateTime.UtcNow;
        _db.Changes.Add(change);
        await _db.SaveChangesAsync();
        return change;
    }

    public async Task UpdateAsync(Change change)
    {
        _db.Changes.Update(change);
        await _db.SaveChangesAsync();
    }
}
