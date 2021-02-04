using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public class ChildService
    {
        private OutOfSchoolDbContext _dbContext;
        private DbSet<Child> _dbSet;

        public ChildService(OutOfSchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Child> Create(Child child)
        {
            await _dbSet.AddAsync(child);
            await _dbContext.SaveChangesAsync();
            return await Task.FromResult(child);
        }
    }
}
