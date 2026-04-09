using NexusITSM.Services;

namespace NexusITSM.Tests.Services;

public class CmdbServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsCIs()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-001", Name = "Server A", Type = "Server", Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-002", Name = "Switch B", Type = "Network", Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-003", Name = "PC C", Type = "Workstation", Tags = [], Services = [] });
        await db.SaveChangesAsync();

        var svc = new CmdbService(db);
        var result = await svc.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_FiltersByName()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-001", Name = "Web Server Alpha", Type = "Server", Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-002", Name = "DB Server Beta", Type = "Server", Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-003", Name = "Workstation Gamma", Type = "Workstation", Tags = [], Services = [] });
        await db.SaveChangesAsync();

        var svc = new CmdbService(db);
        var result = await svc.SearchAsync("Server", null, null);

        Assert.Equal(2, result.Count);
        Assert.All(result, ci => Assert.Contains("Server", ci.Name));
    }

    [Fact]
    public async Task SearchAsync_FiltersByStatus()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-001", Name = "Active CI 1", Type = "Server", Status = CIStatus.Active, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-002", Name = "Active CI 2", Type = "Server", Status = CIStatus.Active, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-003", Name = "Offline CI", Type = "Server", Status = CIStatus.Offline, Tags = [], Services = [] });
        await db.SaveChangesAsync();

        var svc = new CmdbService(db);
        var activeResult = await svc.SearchAsync(null, null, CIStatus.Active);
        var offlineResult = await svc.SearchAsync(null, null, CIStatus.Offline);

        Assert.Equal(2, activeResult.Count);
        Assert.Single(offlineResult);
        Assert.All(activeResult, ci => Assert.Equal(CIStatus.Active, ci.Status));
    }

    [Fact]
    public async Task CreateAsync_GeneratesId()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        var svc = new CmdbService(db);

        var ci = await svc.CreateAsync(new ConfigurationItem
        {
            Name = "New Server",
            Type = "Server",
            Tags = [],
            Services = []
        });

        Assert.StartsWith("CI-", ci.Id);
        var saved = await db.ConfigurationItems.FindAsync(ci.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Server", saved.Name);
    }

    [Fact]
    public async Task CountByStatusAsync_ReturnsCorrectCounts()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-001", Name = "A", Type = "Server", Status = CIStatus.Active, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-002", Name = "B", Type = "Server", Status = CIStatus.Active, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-003", Name = "C", Type = "Server", Status = CIStatus.Offline, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-004", Name = "D", Type = "Server", Status = CIStatus.Maintenance, Tags = [], Services = [] });
        await db.SaveChangesAsync();

        var svc = new CmdbService(db);

        Assert.Equal(2, await svc.CountByStatusAsync(CIStatus.Active));
        Assert.Equal(1, await svc.CountByStatusAsync(CIStatus.Offline));
        Assert.Equal(1, await svc.CountByStatusAsync(CIStatus.Maintenance));
        Assert.Equal(0, await svc.CountByStatusAsync(CIStatus.Degraded));
    }

    [Fact]
    public async Task CountWithAgentAsync_ReturnsOnlyAgentCIs()
    {
        var db = TestDbHelper.CreateInMemoryDb();
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-001", Name = "A", Type = "Server", HasAgent = true, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-002", Name = "B", Type = "Server", HasAgent = true, Tags = [], Services = [] });
        db.ConfigurationItems.Add(new ConfigurationItem { Id = "CI-003", Name = "C", Type = "Server", HasAgent = false, Tags = [], Services = [] });
        await db.SaveChangesAsync();

        var svc = new CmdbService(db);
        var count = await svc.CountWithAgentAsync();

        Assert.Equal(2, count);
    }
}
