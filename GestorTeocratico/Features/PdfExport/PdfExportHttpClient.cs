namespace GestorTeocratico.Features.PdfExport;

public class PdfExportHttpClient
{
    private readonly HttpClient _httpClient;

    public PdfExportHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> DownloadMonthlySchedulePdfAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        // Keep the path in sync with MeetingSchedulesEndpoints.MapMeetingSchedulesEndpoints.
        var response = await _httpClient.GetAsync(
            $"/api/meeting-schedules/monthly-schedule/{year}/{month}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
