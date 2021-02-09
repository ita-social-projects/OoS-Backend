﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public class EntityRepository<T> : IEntityRepository<T> where T : class, new()
    {
        private OutOfSchoolDbContext _dbContext;
        private DbSet<T> _dbSet;

        public EntityRepository(OutOfSchoolDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<T> Create(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet;
        }
        public IEnumerable<T> GetAllWithDetails(string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            return query;
        }
        
        public async Task<T> GetById(long id)
        {
            return await _dbSet.FindAsync(id).AsTask();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _dbContext.SaveChanges();
        }
    }
}
