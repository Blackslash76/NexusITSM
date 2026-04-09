namespace NexusITSM.Services;

public static class ExportEndpoints
{
    public static void MapExportEndpoints(this WebApplication app)
    {
        app.MapGet("/api/export/tickets/csv", async (ExportService export) =>
        {
            var bytes = await export.ExportTicketsCsvAsync();
            return Results.File(bytes, "text/csv", $"nexus-tickets-{DateTime.UtcNow:yyyyMMdd}.csv");
        });

        app.MapGet("/api/export/tickets/pdf", async (ExportService export) =>
        {
            var bytes = await export.ExportTicketsPdfAsync();
            return Results.File(bytes, "application/pdf", $"nexus-report-{DateTime.UtcNow:yyyyMMdd}.pdf");
        });
    }
}
