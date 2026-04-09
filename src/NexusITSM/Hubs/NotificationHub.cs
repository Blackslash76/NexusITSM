using Microsoft.AspNetCore.SignalR;

namespace NexusITSM.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string type, string message, string? ticketId = null)
    {
        await Clients.All.SendAsync("Notification", type, message, ticketId);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
