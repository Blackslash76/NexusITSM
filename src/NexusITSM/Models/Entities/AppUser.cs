using Microsoft.AspNetCore.Identity;

namespace NexusITSM.Models.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Color { get; set; } = "#4B9EFF";
    public string? Department { get; set; }
    public string? Role { get; set; }
    public string? GroupId { get; set; }
    public SupportGroup? Group { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
