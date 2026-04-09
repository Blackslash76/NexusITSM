using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;

namespace NexusITSM.Services;

public class WebhookService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(IServiceScopeFactory scopeFactory, IHttpClientFactory httpFactory, ILogger<WebhookService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task TriggerAsync(string eventType, object payload)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hooks = await db.WebhookConfigs
            .Where(w => w.IsActive && w.Events.Contains(eventType))
            .ToListAsync();

        var json = JsonSerializer.Serialize(new { Event = eventType, Timestamp = DateTime.UtcNow, Data = payload });

        foreach (var hook in hooks)
        {
            try
            {
                var client = _httpFactory.CreateClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add HMAC signature
                var signature = ComputeHmac(json, hook.Secret);
                content.Headers.Add("X-NexusITSM-Signature", signature);
                content.Headers.Add("X-NexusITSM-Event", eventType);

                var response = await client.PostAsync(hook.Url, content);

                hook.LastTriggeredAt = DateTime.UtcNow;
                if (!response.IsSuccessStatusCode)
                {
                    hook.FailCount++;
                    _logger.LogWarning("Webhook {Name} failed: {StatusCode}", hook.Name, response.StatusCode);
                }
                else
                {
                    hook.FailCount = 0;
                }
            }
            catch (Exception ex)
            {
                hook.FailCount++;
                _logger.LogError(ex, "Webhook {Name} error", hook.Name);
            }

            // Auto-disable after 10 consecutive failures
            if (hook.FailCount >= 10)
            {
                hook.IsActive = false;
                _logger.LogWarning("Webhook {Name} disabled after {Count} failures", hook.Name, hook.FailCount);
            }
        }

        await db.SaveChangesAsync();
    }

    private static string ComputeHmac(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexStringLower(hmac.ComputeHash(data));
    }
}
