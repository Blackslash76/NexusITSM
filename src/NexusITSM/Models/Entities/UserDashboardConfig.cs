namespace NexusITSM.Models.Entities;

public class UserDashboardConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Layout { get; set; } = "default";
    public string WidgetsJson { get; set; } = "[]";
    public bool ShowKpis { get; set; } = true;
    public bool ShowRecentTickets { get; set; } = true;
    public bool ShowGroupWorkload { get; set; } = true;
    public bool ShowCategories { get; set; } = true;
    public bool ShowProblems { get; set; } = true;
    public bool ShowChanges { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
