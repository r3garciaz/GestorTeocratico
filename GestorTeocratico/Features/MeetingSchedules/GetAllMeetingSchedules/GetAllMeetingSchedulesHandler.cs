using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.MeetingSchedules;

/// <summary>Query: list all meeting schedules with their assignments, ordered by date.</summary>
public sealed class GetAllMeetingSchedulesHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<MeetingScheduleDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var schedules = await context.MeetingSchedules
            .AsNoTracking()
            .WithAssignments()
            .OrderBy(ms => ms.Date)
            .ToListAsync(cancellationToken);

        return schedules.Select(ms => ms.ToDto()).ToList();
    }
}
