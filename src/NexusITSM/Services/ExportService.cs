using System.Text;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NexusITSM.Data;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public class ExportService
{
    private readonly AppDbContext _db;

    public ExportService(AppDbContext db) => _db = db;

    public async Task<byte[]> ExportTicketsCsvAsync()
    {
        var tickets = await _db.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Group)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID;Titolo;Priorità;Status;Categoria;Richiedente;Reparto;Assegnato;Gruppo;SLA%;SLA Status;Creato;Risolto");

        foreach (var t in tickets)
        {
            sb.AppendLine(string.Join(";",
                t.Id,
                EscapeCsv(t.Title),
                t.Priority,
                t.Status,
                t.Category,
                EscapeCsv(t.Requester ?? ""),
                EscapeCsv(t.Department ?? ""),
                EscapeCsv(t.Assignee?.FullName ?? ""),
                EscapeCsv(t.Group?.Name ?? ""),
                t.SlaPercent,
                t.SlaStatus,
                t.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                t.ResolvedAt?.ToString("yyyy-MM-dd HH:mm") ?? ""
            ));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportTicketsPdfAsync()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var tickets = await _db.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Group)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var open = tickets.Count(t => t.Status == TicketStatus.Open);
        var inProgress = tickets.Count(t => t.Status == TicketStatus.InProgress);
        var breaches = tickets.Count(t => t.SlaStatus == SlaStatus.Breach && t.Status != TicketStatus.Closed);
        var resolved = tickets.Count(t => t.Status == TicketStatus.Resolved);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("NexusITSM — Report Ticket").Bold().FontSize(16);
                        row.ConstantItem(200).AlignRight().Text(now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.AutoItem().PaddingRight(15).Text($"Aperti: {open}").FontSize(10).Bold();
                        row.AutoItem().PaddingRight(15).Text($"In Progress: {inProgress}").FontSize(10);
                        row.AutoItem().PaddingRight(15).Text($"SLA Breach: {breaches}").FontSize(10).FontColor(Colors.Red.Medium);
                        row.AutoItem().Text($"Risolti: {resolved}").FontSize(10).FontColor(Colors.Green.Medium);
                    });
                    col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(65);   // ID
                        cols.RelativeColumn(3);     // Titolo
                        cols.ConstantColumn(55);    // Priorità
                        cols.ConstantColumn(65);    // Status
                        cols.ConstantColumn(65);    // Categoria
                        cols.RelativeColumn(1.5f);  // Assegnato
                        cols.RelativeColumn(1.5f);  // Gruppo
                        cols.ConstantColumn(40);    // SLA%
                        cols.ConstantColumn(85);    // Creato
                    });

                    // Header
                    table.Header(header =>
                    {
                        foreach (var h in new[] { "ID", "Titolo", "Priorità", "Status", "Categoria", "Assegnato", "Gruppo", "SLA", "Creato" })
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).Text(h).Bold().FontSize(8);
                        }
                    });

                    // Data rows
                    foreach (var t in tickets)
                    {
                        var bg = t.SlaStatus == SlaStatus.Breach ? Colors.Red.Lighten5 : Colors.White;

                        table.Cell().Background(bg).Padding(3).Text(t.Id).FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.Title).FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.Priority.ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.Status.ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.Category.ToString()).FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.Assignee?.FullName ?? "—").FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.Group?.Name ?? "—").FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text($"{t.SlaPercent}%").FontSize(8);
                        table.Cell().Background(bg).Padding(3).Text(t.CreatedAt.ToString("dd/MM HH:mm")).FontSize(8);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" di ");
                    x.TotalPages();
                    x.Span($"  —  Generato da NexusITSM il {now:dd/MM/yyyy}");
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string EscapeCsv(string value) =>
        value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
