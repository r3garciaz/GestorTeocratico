using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record UnassignResponsibilityCommand(Guid MeetingScheduleId, Guid ResponsibilityId, Guid PublisherId);

/// <summary>Command: remove a specific assignment. Returns false when it does not exist.</summary>
public sealed class UnassignResponsibilityHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<bool> HandleAsync(UnassignResponsibilityCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var assignment = await context.ResponsibilityAssignments
            .FirstOrDefaultAsync(ra =>
                ra.MeetingScheduleId == command.MeetingScheduleId &&
                ra.ResponsibilityId == command.ResponsibilityId &&
                ra.PublisherId == command.PublisherId, cancellationToken);

        if (assignment is null)
        {
            return false;
        }

        context.ResponsibilityAssignments.Remove(assignment);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
