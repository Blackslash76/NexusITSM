using NexusITSM.Models.Enums;

namespace NexusITSM.Models.Entities;

public class TimelineEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TimelineEventType Type { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}
