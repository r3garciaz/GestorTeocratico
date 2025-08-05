using GestorTeocratico.Features.PdfExport;

namespace GestorTeocratico.Features.MeetingSchedules.Endpoints;

internal static class MeetingSchedulesEndpoints
{
    internal static IEndpointRouteBuilder MapMeetingSchedulesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var meetingSchedulesGroup = endpoints.MapGroup("/meeting-schedules");
        
        // PDF Export API endpoints
        meetingSchedulesGroup.MapGet("/monthly-schedule/{year:int}/{month:int}", async (
                int year, 
                int month, 
                IPdfExportService pdfExportService,
                ILogger<IEndpointRouteBuilder> logger) =>
            {
                try
                {
                    logger.LogInformation("PDF export requested for {Month}/{Year}", month, year);
        
                    if (month is < 1 or > 12)
                    {
                        return Results.BadRequest("Month must be between 1 and 12");
                    }
        
                    if (year is < 2020 or > 2030)
                    {
                        return Results.BadRequest("Year must be between 2020 and 2030");
                    }
        
                    var pdfBytes = await pdfExportService.GenerateMonthlySchedulePdfAsync(month, year);
                    var monthNames = new[]
                    {
                        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                    };
        
                    var fileName = $"Programacion_{monthNames[month]}_{year}.pdf";
        
                    logger.LogInformation("PDF generated successfully: {FileName}, size: {Size} bytes", fileName, pdfBytes.Length);
        
                    return Results.File(pdfBytes, "application/pdf", fileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error generating PDF for {Month}/{Year}", month, year);
                    return Results.Problem($"Error generating PDF: {ex.Message}");
                }
            })
            .WithName("GenerateMonthlySchedulePdf")
            .WithSummary("Generate monthly schedule PDF")
            .Produces(200)
            .Produces(400);
        
        return endpoints;
    }
}