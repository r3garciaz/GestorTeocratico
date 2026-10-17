namespace GestorTeocratico.Features.Congregations;

/// <summary>Read model returned to the UI. Never exposes EF entities.</summary>
public sealed record CongregationDto(
    Guid Id,
    string Name,
    DayOfWeek MidweekMeetingDayEvenYear,
    DayOfWeek MidweekMeetingDayOddYear,
    DayOfWeek WeekendMeetingDayEvenYear,
    DayOfWeek WeekendMeetingDayOddYear,
    string? Address,
    string? City);

/// <summary>Mutable model bound to the create/edit form.</summary>
public sealed class CongregationFormModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DayOfWeek MidweekMeetingDayEvenYear { get; set; }
    public DayOfWeek MidweekMeetingDayOddYear { get; set; }
    public DayOfWeek WeekendMeetingDayEvenYear { get; set; }
    public DayOfWeek WeekendMeetingDayOddYear { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}
