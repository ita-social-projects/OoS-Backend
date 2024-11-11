using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Base;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRepositoryBase{TKey, TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    protected EntityRepositoryBase(OutOfSchoolDbContext dbContext)
    {
        this.dbContext = dbContext;
        dbSet = this.dbContext.Set<TEntity>();
    }

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
                catch (Exception ex)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            });
    }

    /// <inheritdoc/>
    public virtual async Task RunInTransaction(Func<Task> operation)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    await operation().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
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
        => await dbSet
        .IncludeProperties(includeProperties)
        .ToListAsync();

    public virtual async Task<IEnumerable<TEntity>> GetByFilter(
        Expression<Func<TEntity, bool>> whereExpression,
        string includeProperties = "")
        => await this.dbSet
        .Where(whereExpression)
        .IncludeProperties(includeProperties)
        .ToListAsync()
        .ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetByFilterNoTracking(
        Expression<Func<TEntity, bool>> whereExpression,
        string includeProperties = "")
        => this.dbSet
        .Where(whereExpression)
        .IncludeProperties(includeProperties)
        .AsNoTracking();

    /// <inheritdoc/>
    public virtual Task<TEntity> GetById(TKey id) => dbSet.FirstOrDefaultAsync(x => x.Id.Equals(id));

    /// <inheritdoc/>
    public virtual Task<TEntity> GetByIdWithDetails(TKey id, string includeProperties = "")
        => dbSet.Where(x => x.Id.Equals(id)).IncludeProperties(includeProperties).FirstOrDefaultAsync();

    /// <inheritdoc/>
    public virtual async Task<TEntity> Update(TEntity entity)
    {
        dbContext.Entry(entity).State = EntityState.Modified;

        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return entity;
    }

    public async Task<TEntity> ReadAndUpdateWith<TDto>(TDto dto, Func<TDto, TEntity, TEntity> map)
        where TDto : IDto<TEntity, TKey>
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await this.GetById(dto.Id).ConfigureAwait(false);

        if (entity is null)
        {
            var name = typeof(TEntity).Name;
            throw new DbUpdateConcurrencyException($"Updating failed. {name} with Id = {dto.Id} doesn't exist in the system.");
        }

        return await this.Update(map(dto, entity)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual Task<int> Count(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        return whereExpression == null
            ? dbSet.CountAsync()
            : dbSet.Where(whereExpression).CountAsync();
    }

    /// <inheritdoc/>
    public virtual Task<bool> Any(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        return whereExpression == null
            ? dbSet.AnyAsync()
            : dbSet.Where(whereExpression).AnyAsync();
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> whereExpression = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection> orderBy = null,
        bool asNoTracking = false)
    {
        IQueryable<TEntity> query = dbSet;
        if (whereExpression != null)
        {
            query = query.Where(whereExpression);
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

        query = query.IncludeProperties(includeProperties);

        return query.If(asNoTracking, q => q.AsNoTracking());
    }

    public Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess = true,
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public int SaveChanges(bool acceptAllChangesOnSuccess = true)
    {
        return dbContext.SaveChanges();
    }
}