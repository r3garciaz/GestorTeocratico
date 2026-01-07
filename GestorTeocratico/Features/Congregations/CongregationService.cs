using GestorTeocratico.Data;
using GestorTeocratico.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorTeocratico.Features.Congregations;

public class CongregationService : ICongregationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<CongregationService> _logger;

    public CongregationService(IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<CongregationService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Congregation>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Congregations.ToListAsync();
    }

    public async Task<Congregation?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Congregations.FindAsync(id);
        //return await context.Congregations.AsNoTracking().FirstOrDefaultAsync(c => c.CongregationId == id);
    }

    public async Task AddAsync(Congregation congregation)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Ensure only one congregation can exist
        if (await context.Congregations.AnyAsync())
        {
            _logger.LogWarning("Attempted to add a second congregation: {Name}", congregation.Name);
            throw new InvalidOperationException("Ya existe una congregación. No se puede crear otra.");
        }

        // Optional: also avoid same-name duplicates as additional guard (defensive)
        if (await context.Congregations.AsNoTracking().AnyAsync(c => c.Name.ToLower() == congregation.Name.ToLower()))
        {
            _logger.LogWarning("Attempted to add a congregation with duplicate name: {Name}", congregation.Name);
            throw new InvalidOperationException("Ya existe una congregación con el mismo nombre.");
        }

        // Use a transaction and re-check to reduce race conditions
        await using var tx = await context.Database.BeginTransactionAsync();
        context.Congregations.Add(congregation);
        await context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task UpdateAsync(Congregation congregation)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Congregations.Update(congregation);
        await context.SaveChangesAsync();
    }

    public Task<Congregation> DeleteAsync(Guid id)
    {
        // Deleting congregations is not allowed by business rules.
        _logger.LogWarning("Attempted to delete congregation: {Id}", id);
        return Task.FromException<Congregation>(new NotSupportedException("La eliminación de congregación no está permitida."));
    }
}