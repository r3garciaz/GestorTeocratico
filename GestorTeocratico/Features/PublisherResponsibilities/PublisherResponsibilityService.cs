using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Features.Publishers;
using GestorTeocratico.Features.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.PublisherResponsibilities;

public class PublisherResponsibilityService : IPublisherResponsibilityService
{
    private readonly ApplicationDbContext _context;
    private readonly IPublisherService _publisherService;
    private readonly IResponsibilityService _responsibilityService;

    public PublisherResponsibilityService(
        ApplicationDbContext context,
        IPublisherService publisherService,
        IResponsibilityService responsibilityService)
    {
        _context = context;
        _publisherService = publisherService;
        _responsibilityService = responsibilityService;
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetAllAsync()
    {
        return await _context.PublisherResponsibilities
            .Include(pr => pr.Publisher)
            .Include(pr => pr.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(pr => pr.Publisher.FirstName)
            .ThenBy(pr => pr.Publisher.LastName)
            .ThenBy(pr => pr.Responsibility.Name)
            .ToListAsync();
    }

    public async Task<PublisherResponsibility?> GetByIdAsync(Guid publisherId, Guid responsibilityId)
    {
        return await _context.PublisherResponsibilities
            .Include(pr => pr.Publisher)
            .Include(pr => pr.Responsibility)
                .ThenInclude(r => r.Department)
            .FirstOrDefaultAsync(pr => pr.PublisherId == publisherId && pr.ResponsibilityId == responsibilityId);
    }

    public async Task<PublisherResponsibility> CreateAsync(PublisherResponsibility publisherResponsibility)
    {
        // Verificar que el Publisher existe
        var publisher = await _publisherService.GetByIdAsync(publisherResponsibility.PublisherId);
        if (publisher == null)
            throw new ArgumentException("Publisher not found", nameof(publisherResponsibility.PublisherId));

        // Verificar que la Responsibility existe
        var responsibility = await _responsibilityService.GetByIdAsync(publisherResponsibility.ResponsibilityId);
        if (responsibility == null)
            throw new ArgumentException("Responsibility not found", nameof(publisherResponsibility.ResponsibilityId));

        // Verificar que la relación no existe ya
        var existing = await GetByIdAsync(publisherResponsibility.PublisherId, publisherResponsibility.ResponsibilityId);
        if (existing != null)
            throw new InvalidOperationException("Publisher responsibility relationship already exists");

        _context.PublisherResponsibilities.Add(publisherResponsibility);
        await _context.SaveChangesAsync();
        return publisherResponsibility;
    }

    public async Task<bool> DeleteAsync(Guid publisherId, Guid responsibilityId)
    {
        var publisherResponsibility = await _context.PublisherResponsibilities
            .FirstOrDefaultAsync(pr => pr.PublisherId == publisherId && pr.ResponsibilityId == responsibilityId);

        if (publisherResponsibility == null)
            return false;

        _context.PublisherResponsibilities.Remove(publisherResponsibility);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetByPublisherIdAsync(Guid publisherId)
    {
        return await _context.PublisherResponsibilities
            .Where(pr => pr.PublisherId == publisherId)
            .Include(pr => pr.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(pr => pr.Responsibility.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetByResponsibilityIdAsync(Guid responsibilityId)
    {
        return await _context.PublisherResponsibilities
            .Where(pr => pr.ResponsibilityId == responsibilityId)
            .Include(pr => pr.Publisher)
            .OrderBy(pr => pr.Publisher.FirstName)
            .ThenBy(pr => pr.Publisher.LastName)
            .ToListAsync();
    }

    public async Task<bool> AssignResponsibilityToPublisherAsync(Guid publisherId, Guid responsibilityId)
    {
        try
        {
            var publisher = await _publisherService.GetByIdAsync(publisherId);
            var responsibility = await _responsibilityService.GetByIdAsync(responsibilityId);

            if (publisher == null || responsibility == null)
                return false;

            var publisherResponsibility = new PublisherResponsibility
            {
                PublisherId = publisherId,
                ResponsibilityId = responsibilityId,
                Publisher = publisher,
                Responsibility = responsibility
            };

            await CreateAsync(publisherResponsibility);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveResponsibilityFromPublisherAsync(Guid publisherId, Guid responsibilityId)
    {
        return await DeleteAsync(publisherId, responsibilityId);
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetPublishersWithResponsibilityAsync(Guid responsibilityId)
    {
        return await GetByResponsibilityIdAsync(responsibilityId);
    }
}
