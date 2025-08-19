using Microsoft.AspNetCore.Identity;

namespace GestorTeocratico.Features.Roles;

public interface IRoleService
{
    Task<IEnumerable<IdentityRole>> GetAllAsync();
    Task<IdentityRole?> GetByIdAsync(string roleId);
    Task<IdentityRole> CreateAsync(string roleName);
    Task UpdateAsync(string roleId, string roleName);
    Task DeleteAsync(string roleId);
}
