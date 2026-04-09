using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusITSM.Data;
using NexusITSM.Models.Entities;

namespace NexusITSM.Tests;

public static class TestDbHelper
{
    public static AppDbContext CreateInMemoryDb(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static async Task<AppDbContext> CreateSeededDbAsync(string? dbName = null)
    {
        var db = CreateInMemoryDb(dbName);

        db.SupportGroups.AddRange(
            new SupportGroup { Id = "g01", Name = "L1 — Help Desk", Level = GroupLevel.L1, Color = "#30C48D", Icon = "🎧" },
            new SupportGroup { Id = "g02", Name = "L2 — Sistemisti", Level = GroupLevel.L2, Color = "#4B9EFF", Icon = "🖥" }
        );

        db.Users.AddRange(
            new AppUser { Id = "a1", UserName = "marco@test.com", Email = "marco@test.com", FullName = "Marco Rossi", Initials = "MR", Color = "#4B9EFF", GroupId = "g02" },
            new AppUser { Id = "a2", UserName = "sara@test.com", Email = "sara@test.com", FullName = "Sara Bianchi", Initials = "SB", Color = "#30C48D", GroupId = "g01" }
        );

        db.Tickets.AddRange(
            new Ticket
            {
                Id = "INC-1001", Title = "Server down", Priority = TicketPriority.Critical,
                Status = TicketStatus.Open, Category = TicketCategory.Server,
                AssigneeId = "a1", GroupId = "g02", Requester = "Test User", Department = "IT",
                CreatedAt = DateTime.UtcNow.AddHours(-3), SlaBreachAt = DateTime.UtcNow.AddHours(-1),
                SlaPercent = 90, SlaStatus = SlaStatus.Breach, Tags = ["server", "critical"]
            },
            new Ticket
            {
                Id = "INC-1002", Title = "VPN issue", Priority = TicketPriority.Medium,
                Status = TicketStatus.InProgress, Category = TicketCategory.VPN,
                AssigneeId = "a2", GroupId = "g01", Requester = "User 2", Department = "Sales",
                CreatedAt = DateTime.UtcNow.AddHours(-5), SlaBreachAt = DateTime.UtcNow.AddHours(3),
                SlaPercent = 40, SlaStatus = SlaStatus.Ok, Tags = ["vpn"]
            },
            new Ticket
            {
                Id = "INC-1003", Title = "Resolved ticket", Priority = TicketPriority.Low,
                Status = TicketStatus.Resolved, Category = TicketCategory.Software,
                AssigneeId = "a2", GroupId = "g01", Requester = "User 3", Department = "HR",
                CreatedAt = DateTime.UtcNow.AddDays(-2), SlaBreachAt = DateTime.UtcNow.AddDays(-1),
                SlaPercent = 100, SlaStatus = SlaStatus.Ok, Tags = []
            }
        );

        await db.SaveChangesAsync();
        return db;
    }
}
