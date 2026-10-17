using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

/// <summary>Query: get a single responsibility by id (for the edit form).</summary>
public sealed class GetResponsibilityByIdHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<ResponsibilityDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var responsibility = await context.Responsibilities
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .FirstOrDefaultAsync(r => r.ResponsibilityId == id, cancellationToken);

        return responsibility?.ToDto();
    }
}
