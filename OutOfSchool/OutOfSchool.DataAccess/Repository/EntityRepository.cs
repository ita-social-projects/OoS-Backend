using System;
using System.Collections.Generic;
using System.Linq;
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
            dbSet.Remove(entity);
            await dbContext.SaveChangesAsync();
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
        public async Task<T> Update(T entity)
        {
            dbSet.Update(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
    }
}
