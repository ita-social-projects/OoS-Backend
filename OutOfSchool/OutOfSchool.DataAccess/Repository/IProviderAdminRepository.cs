using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderAdminRepository : IEntityRepository<ProviderAdmin>
    {
        Task<bool> IsExistProviderAdminDeputyWithUserIdAsync(Guid providerId, string userId);

        Task<bool> IsExistProviderWithUserIdAsync(Guid providerId, string userId);

        Task<Provider> GetProviderWithUserIdAsync(string userId);

        Task<int> GetNumberProviderAdminsAsync(Guid providerId);

        Task<ProviderAdmin> GetByIdAsync(string id, Guid providerId);
    }
}