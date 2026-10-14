using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

/// <summary>Query: options (responsibilities and eligible publishers) for the department form.</summary>
public sealed class GetDepartmentOptionsHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<DepartmentFormOptions> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var responsibilities = await context.Responsibilities
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new DepartmentResponsibilityOptionDto(r.ResponsibilityId, r.Name))
            .ToListAsync(cancellationToken);

        var publishers = await context.Publishers
            .AsNoTracking()
            .Where(p => p.Privilege.HasValue)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync(cancellationToken);

        var publisherOptions = publishers
            .Select(p => new DepartmentPublisherOptionDto(p.PublisherId, DepartmentMappings.PublisherDisplayName(p)))
            .ToList();

        return new DepartmentFormOptions(responsibilities, publisherOptions);
    }
}
