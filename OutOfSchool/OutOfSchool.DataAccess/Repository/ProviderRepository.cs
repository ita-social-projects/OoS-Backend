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
        public bool Exists(Provider entity) => db.Providers.Any(x => x.EdrpouIpn == entity.EdrpouIpn || x.Email == entity.Email);

        /// <summary>
        /// Checks if the user is trying to create second account.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>Bool.</returns>
        public bool ExistsUserId(string id) => db.Providers.Any(x => x.UserId == id);

        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="entity">Entity to create.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public new async Task<Provider> Create(Provider entity)
        {
            var legalAddress = await db.Addresses.AddAsync(entity.LegalAddress).ConfigureAwait(false);
            await db.SaveChangesAsync();

            entity.LegalAddressId = legalAddress.Entity.Id;

            if (entity.ActualAddress == null || entity.ActualAddress.Equals(entity.LegalAddress))
            {
                entity.ActualAddressId = legalAddress.Entity.Id;
            }
            else
            {
                entity.ActualAddress.Id = default;
                var actualAddress = await db.Addresses.AddAsync(entity.ActualAddress).ConfigureAwait(false);
                await db.SaveChangesAsync();
                entity.ActualAddressId = actualAddress.Entity.Id;
            }

            entity.ActualAddress = default;
            entity.LegalAddress = default;

            await db.Providers.AddAsync(entity);
            await db.SaveChangesAsync();

            return await Task.FromResult(entity);
        }

        /// <summary>
        /// Delete element.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public new async Task Delete(Provider entity)
        {
            db.Entry(entity).State = EntityState.Deleted;

            if (entity.LegalAddressId == entity.ActualAddressId)
            {             
                db.Entry(new Address { Id = entity.LegalAddressId }).State = EntityState.Deleted;
            }
            else
            {
                db.Entry(new Address { Id = entity.LegalAddressId }).State = EntityState.Deleted;
                db.Entry(new Address { Id = entity.ActualAddressId }).State = EntityState.Deleted;
            }

            await db.SaveChangesAsync();
        }
    }
}
