using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

public sealed record CreateDepartmentCommand(
    string Name,
    Guid? ResponsiblePublisherId,
    IReadOnlyCollection<Guid> ResponsibilityIds);

/// <summary>Command: create a department and assign its responsibilities in a single transaction.</summary>
public sealed class CreateDepartmentHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<CreateDepartmentHandler> logger)
{
    public async Task<Guid> HandleAsync(CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var normalized = command.Name.ToLower();
        var exists = await context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Name.ToLower() == normalized, cancellationToken);

        if (exists)
        {
            logger.LogWarning("Attempted to add a department with a duplicate name: {Name}", command.Name);
            throw new DuplicateEntityException($"Ya existe un departamento con el nombre '{command.Name}'.");
        }

        var department = new Department
        {
            Name = command.Name,
            ResponsiblePublisherId = command.ResponsiblePublisherId
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync(cancellationToken);

        await DepartmentResponsibilitySync.SyncAsync(context, department.DepartmentId, command.ResponsibilityIds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
        return department.DepartmentId;
    }
}
