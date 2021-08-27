using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public interface IValidationService
    {
        Task<bool> UserIsProviderOwnerAsync(string userId, long providerId);

        Task<bool> UserIsWorkshopOwnerAsync(string userId, long workshopId);

        Task<bool> UserIsParentOwnerAsync(string userId, long parentId);

        Task<long> GetEntityIdAccordingToUserRole(string userId, string userRole);
    }
}
