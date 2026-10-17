using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record GetAssignmentsByMeetingScheduleQuery(Guid MeetingScheduleId);

/// <summary>Query: assignments of a single meeting schedule, ordered by publisher name.</summary>
public sealed class GetAssignmentsByMeetingScheduleHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<AssignmentDto>> HandleAsync(GetAssignmentsByMeetingScheduleQuery query, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var assignments = await context.ResponsibilityAssignments
            .AsNoTracking()
            .Where(ra => ra.MeetingScheduleId == query.MeetingScheduleId)
            .Include(ra => ra.Publisher)
            .ToListAsync(cancellationToken);

        return assignments
            .Select(a => a.ToDto())
            .OrderBy(a => a.PublisherName)
            .ToList();
    }
}
