using NexusITSM.Models.Enums;

namespace NexusITSM.Models.Entities;

public class Problem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ProblemStatus Status { get; set; } = ProblemStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string? RootCauseAnalysis { get; set; }
    public string? Workaround { get; set; }
    public bool IsKnownError { get; set; }

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Ticket> Incidents { get; set; } = [];
}
