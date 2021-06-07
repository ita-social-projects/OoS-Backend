using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Update information about element.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was updated.</returns>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        public new async Task<Application> Update(Application entity)
        {
            var application = dbSet.Find(entity.Id);

            db.Entry(application).CurrentValues.SetValues(entity);

            db.Entry(application).State = EntityState.Modified;

            await this.db.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Get count of applications by workshop id.
        /// </summary>
        /// <param name="workshopId">Workshop id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public Task<int> GetCountByWorkshop(long workshopId)
        {
            var applications = dbSet.Where(a => a.WorkshopId == workshopId);

            return applications.CountAsync();
        }
    }
}
