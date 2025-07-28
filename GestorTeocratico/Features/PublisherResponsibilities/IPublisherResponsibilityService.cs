using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.PublisherResponsibilities;

public interface IPublisherResponsibilityService
{
    Task<IEnumerable<PublisherResponsibility>> GetAllAsync();
    Task<PublisherResponsibility?> GetByIdAsync(Guid publisherId, Guid responsibilityId);
    Task<PublisherResponsibility> CreateAsync(PublisherResponsibility publisherResponsibility);
    Task<bool> DeleteAsync(Guid publisherId, Guid responsibilityId);
    Task<IEnumerable<PublisherResponsibility>> GetByPublisherIdAsync(Guid publisherId);
    Task<IEnumerable<PublisherResponsibility>> GetByResponsibilityIdAsync(Guid responsibilityId);
    Task<bool> AssignResponsibilityToPublisherAsync(Guid publisherId, Guid responsibilityId);
    Task<bool> RemoveResponsibilityFromPublisherAsync(Guid publisherId, Guid responsibilityId);
    Task<IEnumerable<PublisherResponsibility>> GetPublishersWithResponsibilityAsync(Guid responsibilityId);
}
