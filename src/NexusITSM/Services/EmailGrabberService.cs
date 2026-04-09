using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public record EmailMessage(string From, string Subject, string Body, DateTime Date, string MessageId);

public record EmailGrabberConfig
{
    public string Server { get; set; } = "";
    public int Port { get; set; } = 993;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseSsl { get; set; } = true;
    public string Folder { get; set; } = "INBOX";
}

public class EmailGrabberService
{
    private readonly ILogger<EmailGrabberService> _logger;

    public EmailGrabberService(ILogger<EmailGrabberService> logger) => _logger = logger;

    public async Task<(bool Success, string Message)> TestConnectionAsync(EmailGrabberConfig config)
    {
        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(config.Server, config.Port, config.UseSsl);
            await client.AuthenticateAsync(config.Username, config.Password);
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            var count = inbox.Count;
            await client.DisconnectAsync(true);
            return (true, $"Connessione riuscita. {count} email in {config.Folder}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IMAP connection test failed");
            return (false, $"Errore: {ex.Message}");
        }
    }

    public async Task<List<EmailMessage>> FetchUnreadAsync(EmailGrabberConfig config, int maxMessages = 20)
    {
        var messages = new List<EmailMessage>();
        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(config.Server, config.Port, config.UseSsl);
            await client.AuthenticateAsync(config.Username, config.Password);

            var folder = config.Folder == "INBOX"
                ? client.Inbox
                : await client.GetFolderAsync(config.Folder);

            await folder.OpenAsync(FolderAccess.ReadOnly);

            var uids = await folder.SearchAsync(SearchQuery.NotSeen);
            var toFetch = uids.TakeLast(maxMessages).ToList();

            foreach (var uid in toFetch)
            {
                var message = await folder.GetMessageAsync(uid);
                messages.Add(new EmailMessage(
                    message.From.ToString(),
                    message.Subject ?? "(nessun oggetto)",
                    message.TextBody ?? message.HtmlBody ?? "",
                    message.Date.UtcDateTime,
                    message.MessageId ?? uid.ToString()
                ));
            }

            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch emails from {Server}", config.Server);
        }
        return messages;
    }

    public static (TicketCategory Category, TicketPriority Priority) CategorizeEmail(string subject, string from)
    {
        var sub = subject.ToLowerInvariant();
        var frm = from.ToLowerInvariant();

        var category = TicketCategory.Software;
        var priority = TicketPriority.Medium;

        if (sub.Contains("vpn") || sub.Contains("network") || sub.Contains("wifi"))
            category = TicketCategory.Network;
        else if (sub.Contains("security") || sub.Contains("virus") || sub.Contains("breach") || frm.Contains("security"))
            { category = TicketCategory.Security; priority = TicketPriority.High; }
        else if (sub.Contains("server") || sub.Contains("down") || sub.Contains("offline"))
            { category = TicketCategory.Server; priority = TicketPriority.Critical; }
        else if (sub.Contains("email") || sub.Contains("outlook") || sub.Contains("exchange"))
            category = TicketCategory.Email;
        else if (sub.Contains("printer") || sub.Contains("stampante"))
            category = TicketCategory.Printer;
        else if (sub.Contains("password") || sub.Contains("access") || sub.Contains("account"))
            category = TicketCategory.AccessManagement;
        else if (sub.Contains("backup") || sub.Contains("nas"))
            { category = TicketCategory.Backup; priority = TicketPriority.High; }

        if (sub.Contains("urgent") || sub.Contains("urgente") || sub.Contains("critical"))
            priority = TicketPriority.Critical;

        return (category, priority);
    }
}
