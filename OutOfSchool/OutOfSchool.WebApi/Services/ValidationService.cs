using System;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.WebApi.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IProviderRepository providerRepository;
        private readonly IParentRepository parentRepository;
        private readonly IWorkshopRepository workshopRepository;

        public ValidationService(IProviderRepository providerRepository, IParentRepository parentRepository, IWorkshopRepository workshopRepository)
        {
            this.providerRepository = providerRepository;
            this.parentRepository = parentRepository;
            this.workshopRepository = workshopRepository;
        }

        public async Task<bool> UserIsProviderOwnerAsync(string userId, long providerId)
        {
            var providers = await providerRepository.GetByFilter(item => item.Id == providerId).ConfigureAwait(false);
            var provider = providers.Single();

            return string.Equals(userId, provider.UserId, StringComparison.Ordinal);
        }

        public async Task<bool> UserIsWorkshopOwnerAsync(string userId, long workshopId)
        {
            var workshops = await workshopRepository.GetByFilter(item => item.Id == workshopId, "Provider").ConfigureAwait(false);
            var workshop = workshops.Single();

            return string.Equals(userId, workshop.Provider.UserId, StringComparison.Ordinal);
        }

        public async Task<bool> UserIsParentOwnerAsync(string userId, long parentId)
        {
            var parents = await parentRepository.GetByFilter(item => item.Id == parentId).ConfigureAwait(false);
            var parent = parents.Single();

            return string.Equals(userId, parent.UserId, StringComparison.Ordinal);
        }

        public async Task<long> GetEntityIdAccordingToUserRoleAsync(string userId, string userRole)
        {
            if (string.Equals(userRole, Role.Parent.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var parents = await parentRepository.GetByFilter(item => item.UserId == userId).ConfigureAwait(false);
                var parent = parents.SingleOrDefault();
                if (parent is null)
                {
                    return default;
                }
                else
                {
                    return parent.Id;
                }
            }
            else if (string.Equals(userRole, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var providers = await providerRepository.GetByFilter(item => item.UserId == userId).ConfigureAwait(false);
                var provider = providers.SingleOrDefault();
                if (provider is null)
                {
                    return default;
                }
                else
                {
                    return provider.Id;
                }
            }
            else
            {
                return default;
            }
        }
    }
}
