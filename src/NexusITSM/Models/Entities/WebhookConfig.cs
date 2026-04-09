namespace NexusITSM.Models.Entities;

public class WebhookConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Events { get; set; } = "ticket.created,ticket.escalated,ticket.resolved";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; set; }
    public int FailCount { get; set; }
}
