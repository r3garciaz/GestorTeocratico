using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.PublisherResponsibilities;

public class PublisherResponsibilityService : IPublisherResponsibilityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<PublisherResponsibilityService> _logger;

    public PublisherResponsibilityService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<PublisherResponsibilityService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PublisherResponsibilities
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PublisherResponsibilities
            .Include(pr => pr.Publisher)
            .Include(pr => pr.Responsibility)
                .ThenInclude(r => r.Department)
            .FirstOrDefaultAsync(pr => pr.PublisherId == publisherId && pr.ResponsibilityId == responsibilityId);
    }

    public async Task<PublisherResponsibility> CreateAsync(PublisherResponsibility publisherResponsibility)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Verificar que el Publisher existe
        var publisher = await context.Publishers.FindAsync(publisherResponsibility.PublisherId);
        if (publisher == null)
            throw new ArgumentException("Publisher not found", nameof(publisherResponsibility.PublisherId));

        // Verificar que la Responsibility existe
        var responsibility = await context.Responsibilities.FindAsync(publisherResponsibility.ResponsibilityId);
        if (responsibility == null)
            throw new ArgumentException("Responsibility not found", nameof(publisherResponsibility.ResponsibilityId));

        // Verificar que la relación no existe ya
        var existing = await GetByIdAsync(publisherResponsibility.PublisherId, publisherResponsibility.ResponsibilityId);
        if (existing != null)
            throw new InvalidOperationException("Publisher responsibility relationship already exists");

        context.PublisherResponsibilities.Add(publisherResponsibility);
        await context.SaveChangesAsync();
        return publisherResponsibility;
    }

    public async Task<bool> DeleteAsync(Guid publisherId, Guid responsibilityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var publisherResponsibility = await context.PublisherResponsibilities
            .FirstOrDefaultAsync(pr => pr.PublisherId == publisherId && pr.ResponsibilityId == responsibilityId);

        if (publisherResponsibility == null)
            return false;

        context.PublisherResponsibilities.Remove(publisherResponsibility);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetByPublisherIdAsync(Guid publisherId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PublisherResponsibilities
            .Where(pr => pr.PublisherId == publisherId)
            .Include(pr => pr.Responsibility)
                .ThenInclude(r => r.Department)
            .OrderBy(pr => pr.Responsibility.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<PublisherResponsibility>> GetByResponsibilityIdAsync(Guid responsibilityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PublisherResponsibilities
            .Where(pr => pr.ResponsibilityId == responsibilityId)
            .Include(pr => pr.Publisher)
            .OrderBy(pr => pr.Publisher.FirstName)
            .ThenBy(pr => pr.Publisher.LastName)
            .ToListAsync();
    }

    public async Task<bool> AssignResponsibilityToPublisherAsync(Guid publisherId, Guid responsibilityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var publisher = await context.Publishers.FindAsync(publisherId);
            var responsibility = await context.Responsibilities.FindAsync(responsibilityId);

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
