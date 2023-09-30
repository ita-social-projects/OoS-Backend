using OutOfSchool.Services.Models;
using System.Threading.Tasks;
using System;

namespace OutOfSchool.Services.Repository;

public interface IProviderRepository : ISensitiveEntityRepositorySoftDeleted<Provider>, IExistable<Provider>
{
    bool ExistsUserId(string id);

    Task<Provider> GetWithNavigations(Guid id);
}