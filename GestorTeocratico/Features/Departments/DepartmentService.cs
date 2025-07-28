using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using GestorTeocratico.Features.Publishers;
using GestorTeocratico.Features.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Departments;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DepartmentService> _logger;
    private readonly IPublisherService _publisherService;
    private readonly IResponsibilityService _responsibilityService;

    public DepartmentService(ApplicationDbContext context,
        ILogger<DepartmentService> logger,
        IPublisherService publisherService,
        IResponsibilityService responsibilityService)
    {
        _context = context;
        _logger = logger;
        _publisherService = publisherService;
        _responsibilityService = responsibilityService;
    }

    public async Task<IQueryable<Department>> GetAllAsync()
    {
        var items = _context.Departments
            .Include(d => d.ResponsiblePublisher)
            .Include(d => d.Responsibilities)
            .AsQueryable();
        return await Task.FromResult(items);
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments
            .Include(d => d.ResponsiblePublisher)
            .Include(d => d.Responsibilities)
            .FirstOrDefaultAsync(d => d.DepartmentId == id);
    }

    public async Task AddAsync(Department department)
    {
        var existingDepartment = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Name.ToLower() == department.Name.ToLower());

        if (existingDepartment != null)
        {
            _logger.LogWarning("Attempted to add a department with a duplicate name: {Name}", department.Name);
            return;
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task<Department> DeleteAsync(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department == null)
        {
            _logger.LogWarning("Attempted to delete a department that does not exist: {Id}", id);
            throw new KeyNotFoundException("Department not found.");
        }

        department.IsDeleted = true;
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateDepartmentResponsibilitiesAsync(Guid departmentId, IEnumerable<Guid> responsibilityIds)
    {
        var responsibilityIdsList = responsibilityIds.ToList(); // Evitar múltiple enumeración

        // Obtener todas las responsabilidades que actualmente pertenecen a este departamento
        var currentResponsibilities = await _context.Responsibilities
            .Where(r => r.DepartmentId == departmentId)
            .ToListAsync();

        // Obtener las responsabilidades que se quieren asignar a este departamento
        var newResponsibilities = await _context.Responsibilities
            .Where(r => responsibilityIdsList.Contains(r.ResponsibilityId))
            .ToListAsync();
        
        // Responsibilities to remove from this department
        var responsibilitiesToRemove = currentResponsibilities
            .Where(r => !responsibilityIdsList.Contains(r.ResponsibilityId));
        
        // Unassign removed responsibilities
        foreach (var responsibility in responsibilitiesToRemove)
        {
            responsibility.DepartmentId = null;
        }

        // Las responsabilidades que ya no estarán en este departamento necesitan ser
        // reasignadas a un departamento por defecto o crear un mecanismo diferente
        // Por ahora, solo actualizamos las que sí van a pertenecer a este departamento
        foreach (var responsibility in newResponsibilities)
        {
            responsibility.DepartmentId = departmentId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Publisher>> GetAvailablePublishersAsync()
    {
        return await _publisherService.GetAllAsync();
    }

    public async Task<IEnumerable<Responsibility>> GetAvailableResponsibilitiesAsync()
    {
        return await _responsibilityService.GetAllAsync();
    }
}

