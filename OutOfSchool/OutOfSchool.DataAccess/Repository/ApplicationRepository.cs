using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ApplicationRepository : EntityRepository<Application>, IApplicationRepository
    {
        private readonly OutOfSchoolDbContext db;
        private readonly DbSet<Application> dbSet;

        public ApplicationRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
            dbSet = db.Set<Application>();
        }

        public async Task<IEnumerable<Application>> Create(IEnumerable<Application> applications)
        {
            await dbSet.AddRangeAsync(applications);
            await db.SaveChangesAsync();
            return await Task.FromResult(applications);
        }
    }
}
