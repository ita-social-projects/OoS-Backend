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
        private OutOfSchoolDbContext dbContext;
        private DbSet<T> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        public EntityRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = this.dbContext.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T> Create(T entity)
        {
            await dbSet.AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public async Task Delete(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Deleted;
            
            await this.dbContext.SaveChangesAsync();
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
            foreach (var includeProperty in includeProperties.Split(
            new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<T> GetById(long id)
        {
            return await dbSet.FindAsync(id).AsTask();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllWIthDetails(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            IQueryable<T> query = this.dbSet.Where(predicate);
            foreach (var includeProperty in includeProperties.Split(
            new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await Task.Run(() =>
            {
                return query.ToList();
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<T> Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
            
            await this.dbContext.SaveChangesAsync();
            return entity;
        }
    }
}
