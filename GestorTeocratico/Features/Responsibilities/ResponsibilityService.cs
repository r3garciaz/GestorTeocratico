using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

public class ResponsibilityService : IResponsibilityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ResponsibilityService> _logger;

    public ResponsibilityService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<ResponsibilityService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Responsibility>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Responsibilities
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .ToListAsync();
    }

    public async Task<Responsibility?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Responsibilities
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .FirstOrDefaultAsync(r => r.ResponsibilityId == id);
    }

    public async Task AddAsync(Responsibility responsibility)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingResponsibility = await context.Responsibilities
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name.ToLower() == responsibility.Name.ToLower() &&
                                    r.DepartmentId == responsibility.DepartmentId);

        if (existingResponsibility != null)
        {
            _logger.LogWarning("Attempted to add a responsibility with a duplicate name in the same department: {Name}", responsibility.Name);
            return;
        }

        context.Responsibilities.Add(responsibility);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Responsibility responsibility)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Responsibilities.Update(responsibility);
        await context.SaveChangesAsync();
    }

    public async Task<Responsibility> DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var responsibility = await context.Responsibilities.FindAsync(id);

        if (responsibility == null)
        {
            _logger.LogWarning("Attempted to delete a responsibility that does not exist: {Id}", id);
            throw new KeyNotFoundException("Responsibility not found.");
        }

        responsibility.IsDeleted = true;
        await context.SaveChangesAsync();
        return responsibility;
    }

    public async Task<IEnumerable<Department>> GetAvailableDepartmentsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Publisher>> GetAvailablePublishersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Publishers
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task UpdateResponsibilityPublishersAsync(Guid responsibilityId, IEnumerable<Guid> publisherIds)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var publisherIdsList = publisherIds.ToList();

        // Eliminar publishers actuales de la responsibility
        var currentPublisherResponsibilities = await context.PublisherResponsibilities
            .Where(pr => pr.ResponsibilityId == responsibilityId)
            .ToListAsync();

        context.PublisherResponsibilities.RemoveRange(currentPublisherResponsibilities);

        // Agregar los nuevos publishers seleccionados
        foreach (var publisherId in publisherIdsList)
        {
            var publisherResponsibility = new PublisherResponsibility
            {
                ResponsibilityId = responsibilityId,
                PublisherId = publisherId,
                Responsibility = await context.Responsibilities.FindAsync(responsibilityId) ?? throw new InvalidOperationException(),
                Publisher = await context.Publishers.FindAsync(publisherId) ?? throw new InvalidOperationException()
            };

            context.PublisherResponsibilities.Add(publisherResponsibility);
        }

        await context.SaveChangesAsync();
    }
}
