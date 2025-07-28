using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Publishers;

public class PublisherService : IPublisherService
{
    private readonly ApplicationDbContext _context;

    public PublisherService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Publisher>> GetAllAsync()
    {
        return await _context.Publishers
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .Include(p => p.ResponsibleDepartments)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<Publisher?> GetByIdAsync(Guid id)
    {
        return await _context.Publishers
            .Include(p => p.PublisherResponsibilities)
                .ThenInclude(pr => pr.Responsibility)
            .Include(p => p.ResponsibleDepartments)
            .FirstOrDefaultAsync(p => p.PublisherId == id);
    }

    public async Task<Publisher> CreateAsync(Publisher publisher)
    {
        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync();
        return publisher;
    }

    public async Task<Publisher?> UpdateAsync(Publisher publisher)
    {
        var existingPublisher = await _context.Publishers.FindAsync(publisher.PublisherId);
        if (existingPublisher == null)
            return null;

        existingPublisher.FirstName = publisher.FirstName;
        existingPublisher.LastName = publisher.LastName;
        existingPublisher.MotherLastName = publisher.MotherLastName;
        existingPublisher.Phone = publisher.Phone;
        existingPublisher.Email = publisher.Email;
        existingPublisher.Gender = publisher.Gender;
        existingPublisher.Privilege = publisher.Privilege;

        await _context.SaveChangesAsync();
        return existingPublisher;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var publisher = await _context.Publishers.FindAsync(id);
        if (publisher == null)
            return false;

        publisher.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Publisher>> GetPublishersByGenderAsync(Shared.Enums.Gender gender)
    {
        return await _context.Publishers
            .Where(p => p.Gender == gender)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Publisher>> GetPublishersByPrivilegeAsync(Shared.Enums.Privilege privilege)
    {
        return await _context.Publishers
            .Where(p => p.Privilege == privilege)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Publisher>> GetAvailableResponsiblePublishersAsync()
    {
        return await _context.Publishers
            .Where(p => p.Privilege.HasValue) // Solo publicadores con privilegios pueden ser responsables
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }
}
