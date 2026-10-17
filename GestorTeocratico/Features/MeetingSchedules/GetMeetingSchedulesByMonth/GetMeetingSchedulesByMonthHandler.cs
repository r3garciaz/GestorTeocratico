using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record GetMeetingSchedulesByMonthQuery(int Month, int Year);

/// <summary>Query: list the meeting schedules of a month with their assignments.</summary>
public sealed class GetMeetingSchedulesByMonthHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<MeetingScheduleDto>> HandleAsync(GetMeetingSchedulesByMonthQuery query, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var schedules = await context.MeetingSchedules
            .AsNoTracking()
            .WithAssignments()
            .Where(ms => ms.Month == query.Month && ms.Year == query.Year)
            .OrderBy(ms => ms.Date)
            .ToListAsync(cancellationToken);

        return schedules.Select(ms => ms.ToDto()).ToList();
    }
}
