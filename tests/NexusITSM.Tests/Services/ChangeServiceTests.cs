using NexusITSM.Services;

namespace NexusITSM.Tests.Services;

public class ChangeServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsChanges()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.Changes.Add(new Change { Id = "CHG-001", Title = "Change One" });
        db.Changes.Add(new Change { Id = "CHG-002", Title = "Change Two" });
        await db.SaveChangesAsync();

        var svc = new ChangeService(db);
        var result = await svc.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAsync_GeneratesIdAndSaves()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var svc = new ChangeService(db);

        var change = await svc.CreateAsync(new Change
        {
            Title = "New Change",
            Type = ChangeType.Minor
        });

        Assert.StartsWith("CHG-", change.Id);
        var saved = await db.Changes.FindAsync(change.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Change", saved.Title);
        Assert.Equal(ChangeType.Minor, saved.Type);
    }

    [Fact]
    public async Task UpdateAsync_PersistsStatusChange()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var svc = new ChangeService(db);

        var change = await svc.CreateAsync(new Change
        {
            Title = "Change To Approve"
        });

        change.Status = ChangeStatus.Approved;
        await svc.UpdateAsync(change);

        var reloaded = await db.Changes.FindAsync(change.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(ChangeStatus.Approved, reloaded.Status);
    }
}
