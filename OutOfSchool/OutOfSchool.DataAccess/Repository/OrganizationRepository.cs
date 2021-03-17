using OutOfSchool.Services.Models;
using System.Linq;

namespace OutOfSchool.Services.Repository
{
    public class OrganizationRepository : EntityRepository<Organization>, IOrganizationRepository
    {
        private readonly OutOfSchoolDbContext db;

        public OrganizationRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// Checks entity elements for uniqueness
        /// </summary>
        /// <param name="entity">Entity to check.</param>
        /// <returns>Is entity already created or not.</returns>
        public bool Exists(Organization entity)
        {
            return db.Organizations.Any(x => x.EDRPOU == entity.EDRPOU || x.INPP == entity.INPP);
        }
    }
}