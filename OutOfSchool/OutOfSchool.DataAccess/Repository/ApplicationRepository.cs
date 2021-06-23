using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    /// <summary>
    /// Repository for accessing the Application table in database.
    /// </summary>
    public class ApplicationRepository : EntityRepository<Application>, IApplicationRepository
    {
        private readonly OutOfSchoolDbContext db;
        private readonly DbSet<Application> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRepository"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        public ApplicationRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
            dbSet = db.Set<Application>();
        }

        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="applications">Entity to create.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Application>> Create(IEnumerable<Application> applications)
        {
            await dbSet.AddRangeAsync(applications);
            await db.SaveChangesAsync();
            return await Task.FromResult(applications);
        }

        public new async Task<Application> Create(Application entity)
        {
            Application toCreate = dbSet.CreateProxy();
            db.Entry(toCreate).CurrentValues.SetValues(entity);
            await dbSet.AddAsync(toCreate);
            await db.SaveChangesAsync();
            return await Task.FromResult(toCreate);
        }
    }
}
