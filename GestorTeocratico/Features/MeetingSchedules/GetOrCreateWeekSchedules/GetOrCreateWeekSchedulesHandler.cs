using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

public sealed record GetOrCreateWeekSchedulesQuery(int WeekOfYear, int Year);

/// <summary>
/// Query: returns the two weekly schedules (midweek and weekend), creating them when missing.
/// </summary>
public sealed class GetOrCreateWeekSchedulesHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<MeetingScheduleDto>> HandleAsync(GetOrCreateWeekSchedulesQuery query, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await MeetingScheduleReader.ByWeekAsync(context, query.WeekOfYear, query.Year, cancellationToken);
        if (existing.Count > 0)
        {
            return existing;
        }

        await MeetingScheduleReader.EnsureWeekAsync(context, query.WeekOfYear, query.Year, cancellationToken);

        return await MeetingScheduleReader.ByWeekAsync(context, query.WeekOfYear, query.Year, cancellationToken);
    }
}
