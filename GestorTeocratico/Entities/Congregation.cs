namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a congregation entity with meeting days, address, and related collections.
/// </summary>
public class Congregation : SoftDeleteEntity
{
    public Guid CongregationId { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public DayOfWeek MidweekMeetingDayEvenYear { get; set; }
    public DayOfWeek MidweekMeetingDayOddYear { get; set; }
    public DayOfWeek WeekendMeetingDayEvenYear { get; set; }
    public DayOfWeek WeekendMeetingDayOddYear { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}