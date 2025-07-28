using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Responsibilities;

public interface IResponsibilityService
{
    Task<IQueryable<Responsibility>> GetAllAsync();
    Task<Responsibility?> GetByIdAsync(Guid id);
    Task AddAsync(Responsibility responsibility);
    Task UpdateAsync(Responsibility responsibility);
    Task<Responsibility> DeleteAsync(Guid id);
    Task<IEnumerable<Department>> GetAvailableDepartmentsAsync();
    Task<IEnumerable<Publisher>> GetAvailablePublishersAsync();
    Task UpdateResponsibilityPublishersAsync(Guid responsibilityId, IEnumerable<Guid> publisherIds);
}
