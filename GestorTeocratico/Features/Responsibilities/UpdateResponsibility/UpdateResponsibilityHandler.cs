using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

public sealed record UpdateResponsibilityCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? DepartmentId,
    IReadOnlyCollection<Guid> PublisherIds);

/// <summary>Command: update a responsibility and its publisher assignments in a single transaction.</summary>
public sealed class UpdateResponsibilityHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task HandleAsync(UpdateResponsibilityCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var tracked = await context.Responsibilities.FindAsync([command.Id], cancellationToken)
                      ?? throw new KeyNotFoundException("Responsibility not found.");

        // Copy scalar values onto the tracked entity; related entities are never touched.
        tracked.Name = command.Name;
        tracked.Description = command.Description;
        tracked.DepartmentId = command.DepartmentId;

        await context.SaveChangesAsync(cancellationToken);

        await ResponsibilityPublisherSync.SyncAsync(context, command.Id, command.PublisherIds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }
}
