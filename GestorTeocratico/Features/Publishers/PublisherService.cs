using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

public class PublisherService : IPublisherService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<PublisherService> _logger;

    public PublisherService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<PublisherService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Publisher>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Publishers
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .ToListAsync();
    }

    public async Task<Publisher?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Publishers
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .FirstOrDefaultAsync(p => p.PublisherId == id);
    }

    public async Task AddAsync(Publisher publisher)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingPublisher = await context.Publishers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.FirstName.ToLower() == publisher.FirstName.ToLower() && 
                                    p.LastName != null && publisher.LastName != null &&
                                    p.LastName.ToLower() == publisher.LastName.ToLower());

        if (existingPublisher != null)
        {
            _logger.LogWarning("Attempted to add a publisher with a duplicate name: {FirstName} {LastName}", 
                publisher.FirstName, publisher.LastName);
            return;
        }

        context.Publishers.Add(publisher);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Publisher publisher)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Publishers.Update(publisher);
        await context.SaveChangesAsync();
    }

    public async Task<Publisher> DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var publisher = await context.Publishers.FindAsync(id);

        if (publisher == null)
        {
            _logger.LogWarning("Attempted to delete a publisher that does not exist: {Id}", id);
            throw new KeyNotFoundException("Publisher not found.");
        }

        publisher.IsDeleted = true;
        await context.SaveChangesAsync();
        return publisher;
    }

    public async Task<IEnumerable<Responsibility>> GetAvailableResponsibilitiesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Responsibilities
            .Include(r => r.Department)
            .OrderBy(r => r.Department.Name)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetAvailableDepartmentsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task UpdatePublisherResponsibilitiesAsync(Guid publisherId, IEnumerable<Guid> responsibilityIds)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var responsibilityIdsList = responsibilityIds.ToList();

        // Eliminar responsabilidades actuales del publisher
        var currentResponsibilities = await context.PublisherResponsibilities
            .Where(pr => pr.PublisherId == publisherId)
            .ToListAsync();

        context.PublisherResponsibilities.RemoveRange(currentResponsibilities);

        // Agregar las nuevas responsabilidades seleccionadas
        foreach (var responsibilityId in responsibilityIdsList)
        {
            var publisherResponsibility = new PublisherResponsibility
            {
                PublisherId = publisherId,
                ResponsibilityId = responsibilityId,
                Publisher = await context.Publishers.FindAsync(publisherId) ?? throw new InvalidOperationException(),
                Responsibility = await context.Responsibilities.FindAsync(responsibilityId) ?? throw new InvalidOperationException()
            };

            context.PublisherResponsibilities.Add(publisherResponsibility);
        }

        await context.SaveChangesAsync();
    }

    public async Task UpdatePublisherDepartmentsAsync(Guid publisherId, IEnumerable<Guid> departmentIds)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var departmentIdsList = departmentIds.ToList();

        // Primero, remover al publisher como responsable de todos los departamentos que tenía asignados
        var currentDepartments = await context.Departments
            .Where(d => d.ResponsiblePublisherId == publisherId)
            .ToListAsync();

        foreach (var department in currentDepartments)
        {
            department.ResponsiblePublisherId = null;
        }

        // Luego, asignar al publisher como responsable de los nuevos departamentos seleccionados
        var newDepartments = await context.Departments
            .Where(d => departmentIdsList.Contains(d.DepartmentId))
            .ToListAsync();

        foreach (var department in newDepartments)
        {
            department.ResponsiblePublisherId = publisherId;
        }

        await context.SaveChangesAsync();
    }
}
