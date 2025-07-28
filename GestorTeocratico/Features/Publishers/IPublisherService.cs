using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Publishers;

public interface IPublisherService
{
    Task<IQueryable<Publisher>> GetAllAsync();
    Task<Publisher?> GetByIdAsync(Guid id);
    Task AddAsync(Publisher publisher);
    Task UpdateAsync(Publisher publisher);
    Task<Publisher> DeleteAsync(Guid id);
    Task<IEnumerable<Responsibility>> GetAvailableResponsibilitiesAsync();
    Task UpdatePublisherResponsibilitiesAsync(Guid publisherId, IEnumerable<Guid> responsibilityIds);
    Task<IEnumerable<Department>> GetAvailableDepartmentsAsync();
    Task UpdatePublisherDepartmentsAsync(Guid publisherId, IEnumerable<Guid> departmentIds);
}
