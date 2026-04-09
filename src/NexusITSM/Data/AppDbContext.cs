using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NexusITSM.Models.Entities;

namespace NexusITSM.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TimelineEvent> TimelineEvents => Set<TimelineEvent>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<Change> Changes => Set<Change>();
    public DbSet<SupportGroup> SupportGroups => Set<SupportGroup>();
    public DbSet<ConfigurationItem> ConfigurationItems => Set<ConfigurationItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WebhookConfig> WebhookConfigs => Set<WebhookConfig>();
    public DbSet<UserDashboardConfig> UserDashboardConfigs => Set<UserDashboardConfig>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var stringListConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        builder.Entity<Ticket>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.Assignee).WithMany().HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Group).WithMany(g => g.Tickets).HasForeignKey(t => t.GroupId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Problem).WithMany(p => p.Incidents).HasForeignKey(t => t.ProblemId).OnDelete(DeleteBehavior.SetNull);
            e.Property(t => t.Tags).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            e.Ignore(t => t.IsSelected);
        });

        builder.Entity<TimelineEvent>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.Ticket).WithMany(tk => tk.Timeline).HasForeignKey(t => t.TicketId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Problem>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Assignee).WithMany().HasForeignKey(p => p.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Change>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Assignee).WithMany().HasForeignKey(c => c.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SupportGroup>(e =>
        {
            e.HasKey(g => g.Id);
            e.HasOne(g => g.EscalateToGroup).WithMany().HasForeignKey(g => g.EscalateToGroupId).OnDelete(DeleteBehavior.SetNull);
            e.Property(g => g.Categories).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
        });

        builder.Entity<ConfigurationItem>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Tags).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            e.Property(c => c.Services).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
        });

        builder.Entity<AppUser>(e =>
        {
            e.HasOne(u => u.Group).WithMany(g => g.Members).HasForeignKey(u => u.GroupId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
