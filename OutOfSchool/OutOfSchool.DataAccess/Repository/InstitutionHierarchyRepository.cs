using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class InstitutionHierarchyRepository : EntityRepositorySoftDeleted<Guid, InstitutionHierarchy>, IInstitutionHierarchyRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionHierarchyRepository"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public InstitutionHierarchyRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// Add new element.
    /// </summary>
    /// <param name="entity">Entity to create.</param>
    /// <param name="subDirectionsIds">IDs List of subDirections.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<InstitutionHierarchy> Create(InstitutionHierarchy entity, List<long> subDirectionsIds)
    {
        entity.SubDirections = dbContext.SubDirections.Where(s => subDirectionsIds.Contains(s.Id)).ToList();

        await dbSet.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return await Task.FromResult(entity);
    }

    /// <summary>
    /// Update information about element.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <param name="subDirectionsIds">Long list of subDirections.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<InstitutionHierarchy> Update(InstitutionHierarchy entity, List<long> subDirectionsIds)
    {
        var newEntity = await GetById(entity.Id);

        dbContext.Entry(newEntity).CurrentValues.SetValues(entity);

        newEntity.SubDirections.RemoveAll(x => !subDirectionsIds.Contains(x.Id));
        var exceptSubDirectionsIds = subDirectionsIds.Where(p => newEntity.SubDirections.TrueForAll(x => x.Id != p));
        newEntity.SubDirections.AddRange(dbContext.SubDirections.Where(w => exceptSubDirectionsIds.Contains(w.Id)).ToList());

        dbContext.Entry(newEntity).State = EntityState.Modified;

        await this.dbContext.SaveChangesAsync();
        return newEntity;
    }
}
