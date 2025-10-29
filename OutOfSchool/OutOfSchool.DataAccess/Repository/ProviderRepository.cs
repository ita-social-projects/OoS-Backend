using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class ProviderRepository : SensitiveEntityRepositorySoftDeleted<Provider>, IProviderRepository
{
    public ProviderRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {      
    }

    /// <summary>
    /// Checks entity elements for uniqueness.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Bool.</returns>
    public bool SameExists(Provider entity) => dbSet.Any(x => !x.IsDeleted && x.Edrpou == entity.Edrpou);

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
                    var provider = dbSet.Add(providerEntity);
                    dbContext.SaveChanges();

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
        dbContext.Entry(entity).State = EntityState.Deleted;

        await dbContext.SaveChangesAsync();
    }

    public async Task<Provider> GetWithNavigations(Guid id)
    {
        return await dbSet
         .Include(x => x.Workshops)
         .ThenInclude(w => w.Applications)
         .Include(ws => ws.Contacts).ThenInclude(c => c.Emails)
         .Include(ws => ws.Contacts).ThenInclude(c => c.Phones)
         .Include(ws => ws.Contacts).ThenInclude(c => c.SocialNetworks)
         .Include(ws => ws.Contacts)
         .ThenInclude(c => c.Address)
         .ThenInclude(a => a.CATOTTG)
         .ThenInclude(c => c.Parent)
         .ThenInclude(c => c.Parent)
         .ThenInclude(c => c.Parent)
         .ThenInclude(c => c.Parent)
         .Include(p => p.ProviderSectionItems)
         .SingleOrDefaultAsync(provider => !provider.IsDeleted && provider.Id == id);
    }

    public async Task<List<int>> CheckExistsByEdrpous(Dictionary<int, string> edrpous)
    {
        var existingEdrpouIpn = await dbSet
            .Where(x => edrpous.Values.Contains(x.Edrpou))
            .Select(x => x.Edrpou)
            .ToListAsync();

        return edrpous.Where(x => existingEdrpouIpn.Contains(x.Value)).Select(x => x.Key).ToList();
    }

    public async Task<Guid?> GetIdByEdrpouAsync(string edrpou)
    {
        var providerId = await dbSet
            .Where(p => !p.IsDeleted && p.Edrpou == edrpou)
            .Select(p => p.Id)
            .SingleOrDefaultAsync();

        return providerId == Guid.Empty ? null : providerId;
    }

}