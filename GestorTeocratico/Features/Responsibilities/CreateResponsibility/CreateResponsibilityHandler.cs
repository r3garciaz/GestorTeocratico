using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

public sealed record CreateResponsibilityCommand(
    string Name,
    string? Description,
    Guid? DepartmentId,
    IReadOnlyCollection<Guid> PublisherIds);

/// <summary>Command: create a responsibility and its publisher assignments in a single transaction.</summary>
public sealed class CreateResponsibilityHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<CreateResponsibilityHandler> logger)
{
    public async Task<Guid> HandleAsync(CreateResponsibilityCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var normalized = command.Name.ToLower();
        var exists = await context.Responsibilities
            .AsNoTracking()
            .AnyAsync(r => r.Name.ToLower() == normalized && r.DepartmentId == command.DepartmentId, cancellationToken);

        if (exists)
        {
            logger.LogWarning("Attempted to add a responsibility with a duplicate name in the same department: {Name}", command.Name);
            throw new DuplicateEntityException($"Ya existe una responsabilidad con el nombre '{command.Name}' en el mismo departamento.");
        }

        var responsibility = new Responsibility
        {
            Name = command.Name,
            Description = command.Description,
            DepartmentId = command.DepartmentId
        };

        context.Responsibilities.Add(responsibility);
        await context.SaveChangesAsync(cancellationToken);

        await ResponsibilityPublisherSync.SyncAsync(context, responsibility.ResponsibilityId, command.PublisherIds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
        return responsibility.ResponsibilityId;
    }
}
