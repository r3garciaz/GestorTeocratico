using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GestorTeocratico.Features.Roles;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public Task<IEnumerable<IdentityRole>> GetAllAsync()
    {
        var roles = _roleManager.Roles.ToList().AsEnumerable();
        return Task.FromResult(roles);
    }

    public async Task<IdentityRole?> GetByIdAsync(string roleId)
    {
        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<IdentityRole> CreateAsync(string roleName)
    {
        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        return role;
    }

    public async Task UpdateAsync(string roleId, string roleName)
    {
        var role = await _roleManager.FindByIdAsync(roleId) ?? throw new InvalidOperationException("Role not found");
        role.Name = roleName;
        role.NormalizedName = roleName.ToUpperInvariant();
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeleteAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId) ?? throw new InvalidOperationException("Role not found");
        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
