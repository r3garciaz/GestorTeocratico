namespace GestorTeocratico.Entities;

/// <summary>
/// Represents the assignment of a responsibility to a publisher within a meeting schedule.
/// </summary>
public class ResponsibilityAssignment
{
    public required Guid MeetingScheduleId { get; set; }
    public required Guid ResponsibilityId { get; set; }
    public required Guid PublisherId { get; set; }

    // Navigation properties are non-required so the join row can be created from foreign keys
    // alone (EF still treats the relationships as required because the FKs are non-nullable).
    public MeetingSchedule MeetingSchedule { get; set; } = null!;
    public Responsibility Responsibility { get; set; } = null!;
    public Publisher Publisher { get; set; } = null!;
}