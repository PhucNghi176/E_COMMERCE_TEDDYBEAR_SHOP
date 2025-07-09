using System.Linq.Expressions;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace QUERY.PERSISTENCE.Repositories;
public class RepositoryBase<TEntity, TKey>(ApplicationDbContext dbContext) : IRepositoryBase<TEntity, TKey>, IDisposable
    where TEntity : Entity<TKey>
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var items = dbContext.Set<TEntity>().AsNoTracking(); // Importance Always include AsNoTracking for Query Side
        if (includeProperties != null)
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));

        if (predicate is not null)
            items = items.Where(predicate);

        return items;
    }

    // Optimized: Keep AsNoTracking for query side, use direct query for better performance
    public async Task<TEntity> FindByIdAsync(TKey id, CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var query = dbContext.Set<TEntity>().AsNoTracking();
        
        if (includeProperties != null)
            query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            
        return await query.SingleOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }

    // Optimized: Keep AsNoTracking for query side
    public async Task<TEntity> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var query = dbContext.Set<TEntity>().AsNoTracking();
        
        if (includeProperties != null)
            query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            
        if (predicate is not null)
            query = query.Where(predicate);
            
        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public void Add(TEntity entity)
    {
        _ = dbContext.Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        dbContext.AddRange(entities);
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        dbContext.UpdateRange(entities);
    }

    public void Remove(TEntity entity)
    {
        _ = dbContext.Set<TEntity>().Remove(entity);
    }

    public void RemoveMultiple(List<TEntity> entities)
    {
        dbContext.Set<TEntity>().RemoveRange(entities);
    }

    public void Update(TEntity entity)
    {
        _ = dbContext.Set<TEntity>().Update(entity);
    }
}