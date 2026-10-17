using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    Guid? ResponsiblePublisherId,
    IReadOnlyCollection<Guid> ResponsibilityIds);

/// <summary>Command: update a department and its responsibilities in a single transaction.</summary>
public sealed class UpdateDepartmentHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task HandleAsync(UpdateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var tracked = await context.Departments.FindAsync([command.Id], cancellationToken)
                      ?? throw new KeyNotFoundException("Department not found.");

        // Copy scalar values onto the tracked entity; related entities are never touched.
        tracked.Name = command.Name;
        tracked.ResponsiblePublisherId = command.ResponsiblePublisherId;

        await context.SaveChangesAsync(cancellationToken);

        await DepartmentResponsibilitySync.SyncAsync(context, command.Id, command.ResponsibilityIds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }
}
