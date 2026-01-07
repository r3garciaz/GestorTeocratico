using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

public class DepartmentService : IDepartmentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<DepartmentService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Departments
            .Include(d => d.ResponsiblePublisher)
            .Include(d => d.Responsibilities)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Departments
            .Include(d => d.ResponsiblePublisher)
            .Include(d => d.Responsibilities)
            .FirstOrDefaultAsync(d => d.DepartmentId == id);
    }

    public async Task AddAsync(Department department)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingDepartment = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Name.ToLower() == department.Name.ToLower());

        if (existingDepartment != null)
        {
            _logger.LogWarning("Attempted to add a department with a duplicate name: {Name}", department.Name);
            return;
        }

        context.Departments.Add(department);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Departments.Update(department);
        await context.SaveChangesAsync();
    }

    public async Task<Department> DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var department = await context.Departments.FindAsync(id);

        if (department == null)
        {
            _logger.LogWarning("Attempted to delete a department that does not exist: {Id}", id);
            throw new KeyNotFoundException("Department not found.");
        }

        department.IsDeleted = true;
        await context.SaveChangesAsync();
        return department;
    }

    public async Task<IEnumerable<Publisher>> GetAvailablePublishersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Publishers
            .Where(p => p.Privilege.HasValue)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Responsibility>> GetAvailableResponsibilitiesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Responsibilities
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task UpdateDepartmentResponsibilitiesAsync(Guid departmentId, IEnumerable<Guid> responsibilityIds)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var responsibilityIdsList = responsibilityIds.ToList();

        // Obtener todas las responsabilidades que actualmente pertenecen a este departamento
        var currentResponsibilities = await context.Responsibilities
            .Where(r => r.DepartmentId == departmentId)
            .ToListAsync();

        // Remover el departamento de las responsabilidades que ya no están seleccionadas
        foreach (var responsibility in currentResponsibilities)
        {
            if (!responsibilityIdsList.Contains(responsibility.ResponsibilityId))
            {
                responsibility.DepartmentId = null;
            }
        }

        // Obtener las responsabilidades que se quieren asignar a este departamento
        var newResponsibilities = await context.Responsibilities
            .Where(r => responsibilityIdsList.Contains(r.ResponsibilityId))
            .ToListAsync();

        // Asignar el departamento a las nuevas responsabilidades seleccionadas
        foreach (var responsibility in newResponsibilities)
        {
            responsibility.DepartmentId = departmentId;
        }

        await context.SaveChangesAsync();
    }
}
