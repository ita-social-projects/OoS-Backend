using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderRepository : IEntityRepository<Provider>
    {
        bool IsUnique(Provider entity);
    }
}
