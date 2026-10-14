using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record CopyWeekAssignmentsCommand(int SourceWeek, int SourceYear, int TargetWeek, int TargetYear);

/// <summary>
/// Command: copy/synchronize the assignments of one week onto another. There is at most one
/// publisher per responsibility (primary key is MeetingScheduleId + ResponsibilityId), so the
/// target is synchronized by responsibility.
/// </summary>
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

            // Desired state: responsibility -> publisher (keep one publisher per responsibility).
            var desired = sourceSchedule.Assignments
                .GroupBy(a => a.ResponsibilityId)
                .ToDictionary(g => g.Key, g => g.First().PublisherId);

            var existing = await context.ResponsibilityAssignments
                .Where(ra => ra.MeetingScheduleId == targetSchedule.Id)
                .ToListAsync(cancellationToken);

            // Remove responsibilities that are no longer assigned in the source week.
            context.ResponsibilityAssignments.RemoveRange(
                existing.Where(ra => !desired.ContainsKey(ra.ResponsibilityId)));

            foreach (var (responsibilityId, publisherId) in desired)
            {
                var current = existing.FirstOrDefault(ra => ra.ResponsibilityId == responsibilityId);
                if (current is null)
                {
                    context.ResponsibilityAssignments.Add(new ResponsibilityAssignment
                    {
                        MeetingScheduleId = targetSchedule.Id,
                        ResponsibilityId = responsibilityId,
                        PublisherId = publisherId
                    });
                }
                else if (current.PublisherId != publisherId)
                {
                    // Update in place (same key) instead of remove + add.
                    current.PublisherId = publisherId;
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Copied assignments from week {SourceWeek}/{SourceYear} to week {TargetWeek}/{TargetYear}.",
            command.SourceWeek, command.SourceYear, command.TargetWeek, command.TargetYear);
    }
}
