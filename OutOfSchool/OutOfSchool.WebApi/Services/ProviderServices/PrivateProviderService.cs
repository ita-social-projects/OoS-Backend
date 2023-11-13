using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.ProviderServices;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements interface for functionality for Private Provider.
/// </summary>
public class PrivateProviderService : ProviderService, IPrivateProviderService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrivateProviderService"/> class.
    /// </summary>
    /// <param name="providerRepository">Provider repository.</param>
    /// <param name="usersRepository">UsersRepository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="addressRepository">AddressRepository.</param>
    /// <param name="workshopServiceCombiner">WorkshopServiceCombiner.</param>
    /// <param name="providerAdminRepository">Provider admin repository.</param>
    /// <param name="providerImagesService">Images service.</param>
    /// <param name="changesLogService">ChangesLogService.</param>
    /// <param name="notificationService">Notification service.</param>
    /// <param name="providerAdminService">Service for getting provider admins and deputies.</param>
    /// <param name="institutionAdminRepository">Repository for getting ministry admins.</param>
    /// <param name="currentUserService">Service for manage current user.</param>
    /// <param name="ministryAdminService">Service for manage ministry admin.</param>
    /// <param name="regionAdminService">Service for managing region admin rigths.</param>
    /// <param name="codeficatorService">Codeficator service.</param>
    /// <param name="regionAdminRepository">RegionAdminRepository</param>
    /// <param name="averageRatingService">Average rating service.</param>
    /// <param name="areaAdminService">Service for manage area admin.</param>
    public PrivateProviderService(
        IProviderRepository providerRepository,
        IEntityRepositorySoftDeleted<string, User> usersRepository,
        ILogger<ProviderService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        IEntityRepositorySoftDeleted<long, Address> addressRepository,
        IWorkshopServicesCombiner workshopServiceCombiner,
        IProviderAdminRepository providerAdminRepository,
        IImageDependentEntityImagesInteractionService<Provider> providerImagesService,
        IChangesLogService changesLogService,
        INotificationService notificationService,
        IProviderAdminService providerAdminService,
        IInstitutionAdminRepository institutionAdminRepository,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        ICodeficatorService codeficatorService,
        IRegionAdminRepository regionAdminRepository,
        IAverageRatingService averageRatingService,
        IAreaAdminService areaAdminService)
        : base(providerRepository,
        usersRepository,
        logger,
        localizer,
        mapper,
        addressRepository,
        workshopServiceCombiner,
        providerAdminRepository,
        providerImagesService,
        changesLogService,
        notificationService,
        providerAdminService,
        institutionAdminRepository,
        currentUserService,
        ministryAdminService,
        regionAdminService,
        codeficatorService,
        regionAdminRepository,
        averageRatingService,
        areaAdminService)
    { }

    public async Task<ProviderLicenseStatusDto> UpdateLicenseStatus(ProviderLicenseStatusDto dto, string userId)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        var provider = await providerRepository.GetById(dto.ProviderId).ConfigureAwait(false);

        if (provider is null)
        {
            logger.LogInformation($"Provider(id) {dto.ProviderId} not found. User(id): {userId}");

            return null;
        }

        if (string.IsNullOrEmpty(provider.License) && dto.LicenseStatus != ProviderLicenseStatus.NotProvided)
        {
            logger.LogInformation($"Provider(id) {provider.Id} license is not provided. It cannot be approved. UserId: {userId}");
            throw new ArgumentException("Provider license is not provided. It cannot be approved.");
        }

        if (!string.IsNullOrEmpty(provider.License) && dto.LicenseStatus == ProviderLicenseStatus.NotProvided)
        {
            logger.LogInformation("Cannot set NotProvided license status when license is provided. " +
                                  $"Provider: {provider.Id}. License: {provider.License}. UserId: {userId}");
            throw new ArgumentException("Cannot set NotProvided license status when license is provided.");
        }

        // TODO: validate if current user has permission to update the provider status
        provider.LicenseStatus = dto.LicenseStatus;
        await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);

        logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.LicenseStatus}");

        await SendNotification(provider, NotificationAction.Update, false, true).ConfigureAwait(false);

        return dto;
    }
}