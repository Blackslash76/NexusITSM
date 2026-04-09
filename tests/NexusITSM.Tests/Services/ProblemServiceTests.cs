using NexusITSM.Services;

namespace NexusITSM.Tests.Services;

public class ProblemServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsProblems()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.Problems.Add(new Problem { Id = "PRB-001", Title = "Problem One" });
        db.Problems.Add(new Problem { Id = "PRB-002", Title = "Problem Two" });
        await db.SaveChangesAsync();

        var svc = new ProblemService(db);
        var result = await svc.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAsync_GeneratesIdAndSaves()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var svc = new ProblemService(db);

        var problem = await svc.CreateAsync(new Problem
        {
            Title = "New Problem"
        });

        Assert.StartsWith("PRB-", problem.Id);
        var saved = await db.Problems.FindAsync(problem.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Problem", saved.Title);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var svc = new ProblemService(db);

        var problem = await svc.CreateAsync(new Problem
        {
            Title = "Problem To Update"
        });

        problem.Status = ProblemStatus.KnownError;
        await svc.UpdateAsync(problem);

        var reloaded = await db.Problems.FindAsync(problem.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(ProblemStatus.KnownError, reloaded.Status);
    }
}
