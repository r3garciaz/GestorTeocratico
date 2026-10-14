using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record CopyWeekAssignmentsCommand(int SourceWeek, int SourceYear, int TargetWeek, int TargetYear);

/// <summary>Command: copy/synchronize the assignments of one week onto another.</summary>
public sealed class CopyWeekAssignmentsHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<CopyWeekAssignmentsHandler> logger)
{
    public async Task HandleAsync(CopyWeekAssignmentsCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Make sure the target week exists before copying.
        await MeetingScheduleReader.EnsureWeekAsync(context, command.TargetWeek, command.TargetYear, cancellationToken);

        var source = await MeetingScheduleReader.ByWeekAsync(context, command.SourceWeek, command.SourceYear, cancellationToken);
        var target = await MeetingScheduleReader.ByWeekAsync(context, command.TargetWeek, command.TargetYear, cancellationToken);

        foreach (var sourceSchedule in source)
        {
            var targetSchedule = target.FirstOrDefault(t => t.MeetingType == sourceSchedule.MeetingType);
            if (targetSchedule is null)
            {
                continue;
            }

            // Use only foreign keys: the DTOs are detached, so assigning navigations would make
            // EF treat the whole graph as Added and cause duplicate key errors.
            var desired = sourceSchedule.Assignments
                .Select(a => (a.ResponsibilityId, a.PublisherId))
                .Distinct()
                .ToList();

            var existing = await context.ResponsibilityAssignments
                .Where(ra => ra.MeetingScheduleId == targetSchedule.Id)
                .ToListAsync(cancellationToken);

            var existingKeys = existing
                .Select(a => (a.ResponsibilityId, a.PublisherId))
                .ToHashSet();

            context.ResponsibilityAssignments.RemoveRange(
                existing.Where(a => !desired.Contains((a.ResponsibilityId, a.PublisherId))));

            foreach (var (responsibilityId, publisherId) in desired)
            {
                if (existingKeys.Contains((responsibilityId, publisherId)))
                {
                    continue;
                }

                context.ResponsibilityAssignments.Add(new ResponsibilityAssignment
                {
                    MeetingScheduleId = targetSchedule.Id,
                    ResponsibilityId = responsibilityId,
                    PublisherId = publisherId
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Copied assignments from week {SourceWeek}/{SourceYear} to week {TargetWeek}/{TargetYear}.",
            command.SourceWeek, command.SourceYear, command.TargetWeek, command.TargetYear);
    }
}
