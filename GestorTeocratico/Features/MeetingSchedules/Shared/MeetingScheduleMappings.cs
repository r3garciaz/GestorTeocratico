using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.MeetingSchedules;

internal static class MeetingScheduleMappings
{
    public static MeetingScheduleDto ToDto(this MeetingSchedule schedule) => new(
        schedule.MeetingScheduleId,
        schedule.Date,
        schedule.Month,
        schedule.Year,
        schedule.WeekOfYear,
        schedule.MeetingType,
        schedule.ResponsibilityAssignments.Select(a => a.ToDto()).ToList());

    public static AssignmentDto ToDto(this ResponsibilityAssignment assignment)
    {
        var publisher = assignment.Publisher;
        return new AssignmentDto(
            assignment.MeetingScheduleId,
            assignment.ResponsibilityId,
            assignment.PublisherId,
            publisher is null ? string.Empty : PublisherFullName(publisher),
            publisher is null ? string.Empty : PublisherShortName(publisher));
    }

    public static string PublisherFullName(Publisher publisher)
    {
        var name = publisher.FirstName;
        if (!string.IsNullOrEmpty(publisher.LastName)) name += $" {publisher.LastName}";
        return name;
    }

    public static string PublisherShortName(Publisher publisher)
    {
        var shortName = $"{publisher.FirstName[0]}.{publisher.LastName}";
        if (!string.IsNullOrEmpty(publisher.MotherLastName))
        {
            shortName += $".{publisher.MotherLastName[0]}";
        }

        return shortName;
    }
}
