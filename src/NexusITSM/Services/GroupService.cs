using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;

namespace NexusITSM.Services;

public class GroupService
{
    private readonly AppDbContext _db;

    public GroupService(AppDbContext db) => _db = db;

    public async Task<List<SupportGroup>> GetAllAsync() =>
        await _db.SupportGroups.Include(g => g.Members).OrderBy(g => g.Level).ToListAsync();

    public async Task<SupportGroup?> GetByIdAsync(string id) =>
        await _db.SupportGroups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id);

    public async Task<List<AppUser>> GetAgentsAsync() =>
        await _db.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
}
