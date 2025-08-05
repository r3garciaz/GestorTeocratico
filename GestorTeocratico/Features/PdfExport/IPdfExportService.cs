using GestorTeocratico.Features.PdfExport.Models;

namespace GestorTeocratico.Features.PdfExport;

public interface IPdfExportService
{
    Task<byte[]> GenerateMonthlySchedulePdfAsync(int month, int year);
    Task ShowInCompanionAsync(int month, int year);
}
