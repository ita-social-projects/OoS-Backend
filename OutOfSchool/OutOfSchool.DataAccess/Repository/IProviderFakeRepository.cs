using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IProviderFakeRepository<T> : ISensitiveEntityRepositorySoftDeleted<T>, IExistable<T> where T : Provider, new()
{
    bool ExistsUserId(string id);

    Task<T> GetWithNavigations(Guid id);

    Task<List<T>> GetAllWithDeleted(DateTime updatedAfter, int from, int size);

    Task<int> CountWithDeleted(DateTime updatedAfter);
}