using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Congregations;

public class CongregationService : ICongregationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CongregationService> _logger;

    public CongregationService(ApplicationDbContext context,
        ILogger<CongregationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<IQueryable<Congregation>> GetAllAsync()
    {
        //return await _context.Congregations.AsNoTracking().ToListAsync();
        var items = _context.Congregations.AsQueryable();
        return await Task.FromResult(items);
    }

    public async Task<Congregation?> GetByIdAsync(Guid id)
    {
        return await _context.Congregations.FindAsync(id);
        //return await _context.Congregations.AsNoTracking().FirstOrDefaultAsync(c => c.CongregationId == id);
    }

    public async Task AddAsync(Congregation congregation)
    {
        var existingCongregation = await _context.Congregations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == congregation.Name.ToLower());
        
        if (existingCongregation != null)
        {
            _logger.LogWarning("Attempted to add a congregation with a duplicate name: {Name}", congregation.Name);
            return;
            //throw new InvalidOperationException("A congregation with the same name already exists.");
        }

        _context.Congregations.Add(congregation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Congregation congregation)
    {
        _context.Congregations.Update(congregation);
        await _context.SaveChangesAsync();
    }

    public async Task<Congregation> DeleteAsync(Guid id)
    {
        var congregation = await _context.Congregations.FindAsync(id);
        
        if (congregation == null)
        {
            _logger.LogWarning("Attempted to delete a congregation that does not exist: {Id}", id);
            throw new KeyNotFoundException("Congregation not found.");
        }
        
        congregation.IsDeleted = true;
        await _context.SaveChangesAsync();
        return congregation;
    }
}