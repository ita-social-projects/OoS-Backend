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
        private readonly OutOfSchoolDbContext context;
        private readonly DbSet<T> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
        /// </summary>
        /// <param name="context">OutOfSchoolDbContext.</param>
        public EntityRepository(OutOfSchoolDbContext context)
        {
            this.context = context;
            dbSet = this.context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T> Create(T entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public async Task Delete(T entity)
        {
            context.Entry(entity).State = EntityState.Deleted;

            await this.context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllWithDetails(string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            return await GetWithDetails(query, includeProperties);
        }

        public async Task<IEnumerable<T>> GetByFilter(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            IQueryable<T> query = dbSet.Where(predicate);

            return await GetWithDetails(query, includeProperties);
        }

        /// <inheritdoc/>
        public async Task<T> GetById(long id)
        {
            return await dbSet.FindAsync(id).AsTask();
        }

        /// <inheritdoc/>
        public async Task<T> Update(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await this.context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<int> Count(Expression<Func<T, bool>> where = null)
        {
            if (where == null)
            {
                return await dbSet.CountAsync().ConfigureAwait(false);
            }

            return await dbSet.Where(@where).CountAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get<TOrderKey>(
            int skip = 0, 
            int take = 0, 
            string includeProperties = "", 
            Expression<Func<T, bool>> where = null,
            Expression<Func<T, TOrderKey>> orderBy = null, 
            bool ascending = true)
        {
            IQueryable<T> query = dbSet;
            
            if (where != null)
            {
                query = query.Where(where);
            }

            if (orderBy != null)
            {
                query = @ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
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
                new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query;
        }

        private static async Task<IEnumerable<T>> GetWithDetails(IQueryable<T> query, string includeProperties = "")
        {
            foreach (var includeProperty in includeProperties.Split(
                new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            
            return await query.ToListAsync();
        }
    }
}