namespace GestorTeocratico.Features.PdfExport;

public class PdfExportHttpClient
{
    private readonly HttpClient _httpClient;

    public PdfExportHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> DownloadMonthlySchedulePdfAsync(int year, int month)
    {
        var response = await _httpClient.GetAsync($"/api/pdf/monthly-schedule/{year}/{month}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
