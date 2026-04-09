using NexusITSM.Models.Enums;

namespace NexusITSM.Models.Entities;

public class Change
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChangeType Type { get; set; } = ChangeType.Standard;
    public ChangeStatus Status { get; set; } = ChangeStatus.Draft;
    public int Risk { get; set; } = 1;
    public int Impact { get; set; } = 1;
    public string? Requester { get; set; }
    public string? ImplementationPlan { get; set; }
    public string? RollbackPlan { get; set; }
    public DateTime? CabDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }
}
