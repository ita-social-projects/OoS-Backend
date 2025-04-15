using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IProviderRepository : ISensitiveEntityRepositorySoftDeleted<Provider>, IExistable<Provider>
{
    Task<Provider> GetWithNavigations(Guid id);

    Task<List<int>> CheckExistsByEdrpous(Dictionary<int, string> edrpous);
}