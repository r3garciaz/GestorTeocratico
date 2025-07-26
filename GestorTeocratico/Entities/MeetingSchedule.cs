using System.Globalization;

namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a schedule for a meeting, including date, type, congregation, and assignments.
/// </summary>
public class MeetingSchedule
{
    public MeetingSchedule()
    {
        Month = Date.Month;
        Year = Date.Year;
        WeekOfYear = new GregorianCalendar()
            .GetWeekOfYear(Date.ToDateTime(TimeOnly.MinValue),
                CalendarWeekRule.FirstFullWeek,
                DayOfWeek.Monday);

    }

    public Guid MeetingScheduleId { get; set; } = Guid.CreateVersion7();
    public required Guid MeetingTypeId { get; set; }
    public required DateOnly Date { get; set; }
    public required int Month { get; set; }
    public required int Year { get; set; }
    public required int WeekOfYear { get; set; }

    public required MeetingType MeetingType { get; set; }
    public ICollection<ResponsibilityAssignment> ResponsibilityAssignments { get; set; } = [];
}