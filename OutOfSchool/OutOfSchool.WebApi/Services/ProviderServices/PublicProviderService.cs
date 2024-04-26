//using OutOfSchool.Services.Enums;
using AutoMapper;
//using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Models.Providers;
//using OutOfSchool.Services.Enums;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.Communication.ICommunication;
//using OutOfSchool.WebApi.Services.Images;
//using System.Linq.Expressions;

namespace OutOfSchool.WebApi.Services.ProviderServices;

/// <summary>
/// Implements interface for functionality for Public Provider.
/// </summary>
public class PublicProviderService : BaseProviderService<PublicProvider>, IPublicProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly IPublicProviderRepository publicProviderRepository;
    private readonly ILogger<ProviderService> logger;
    private readonly IProviderService providerService;
    private readonly IAverageRatingService averageRatingService;
    private readonly IMapper mapper;
    private readonly OutOfSchoolDbContext dbContext;
    //public PublicProviderService(
    //    IProviderRepository providerRepository,
    //    IPublicProviderRepository publicProviderRepository,
    //    ILogger<ProviderService> logger,
    //    BaseProviderService providerService,
    //    IAverageRatingService averageRatingService,
    //    IMapper mapper)
    //{
    //    this.providerRepository = providerRepository;
    //    this.logger = logger;
    //    this.providerService = providerService;
    //    this.averageRatingService = averageRatingService;
    //    this.mapper = mapper;
    //}

    //public async Task<ProviderDto> GetById(Guid id)
    //{
    //    return providerService.GetById(id);
    //}



    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderService"/> class.
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
    /// <param name="regionAdminRepository">RegionAdminRepository.</param>
    /// <param name="averageRatingService">Average rating service.</param>
    /// <param name="areaAdminService">Service for manage area admin.</param>
    /// <param name="areaAdminRepository">Repository for manage area admin.</param>
    /// <param name="userService">Service for manage users.</param>
    /// <param name="authorizationServerConfig">Path to authorization server.</param>
    /// <param name="communicationService">Service for communication.</param>
    public PublicProviderService(
        IPublicProviderRepository providerRepository,
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
        IAreaAdminService areaAdminService,
        IAreaAdminRepository areaAdminRepository,
        IUserService userService,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService)
        : base(
        providerRepository,
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
        areaAdminService,
        areaAdminRepository,
        userService,
        authorizationServerConfig,
        communicationService)
    {
        Console.WriteLine("hello public provider");
    }

    ///// <inheritdoc/>
    ///// Public Provider Dto
    //public async Task<SearchResult<ProviderDto>> GetByFilter(BaseProviderFilter filter)
    //{
    //    logger.LogInformation("Getting all Providers started (by filter).");

    //    filter ??= new BaseProviderFilter();
    //    ModelValidationHelper.ValidateOffsetFilter(filter);

    //    var filterPredicate = PredicateBuild(filter);

    //    if (filter.CATOTTGId != 0)
    //    {
    //        var childSettlementsIds = await codeficatorService
    //            .GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId).ConfigureAwait(false);

    //        filterPredicate = filterPredicate.And(x => childSettlementsIds.Contains(x.LegalAddress.CATOTTGId));
    //    }

    //    if (currentUserService.IsMinistryAdmin())
    //    {
    //        var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
    //        filterPredicate = filterPredicate.And(p => p.InstitutionId == ministryAdmin.InstitutionId);
    //    }

    //    if (currentUserService.IsRegionAdmin())
    //    {
    //        var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
    //        filterPredicate = filterPredicate.And(p => p.InstitutionId == regionAdmin.InstitutionId);

    //        var subSettlementsIds = await codeficatorService
    //            .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

    //        var tempPredicate = PredicateBuilder.False<Provider>();

    //        foreach (var item in subSettlementsIds)
    //        {
    //            tempPredicate = tempPredicate.Or(x => x.LegalAddress.CATOTTGId == item);
    //        }

    //        filterPredicate = filterPredicate.And(tempPredicate);
    //    }

    //    if (currentUserService.IsAreaAdmin())
    //    {
    //        var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
    //        filterPredicate = filterPredicate.And(p => p.InstitutionId == areaAdmin.InstitutionId);

    //        var subSettlementsIds = await codeficatorService
    //            .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

    //        var tempPredicate = PredicateBuilder.False<Provider>();

    //        foreach (var item in subSettlementsIds)
    //        {
    //            tempPredicate = tempPredicate.Or(x => x.LegalAddress.CATOTTGId == item);
    //        }

    //        filterPredicate = filterPredicate.And(tempPredicate);
    //    }

    //    int count = await providerRepository.Count(filterPredicate).ConfigureAwait(false);

    //    var sortExpression = new Dictionary<Expression<Func<Provider, object>>, SortDirection>
    //    {
    //        { x => x.IsBlocked, SortDirection.Ascending },
    //        { x => x.Status, SortDirection.Ascending },
    //    };

    //    var providers = await publicProviderRepository
    //        .Get(
    //            skip: filter.From,
    //            take: filter.Size,
    //            includeProperties: string.Empty,
    //            whereExpression: filterPredicate,
    //            orderBy: sortExpression,
    //            asNoTracking: false)
    //        .ToListAsync()
    //        .ConfigureAwait(false);

    //    logger.LogInformation(!providers.Any()
    //        ? "Parents table is empty."
    //        : $"All {providers.Count} records were successfully received from the Parent table");

    //    var providersDTO = providers.Select(provider => mapper.Map<ProviderDto>(provider)).ToList();
    //    await FillRatingsForProviders(providersDTO).ConfigureAwait(false);

    //    var result = new SearchResult<ProviderDto>()
    //    {
    //        TotalAmount = count,
    //        Entities = providersDTO,
    //    };

    //    return result;
    //}



    //    logger.LogInformation($"Successfully got a Provider with Id = {id}.");

    //    var providerDTO = mapper.Map<ProviderDto>(provider);

    //    var rating = await averageRatingService.GetByEntityIdAsync(providerDTO.Id).ConfigureAwait(false);

    //    providerDTO.Rating = rating?.Rate ?? default;
    //    providerDTO.NumberOfRatings = rating?.RateQuantity ?? default;

    //    return providerDTO;
    //}

    //    public async Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId)
    //{
    //    _ = dto ?? throw new ArgumentNullException(nameof(dto));

    //    var provider = await providerRepository.GetById(dto.ProviderId).ConfigureAwait(false);

    //    if (provider is null)
    //    {
    //        logger.LogInformation($"Provider(id) {dto.ProviderId} not found. User(id): {userId}");

    //        return null;
    //    }

    //    // TODO: validate if current user has permission to update the provider status
    //    //provider.Status = dto.Status;
    //    //provider.StatusReason = dto.StatusReason;
    //    await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);

    //    logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.Status}");

    //    await providerService.UpdateWorkshopsProviderStatus(dto.ProviderId, dto.Status).ConfigureAwait(false);

    //    //providerService.SendNotification(provider, NotificationAction.Update, true, false);

    //    return dto;
    //}

    /// <inheritdoc/>
    public async Task<ProviderDto> GetById(Guid id)
    {
        var result = await base.GetById(id);
        Console.WriteLine("test");
        return result;
    }

    public Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId)
    {
        throw new NotImplementedException();
    }
}