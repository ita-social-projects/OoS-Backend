using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Repository
{
    public interface IOrganizationRepository : IEntityRepository<Organization>
    {
        bool IsUnique(Organization entity);
    }
}
