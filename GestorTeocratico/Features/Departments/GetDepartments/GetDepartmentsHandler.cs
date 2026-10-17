using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

/// <summary>Query: list all departments with their responsible publisher and responsibility names.</summary>
public sealed class GetDepartmentsHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<IReadOnlyList<DepartmentDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var departments = await context.Departments
            .AsNoTracking()
            .Include(d => d.ResponsiblePublisher)
            .Include(d => d.Responsibilities)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return departments.Select(d => d.ToDto()).ToList();
    }
}
