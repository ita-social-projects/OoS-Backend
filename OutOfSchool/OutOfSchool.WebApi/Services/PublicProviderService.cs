using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements interface for functionality for Public Provider.
/// </summary>
public class PublicProviderService : ProviderService, IPublicProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly IProviderAdminRepository providerAdminRepository;
    private readonly ILogger<ProviderService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly IEntityRepositorySoftDeleted<long, Address> addressRepository;
    private readonly IWorkshopServicesCombiner workshopServiceCombiner;
    private readonly IChangesLogService changesLogService;
    private readonly INotificationService notificationService;
    private readonly IProviderAdminService providerAdminService;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;
    private readonly IRegionAdminRepository regionAdminRepository;
    private readonly IAverageRatingService averageRatingService;
    private readonly IAreaAdminService areaAdminService;

    // TODO: It should be removed after models revision.
    //       Temporary instance to fill 'Provider' model 'User' property
    private readonly IEntityRepositorySoftDeleted<string, User> usersRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicProviderService"/> class.
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
    public PublicProviderService(
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
        areaAdminService) { }

    public async Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        var provider = await providerRepository.GetById(dto.ProviderId).ConfigureAwait(false);

        if (provider is null)
        {
            logger.LogInformation($"Provider(id) {dto.ProviderId} not found. User(id): {userId}");

            return null;
        }

        // TODO: validate if current user has permission to update the provider status
        provider.Status = dto.Status;
        provider.StatusReason = dto.StatusReason;
        await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);

        logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.Status}");

        await UpdateWorkshopsProviderStatus(dto.ProviderId, dto.Status).ConfigureAwait(false);

        await SendNotification(provider, NotificationAction.Update, true, false).ConfigureAwait(false);

        return dto;
    }
}
