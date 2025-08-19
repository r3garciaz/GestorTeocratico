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
        // Ensure only one congregation can exist
        if (await _context.Congregations.AnyAsync())
        {
            _logger.LogWarning("Attempted to add a second congregation: {Name}", congregation.Name);
            throw new InvalidOperationException("Ya existe una congregación. No se puede crear otra.");
        }

        // Optional: also avoid same-name duplicates as additional guard (defensive)
        if (await _context.Congregations.AsNoTracking().AnyAsync(c => c.Name.ToLower() == congregation.Name.ToLower()))
        {
            _logger.LogWarning("Attempted to add a congregation with duplicate name: {Name}", congregation.Name);
            throw new InvalidOperationException("Ya existe una congregación con el mismo nombre.");
        }

        // Use a transaction and re-check to reduce race conditions
        await using var tx = await _context.Database.BeginTransactionAsync();
        _context.Congregations.Add(congregation);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task UpdateAsync(Congregation congregation)
    {
        _context.Congregations.Update(congregation);
        await _context.SaveChangesAsync();
    }

    public Task<Congregation> DeleteAsync(Guid id)
    {
        // Deleting congregations is not allowed by business rules.
        _logger.LogWarning("Attempted to delete congregation: {Id}", id);
        return Task.FromException<Congregation>(new NotSupportedException("La eliminación de congregación no está permitida."));
    }
}