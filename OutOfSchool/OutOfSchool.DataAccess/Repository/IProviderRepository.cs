using OutOfSchool.Services.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OutOfSchool.Services.Repository;

public interface IProviderRepository : ISensitiveEntityRepositorySoftDeleted<Provider>, IExistable<Provider>
{
    bool ExistsUserId(string id);

    Task<Provider> GetWithNavigations(Guid id);

    Task<IEnumerable<Provider>> GetAllWithDeleted(int? take = null);
}