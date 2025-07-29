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
        WeekOfYear = ISOWeek.GetWeekOfYear(Date.ToDateTime(new TimeOnly()));
    }

    public Guid MeetingScheduleId { get; set; } = Guid.CreateVersion7();
    public DateOnly Date { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int WeekOfYear { get; set; }
    public Shared.Enums.MeetingType MeetingType { get; set; }
    
    public ICollection<ResponsibilityAssignment> ResponsibilityAssignments { get; set; } = [];
}