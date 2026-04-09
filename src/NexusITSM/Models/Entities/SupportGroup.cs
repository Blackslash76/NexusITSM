using NexusITSM.Models.Enums;

namespace NexusITSM.Models.Entities;

public class SupportGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public GroupLevel Level { get; set; }
    public string Color { get; set; } = "#4B9EFF";
    public string Icon { get; set; } = "🎧";
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? SlaResponse { get; set; }
    public bool AutoAssign { get; set; }
    public string? EscalateToGroupId { get; set; }
    public SupportGroup? EscalateToGroup { get; set; }

    public List<string> Categories { get; set; } = [];
    public List<AppUser> Members { get; set; } = [];
    public List<Ticket> Tickets { get; set; } = [];
}
