using GestorTeocratico.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

/// <summary>Query: options (departments and publishers) for the responsibility form.</summary>
public sealed class GetResponsibilityOptionsHandler(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<ResponsibilityFormOptions> HandleAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var departments = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new ResponsibilityDepartmentOptionDto(d.DepartmentId, d.Name))
            .ToListAsync(cancellationToken);

        var publishers = await context.Publishers
            .AsNoTracking()
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync(cancellationToken);

        var publisherOptions = publishers
            .Select(p => new ResponsibilityPublisherOptionDto(p.PublisherId, ResponsibilityMappings.PublisherDisplayName(p)))
            .ToList();

        return new ResponsibilityFormOptions(departments, publisherOptions);
    }
}
