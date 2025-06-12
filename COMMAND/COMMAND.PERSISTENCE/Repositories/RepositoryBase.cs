using CONTRACT.CONTRACT.DOMAIN.Abstractions.Entities;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace COMMAND.PERSISTENCE.Repositories;
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

    public async Task<TEntity?> FindByIdAsync(TKey id, CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(null, includeProperties)
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public async Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(null, includeProperties)
            .AsTracking()
            .SingleOrDefaultAsync(predicate, cancellationToken);
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