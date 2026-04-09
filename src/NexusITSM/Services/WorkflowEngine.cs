using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public class WorkflowEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(IServiceScopeFactory scopeFactory, ILogger<WorkflowEngine> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteOnTicketCreatedAsync(string ticketId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ticket = await db.Tickets.Include(t => t.Group).FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return;

        _logger.LogInformation("Workflow: processing ticket {Id} — {Title}", ticket.Id, ticket.Title);

        // Rule 1: Auto-assign to L1 if no group
        if (string.IsNullOrEmpty(ticket.GroupId))
        {
            var l1 = await db.SupportGroups.FirstOrDefaultAsync(g => g.Level == GroupLevel.L1);
            if (l1 != null)
            {
                ticket.GroupId = l1.Id;
                db.TimelineEvents.Add(new TimelineEvent
                {
                    TicketId = ticketId, Type = TimelineEventType.Assigned,
                    Actor = "Workflow Engine", Message = $"Auto-assegnato a {l1.Name}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Rule 2: Critical tickets go directly to L2
        if (ticket.Priority == TicketPriority.Critical)
        {
            var l2 = await db.SupportGroups.FirstOrDefaultAsync(g => g.Level == GroupLevel.L2);
            if (l2 != null)
            {
                ticket.GroupId = l2.Id;
                db.TimelineEvents.Add(new TimelineEvent
                {
                    TicketId = ticketId, Type = TimelineEventType.Assigned,
                    Actor = "Workflow Engine", Message = $"Priority Critical → auto-routing a {l2.Name}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Rule 3: Security category → Security Team
        if (ticket.Category == TicketCategory.Security || ticket.Category == TicketCategory.AccessManagement)
        {
            var sec = await db.SupportGroups.FirstOrDefaultAsync(g => g.Name.Contains("Security"));
            if (sec != null)
            {
                ticket.GroupId = sec.Id;
                db.TimelineEvents.Add(new TimelineEvent
                {
                    TicketId = ticketId, Type = TimelineEventType.Assigned,
                    Actor = "Workflow Engine", Message = $"Categoria Security → auto-routing a {sec.Name}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Rule 4: Auto-assign to first available agent in group
        if (!string.IsNullOrEmpty(ticket.GroupId) && string.IsNullOrEmpty(ticket.AssigneeId))
        {
            var agent = await db.Users
                .Where(u => u.GroupId == ticket.GroupId && u.IsActive)
                .OrderBy(u => db.Tickets.Count(t => t.AssigneeId == u.Id && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved))
                .FirstOrDefaultAsync();

            if (agent != null)
            {
                ticket.AssigneeId = agent.Id;
                db.TimelineEvents.Add(new TimelineEvent
                {
                    TicketId = ticketId, Type = TimelineEventType.Assigned,
                    Actor = "Workflow Engine", Message = $"Auto-assegnato a {agent.FullName} (carico minimo)",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("Workflow: ticket {Id} processed — Group={Group}, Assignee={Assignee}",
            ticket.Id, ticket.GroupId, ticket.AssigneeId);
    }

    public async Task ExecuteOnSlaWarningAsync(string ticketId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notify = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var ticket = await db.Tickets.Include(t => t.Assignee).FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return;

        // Auto-escalate if SLA > 75% and not already escalated
        if (ticket.SlaPercent >= 75 && !ticket.IsEscalated)
        {
            ticket.EscalationLevel = EscalationLevel.L1;
            ticket.Status = TicketStatus.Escalated;
            ticket.IsEscalated = true;

            db.TimelineEvents.Add(new TimelineEvent
            {
                TicketId = ticketId, Type = TimelineEventType.Escalated,
                Actor = "Workflow Engine", Message = "Auto-escalation: SLA > 75%",
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await notify.NotifyTicketEscalated(ticketId, ticket.Title);
            _logger.LogWarning("Workflow: auto-escalated {Id} — SLA at {Pct}%", ticketId, ticket.SlaPercent);
        }
    }

    public async Task ExecuteOnSlaBreachAsync(string ticketId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notify = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return;

        // Escalate to L2 on breach
        if (ticket.EscalationLevel < EscalationLevel.L2)
        {
            ticket.EscalationLevel = EscalationLevel.L2;
            ticket.Status = TicketStatus.Escalated;
            ticket.IsEscalated = true;

            db.TimelineEvents.Add(new TimelineEvent
            {
                TicketId = ticketId, Type = TimelineEventType.Escalated,
                Actor = "Workflow Engine", Message = "SLA Breach → escalation a L2",
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await notify.NotifySlaBreach(ticketId, ticket.Title);
            _logger.LogCritical("Workflow: SLA BREACH escalation L2 for {Id}", ticketId);
        }
    }
}
