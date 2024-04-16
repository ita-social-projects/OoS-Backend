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

    Task<List<int>> CheckExistsByEdrpous(Dictionary<int, string> edrpous);

    Task<List<int>> CheckExistsByEmails(Dictionary<int, string> emails);
}