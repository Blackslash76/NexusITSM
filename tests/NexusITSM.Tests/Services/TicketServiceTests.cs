using NexusITSM.Models.Enums;
using NexusITSM.Services;

namespace NexusITSM.Tests.Services;

public class TicketServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllTickets()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        var tickets = await svc.GetAllAsync();

        Assert.Equal(3, tickets.Count);
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesResolvedAndClosed()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        var active = await svc.GetActiveAsync();

        Assert.Equal(2, active.Count);
        Assert.DoesNotContain(active, t => t.Status == TicketStatus.Resolved);
        Assert.DoesNotContain(active, t => t.Status == TicketStatus.Closed);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectTicket()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        var ticket = await svc.GetByIdAsync("INC-1001");

        Assert.NotNull(ticket);
        Assert.Equal("Server down", ticket.Title);
        Assert.Equal(TicketPriority.Critical, ticket.Priority);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForInvalid()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        var ticket = await svc.GetByIdAsync("INC-9999");

        Assert.Null(ticket);
    }

    [Fact]
    public async Task CreateAsync_AddsTicketAndTimeline()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        var ticket = await svc.CreateAsync(new Ticket
        {
            Title = "New ticket",
            Priority = TicketPriority.High,
            Category = TicketCategory.Network,
            Requester = "Tester",
            Department = "QA",
            Description = "Test description"
        });

        Assert.StartsWith("INC-", ticket.Id);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(SlaStatus.Ok, ticket.SlaStatus);
        Assert.True(ticket.SlaBreachAt > DateTime.UtcNow);

        var timeline = db.TimelineEvents.Where(e => e.TicketId == ticket.Id).ToList();
        Assert.Single(timeline);
        Assert.Equal(TimelineEventType.Created, timeline[0].Type);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatusAndAddsTimeline()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        await svc.UpdateStatusAsync("INC-1001", TicketStatus.Resolved, "Test Admin");

        var ticket = await db.Tickets.FindAsync("INC-1001");
        Assert.Equal(TicketStatus.Resolved, ticket!.Status);
        Assert.NotNull(ticket.ResolvedAt);

        var events = db.TimelineEvents.Where(e => e.TicketId == "INC-1001").ToList();
        Assert.Contains(events, e => e.Type == TimelineEventType.Resolved);
    }

    [Fact]
    public async Task EscalateAsync_IncrementsLevelAndChangesStatus()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        await svc.EscalateAsync("INC-1002", "Test Admin");

        var ticket = await db.Tickets.FindAsync("INC-1002");
        Assert.Equal(TicketStatus.Escalated, ticket!.Status);
        Assert.Equal(EscalationLevel.L1, ticket.EscalationLevel);
        Assert.True(ticket.IsEscalated);

        var events = db.TimelineEvents.Where(e => e.TicketId == "INC-1002").ToList();
        Assert.Contains(events, e => e.Type == TimelineEventType.Escalated);
    }

    [Fact]
    public async Task AddNoteAsync_AddsTimelineEvent()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        await svc.AddNoteAsync("INC-1001", "Marco", "Test note");

        var events = db.TimelineEvents.Where(e => e.TicketId == "INC-1001" && e.Type == TimelineEventType.Note).ToList();
        Assert.Contains(events, e => e.Message == "Test note" && e.Actor == "Marco");
    }

    [Fact]
    public async Task CountByStatusAsync_ReturnsCorrectCount()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        Assert.Equal(1, await svc.CountByStatusAsync(TicketStatus.Open));
        Assert.Equal(1, await svc.CountByStatusAsync(TicketStatus.InProgress));
        Assert.Equal(1, await svc.CountByStatusAsync(TicketStatus.Resolved));
        Assert.Equal(0, await svc.CountByStatusAsync(TicketStatus.Closed));
    }

    [Fact]
    public async Task CountBySlaStatusAsync_ExcludesClosedTickets()
    {
        var db = await TestDbHelper.CreateSeededDbAsync();
        var svc = new TicketService(db);

        var breachCount = await svc.CountBySlaStatusAsync(SlaStatus.Breach);
        Assert.Equal(1, breachCount);
    }
}
