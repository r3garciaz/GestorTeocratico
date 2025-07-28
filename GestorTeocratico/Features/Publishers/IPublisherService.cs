using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Publishers;

public interface IPublisherService
{
    Task<IEnumerable<Publisher>> GetAllAsync();
    Task<Publisher?> GetByIdAsync(Guid id);
    Task<Publisher> CreateAsync(Publisher publisher);
    Task<Publisher?> UpdateAsync(Publisher publisher);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Publisher>> GetPublishersByGenderAsync(Shared.Enums.Gender gender);
    Task<IEnumerable<Publisher>> GetPublishersByPrivilegeAsync(Shared.Enums.Privilege privilege);
    Task<IEnumerable<Publisher>> GetAvailableResponsiblePublishersAsync();
}
