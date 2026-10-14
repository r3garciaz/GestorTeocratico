using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Congregations;

/// <summary>Query: list the congregation(s). The domain allows a single congregation.</summary>
public sealed class GetCongregationsHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<CongregationDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var congregations = await context.Congregations
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return congregations.Select(c => c.ToDto()).ToList();
    }
}
