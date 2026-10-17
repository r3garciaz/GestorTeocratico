using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

/// <summary>Query: options (responsibilities and departments) for the publisher form.</summary>
public sealed class GetPublisherOptionsHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<PublisherFormOptions> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var responsibilities = await context.Responsibilities
            .AsNoTracking()
            .OrderBy(r => r.Department != null ? r.Department.Name : string.Empty)
            .ThenBy(r => r.Name)
            .Select(r => new ResponsibilityOptionDto(
                r.ResponsibilityId,
                r.Name,
                r.Department != null ? r.Department.Name : null))
            .ToListAsync(cancellationToken);

        var departments = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentOptionDto(d.DepartmentId, d.Name))
            .ToListAsync(cancellationToken);

        return new PublisherFormOptions(responsibilities, departments);
    }
}
