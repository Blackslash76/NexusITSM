using NexusITSM.Models.Enums;

namespace NexusITSM.Models.Entities;

public class Ticket
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketCategory Category { get; set; }
    public string? Requester { get; set; }
    public string? Department { get; set; }

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }

    public string? GroupId { get; set; }
    public SupportGroup? Group { get; set; }

    public EscalationLevel EscalationLevel { get; set; } = EscalationLevel.None;
    public bool IsEscalated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime SlaBreachAt { get; set; }
    public int SlaPercent { get; set; }
    public SlaStatus SlaStatus { get; set; } = SlaStatus.Ok;

    public List<string> Tags { get; set; } = [];
    public List<TimelineEvent> Timeline { get; set; } = [];

    public string? ProblemId { get; set; }
    public Problem? Problem { get; set; }

    // UI-only (not persisted)
    public bool IsSelected { get; set; }
}
