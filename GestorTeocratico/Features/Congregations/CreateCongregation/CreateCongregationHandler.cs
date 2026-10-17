using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Congregations;

public sealed record CreateCongregationCommand(
    string Name,
    DayOfWeek MidweekMeetingDayEvenYear,
    DayOfWeek MidweekMeetingDayOddYear,
    DayOfWeek WeekendMeetingDayEvenYear,
    DayOfWeek WeekendMeetingDayOddYear,
    string? Address,
    string? City);

/// <summary>Command: create the congregation. Only one congregation is allowed.</summary>
public sealed class CreateCongregationHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<CreateCongregationHandler> logger)
{
    public async Task<Guid> HandleAsync(CreateCongregationCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Congregations.AnyAsync(cancellationToken))
        {
            logger.LogWarning("Attempted to add a second congregation: {Name}", command.Name);
            throw new DuplicateEntityException("Ya existe una congregación. No se puede crear otra.");
        }

        var normalized = command.Name.ToLower();
        if (await context.Congregations.AnyAsync(c => c.Name.ToLower() == normalized, cancellationToken))
        {
            logger.LogWarning("Attempted to add a congregation with duplicate name: {Name}", command.Name);
            throw new DuplicateEntityException("Ya existe una congregación con el mismo nombre.");
        }

        var congregation = new Congregation
        {
            Name = command.Name,
            MidweekMeetingDayEvenYear = command.MidweekMeetingDayEvenYear,
            MidweekMeetingDayOddYear = command.MidweekMeetingDayOddYear,
            WeekendMeetingDayEvenYear = command.WeekendMeetingDayEvenYear,
            WeekendMeetingDayOddYear = command.WeekendMeetingDayOddYear,
            Address = command.Address,
            City = command.City
        };

        context.Congregations.Add(congregation);
        await context.SaveChangesAsync(cancellationToken);
        return congregation.CongregationId;
    }
}
