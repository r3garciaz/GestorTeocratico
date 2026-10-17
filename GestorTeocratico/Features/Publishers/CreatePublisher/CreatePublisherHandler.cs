using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Features.Common;
using GestorTeocratico.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

public sealed record CreatePublisherCommand(
    string FirstName,
    string LastName,
    string? MotherLastName,
    string? Phone,
    string? Email,
    Gender Gender,
    Privilege? Privilege,
    IReadOnlyCollection<Guid> ResponsibilityIds,
    IReadOnlyCollection<Guid> DepartmentIds);

/// <summary>Command: create a publisher and its assignments in a single transaction.</summary>
public sealed class CreatePublisherHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<CreatePublisherHandler> logger)
{
    public async Task<Guid> HandleAsync(CreatePublisherCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var firstName = command.FirstName.ToLower();
        var lastName = command.LastName.ToLower();
        var exists = await context.Publishers
            .AsNoTracking()
            .AnyAsync(p => p.FirstName.ToLower() == firstName && p.LastName.ToLower() == lastName, cancellationToken);

        if (exists)
        {
            logger.LogWarning("Attempted to add a publisher with a duplicate name: {FirstName} {LastName}",
                command.FirstName, command.LastName);
            throw new DuplicateEntityException($"Ya existe un publicador con el nombre '{command.FirstName} {command.LastName}'.");
        }

        var publisher = new Publisher
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            MotherLastName = command.MotherLastName,
            Phone = command.Phone,
            Email = command.Email,
            Gender = command.Gender,
            Privilege = command.Privilege
        };

        context.Publishers.Add(publisher);
        await context.SaveChangesAsync(cancellationToken);

        await PublisherAssignmentSync.SyncAsync(
            context, publisher.PublisherId, command.ResponsibilityIds, command.DepartmentIds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
        return publisher.PublisherId;
    }
}
