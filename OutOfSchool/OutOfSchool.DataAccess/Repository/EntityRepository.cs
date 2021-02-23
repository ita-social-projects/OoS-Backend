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
        /// <param name="dbContext">OutOfSchoolDbContext</param>
        public EntityRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T> Create(T entity)
        {
            await this.dbSet.AddAsync(entity);
            await this.dbContext.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public async Task Delete(T entity)
        {
           
            this.dbSet.Remove(entity);        
            await this.dbContext.SaveChangesAsync();
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAll()
        {
            return await this.dbSet.ToListAsync();         
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllWithDetails(string includeProperties = "")
        {
            IQueryable<T> query = this.dbSet;
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
            return await this.dbSet.FindAsync(id).AsTask();
        }

        /// <inheritdoc/>
        public async Task<T> Update(T entity)
        {
            this.dbSet.Update(entity);
            await this.dbContext.SaveChangesAsync();
            return entity;
        }   
    }
}
