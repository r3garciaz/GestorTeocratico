namespace GestorTeocratico.Entities;

/// <summary>
/// Represents the assignment of a responsibility to a publisher within a meeting schedule.
/// </summary>
public class ResponsibilityAssignment
{
    public required Guid MeetingScheduleId { get; set; }
    public required Guid ResponsibilityId { get; set; }
    public required Guid PublisherId { get; set; }

    public required MeetingSchedule MeetingSchedule { get; set; }
    public required Responsibility Responsibility { get; set; }
    public required Publisher Publisher { get; set; }
}