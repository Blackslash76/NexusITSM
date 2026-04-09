using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusITSM.Data;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1").RequireAuthorization();

        // ─── Tickets ────────────────────────────────────
        api.MapGet("/tickets", async (AppDbContext db, [FromQuery] string? status, [FromQuery] string? priority, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = db.Tickets
                .Include(t => t.Assignee)
                .Include(t => t.Group)
                .AsQueryable();

            if (Enum.TryParse<TicketStatus>(status, out var s))
                query = query.Where(t => t.Status == s);
            if (Enum.TryParse<TicketPriority>(priority, out var p))
                query = query.Where(t => t.Priority == p);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * size).Take(size)
                .Select(t => new
                {
                    t.Id, t.Title, Status = t.Status.ToString(), Priority = t.Priority.ToString(),
                    Category = t.Category.ToString(), t.Requester, t.Department,
                    Assignee = t.Assignee != null ? t.Assignee.FullName : null,
                    Group = t.Group != null ? t.Group.Name : null,
                    t.SlaPercent, SlaStatus = t.SlaStatus.ToString(),
                    t.CreatedAt, t.ResolvedAt
                })
                .ToListAsync();

            return Results.Ok(new { total, page, size, items });
        });

        api.MapGet("/tickets/{id}", async (AppDbContext db, string id) =>
        {
            var ticket = await db.Tickets
                .Include(t => t.Assignee).Include(t => t.Group).Include(t => t.Timeline)
                .FirstOrDefaultAsync(t => t.Id == id);
            return ticket is null ? Results.NotFound() : Results.Ok(ticket);
        });

        api.MapPost("/tickets", async (AppDbContext db, TicketService svc, [FromBody] CreateTicketRequest req) =>
        {
            var ticket = await svc.CreateAsync(new Ticket
            {
                Title = req.Title, Priority = req.Priority, Category = req.Category,
                Requester = req.Requester, Department = req.Department,
                Description = req.Description, GroupId = req.GroupId, Tags = req.Tags ?? []
            });
            return Results.Created($"/api/v1/tickets/{ticket.Id}", new { ticket.Id, ticket.Title, ticket.CreatedAt });
        });

        api.MapPut("/tickets/{id}/status", async (TicketService svc, string id, [FromBody] StatusChangeRequest req) =>
        {
            await svc.UpdateStatusAsync(id, req.Status, req.Actor ?? "API");
            return Results.Ok(new { id, status = req.Status.ToString() });
        });

        api.MapPost("/tickets/{id}/escalate", async (TicketService svc, string id) =>
        {
            await svc.EscalateAsync(id, "API");
            return Results.Ok(new { id, escalated = true });
        });

        api.MapPost("/tickets/{id}/notes", async (TicketService svc, string id, [FromBody] NoteRequest req) =>
        {
            await svc.AddNoteAsync(id, req.Actor ?? "API", req.Message);
            return Results.Ok(new { id, noted = true });
        });

        // ─── Problems ───────────────────────────────────
        api.MapGet("/problems", async (AppDbContext db) =>
            await db.Problems.Include(p => p.Assignee).Include(p => p.Incidents)
                .Select(p => new { p.Id, p.Title, Status = p.Status.ToString(), Priority = p.Priority.ToString(), p.IsKnownError, IncidentCount = p.Incidents.Count, p.CreatedAt })
                .ToListAsync());

        // ─── Changes ────────────────────────────────────
        api.MapGet("/changes", async (AppDbContext db) =>
            await db.Changes.Include(c => c.Assignee)
                .Select(c => new { c.Id, c.Title, Type = c.Type.ToString(), Status = c.Status.ToString(), c.Risk, c.Impact, c.CabDate, c.CreatedAt })
                .ToListAsync());

        // ─── CMDB ───────────────────────────────────────
        api.MapGet("/cmdb", async (AppDbContext db, [FromQuery] string? type, [FromQuery] string? status) =>
        {
            var query = db.ConfigurationItems.AsQueryable();
            if (!string.IsNullOrEmpty(type)) query = query.Where(c => c.Type == type);
            if (Enum.TryParse<CIStatus>(status, out var st)) query = query.Where(c => c.Status == st);
            return await query.OrderBy(c => c.Name).ToListAsync();
        });

        // ─── Groups ─────────────────────────────────────
        api.MapGet("/groups", async (AppDbContext db) =>
            await db.SupportGroups.Include(g => g.Members)
                .Select(g => new { g.Id, g.Name, Level = g.Level.ToString(), g.Color, g.Icon, MemberCount = g.Members.Count })
                .ToListAsync());

        // ─── Stats ──────────────────────────────────────
        api.MapGet("/stats", async (AppDbContext db) =>
        {
            var tickets = await db.Tickets.ToListAsync();
            var active = tickets.Where(t => t.Status != TicketStatus.Closed).ToList();
            return new
            {
                TotalTickets = tickets.Count,
                Open = active.Count(t => t.Status == TicketStatus.Open),
                InProgress = active.Count(t => t.Status == TicketStatus.InProgress),
                Escalated = active.Count(t => t.Status == TicketStatus.Escalated),
                Resolved = tickets.Count(t => t.Status == TicketStatus.Resolved),
                SlaBreaches = active.Count(t => t.SlaStatus == SlaStatus.Breach),
                SlaCompliance = active.Count > 0 ? 100 - (active.Count(t => t.SlaStatus == SlaStatus.Breach) * 100 / active.Count) : 100,
                Problems = await db.Problems.CountAsync(),
                Changes = await db.Changes.CountAsync(),
                CIs = await db.ConfigurationItems.CountAsync(),
            };
        });

        // ─── Audit Log ──────────────────────────────────
        api.MapGet("/audit", async (AppDbContext db, [FromQuery] int page = 1, [FromQuery] int size = 50) =>
        {
            var total = await db.AuditLogs.CountAsync();
            var items = await db.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * size).Take(size)
                .ToListAsync();
            return new { total, page, size, items };
        });

        // ─── Webhooks ───────────────────────────────────
        api.MapGet("/webhooks", async (AppDbContext db) => await db.WebhookConfigs.ToListAsync());

        api.MapPost("/webhooks", async (AppDbContext db, [FromBody] WebhookConfig hook) =>
        {
            hook.Id = Guid.NewGuid().ToString();
            hook.CreatedAt = DateTime.UtcNow;
            db.WebhookConfigs.Add(hook);
            await db.SaveChangesAsync();
            return Results.Created($"/api/v1/webhooks/{hook.Id}", hook);
        });

        api.MapDelete("/webhooks/{id}", async (AppDbContext db, string id) =>
        {
            var hook = await db.WebhookConfigs.FindAsync(id);
            if (hook == null) return Results.NotFound();
            db.WebhookConfigs.Remove(hook);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    public record CreateTicketRequest(string Title, TicketPriority Priority, TicketCategory Category,
        string? Requester, string? Department, string? Description, string? GroupId, List<string>? Tags);

    public record StatusChangeRequest(TicketStatus Status, string? Actor);

    public record NoteRequest(string Message, string? Actor);
}
