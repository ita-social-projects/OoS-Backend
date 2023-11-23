using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IProviderRepository : ISensitiveEntityRepositorySoftDeleted<Provider>, IExistable<Provider>
{
    bool ExistsUserId(string id);

    Task<Provider> GetWithNavigations(Guid id);

    Task<IEnumerable<Provider>> GetAllWithDeleted();
}