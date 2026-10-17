using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

/// <summary>Command: soft-delete a department. Returns false when the department does not exist.</summary>
public sealed class DeleteDepartmentHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var department = await context.Departments.FindAsync([id], cancellationToken);

        if (department == null)
        {
            return false;
        }

        department.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
