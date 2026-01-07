using GestorTeocratico.Entities;

namespace GestorTeocratico.Features.Congregations;

public interface ICongregationService
{
    Task<IEnumerable<Congregation>> GetAllAsync();
    Task<Congregation?> GetByIdAsync(Guid id);
    Task AddAsync(Congregation congregation);
    Task UpdateAsync(Congregation congregation);

}
