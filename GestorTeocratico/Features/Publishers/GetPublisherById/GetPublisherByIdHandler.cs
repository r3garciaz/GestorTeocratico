using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

/// <summary>Query: get a single publisher by id (for the edit form).</summary>
public sealed class GetPublisherByIdHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<PublisherDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var publisher = await context.Publishers
            .AsNoTracking()
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .FirstOrDefaultAsync(p => p.PublisherId == id, cancellationToken);

        return publisher?.ToDto();
    }
}
