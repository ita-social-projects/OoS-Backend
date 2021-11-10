using System;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderRepository : ISensitiveEntityRepository<Provider>, IExistable<Provider>
    {
        IUnitOfWork UnitOfWork { get; }

        bool ExistsUserId(string id);
    }
}
