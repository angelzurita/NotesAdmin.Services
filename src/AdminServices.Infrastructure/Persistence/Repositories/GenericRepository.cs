using AdminServices.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdminServices.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
