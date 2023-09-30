﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ProviderRepository : SensitiveEntityRepositorySoftDeleted<Provider>, IProviderRepository
{
    private readonly OutOfSchoolDbContext db;

    public ProviderRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <summary>
    /// Checks entity elements for uniqueness.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Bool.</returns>
    public bool SameExists(Provider entity) => db.Providers.Any(x => !x.IsDeleted && (x.EdrpouIpn == entity.EdrpouIpn || x.Email == entity.Email));

    /// <summary>
    /// Checks if the user is trying to create second account.
    /// </summary>
    /// <param name="id">User id.</param>
    /// <returns>Bool.</returns>
    public bool ExistsUserId(string id) => db.Providers.Any(x => !x.IsDeleted && x.UserId == id);

    /// <summary>
    /// Tries to insert a new <see cref="Provider"/> entity with all related objects into the database.
    /// Runs insert operation inside a transaction.
    /// </summary>
    /// <param name="providerEntity"><see cref="Provider"/> entity to insert into database.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public new async Task<Provider> Create(Provider providerEntity)
    {
        return await RunInTransaction(
                () =>
                {
                    var provider = db.Providers.Add(providerEntity);
                    db.SaveChanges();

                    return Task.FromResult(provider.Entity);
                })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Delete element.
    /// </summary>
    /// <param name="entity">Entity to delete.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public new async Task Delete(Provider entity)
    {
        db.Entry(entity).State = EntityState.Deleted;
        if (entity.LegalAddress != null)
        {
            db.Entry(entity.LegalAddress).State = EntityState.Deleted;
        }

        if (entity.ActualAddressId.HasValue
            && entity.ActualAddressId.Value != entity.LegalAddressId
            && entity.ActualAddress != null)
        {
            db.Entry(entity.ActualAddress).State = EntityState.Deleted;
        }

        await db.SaveChangesAsync();
    }

    public async Task<Provider> GetWithNavigations(Guid id)
    {
        return await db.Providers
         .Include(x => x.LegalAddress) // TODO: Doesn't work softDelete using Include, only using loop below in Delete() method
         .Include(x => x.Workshops)
         .ThenInclude(w => w.Applications)
         .SingleOrDefaultAsync(provider => !provider.IsDeleted && provider.Id == id);
    }
}