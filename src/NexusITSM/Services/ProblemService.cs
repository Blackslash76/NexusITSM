using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public class ProblemService
{
    private readonly AppDbContext _db;
    private static int _seq = 10;

    public ProblemService(AppDbContext db) => _db = db;

    public async Task<List<Problem>> GetAllAsync() =>
        await _db.Problems.Include(p => p.Assignee).Include(p => p.Incidents).OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<Problem?> GetByIdAsync(string id) =>
        await _db.Problems.Include(p => p.Assignee).Include(p => p.Incidents).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Problem> CreateAsync(Problem problem)
    {
        problem.Id = $"PRB-{Interlocked.Increment(ref _seq):D3}";
        problem.CreatedAt = DateTime.UtcNow;
        _db.Problems.Add(problem);
        await _db.SaveChangesAsync();
        return problem;
    }

    public async Task UpdateAsync(Problem problem)
    {
        _db.Problems.Update(problem);
        await _db.SaveChangesAsync();
    }
}
