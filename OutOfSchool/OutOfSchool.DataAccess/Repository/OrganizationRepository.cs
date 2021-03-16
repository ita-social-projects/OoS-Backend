using OutOfSchool.Services.Models;
using System.Linq;

namespace OutOfSchool.Services.Repository
{
    public class OrganizationRepository : EntityRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Checks entity elements for uniqueness
        /// </summary>
        /// <param name="entity">Entity to check.</param>
        /// <returns>Is entity already created or not.</returns>
        public bool IsUnique(Organization entity) => GetAll().Result.Any(x => x.EDRPOU == entity.EDRPOU || x.INPP == entity.INPP);
    }
}
