using System.Linq;
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
    }
}
