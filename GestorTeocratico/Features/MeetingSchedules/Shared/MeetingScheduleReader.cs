using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GestorTeocratico.Features.MeetingSchedules;

/// <summary>Shared read/lookup helpers for the meeting-schedule handlers.</summary>
internal static class MeetingScheduleReader
{
    public static IQueryable<MeetingSchedule> WithAssignments(this IQueryable<MeetingSchedule> query)
        => query
            .Include(ms => ms.ResponsibilityAssignments)
                .ThenInclude(ra => ra.Publisher);

    public static async Task<List<MeetingScheduleDto>> ByWeekAsync(
        ApplicationDbContext context, int week, int year, CancellationToken cancellationToken)
    {
        var schedules = await context.MeetingSchedules
            .AsNoTracking()
            .WithAssignments()
            .Where(ms => ms.WeekOfYear == week && ms.Year == year)
            .OrderBy(ms => ms.Date)
            .ToListAsync(cancellationToken);

        return schedules.Select(ms => ms.ToDto()).ToList();
    }

    /// <summary>Creates the midweek and weekend schedules for a week when they do not exist yet.</summary>
    public static async Task EnsureWeekAsync(
        ApplicationDbContext context, int week, int year, CancellationToken cancellationToken)
    {
        var exists = await context.MeetingSchedules
            .AnyAsync(ms => ms.WeekOfYear == week && ms.Year == year, cancellationToken);

        if (exists)
        {
            return;
        }

        var monday = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        var date = DateOnly.FromDateTime(monday);

        context.MeetingSchedules.Add(new MeetingSchedule { Date = date, MeetingType = MeetingType.Midweek });
        context.MeetingSchedules.Add(new MeetingSchedule { Date = date, MeetingType = MeetingType.Weekend });

        await context.SaveChangesAsync(cancellationToken);
    }
}
