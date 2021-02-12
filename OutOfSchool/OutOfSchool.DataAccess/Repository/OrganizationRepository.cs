using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public class OrganizationRepository : EntityRepository<Organization>
    {
        public OrganizationRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
        {

        }

        public void CheckForUniqueness(Organization entity)
        {
            var organizations = GetAll();
            foreach (var organization in organizations)
            {
                if(entity.EDRPOU == organization.EDRPOU || entity.INPP == organization.INPP)
                {
                    entity = null;
                }
            }         
        }
    }
}
