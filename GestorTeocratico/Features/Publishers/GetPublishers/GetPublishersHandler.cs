using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

/// <summary>Query: list all publishers with their department and responsibility names.</summary>
public sealed class GetPublishersHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<PublisherDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var publishers = await context.Publishers
            .AsNoTracking()
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync(cancellationToken);

        return publishers.Select(p => p.ToDto()).ToList();
    }
}
