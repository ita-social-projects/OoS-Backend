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
    private readonly OutOfSchoolDbContext db;

    public ProviderRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        this.db = dbContext;        
    }

    /// <summary>
    /// Checks entity elements for uniqueness.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Bool.</returns>
    public bool SameExists(Provider entity) => db.Providers.Any(x => !x.IsDeleted && x.Edrpou == entity.Edrpou);

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

        await db.SaveChangesAsync();
    }

    public async Task<Provider> GetWithNavigations(Guid id)
    {
        return await db.Providers
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
         .SingleOrDefaultAsync(provider => !provider.IsDeleted && provider.Id == id);
    }

    public async Task<List<int>> CheckExistsByEdrpous(Dictionary<int, string> edrpous)
    {
        var existingEdrpouIpn = await db.Providers
            .Where(x => edrpous.Values.Contains(x.Edrpou))
            .Select(x => x.Edrpou)
            .ToListAsync();

        return edrpous.Where(x => existingEdrpouIpn.Contains(x.Value)).Select(x => x.Key).ToList();
    }

    public async Task<List<int>> CheckExistsByEmails(Dictionary<int, string> emails)
    {
        var existingEmails = await db.Providers
            .Include(x => x.User)
            .Where(x => emails.Values.Contains(x.User.Email))
            .Select(x => x.User.Email)
            .ToListAsync();

        return emails.Where(x => existingEmails.Contains(x.Value)).Select(x => x.Key).ToList();
    }
}