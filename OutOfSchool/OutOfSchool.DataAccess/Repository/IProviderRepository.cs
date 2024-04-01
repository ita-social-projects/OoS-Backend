using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IProviderRepository : ISensitiveEntityRepositorySoftDeleted<Provider>, IExistable<Provider>
{
    bool ExistsUserId(string id);

    Task<Provider> GetWithNavigations(Guid id);

    Task<List<Provider>> GetAllWithDeleted(DateTime updatedAfter, int from, int size);

    Task<int> CountWithDeleted(DateTime updatedAfter);

    Task<List<string>> CheckExistsByEdrpous(List<string> edrpous);

    Task<List<string>> CheckExistsByEmails(List<string> emails);
}