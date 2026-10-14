using GestorTeocratico.Features.MeetingSchedules;
using GestorTeocratico.Features.Publishers;
using GestorTeocratico.Features.Responsibilities;
using GestorTeocratico.Shared.Enums;
using GestorTeocratico.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GestorTeocratico.Tests.Features.MeetingSchedules;

public class MeetingScheduleHandlersTests
{
    private static AssignResponsibilityHandler Assign(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<AssignResponsibilityHandler>.Instance);

    private static CreatePublisherHandler CreatePublisher(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreatePublisherHandler>.Instance);

    private static CreateResponsibilityHandler CreateResponsibility(SqliteTestDbContextFactory factory)
        => new(factory, NullLogger<CreateResponsibilityHandler>.Instance);

    [Fact]
    public async Task AssignHandler_replaces_previous_assignment_for_same_responsibility()
    {
        using var factory = new SqliteTestDbContextFactory();
        var week = new GetOrCreateWeekSchedulesHandler(factory);
        var assign = Assign(factory);
        var getAssignments = new GetAssignmentsByMeetingScheduleHandler(factory);
        var createPublisher = CreatePublisher(factory);
        var createResponsibility = CreateResponsibility(factory);

        var schedules = await week.HandleAsync(new GetOrCreateWeekSchedulesQuery(5, 2026));
        var midweek = schedules.First(s => s.MeetingType == MeetingType.Midweek);

        var firstId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Luis", "Uno", null, null, null, Gender.Male, null, [], []));
        var secondId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Mario", "Dos", null, null, null, Gender.Male, null, [], []));
        var responsibilityId = await createResponsibility.HandleAsync(
            new CreateResponsibilityCommand("Presidente", null, null, []));

        await assign.HandleAsync(new AssignResponsibilityCommand(midweek.Id, responsibilityId, firstId));
        await assign.HandleAsync(new AssignResponsibilityCommand(midweek.Id, responsibilityId, secondId));

        var assignments = await getAssignments.HandleAsync(new GetAssignmentsByMeetingScheduleQuery(midweek.Id));

        Assert.Single(assignments);
        Assert.Equal(secondId, assignments[0].PublisherId);
    }

    [Fact]
    public async Task AssignHandler_returns_false_when_entities_missing()
    {
        using var factory = new SqliteTestDbContextFactory();
        var assign = Assign(factory);

        var result = await assign.HandleAsync(new AssignResponsibilityCommand(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7()));

        Assert.False(result);
    }

    [Fact]
    public async Task UnassignHandler_removes_assignment()
    {
        using var factory = new SqliteTestDbContextFactory();
        var week = new GetOrCreateWeekSchedulesHandler(factory);
        var assign = Assign(factory);
        var unassign = new UnassignResponsibilityHandler(factory);
        var getAssignments = new GetAssignmentsByMeetingScheduleHandler(factory);
        var createPublisher = CreatePublisher(factory);
        var createResponsibility = CreateResponsibility(factory);

        var schedules = await week.HandleAsync(new GetOrCreateWeekSchedulesQuery(6, 2026));
        var midweek = schedules.First(s => s.MeetingType == MeetingType.Midweek);

        var publisherId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Ana", "Tres", null, null, null, Gender.Female, null, [], []));
        var responsibilityId = await createResponsibility.HandleAsync(
            new CreateResponsibilityCommand("Oración", null, null, []));

        await assign.HandleAsync(new AssignResponsibilityCommand(midweek.Id, responsibilityId, publisherId));
        Assert.True(await unassign.HandleAsync(new UnassignResponsibilityCommand(midweek.Id, responsibilityId, publisherId)));

        var assignments = await getAssignments.HandleAsync(new GetAssignmentsByMeetingScheduleQuery(midweek.Id));
        Assert.Empty(assignments);
    }

    [Fact]
    public async Task CopyWeekHandler_copies_assignments_to_target_week()
    {
        using var factory = new SqliteTestDbContextFactory();
        var week = new GetOrCreateWeekSchedulesHandler(factory);
        var assign = Assign(factory);
        var copy = new CopyWeekAssignmentsHandler(factory, NullLogger<CopyWeekAssignmentsHandler>.Instance);
        var getAssignments = new GetAssignmentsByMeetingScheduleHandler(factory);
        var createPublisher = CreatePublisher(factory);
        var createResponsibility = CreateResponsibility(factory);

        var sourceSchedules = await week.HandleAsync(new GetOrCreateWeekSchedulesQuery(10, 2026));
        var source = sourceSchedules.First(s => s.MeetingType == MeetingType.Midweek);

        var publisherId = await createPublisher.HandleAsync(new CreatePublisherCommand(
            "Rosa", "Cuatro", null, null, null, Gender.Female, null, [], []));
        var responsibilityId = await createResponsibility.HandleAsync(
            new CreateResponsibilityCommand("Micrófonos", null, null, []));

        await assign.HandleAsync(new AssignResponsibilityCommand(source.Id, responsibilityId, publisherId));

        await copy.HandleAsync(new CopyWeekAssignmentsCommand(10, 2026, 11, 2026));

        var targetSchedules = await week.HandleAsync(new GetOrCreateWeekSchedulesQuery(11, 2026));
        var target = targetSchedules.First(s => s.MeetingType == MeetingType.Midweek);

        var targetAssignments = await getAssignments.HandleAsync(new GetAssignmentsByMeetingScheduleQuery(target.Id));
        Assert.Single(targetAssignments);
        Assert.Equal(publisherId, targetAssignments[0].PublisherId);
    }
}
