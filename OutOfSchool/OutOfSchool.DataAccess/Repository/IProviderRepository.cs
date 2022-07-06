using System;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IProviderRepository : ISensitiveEntityRepository<Provider>, IExistable<Provider>
{
    bool ExistsUserId(string id);
}