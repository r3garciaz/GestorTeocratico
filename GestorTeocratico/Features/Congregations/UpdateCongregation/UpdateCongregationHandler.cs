using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Congregations;

public sealed record UpdateCongregationCommand(
    Guid Id,
    string Name,
    DayOfWeek MidweekMeetingDayEvenYear,
    DayOfWeek MidweekMeetingDayOddYear,
    DayOfWeek WeekendMeetingDayEvenYear,
    DayOfWeek WeekendMeetingDayOddYear,
    string? Address,
    string? City);

/// <summary>Command: update the congregation.</summary>
public sealed class UpdateCongregationHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task HandleAsync(UpdateCongregationCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tracked = await context.Congregations.FindAsync([command.Id], cancellationToken)
                      ?? throw new KeyNotFoundException("Congregation not found.");

        tracked.Name = command.Name;
        tracked.MidweekMeetingDayEvenYear = command.MidweekMeetingDayEvenYear;
        tracked.MidweekMeetingDayOddYear = command.MidweekMeetingDayOddYear;
        tracked.WeekendMeetingDayEvenYear = command.WeekendMeetingDayEvenYear;
        tracked.WeekendMeetingDayOddYear = command.WeekendMeetingDayOddYear;
        tracked.Address = command.Address;
        tracked.City = command.City;

        await context.SaveChangesAsync(cancellationToken);
    }
}
