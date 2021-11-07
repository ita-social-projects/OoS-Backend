using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderAdminRepository : IEntityRepository<ProviderAdmin>
    {
        Task<bool> IsExistProviderAdminWithUserIdAsync(Guid providerId, string userId);

        Task<bool> IsExistProviderWithUserIdAsync(Guid providerId, string userId);

        Task<int> GetNumberProviderAdminsAsync(Guid providerId);
    }
}
