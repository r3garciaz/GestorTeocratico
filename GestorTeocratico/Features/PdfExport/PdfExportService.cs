using GestorTeocratico.Features.Congregations;
using GestorTeocratico.Features.MeetingSchedules;
using GestorTeocratico.Features.Responsibilities;
using GestorTeocratico.Features.PdfExport.Models;
using GestorTeocratico.Shared.Enums;
using QuestPDF.Fluent;
using System.Globalization;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Companion;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestorTeocratico.Features.PdfExport;

public class PdfExportService : IPdfExportService
{
    private readonly IMeetingScheduleService _meetingScheduleService;
    private readonly IResponsibilityService _responsibilityService;
    private readonly ICongregationService _congregationService;
    private readonly ILogger<PdfExportService> _logger;

    public PdfExportService(
        IMeetingScheduleService meetingScheduleService,
        IResponsibilityService responsibilityService,
        ICongregationService congregationService,
        ILogger<PdfExportService> logger)
    {
        _meetingScheduleService = meetingScheduleService;
        _responsibilityService = responsibilityService;
        _congregationService = congregationService;
        _logger = logger;
    }

    public async Task<byte[]> GenerateMonthlySchedulePdfAsync(int month, int year)
    {
        try
        {
            _logger.LogInformation("Generating PDF for month {Month}, year {Year}", month, year);
            
            var model = await GetMonthlyScheduleDataAsync(month, year);
            
            _logger.LogInformation("Data retrieved: {ScheduleCount} schedules, {ResponsibilityCount} responsibilities", 
                model.ScheduleRows.Count, model.ResponsibilityColumns.Count);
            
            var document = new MonthlyScheduleDocument(model);
            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("PDF generated successfully, size: {Size} bytes", pdfBytes.Length);
            
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for month {Month}, year {Year}", month, year);
            throw;
        }
    }
    
    public async Task ShowInCompanionAsync(int month, int year)
    {
        try
        {
            _logger.LogInformation("Generating Companion PDF for month {Month}, year {Year}", month, year);
            
            var model = await GetMonthlyScheduleDataAsync(month, year);
            
            _logger.LogInformation("Data retrieved: {ScheduleCount} schedules, {ResponsibilityCount} responsibilities", 
                model.ScheduleRows.Count, model.ResponsibilityColumns.Count);
            
            var document = new MonthlyScheduleDocument(model);
            await document.ShowInCompanionAsync();
            _logger.LogInformation("Companion PDF generated successfully");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for month {Month}, year {Year}", month, year);
            throw;
        }
    }

    private async Task<MonthlySchedulePdfModel> GetMonthlyScheduleDataAsync(int month, int year)
    {
        // Obtener datos del mes
        var meetingSchedules = await _meetingScheduleService.GetByMonthAsync(month, year);
        var responsibilities = await _responsibilityService.GetAllAsync();
        var congregations = await _congregationService.GetAllAsync();
        var congregation = await congregations.FirstOrDefaultAsync();
        

        var culture = new CultureInfo("es-ES");
        var monthName = culture.DateTimeFormat.GetMonthName(month);

        // Crear modelo
        var model = new MonthlySchedulePdfModel
        {
            Month = month,
            Year = year,
            MonthName = $"{monthName} {year}",
            ResponsibilityColumns = responsibilities.Select(r => new ResponsibilityColumnModel
            {
                ResponsibilityId = r.ResponsibilityId,
                Name = r.Name,
                DepartmentName = r.Department?.Name
            }).ToList()
        };

        // Procesar horarios por fecha
        var schedulesByDate = meetingSchedules
            .GroupBy(ms => ms.Date)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var dateGroup in schedulesByDate)
        {
            // var date = dateGroup.Key;
            var schedulesForDate = dateGroup.OrderBy(ms => ms.MeetingType).ToList();

            foreach (var schedule in schedulesForDate)
            {
                var date = GetDayOfMeeting(congregation, schedule);
                var assignments = new Dictionary<Guid, string>();

                // Mapear asignaciones
                foreach (var assignment in schedule.ResponsibilityAssignments)
                {
                    var publisherName = GetPublisherShortName(assignment.Publisher);
                    assignments[assignment.ResponsibilityId] = publisherName;
                }
                
                

                var scheduleRow = new ScheduleRowModel
                {
                    Date = date,
                    MeetingType = schedule.MeetingType,
                    DateDisplay = date.ToString("dd/MM"),
                    MeetingTypeDisplay = GetMeetingTypeDisplay(schedule.MeetingType),
                    Assignments = assignments
                };

                model.ScheduleRows.Add(scheduleRow);
            }
        }

        return model;
    }

    private static DateOnly GetDayOfMeeting(
        Congregation congregation,
        MeetingSchedule meetingSchedule)
    {
        DayOfWeek meetingDay = meetingSchedule.MeetingType switch
        {
            MeetingType.Midweek => (meetingSchedule.Year % 2 == 0)
                ? congregation.MidweekMeetingDayEvenYear
                : congregation.MidweekMeetingDayOddYear,
            MeetingType.Weekend => (meetingSchedule.Year % 2 == 0)
                ? congregation.WeekendMeetingDayEvenYear
                : congregation.WeekendMeetingDayOddYear,
            _ => throw new ArgumentOutOfRangeException()
        };

        int offset = ((int)meetingDay - (int)DayOfWeek.Monday + 7) % 7;
        return meetingSchedule.Date.AddDays(offset);
    }

    private static string GetPublisherFullName(Entities.Publisher? publisher)
    {
        if (publisher == null) return string.Empty;
        
        var fullName = publisher.FirstName;
        if (!string.IsNullOrEmpty(publisher.LastName))
            fullName += $" {publisher.LastName}";
        
        return fullName;
    }

    private static string GetPublisherShortName(Entities.Publisher? publisher)
    {
        if (publisher == null) return string.Empty;
        var shortName = $"{publisher.FirstName[0]}.{publisher.LastName}";
        if (!string.IsNullOrEmpty(publisher.MotherLastName))
        {
            shortName += $".{publisher.MotherLastName[0]}";
        }
        return shortName;
    }

    private string GetMeetingTypeDisplay(MeetingType meetingType)
    {
        return meetingType switch
        {
            MeetingType.Midweek => "Entre Semana",
            MeetingType.Weekend => "Fin de Semana",
            _ => meetingType.ToString()
        };
    }
}

// Incluir la clase del documento en el mismo namespace para simplicidad
public class MonthlyScheduleDocument(MonthlySchedulePdfModel model) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            // Espacio reservado para logo (60px de ancho)
            row.ConstantItem(60).Height(50).Border(1).BorderColor(Colors.Grey.Lighten3);
            
            row.RelativeItem().Column(column =>
            {
                column.Item().AlignCenter().Text($"PROGRAMACIÓN DE RESPONSABILIDADES")
                    .FontSize(16).Bold().FontColor(Colors.Grey.Darken2);
                
                column.Item().AlignCenter().Text($"{model.MonthName.ToUpper()}")
                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                
                column.Item().AlignCenter().Text($"Generado el {model.GeneratedAt:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
            
            // Espacio simétrico al logo
            row.ConstantItem(60);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Table(table =>
        {
            //Definir columnas
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60); // Fecha
                columns.ConstantColumn(60); // Tipo reunión
                
                // Columnas dinámicas para responsabilidades
                foreach (var responsibility in model.ResponsibilityColumns)
                {
                    columns.RelativeColumn();
                }
            });

            // Header de la tabla
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).AlignCenter().AlignMiddle()
                    .Text("FECHA").Bold().FontSize(8).FontColor(Colors.White);
            
                header.Cell().Element(CellStyle).AlignCenter().AlignMiddle()
                    .Text("REUNIÓN").Bold().FontSize(8).FontColor(Colors.White);
            
                foreach (var responsibility in model.ResponsibilityColumns)
                {
                    header.Cell().Element(CellStyle).AlignCenter().AlignMiddle()
                        .Column(column =>
                        {
                            column.Item().Text(responsibility.Name.ToUpper())
                                .Bold().FontSize(7).FontColor(Colors.White);
                            
                            if (!string.IsNullOrEmpty(responsibility.DepartmentName))
                            {
                                column.Item().Text($"({responsibility.DepartmentName})")
                                    .FontSize(6).FontColor(Colors.Grey.Lighten3);
                            }
                        });
                }
            
                static IContainer CellStyle(IContainer container) => container
                    .Border(0.5f).BorderColor(Colors.Grey.Darken1)
                    .Background(Colors.Blue.Darken2).Padding(4);
            });
            
            // Filas de datos
            foreach (var row in model.ScheduleRows)
            {
                // Celda de fecha
                table.Cell().Element(DataCellStyle).AlignCenter().AlignMiddle()
                    .Column(column =>
                    {
                        column.Item().Text(row.DateDisplay).FontSize(8).Bold();
                        column.Item().Text(GetDayName(row.Date)).FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
            
                // Celda de tipo de reunión
                table.Cell().Element(container =>
                {
                    var backgroundColor = row.MeetingType == MeetingType.Midweek 
                        ? Colors.Blue.Lighten4 
                        : Colors.Green.Lighten4;

                    container
                        .Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Background(backgroundColor).Padding(3).MinHeight(25)
                        .AlignCenter().AlignMiddle()
                        .Text(row.MeetingTypeDisplay).FontSize(8).Bold()
                        .FontColor(row.MeetingType == MeetingType.Midweek ? Colors.Blue.Darken1 : Colors.Green.Darken1);
                });
            
                // Celdas de asignaciones
                foreach (var responsibility in model.ResponsibilityColumns)
                {
                    var assignedPublisher = row.Assignments.GetValueOrDefault(responsibility.ResponsibilityId, string.Empty);
                    
                    table.Cell().Element(DataCellStyle).AlignCenter().AlignMiddle()
                        .Text(assignedPublisher).FontSize(8);
                }
            }

            static IContainer DataCellStyle(IContainer container) => container
                .Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                .Background(Colors.White).Padding(3).MinHeight(25);
        });
    }
    
    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().AlignLeft()
                .Text("Gestor Teocrático").FontSize(8).FontColor(Colors.Grey.Darken1);
                
            row.RelativeItem().AlignCenter()
                .Text($"Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                
            row.RelativeItem().AlignRight()
                .Text($"Total de asignaciones: {GetTotalAssignments()}").FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private string GetDayName(DateOnly date)
    {
        var culture = new CultureInfo("es-ES");
        return culture.DateTimeFormat.GetDayName(date.DayOfWeek).Substring(0, 3).ToUpper();
    }

    private int GetTotalAssignments()
    {
        return model.ScheduleRows
            .SelectMany(row => row.Assignments.Values)
            .Count(assignment => !string.IsNullOrEmpty(assignment));
    }
}
