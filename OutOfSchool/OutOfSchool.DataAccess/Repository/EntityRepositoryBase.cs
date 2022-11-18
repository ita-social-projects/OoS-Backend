using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

/// <summary>
/// Repository for accessing the database.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
public abstract class EntityRepositoryBase<TKey, TEntity> : IEntityRepositoryBase<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    protected readonly OutOfSchoolDbContext dbContext;
    protected readonly DbSet<TEntity> dbSet;
    private IEntityRepositoryBase<TKey, TEntity> entityRepositoryBaseImplementation;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRepositoryBase{TKey, TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    protected EntityRepositoryBase(OutOfSchoolDbContext dbContext)
    {
        this.dbContext = dbContext;
        dbSet = this.dbContext.Set<TEntity>();
    }

    public IUnitOfWork UnitOfWork => dbContext;

    /// <inheritdoc/>
    // TODO: Investigate why sometimes can add entities with their own ids, given with the entity instead of EF Core generate it
    public virtual async Task<TEntity> Create(TEntity entity)
    {
        await dbSet.AddAsync(entity).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return await Task.FromResult(entity).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities)
    {
        await dbSet.AddRangeAsync(entities).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return await Task.FromResult(entities).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<T> RunInTransaction<T>(Func<Task<T>> operation)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await operation().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            });
    }

    /// <inheritdoc/>
    public virtual async Task Delete(TEntity entity)
    {
        dbContext.Entry(entity).State = EntityState.Deleted;

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> GetAll()
    {
        return await dbSet.ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> GetAllWithDetails(string includeProperties = "")
    {
        IQueryable<TEntity> query = dbSet;
        foreach (var includeProperty in includeProperties.Split(
                     new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetByFilter(Expression<Func<TEntity, bool>> predicate,
        string includeProperties = "")
    {
        var query = this.dbSet.Where(predicate);

        foreach (var includeProperty in includeProperties.Split(
                     new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetByFilterNoTracking(Expression<Func<TEntity, bool>> predicate,
        string includeProperties = "")
    {
        var query = this.dbSet.Where(predicate);

        foreach (var includeProperty in includeProperties.Split(
                     new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        return query.AsNoTracking();
    }

    /// <inheritdoc/>
    public virtual Task<TEntity> GetById(TKey id) => dbSet.FirstOrDefaultAsync(x => x.Id.Equals(id));

    /// <inheritdoc/>
    public virtual Task<TEntity> GetProviderStatusById(TKey id) => dbSet.FirstOrDefaultAsync(x => x.Id.Equals(id));

    /// <inheritdoc/>
    public virtual async Task<TEntity> Update(TEntity entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;

        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return entity;
    }

    /// <inheritdoc/>
    public virtual Task<int> Count(Expression<Func<TEntity, bool>> where = null)
    {
        return where == null
            ? dbSet.CountAsync()
            : dbSet.Where(where).CountAsync();
    }

    /// <inheritdoc/>
    public virtual Task<bool> Any(Expression<Func<TEntity, bool>> where = null)
    {
        return where == null
            ? dbSet.AnyAsync()
            : dbSet.Where(where).AnyAsync();
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> where = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection> orderBy = null,
        bool asNoTracking = false)
    {
        IQueryable<TEntity> query = dbSet;
        if (where != null)
        {
            query = query.Where(where);
        }

        if ((orderBy != null) && orderBy.Any())
        {
            var orderedData = orderBy.Values.First() == SortDirection.Ascending
                ? query.OrderBy(orderBy.Keys.First())
                : query.OrderByDescending(orderBy.Keys.First());

            foreach (var expression in orderBy.Skip(1))
            {
                orderedData = expression.Value == SortDirection.Ascending
                    ? orderedData.ThenBy(expression.Key)
                    : orderedData.ThenByDescending(expression.Key);
            }

            query = orderedData;
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take > 0)
        {
            query = query.Take(take);
        }

        foreach (var includeProperty in includeProperties.Split(
                     new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        return query.If(asNoTracking, q => q.AsNoTracking());
    }
}