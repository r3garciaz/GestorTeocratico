using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

public class ResponsibilityService : IResponsibilityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResponsibilityService> _logger;

    public ResponsibilityService(ApplicationDbContext context, ILogger<ResponsibilityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Responsibility>> GetAllAsync()
    {
        return await _context.Responsibilities
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .ToListAsync();
    }

    public async Task<Responsibility?> GetByIdAsync(Guid id)
    {
        return await _context.Responsibilities
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .FirstOrDefaultAsync(r => r.ResponsibilityId == id);
    }

    public async Task AddAsync(Responsibility responsibility)
    {
        var existingResponsibility = await _context.Responsibilities
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name.ToLower() == responsibility.Name.ToLower() &&
                                    r.DepartmentId == responsibility.DepartmentId);

        if (existingResponsibility != null)
        {
            _logger.LogWarning("Attempted to add a responsibility with a duplicate name in the same department: {Name}", responsibility.Name);
            return;
        }

        _context.Responsibilities.Add(responsibility);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Responsibility responsibility)
    {
        _context.Responsibilities.Update(responsibility);
        await _context.SaveChangesAsync();
    }

    public async Task<Responsibility> DeleteAsync(Guid id)
    {
        var responsibility = await _context.Responsibilities.FindAsync(id);

        if (responsibility == null)
        {
            _logger.LogWarning("Attempted to delete a responsibility that does not exist: {Id}", id);
            throw new KeyNotFoundException("Responsibility not found.");
        }

        responsibility.IsDeleted = true;
        await _context.SaveChangesAsync();
        return responsibility;
    }

    public async Task<IEnumerable<Department>> GetAvailableDepartmentsAsync()
    {
        return await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Publisher>> GetAvailablePublishersAsync()
    {
        return await _context.Publishers
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task UpdateResponsibilityPublishersAsync(Guid responsibilityId, IEnumerable<Guid> publisherIds)
    {
        var publisherIdsList = publisherIds.ToList();

        // Eliminar publishers actuales de la responsibility
        var currentPublisherResponsibilities = await _context.PublisherResponsibilities
            .Where(pr => pr.ResponsibilityId == responsibilityId)
            .ToListAsync();

        _context.PublisherResponsibilities.RemoveRange(currentPublisherResponsibilities);

        // Agregar los nuevos publishers seleccionados
        foreach (var publisherId in publisherIdsList)
        {
            var publisherResponsibility = new PublisherResponsibility
            {
                ResponsibilityId = responsibilityId,
                PublisherId = publisherId,
                Responsibility = await _context.Responsibilities.FindAsync(responsibilityId) ?? throw new InvalidOperationException(),
                Publisher = await _context.Publishers.FindAsync(publisherId) ?? throw new InvalidOperationException()
            };

            _context.PublisherResponsibilities.Add(publisherResponsibility);
        }

        await _context.SaveChangesAsync();
    }
}
