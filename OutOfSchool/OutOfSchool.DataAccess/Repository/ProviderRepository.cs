using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ProviderRepository : EntityRepository<Provider>, IProviderRepository
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
        public bool SameExists(Provider entity) => db.Providers.Any(x => x.EdrpouIpn == entity.EdrpouIpn || x.Email == entity.Email);

        /// <summary>
        /// Checks if the user is trying to create second account.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>Bool.</returns>
        public bool ExistsUserId(string id) => db.Providers.Any(x => x.UserId == id);

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

            // TODO: Q: why we are using new entity instead of Provider.LegalAddress here and belowe ?
            db.Entry(new Address { Id = entity.LegalAddressId }).State = EntityState.Deleted;

            if (entity.ActualAddressId.HasValue
                && entity.ActualAddressId.Value != entity.LegalAddressId)
            {
                db.Entry(new Address { Id = entity.ActualAddressId.Value }).State = EntityState.Deleted;
            }

            await db.SaveChangesAsync();
        }
    }
}
