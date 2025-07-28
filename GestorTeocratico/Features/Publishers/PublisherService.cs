using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

public class PublisherService : IPublisherService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PublisherService> _logger;

    public PublisherService(ApplicationDbContext context, ILogger<PublisherService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IQueryable<Publisher>> GetAllAsync()
    {
        var items = _context.Publishers
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .AsQueryable();
        return await Task.FromResult(items);
    }

    public async Task<Publisher?> GetByIdAsync(Guid id)
    {
        return await _context.Publishers
            .Include(p => p.ResponsibleDepartments)
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .FirstOrDefaultAsync(p => p.PublisherId == id);
    }

    public async Task AddAsync(Publisher publisher)
    {
        var existingPublisher = await _context.Publishers
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

        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Publisher publisher)
    {
        _context.Publishers.Update(publisher);
        await _context.SaveChangesAsync();
    }

    public async Task<Publisher> DeleteAsync(Guid id)
    {
        var publisher = await _context.Publishers.FindAsync(id);

        if (publisher == null)
        {
            _logger.LogWarning("Attempted to delete a publisher that does not exist: {Id}", id);
            throw new KeyNotFoundException("Publisher not found.");
        }

        publisher.IsDeleted = true;
        await _context.SaveChangesAsync();
        return publisher;
    }

    public async Task<IEnumerable<Responsibility>> GetAvailableResponsibilitiesAsync()
    {
        return await _context.Responsibilities
            .Include(r => r.Department)
            .OrderBy(r => r.Department.Name)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetAvailableDepartmentsAsync()
    {
        return await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task UpdatePublisherResponsibilitiesAsync(Guid publisherId, IEnumerable<Guid> responsibilityIds)
    {
        var responsibilityIdsList = responsibilityIds.ToList();

        // Eliminar responsabilidades actuales del publisher
        var currentResponsibilities = await _context.PublisherResponsibilities
            .Where(pr => pr.PublisherId == publisherId)
            .ToListAsync();

        _context.PublisherResponsibilities.RemoveRange(currentResponsibilities);

        // Agregar las nuevas responsabilidades seleccionadas
        foreach (var responsibilityId in responsibilityIdsList)
        {
            var publisherResponsibility = new PublisherResponsibility
            {
                PublisherId = publisherId,
                ResponsibilityId = responsibilityId,
                Publisher = await _context.Publishers.FindAsync(publisherId) ?? throw new InvalidOperationException(),
                Responsibility = await _context.Responsibilities.FindAsync(responsibilityId) ?? throw new InvalidOperationException()
            };

            _context.PublisherResponsibilities.Add(publisherResponsibility);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdatePublisherDepartmentsAsync(Guid publisherId, IEnumerable<Guid> departmentIds)
    {
        var departmentIdsList = departmentIds.ToList();

        // Primero, remover al publisher como responsable de todos los departamentos que tenía asignados
        var currentDepartments = await _context.Departments
            .Where(d => d.ResponsiblePublisherId == publisherId)
            .ToListAsync();

        foreach (var department in currentDepartments)
        {
            department.ResponsiblePublisherId = null;
        }

        // Luego, asignar al publisher como responsable de los nuevos departamentos seleccionados
        var newDepartments = await _context.Departments
            .Where(d => departmentIdsList.Contains(d.DepartmentId))
            .ToListAsync();

        foreach (var department in newDepartments)
        {
            department.ResponsiblePublisherId = publisherId;
        }

        await _context.SaveChangesAsync();
    }
}
