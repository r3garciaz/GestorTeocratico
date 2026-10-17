using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

/// <summary>Command: soft-delete a responsibility. Returns false when the responsibility does not exist.</summary>
public sealed class DeleteResponsibilityHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var responsibility = await context.Responsibilities.FindAsync([id], cancellationToken);

        if (responsibility == null)
        {
            return false;
        }

        responsibility.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
