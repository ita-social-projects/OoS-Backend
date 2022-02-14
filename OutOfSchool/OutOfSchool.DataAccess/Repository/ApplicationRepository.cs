﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    /// <summary>
    /// Repository for accessing the Application table in database.
    /// </summary>
    public class ApplicationRepository : EntityRepositoryBase<Guid, Application>, IApplicationRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRepository"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        public ApplicationRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
        }

        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="applications">Entity to create.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Application>> Create(IEnumerable<Application> applications)
        {
            await dbSet.AddRangeAsync(applications);
            await dbContext.SaveChangesAsync();

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

            dbContext.Entry(application).CurrentValues.SetValues(entity);

            dbContext.Entry(application).State = EntityState.Modified;

            await this.dbContext.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Get count of applications by workshop id.
        /// </summary>
        /// <param name="workshopId">Workshop id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public Task<int> GetCountByWorkshop(Guid workshopId)
        {
            var applications = dbSet.Where(a => a.WorkshopId == workshopId);

            return applications.CountAsync();
        }
    }
}
