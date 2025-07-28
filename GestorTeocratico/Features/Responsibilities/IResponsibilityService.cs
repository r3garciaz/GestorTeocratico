using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Responsibilities;

public interface IResponsibilityService
{
    Task<IEnumerable<Responsibility>> GetAllAsync();
    Task<Responsibility?> GetByIdAsync(Guid id);
    Task<Responsibility> CreateAsync(Responsibility responsibility);
    Task<Responsibility?> UpdateAsync(Responsibility responsibility);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Responsibility>> GetByDepartmentIdAsync(Guid departmentId);
    Task<IEnumerable<Responsibility>> GetAvailableForAssignmentAsync();
}
