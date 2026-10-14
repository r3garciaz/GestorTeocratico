namespace GestorTeocratico.Features.PdfExport;

public interface IPdfExportService
{
    Task<byte[]> GenerateMonthlySchedulePdfAsync(int month, int year, CancellationToken cancellationToken = default);
    Task ShowInCompanionAsync(int month, int year, CancellationToken cancellationToken = default);
}
