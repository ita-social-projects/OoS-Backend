using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderAdminRepository : IEntityRepository<ProviderAdmin>
    {
        Task<bool> IsExistProviderAdminWithUserIdAsync(long providerId, string userId);

        Task<bool> IsExistProviderWithUserIdAsync(long providerId, string userId);

        Task<int> GetNumberProviderAdminsAsync(long providerId);
    }
}
