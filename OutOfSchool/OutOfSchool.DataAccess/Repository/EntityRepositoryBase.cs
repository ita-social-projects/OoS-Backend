using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Extensions;

namespace OutOfSchool.Services.Repository
{
    /// <summary>
    /// Repository for accessing the database.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Entity type.</typeparam>
    public abstract class EntityRepositoryBase<TKey, TValue> : IEntityRepositoryBase<TKey, TValue>
        where TValue : class, new()
    {
        protected readonly OutOfSchoolDbContext dbContext;
        protected readonly DbSet<TValue> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        protected EntityRepositoryBase(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = this.dbContext.Set<TValue>();
        }

        public IUnitOfWork UnitOfWork => dbContext;

        /// <inheritdoc/>
        // TODO: Investigate why sometimes can add entities with their own ids, given with the entity instead of EF Core generate it
        public virtual async Task<TValue> Create(TValue entity)
        {
            await dbSet.AddAsync(entity).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return await Task.FromResult(entity).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TValue>> Create(IEnumerable<TValue> entities)
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
        public virtual async Task Delete(TValue entity)
        {
            dbContext.Entry(entity).State = EntityState.Deleted;

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TValue>> GetAll()
        {
            return await dbSet.ToListAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TValue>> GetAllWithDetails(string includeProperties = "")
        {
            IQueryable<TValue> query = dbSet;
            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<TValue>> GetByFilter(Expression<Func<TValue, bool>> predicate, string includeProperties = "")
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
        public virtual IQueryable<TValue> GetByFilterNoTracking(Expression<Func<TValue, bool>> predicate, string includeProperties = "")
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
        public virtual Task<TValue> GetById(TKey id) => dbSet.FindAsync(id).AsTask();

        /// <inheritdoc/>
        public virtual async Task<TValue> Update(TValue entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;

            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return entity;
        }

        /// <inheritdoc/>
        public virtual Task<int> Count(Expression<Func<TValue, bool>> where = null)
        {
            return where == null
                   ? dbSet.CountAsync()
                   : dbSet.Where(where).CountAsync();
        }

        /// <inheritdoc/>
        public virtual Task<bool> Any(Expression<Func<TValue, bool>> where = null)
        {
            return where == null
                     ? dbSet.AnyAsync()
                     : dbSet.Where(where).AnyAsync();
        }

        /// <inheritdoc/>
        public virtual IQueryable<TValue> Get(
            int skip = 0,
            int take = 0,
            string includeProperties = "",
            Expression<Func<TValue, bool>> where = null,
            Dictionary<Expression<Func<TValue, object>>, SortDirection> orderBy = null,
            bool asNoTracking = false)
        {
            IQueryable<TValue> query = (IQueryable<TValue>)dbSet;
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
}
