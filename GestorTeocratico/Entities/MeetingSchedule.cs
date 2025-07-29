using System.Globalization;

namespace GestorTeocratico.Entities;

/// <summary>
/// Represents a schedule for a meeting, including date, type, congregation, and assignments.
/// </summary>
public class MeetingSchedule
{
    private DateOnly _date = DateOnly.FromDateTime(DateTime.Today);

    public MeetingSchedule()
    {
        Date = _date;
        Month = _date.Month;
        Year = _date.Year;
        WeekOfYear = ISOWeek.GetWeekOfYear(_date.ToDateTime(new TimeOnly()));
    }

    public Guid MeetingScheduleId { get; set; } = Guid.CreateVersion7();
    
    public DateOnly Date 
    { 
        get => _date;
        set
        {
            _date = value;
            Month = value.Month;
            Year = value.Year;
            WeekOfYear = ISOWeek.GetWeekOfYear(value.ToDateTime(new TimeOnly()));
        }
    }
    
    public int Month { get; set; }
    public int Year { get; set; }
    public int WeekOfYear { get; set; }
    public Shared.Enums.MeetingType MeetingType { get; set; }
    
    public ICollection<ResponsibilityAssignment> ResponsibilityAssignments { get; set; } = [];
}