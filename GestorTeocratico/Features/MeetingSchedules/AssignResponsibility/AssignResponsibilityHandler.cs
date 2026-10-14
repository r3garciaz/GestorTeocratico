using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record AssignResponsibilityCommand(Guid MeetingScheduleId, Guid ResponsibilityId, Guid PublisherId);

/// <summary>
/// Command: assign a publisher to a responsibility of a meeting. The primary key is
/// (MeetingScheduleId, ResponsibilityId), so there is at most one publisher per responsibility;
/// assigning a new publisher replaces the previous one. Returns false when an entity is missing.
/// </summary>
public sealed class AssignResponsibilityHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<AssignResponsibilityHandler> logger)
{
    public async Task<bool> HandleAsync(AssignResponsibilityCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var scheduleExists = await context.MeetingSchedules
            .AnyAsync(ms => ms.MeetingScheduleId == command.MeetingScheduleId, cancellationToken);
        var publisherExists = await context.Publishers
            .AnyAsync(p => p.PublisherId == command.PublisherId, cancellationToken);
        var responsibilityExists = await context.Responsibilities
            .AnyAsync(r => r.ResponsibilityId == command.ResponsibilityId, cancellationToken);

        if (!scheduleExists || !publisherExists || !responsibilityExists)
        {
            logger.LogWarning(
                "Cannot assign responsibility {ResponsibilityId} to publisher {PublisherId} on schedule {MeetingScheduleId}: entity not found.",
                command.ResponsibilityId, command.PublisherId, command.MeetingScheduleId);
            return false;
        }

        var existing = await context.ResponsibilityAssignments
            .FirstOrDefaultAsync(ra => ra.MeetingScheduleId == command.MeetingScheduleId
                                       && ra.ResponsibilityId == command.ResponsibilityId, cancellationToken);

        if (existing is not null)
        {
            // Update in place to avoid tracking two entities with the same key at once.
            existing.PublisherId = command.PublisherId;
        }
        else
        {
            context.ResponsibilityAssignments.Add(new ResponsibilityAssignment
            {
                MeetingScheduleId = command.MeetingScheduleId,
                ResponsibilityId = command.ResponsibilityId,
                PublisherId = command.PublisherId
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
