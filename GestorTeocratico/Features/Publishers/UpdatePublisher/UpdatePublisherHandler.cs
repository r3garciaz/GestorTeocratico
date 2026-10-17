using GestorTeocratico.Data;
using GestorTeocratico.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

public sealed record UpdatePublisherCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? MotherLastName,
    string? Phone,
    string? Email,
    Gender Gender,
    Privilege? Privilege,
    IReadOnlyCollection<Guid> ResponsibilityIds,
    IReadOnlyCollection<Guid> DepartmentIds);

/// <summary>Command: update a publisher and its assignments in a single transaction.</summary>
public sealed class UpdatePublisherHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task HandleAsync(UpdatePublisherCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var tracked = await context.Publishers.FindAsync([command.Id], cancellationToken)
                      ?? throw new KeyNotFoundException("Publisher not found.");

        // Copy scalar values onto the tracked entity; related entities are never touched.
        tracked.FirstName = command.FirstName;
        tracked.LastName = command.LastName;
        tracked.MotherLastName = command.MotherLastName;
        tracked.Phone = command.Phone;
        tracked.Email = command.Email;
        tracked.Gender = command.Gender;
        tracked.Privilege = command.Privilege;

        await context.SaveChangesAsync(cancellationToken);

        await PublisherAssignmentSync.SyncAsync(
            context, command.Id, command.ResponsibilityIds, command.DepartmentIds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }
}
