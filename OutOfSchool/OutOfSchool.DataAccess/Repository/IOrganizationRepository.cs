using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public interface IOrganizationRepository : IEntityRepository<Organization>
    {
        bool Exists(Organization entity);
    }
}
