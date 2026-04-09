using Microsoft.EntityFrameworkCore;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Tests.Data;

public class DbContextTests
{
    [Fact]
    public async Task CanCreateAndReadTicket()
    {
        var db = TestDbHelper.CreateInMemoryDb();

        db.Tickets.Add(new Ticket
        {
            Id = "INC-TEST", Title = "Test ticket", Priority = TicketPriority.Medium,
            Status = TicketStatus.Open, Category = TicketCategory.Software,
            Requester = "Test", CreatedAt = DateTime.UtcNow,
            SlaBreachAt = DateTime.UtcNow.AddHours(8), Tags = ["test"]
        });
        await db.SaveChangesAsync();

        var ticket = await db.Tickets.FindAsync("INC-TEST");
        Assert.NotNull(ticket);
        Assert.Equal("Test ticket", ticket.Title);
        Assert.Contains("test", ticket.Tags);
    }

    [Fact]
    public async Task TicketAssigneeRelationship_Works()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();

        var ticket = await db.Tickets.Include(t => t.Assignee).FirstAsync(t => t.Id == "INC-1001");

        Assert.NotNull(ticket.Assignee);
        Assert.Equal("Marco Rossi", ticket.Assignee.FullName);
    }

    [Fact]
    public async Task TicketGroupRelationship_Works()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();

        var ticket = await db.Tickets.Include(t => t.Group).FirstAsync(t => t.Id == "INC-1001");

        Assert.NotNull(ticket.Group);
        Assert.Equal("L2 — Sistemisti", ticket.Group.Name);
    }

    [Fact]
    public async Task TimelineEvents_CascadeDeleteWithTicket()
    {
        var db = TestDbHelper.CreateInMemoryDb();

        db.Tickets.Add(new Ticket
        {
            Id = "INC-DEL", Title = "Delete me", Priority = TicketPriority.Low,
            Status = TicketStatus.Open, Category = TicketCategory.Software,
            CreatedAt = DateTime.UtcNow, SlaBreachAt = DateTime.UtcNow.AddHours(24), Tags = []
        });
        db.TimelineEvents.Add(new TimelineEvent
        {
            TicketId = "INC-DEL", Type = TimelineEventType.Created,
            Actor = "Test", Message = "Created"
        });
        await db.SaveChangesAsync();

        var ticket = await db.Tickets.FindAsync("INC-DEL");
        db.Tickets.Remove(ticket!);
        await db.SaveChangesAsync();

        Assert.Empty(db.TimelineEvents.Where(e => e.TicketId == "INC-DEL"));
    }

    [Fact]
    public async Task SupportGroup_MembersRelationship_Works()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();

        var group = await db.SupportGroups.Include(g => g.Members).FirstAsync(g => g.Id == "g02");

        Assert.NotEmpty(group.Members);
        Assert.Contains(group.Members, m => m.FullName == "Marco Rossi");
    }

    [Fact]
    public async Task ConfigurationItem_CanStoreAndRetrieve()
    {
        var db = TestDbHelper.CreateInMemoryDb();

        db.ConfigurationItems.Add(new ConfigurationItem
        {
            Id = "CI-TEST", Name = "Test Server", Type = "Server",
            IpAddress = "10.0.0.1", Status = CIStatus.Active,
            Criticality = CICriticality.High,
            Tags = ["test", "server"], Services = ["Web", "API"]
        });
        await db.SaveChangesAsync();

        var ci = await db.ConfigurationItems.FindAsync("CI-TEST");
        Assert.NotNull(ci);
        Assert.Equal(2, ci.Tags.Count);
        Assert.Equal(2, ci.Services.Count);
    }
}
