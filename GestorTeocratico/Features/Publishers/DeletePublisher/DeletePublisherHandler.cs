using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

/// <summary>Command: soft-delete a publisher. Returns false when the publisher does not exist.</summary>
public sealed class DeletePublisherHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var publisher = await context.Publishers.FindAsync([id], cancellationToken);

        if (publisher == null)
        {
            return false;
        }

        publisher.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
