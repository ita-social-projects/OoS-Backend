using System;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.WebApi.Services;

public class ValidationService : IValidationService
{
    private readonly IProviderRepository providerRepository;
    private readonly IParentRepository parentRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IProviderAdminRepository providerAdminRepository;

    public ValidationService(
        IProviderRepository providerRepository,
        IParentRepository parentRepository,
        IWorkshopRepository workshopRepository,
        IProviderAdminRepository providerAdminRepository)
    {
        this.providerRepository = providerRepository;
        this.parentRepository = parentRepository;
        this.workshopRepository = workshopRepository;
        this.providerAdminRepository = providerAdminRepository;
    }

    /// <inheritdoc/>>
    public async Task<bool> UserIsProviderOwnerAsync(string userId, Guid providerId)
    {
        var providers = await providerRepository.GetByFilter(item => item.Id == providerId).ConfigureAwait(false);
        var provider = providers.SingleOrDefault();

        return provider is null ? false : userId.Equals(provider.UserId, StringComparison.Ordinal);
    }

    /// <inheritdoc/>>
    public async Task<bool> UserIsWorkshopOwnerAsync(string userId, Guid workshopId, Subrole userSubrole = Subrole.None)
    {
        var workshops = await workshopRepository.GetByFilter(item => item.Id == workshopId, nameof(Workshop.Provider)).ConfigureAwait(false);
        var workshop = workshops.SingleOrDefault();

        if (workshop is null || userId is null)
        {
            return false;
        }

        if (userSubrole == Subrole.ProviderDeputy)
        {
            var providersDeputies = await providerAdminRepository.GetByFilter(p => p.ProviderId == workshop.ProviderId
                && p.IsDeputy
                && p.UserId == userId).ConfigureAwait(false);
            return providersDeputies.Any();
        }

        if (userSubrole == Subrole.ProviderAdmin)
        {
            var providersAdmins = await providerAdminRepository.GetByFilter(p => p.ManagedWorkshops.Any(w => w.Id == workshopId)
                && !p.IsDeputy
                && p.UserId == userId).ConfigureAwait(false);

            return providersAdmins.Any();
        }

        return userId.Equals(workshop.Provider.UserId, StringComparison.Ordinal);
    }

    /// <inheritdoc/>>
    public async Task<bool> UserIsParentOwnerAsync(string userId, Guid parentId)
    {
        var parents = await parentRepository.GetByFilter(item => item.Id == parentId).ConfigureAwait(false);
        var parent = parents.SingleOrDefault();

        return parent is null ? false : userId.Equals(parent.UserId, StringComparison.Ordinal);
    }

    /// <inheritdoc/>>
    public async Task<Guid> GetParentOrProviderIdByUserRoleAsync(string userId, Role userRole)
    {
        if (userRole == Role.Parent)
        {
            var parents = await parentRepository.GetByFilter(item => item.UserId == userId).ConfigureAwait(false);
            var parent = parents.SingleOrDefault();

            return parent is null ? default : parent.Id;
        }

        if (userRole == Role.Provider)
        {
            var providers = await providerRepository.GetByFilter(item => item.UserId == userId).ConfigureAwait(false);
            if (providers.Any())
            {
                return providers.Single().Id;
            }
            else
            {
                var providersDeputies = await providerAdminRepository.GetByFilter(a => a.UserId == userId && a.IsDeputy).ConfigureAwait(false);
                if (providersDeputies.Any())
                {
                    return providersDeputies.Single().ProviderId;
                }

                return default;
            }
        }

        return default;
    }
}