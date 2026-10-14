using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

/// <summary>Query: list all responsibilities with their department and assigned publishers.</summary>
public sealed class GetResponsibilitiesHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<ResponsibilityDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var responsibilities = await context.Responsibilities
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return responsibilities.Select(r => r.ToDto()).ToList();
    }
}
