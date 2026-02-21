using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Util;

namespace OutOfSchool.Services.Repository;

/// <summary>
/// Repository for accessing the Application table in database.
/// </summary>
public class ApplicationRepository : EntityRepositorySoftDeleted<Guid, Application>, IApplicationRepository
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
    /// Update information about element.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was updated.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public new Task<Application> Update(Application entity)
    {
        return Update(entity, null);
    }

    /// <summary>
    /// Update information about element.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <param name="onSaveChanges">Called before SaveChangesAsync method is invoked.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was updated.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<Application> Update(Application entity, Action<Application> onSaveChanges)
    {
        var application = await GetById(entity.Id);

        dbContext.Entry(application).CurrentValues.SetValues(entity);

        dbContext.Entry(application).State = EntityState.Modified;

        onSaveChanges?.Invoke(application);

        await dbContext.SaveChangesAsync();
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

    /// <summary>
    /// Changes status to StudyingForYears of all applications where status is Approved.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<int> UpdateAllApprovedApplications()
    {
        // in latest versions EF multiple update should be implemented
        return await dbContext.Database
            .ExecuteSqlRawAsync(
                "UPDATE `Applications` SET `Status` = {0} WHERE NOT (`IsDeleted`) AND (`Status` = {1})",
                (int)ApplicationStatus.StudyingForYears,
                (int)ApplicationStatus.Approved)
            .ConfigureAwait(false);
    }

    public async Task DeleteChildApplications(Guid childId)
    {
        List<Application> applicationsToDelete = await dbContext.Applications
                                                    .Where(app => app.ChildId == childId).ToListAsync();

        if (applicationsToDelete.Any())
        {
            dbContext.Applications.RemoveRange(applicationsToDelete);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public Task<List<WorkshopTakenSeats>> CountTakenSeatsForWorkshops(List<Guid> workshopIds)
    {
        return dbSet
            .Where(x =>
                (x.Status == ApplicationStatus.Approved || x.Status == ApplicationStatus.StudyingForYears)
                && !x.IsDeleted
                && x.Child != null
                && !x.Child.IsDeleted
                && x.Parent != null
                && !x.Parent.IsDeleted
                && workshopIds.Contains(x.WorkshopId))
            .GroupBy(a => a.WorkshopId)
            .Select(g => new WorkshopTakenSeats(g.Key, g.Count()))
            .ToListAsync();
    }
}