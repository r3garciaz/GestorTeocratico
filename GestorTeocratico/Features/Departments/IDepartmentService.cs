using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Departments;

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(Guid id);
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task<Department> DeleteAsync(Guid id);
    Task<IEnumerable<Publisher>> GetAvailablePublishersAsync();
    Task<IEnumerable<Responsibility>> GetAvailableResponsibilitiesAsync();
    Task UpdateDepartmentResponsibilitiesAsync(Guid departmentId, IEnumerable<Guid> responsibilityIds);
}
