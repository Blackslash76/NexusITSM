using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public class TicketService
{
    private readonly AppDbContext _db;
    private readonly NotificationService? _notify;
    private static int _seq = 1020;

    public TicketService(AppDbContext db, NotificationService? notify = null)
    {
        _db = db;
        _notify = notify;
    }

    public async Task<List<Ticket>> GetAllAsync() =>
        await _db.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Group)
            .Include(t => t.Timeline)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Ticket?> GetByIdAsync(string id) =>
        await _db.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Group)
            .Include(t => t.Timeline)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<Ticket>> GetActiveAsync() =>
        await _db.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Group)
            .Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        ticket.Id = $"INC-{Interlocked.Increment(ref _seq)}";
        ticket.CreatedAt = DateTime.UtcNow;
        var slaHours = ticket.Priority switch
        {
            TicketPriority.Critical => 2, TicketPriority.High => 4,
            TicketPriority.Medium => 8, _ => 24
        };
        ticket.SlaBreachAt = ticket.CreatedAt.AddHours(slaHours);
        ticket.SlaPercent = 0;
        ticket.SlaStatus = SlaStatus.Ok;

        _db.Tickets.Add(ticket);
        _db.TimelineEvents.Add(new TimelineEvent
        {
            TicketId = ticket.Id,
            Type = TimelineEventType.Created,
            Actor = ticket.Requester ?? "Sistema",
            Message = ticket.Description ?? "Ticket creato",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        if (_notify != null) await _notify.NotifyTicketCreated(ticket.Id, ticket.Title);
        return ticket;
    }

    public async Task UpdateStatusAsync(string ticketId, TicketStatus newStatus, string actor)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket == null) return;

        ticket.Status = newStatus;
        if (newStatus == TicketStatus.Resolved) ticket.ResolvedAt = DateTime.UtcNow;
        if (newStatus == TicketStatus.Closed) ticket.ClosedAt = DateTime.UtcNow;

        _db.TimelineEvents.Add(new TimelineEvent
        {
            TicketId = ticketId,
            Type = newStatus == TicketStatus.Resolved ? TimelineEventType.Resolved : TimelineEventType.StatusChanged,
            Actor = actor,
            Message = $"Status aggiornato: {newStatus}",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        if (_notify != null && newStatus == TicketStatus.Resolved)
            await _notify.NotifyTicketResolved(ticketId, ticket.Title);
    }

    public async Task EscalateAsync(string ticketId, string actor)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket == null) return;

        ticket.EscalationLevel = ticket.EscalationLevel < EscalationLevel.L3
            ? ticket.EscalationLevel + 1 : ticket.EscalationLevel;
        ticket.Status = TicketStatus.Escalated;
        ticket.IsEscalated = true;

        var levels = new[] { "", "L1 Support", "L2 Engineering", "Management" };
        _db.TimelineEvents.Add(new TimelineEvent
        {
            TicketId = ticketId,
            Type = TimelineEventType.Escalated,
            Actor = actor,
            Message = $"Escalation manuale a {levels[(int)ticket.EscalationLevel]}",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        if (_notify != null) await _notify.NotifyTicketEscalated(ticketId, ticket.Title);
    }

    public async Task AddNoteAsync(string ticketId, string actor, string note)
    {
        _db.TimelineEvents.Add(new TimelineEvent
        {
            TicketId = ticketId,
            Type = TimelineEventType.Note,
            Actor = actor,
            Message = note,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    // Dashboard stats
    public async Task<int> CountByStatusAsync(TicketStatus status) =>
        await _db.Tickets.CountAsync(t => t.Status == status);

    public async Task<int> CountBySlaStatusAsync(SlaStatus status) =>
        await _db.Tickets.CountAsync(t => t.SlaStatus == status && t.Status != TicketStatus.Closed);

    public async Task<int> CountActiveAsync() =>
        await _db.Tickets.CountAsync(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);
}
