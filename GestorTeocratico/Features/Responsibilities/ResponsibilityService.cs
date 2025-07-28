using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Responsibilities;

public class ResponsibilityService : IResponsibilityService
{
    private readonly ApplicationDbContext _context;

    public ResponsibilityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Responsibility>> GetAllAsync()
    {
        return await _context.Responsibilities
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Responsibility?> GetByIdAsync(Guid id)
    {
        return await _context.Responsibilities
            .Include(r => r.Department)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .FirstOrDefaultAsync(r => r.ResponsibilityId == id);
    }

    public async Task<Responsibility> CreateAsync(Responsibility responsibility)
    {
        _context.Responsibilities.Add(responsibility);
        await _context.SaveChangesAsync();
        return responsibility;
    }

    public async Task<Responsibility?> UpdateAsync(Responsibility responsibility)
    {
        var existingResponsibility = await _context.Responsibilities.FindAsync(responsibility.ResponsibilityId);
        if (existingResponsibility == null)
            return null;

        existingResponsibility.Name = responsibility.Name;
        existingResponsibility.Description = responsibility.Description;
        existingResponsibility.DepartmentId = responsibility.DepartmentId;

        await _context.SaveChangesAsync();
        return existingResponsibility;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var responsibility = await _context.Responsibilities.FindAsync(id);
        if (responsibility == null)
            return false;

        responsibility.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Responsibility>> GetByDepartmentIdAsync(Guid departmentId)
    {
        return await _context.Responsibilities
            .Where(r => r.DepartmentId == departmentId)
            .Include(r => r.PublisherResponsibilities)
                .ThenInclude(pr => pr.Publisher)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Responsibility>> GetAvailableForAssignmentAsync()
    {
        return await _context.Responsibilities
            .Include(r => r.Department)
            .OrderBy(r => r.Department.Name)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }
}
