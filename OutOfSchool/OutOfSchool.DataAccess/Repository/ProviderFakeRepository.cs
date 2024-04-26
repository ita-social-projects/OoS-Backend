using System;
using System.Collections.Generic;
//using System.Collections.Generic;
using System.Linq;
//using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ProviderFakeRepository<T> : SensitiveEntityRepositorySoftDeleted<T>, IProviderFakeRepository<T> where T : Provider, new()
{
    private readonly OutOfSchoolDbContext db;
    private readonly DbSet<T> providers;

    public ProviderFakeRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
        providers = db.Set<T>();
    }

    /// <summary>
    /// Checks entity elements for uniqueness.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Bool.</returns>
    public bool SameExists(T entity) => providers.Any(x => !x.IsDeleted && (x.EdrpouIpn == entity.EdrpouIpn || x.Email == entity.Email));

    /// <summary>
    /// Checks if the user is trying to create second account.
    /// </summary>
    /// <param name="id">User id.</param>
    /// <returns>Bool.</returns>
    public bool ExistsUserId(string id) => providers.Any(x => !x.IsDeleted && x.UserId == id);

    /// <summary>
    /// Tries to insert a new <see cref="Provider"/> entity with all related objects into the database.
    /// Runs insert operation inside a transaction.
    /// </summary>
    /// <param name="providerEntity"><see cref="Provider"/> entity to insert into database.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public new async Task<T> Create(T providerEntity)
    {
        return await RunInTransaction(
                () =>
                {
                    var provider = providers.Add(providerEntity);
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
    public new async Task Delete(T entity)
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

    public async Task<T> GetWithNavigations(Guid id)
    {
        return await providers
         .Include(x => x.LegalAddress) // TODO: Doesn't work softDelete using Include, only using loop below in Delete() method
         .Include(x => x.Workshops)
         .ThenInclude(w => w.Applications)
         .SingleOrDefaultAsync(provider => !provider.IsDeleted && provider.Id == id);
    }

    public Task<List<T>> GetAllWithDeleted(DateTime updatedAfter, int from, int size)
    {
        IQueryable<T> query = providers;

        query = updatedAfter == default
            ? query.Where(provider => !provider.IsDeleted)
            : query.Where(provider => provider.UpdatedAt > updatedAfter || provider.Workshops.Any(w => w.UpdatedAt > updatedAfter));

        return query.Skip(from).Take(size).ToListAsync();
    }

    public Task<int> CountWithDeleted(DateTime updatedAfter)
    {
        IQueryable<T> query = dbSet;

        query = updatedAfter == default
        ? query.Where(provider => !provider.IsDeleted)
        : query.Where(provider => provider.UpdatedAt > updatedAfter || provider.Workshops.Any(w => w.UpdatedAt > updatedAfter));

        return query.CountAsync();
    }
}