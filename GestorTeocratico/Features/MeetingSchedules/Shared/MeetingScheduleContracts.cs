using GestorTeocratico.Shared.Enums;

namespace GestorTeocratico.Features.MeetingSchedules;

/// <summary>Read model for a single responsibility assignment within a meeting.</summary>
public sealed record AssignmentDto(
    Guid MeetingScheduleId,
    Guid ResponsibilityId,
    Guid PublisherId,
    string PublisherName,
    string PublisherShortName);

/// <summary>Read model for a meeting schedule and its assignments.</summary>
public sealed record MeetingScheduleDto(
    Guid Id,
    DateOnly Date,
    int Month,
    int Year,
    int WeekOfYear,
    MeetingType MeetingType,
    IReadOnlyList<AssignmentDto> Assignments);
