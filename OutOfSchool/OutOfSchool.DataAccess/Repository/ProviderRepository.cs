using System.Linq;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ProviderRepository : EntityRepository<Provider>, IProviderRepository
    {
        private readonly OutOfSchoolDbContext context;

        public ProviderRepository(OutOfSchoolDbContext context)
         : base(context)
        {
            this.context = context;
        }

        /// <summary>
        /// Checks entity elements for uniqueness.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Bool.</returns>
        public bool Exists(Provider entity) => context.Providers.Any(x => x.EDRPOU == entity.EDRPOU || x.INPP == entity.INPP);
    }
}
