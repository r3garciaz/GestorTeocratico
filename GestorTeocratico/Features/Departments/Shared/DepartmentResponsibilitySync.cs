using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

/// <summary>
/// Reconciles which responsibilities belong to a department by setting/clearing
/// <see cref="Entities.Responsibility.DepartmentId"/>. Uses only scalar foreign keys.
/// </summary>
internal static class DepartmentResponsibilitySync
{
    public static async Task SyncAsync(
        ApplicationDbContext context,
        Guid departmentId,
        IReadOnlyCollection<Guid> responsibilityIds,
        CancellationToken cancellationToken)
    {
        var selected = responsibilityIds.ToHashSet();

        var current = await context.Responsibilities
            .Where(r => r.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);

        foreach (var responsibility in current.Where(r => !selected.Contains(r.ResponsibilityId)))
        {
            responsibility.DepartmentId = null;
        }

        var toAssign = await context.Responsibilities
            .Where(r => selected.Contains(r.ResponsibilityId))
            .ToListAsync(cancellationToken);

        foreach (var responsibility in toAssign)
        {
            responsibility.DepartmentId = departmentId;
        }
    }
}
