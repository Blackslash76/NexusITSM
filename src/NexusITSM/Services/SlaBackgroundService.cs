using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using NexusITSM.Data;
using NexusITSM.Models.Enums;
using NexusITSM.Hubs;

namespace NexusITSM.Services;

public class SlaBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public SlaBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SlaBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA Background Service started — interval: {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RecalculateSlaAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating SLA");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RecalculateSlaAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hub = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

        var now = DateTime.UtcNow;
        var activeTickets = await db.Tickets
            .Where(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
            .ToListAsync(ct);

        var breachNotifications = new List<string>();
        var warningTickets = new List<string>();

        foreach (var ticket in activeTickets)
        {
            var totalSlaMinutes = (ticket.SlaBreachAt - ticket.CreatedAt).TotalMinutes;
            if (totalSlaMinutes <= 0) totalSlaMinutes = 1;

            var elapsedMinutes = (now - ticket.CreatedAt).TotalMinutes;
            var newPercent = (int)Math.Min(100, Math.Max(0, elapsedMinutes / totalSlaMinutes * 100));

            var oldStatus = ticket.SlaStatus;
            var newStatus = newPercent >= 100 ? SlaStatus.Breach
                          : newPercent >= 75 ? SlaStatus.Warning
                          : SlaStatus.Ok;

            ticket.SlaPercent = newPercent;
            ticket.SlaStatus = newStatus;

            // Detect new breaches and warnings
            if (newStatus == SlaStatus.Breach && oldStatus != SlaStatus.Breach)
            {
                breachNotifications.Add(ticket.Id);
                _logger.LogWarning("SLA BREACH: {TicketId} — {Title}", ticket.Id, ticket.Title);
            }
            else if (newStatus == SlaStatus.Warning && oldStatus == SlaStatus.Ok)
            {
                warningTickets.Add(ticket.Id);
            }
        }

        await db.SaveChangesAsync(ct);

        // Trigger workflow engine for warnings and breaches
        var workflow = scope.ServiceProvider.GetRequiredService<WorkflowEngine>();
        foreach (var id in warningTickets)
            await workflow.ExecuteOnSlaWarningAsync(id);
        foreach (var id in breachNotifications)
            await workflow.ExecuteOnSlaBreachAsync(id);

        // Send SignalR notifications
        if (breachNotifications.Count > 0)
        {
            foreach (var ticketId in breachNotifications)
            {
                await hub.Clients.All.SendAsync("SLABreach", ticketId, ct);
            }
        }

        // Always notify clients to refresh SLA data
        await hub.Clients.All.SendAsync("SLAUpdated", activeTickets.Count, ct);

        _logger.LogDebug("SLA recalculated for {Count} tickets, {Breaches} new breaches",
            activeTickets.Count, breachNotifications.Count);
    }
}
