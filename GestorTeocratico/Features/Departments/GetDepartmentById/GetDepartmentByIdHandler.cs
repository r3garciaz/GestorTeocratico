using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

/// <summary>Query: get a single department by id (for the edit form).</summary>
public sealed class GetDepartmentByIdHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<DepartmentDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var department = await context.Departments
            .AsNoTracking()
            .Include(d => d.ResponsiblePublisher)
            .Include(d => d.Responsibilities)
            .FirstOrDefaultAsync(d => d.DepartmentId == id, cancellationToken);

        return department?.ToDto();
    }
}
