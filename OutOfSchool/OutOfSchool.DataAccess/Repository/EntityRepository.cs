using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Repository
{
    /// <summary>
    /// Repository for accessing the database.
    /// </summary>
    /// <typeparam name="T">Entity.</typeparam>
    public class EntityRepository<T> : IEntityRepository<T>
        where T : class, new()
    {
        private readonly OutOfSchoolDbContext dbContext;
        private readonly DbSet<T> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        public EntityRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = this.dbContext.Set<T>();
        }

        // TODO: make all public methods virtual

        /// <inheritdoc/>
        public async Task<T> Create(T entity)
        {
            await dbSet.AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public async Task<T> RunInTransaction(Func<Task<T>> operation)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Deleted;

            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllWithDetails(string includeProperties = "")
        {
            IQueryable<T> query = dbSet;
            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByFilter(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            var query = this.dbSet.Where(predicate);

            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc/>
        public IQueryable<T> GetByFilterNoTracking(Expression<Func<T, bool>> predicate, string includeProperties = "")
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
        public async Task<T> GetById(long id)
        {
            return await dbSet.FindAsync(id).AsTask();
        }

        /// <inheritdoc/>
        public async Task<T> Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;

            await this.dbContext.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<int> Count(Expression<Func<T, bool>> where = null)
        {
            if (where == null)
            {
                return await dbSet.CountAsync().ConfigureAwait(false);
            }
            else
            {
                return await dbSet.Where(where).CountAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public IQueryable<T> Get<TOrderKey>(
        int skip = 0, int take = 0, string includeProperties = "", Expression<Func<T, bool>> where = null, Expression<Func<T, TOrderKey>> orderBy = null, bool ascending = true)
        {
            IQueryable<T> query = (IQueryable<T>)dbSet;
            if (where != null)
            {
                query = query.Where(where);
            }

            if (orderBy != null)
            {
                if (ascending)
                {
                    query = query.OrderBy(orderBy);
                }
                else
                {
                    query = query.OrderByDescending(orderBy);
                }
            }

            if (skip != 0)
            {
                query = query.Skip(skip);
            }

            if (take != 0)
            {
                query = query.Take(take);
            }

            foreach (var includeProperty in includeProperties.Split(
           new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query;
        }
    }
}
