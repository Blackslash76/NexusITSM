using NexusITSM.Models.Enums;

namespace NexusITSM.Models.Entities;

public class ConfigurationItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Version { get; set; }
    public CIStatus Status { get; set; } = CIStatus.Active;
    public CICriticality Criticality { get; set; } = CICriticality.Medium;
    public string? Owner { get; set; }
    public string? Location { get; set; }
    public string? Vlan { get; set; }
    public string? SnmpVersion { get; set; }
    public bool HasAgent { get; set; }
    public string? AgentVersion { get; set; }
    public DateTime? LastSeen { get; set; }
    public int TicketCount { get; set; }
    public string? Vendor { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<string> Services { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
