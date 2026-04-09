using Microsoft.AspNetCore.SignalR;
using NexusITSM.Hubs;

namespace NexusITSM.Services;

public class NotificationService
{
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(IHubContext<NotificationHub> hub) => _hub = hub;

    public async Task NotifyTicketCreated(string ticketId, string title)
    {
        await _hub.Clients.All.SendAsync("Notification", "ticket_created",
            $"Nuovo ticket: {ticketId} — {title}", ticketId);
    }

    public async Task NotifyTicketEscalated(string ticketId, string title)
    {
        await _hub.Clients.All.SendAsync("Notification", "escalation",
            $"Escalation: {ticketId} — {title}", ticketId);
    }

    public async Task NotifyTicketResolved(string ticketId, string title)
    {
        await _hub.Clients.All.SendAsync("Notification", "resolved",
            $"Risolto: {ticketId} — {title}", ticketId);
    }

    public async Task NotifySlaBreach(string ticketId, string title)
    {
        await _hub.Clients.All.SendAsync("Notification", "sla_breach",
            $"SLA Breach: {ticketId} — {title}", ticketId);
    }
}
