using System.Data;
using System.Linq.Expressions;
using System.Text;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

/// <summary>
/// Implements the interface with CRUD functionality for Provider entity.
/// </summary>
public class ProviderService : IProviderService, ISensitiveProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly IProviderAdminRepository providerAdminRepository;
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
    private readonly IAreaAdminRepository areaAdminRepository;
    private readonly IUserService userService;
    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly ICommunicationService communicationService;
    private readonly ILogger<ProviderService> logger;
    private readonly ISearchStringService searchStringService;

    // TODO: It should be removed after models revision.
    //       Temporary instance to fill 'Provider' model 'User' property
    private readonly IEntityRepositorySoftDeleted<string, User> usersRepository;

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
    /// <param name="searchStringService">Service for handling search string.</param>
    public ProviderService(
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
        IAreaAdminService areaAdminService,
        IAreaAdminRepository areaAdminRepository,
        IUserService userService,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService,
        ISearchStringService searchStringService)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        this.workshopServiceCombiner = workshopServiceCombiner ?? throw new ArgumentNullException(nameof(workshopServiceCombiner));
        this.providerAdminRepository = providerAdminRepository ?? throw new ArgumentNullException(nameof(providerAdminRepository));
        ProviderImagesService = providerImagesService ?? throw new ArgumentNullException(nameof(providerImagesService));
        this.changesLogService = changesLogService ?? throw new ArgumentNullException(nameof(changesLogService));
        this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.institutionAdminRepository = institutionAdminRepository;
        this.providerAdminService = providerAdminService ?? throw new ArgumentNullException(nameof(providerAdminService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
        this.regionAdminRepository = regionAdminRepository;
        this.averageRatingService = averageRatingService ?? throw new ArgumentNullException(nameof(averageRatingService));
        this.areaAdminService = areaAdminService ?? throw new ArgumentNullException(nameof(areaAdminService));
        this.areaAdminRepository = areaAdminRepository;
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.authorizationServerConfig = authorizationServerConfig.Value ?? throw new ArgumentNullException(nameof(authorizationServerConfig));
        this.communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.searchStringService = searchStringService ?? throw new ArgumentNullException(nameof(searchStringService));
    }

    private protected IImageDependentEntityImagesInteractionService<Provider> ProviderImagesService { get; }

    /// <inheritdoc/>
    public async Task<ProviderDto> Create(ProviderCreateDto providerDto)
        => await CreateProviderWithActionAfterAsync(providerDto).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<SearchResult<ProviderDto>> GetByFilter(ProviderFilter filter)
    {
        logger.LogInformation("Getting all Providers started (by filter).");

        filter ??= new ProviderFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var filterPredicate = PredicateBuild(filter);

        if (filter.CATOTTGId != 0)
        {
            var childSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId).ConfigureAwait(false);

            filterPredicate = filterPredicate.And(x => childSettlementsIds.Contains(x.LegalAddress.CATOTTGId));
        }

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            filterPredicate = filterPredicate.And(p => p.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            filterPredicate = filterPredicate.And(p => p.InstitutionId == regionAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

            var tempPredicate = PredicateBuilder.False<Provider>();

            foreach (var item in subSettlementsIds)
            {
                tempPredicate = tempPredicate.Or(x => x.LegalAddress.CATOTTGId == item);
            }

            filterPredicate = filterPredicate.And(tempPredicate);
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
            filterPredicate = filterPredicate.And(p => p.InstitutionId == areaAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

            var tempPredicate = PredicateBuilder.False<Provider>();

            foreach (var item in subSettlementsIds)
            {
                tempPredicate = tempPredicate.Or(x => x.LegalAddress.CATOTTGId == item);
            }

            filterPredicate = filterPredicate.And(tempPredicate);
        }

        int count = await providerRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Provider, object>>, SortDirection>
        {
            { x => x.IsBlocked, SortDirection.Ascending },
            { x => x.Status, SortDirection.Ascending },
            { x => x.UpdatedAt, SortDirection.Descending },
        };

        var providers = await providerRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: string.Empty,
                whereExpression: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: false)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!providers.Any()
            ? "Parents table is empty."
            : $"All {providers.Count} records were successfully received from the Parent table");

        var providersDTO = providers.Select(provider => mapper.Map<ProviderDto>(provider)).ToList();
        await FillRatingsForProviders(providersDTO).ConfigureAwait(false);

        var result = new SearchResult<ProviderDto>()
        {
            TotalAmount = count,
            Entities = providersDTO,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProviderDto> GetById(Guid id)
    {
        logger.LogInformation($"Getting Provider by Id started. Looking Id = {id}.");
        var isProviderExists = await Exists(id).ConfigureAwait(false);

        if (!isProviderExists)
        {
            return null;
        }

        Expression<Func<Provider, bool>> providerFilter = p => p.Id == id;
        var provider = await providerRepository.Get(
            whereExpression: providerFilter,
            asNoTracking: true)
            .Include(p => p.ActualAddress).ThenInclude(a => a.CATOTTG)
                .ThenInclude(ac => ac.Parent).ThenInclude(acp => acp.Parent).ThenInclude(acpp => acpp.Parent).ThenInclude(acppp => acppp.Parent)
            .Include(p => p.LegalAddress).ThenInclude(a => a.CATOTTG)
                .ThenInclude(ac => ac.Parent).ThenInclude(acp => acp.Parent).ThenInclude(acpp => acpp.Parent).ThenInclude(acppp => acppp.Parent)
            .Include(p => p.ProviderSectionItems)
            .Include(p => p.Type)
            .Include(p => p.Institution)
            .Include(p => p.Images)
            .FirstAsync()
            .ConfigureAwait(false);

        logger.LogInformation($"Successfully got a Provider with Id = {id}.");

        var providerDTO = mapper.Map<ProviderDto>(provider);

        var rating = await averageRatingService.GetByEntityIdAsync(providerDTO.Id).ConfigureAwait(false);

        providerDTO.Rating = rating?.Rate ?? default;
        providerDTO.NumberOfRatings = rating?.RateQuantity ?? default;

        return providerDTO;
    }

    public async Task<ProviderStatusDto> GetProviderStatusById(Guid id)
    {
        logger.LogInformation($"Getting ProviderStatus by Id started. Looking Id = {id}.");
        var provider = await providerRepository.GetById(id).ConfigureAwait(false);

        if (provider == null)
        {
            return null;
        }

        logger.LogInformation($"Successfully got a ProviderStatus with Id = {id}.");

        var providerStatusDTO = mapper.Map<ProviderStatusDto>(provider);
        return providerStatusDTO;
    }

    /// <inheritdoc/>
    public async Task<ProviderDto> GetByUserId(string id, bool isDeputyOrAdmin = false)
    {
        logger.LogInformation("Getting Provider by UserId started. Looking UserId is {Id}", id);
        Provider provider = default;

        if (isDeputyOrAdmin)
        {
            var providerAdmins = await providerAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false);
            var providerAdmin = providerAdmins.FirstOrDefault();
            if (providerAdmin != null)
            {
                provider = providerAdmin.Provider;
            }
        }
        else
        {
            var providers = await providerRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false);
            provider = providers.FirstOrDefault();
        }

        if (provider != null)
        {
            logger.LogInformation("Successfully got a Provider with UserId = {Id}", id);
        }

        return mapper.Map<ProviderDto>(provider);
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetCsvExportData()
    {
        var providers = await providerRepository
            .Get(asNoTracking: true)
            .ToArrayAsync()
            .ConfigureAwait(false);

        if (providers.Length == 0)
        {
            return [];
        }

        var providerDtos = mapper.Map<Provider[], ProviderCsvDto[]>(providers);

        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            Encoding = Encoding.UTF8,
        };

        using var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream);
        using var csvWriter = new CsvWriter(streamWriter, csvConfiguration);

        await csvWriter.WriteRecordsAsync(providerDtos).ConfigureAwait(false);
        await csvWriter.FlushAsync().ConfigureAwait(false);

        var csvData = stream.GetBuffer();

        return csvData;
    }

    /// <inheritdoc/>
    public async Task<ProviderDto> Update(ProviderUpdateDto providerUpdateDto, string userId)
        => await UpdateProviderWithActionBeforeSavingChanges(providerUpdateDto, userId).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, ActionResult>> Delete(Guid id, string token) => await DeleteProviderWithActionBefore(id, token).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Guid> GetProviderIdForWorkshopById(Guid workshopId) =>
        await workshopServiceCombiner.GetWorkshopProviderId(workshopId).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<ResponseDto> Block(ProviderBlockDto providerBlockDto, string token = default)
    {
        logger.LogInformation($"Block/Unblock Provider by Id started.");

        _ = providerBlockDto ?? throw new ArgumentNullException(nameof(providerBlockDto));

        var provider = await providerRepository.GetById(providerBlockDto.Id).ConfigureAwait(false);

        if (provider is null)
        {
            logger.LogInformation($"Provider(id) {providerBlockDto.Id} not found.");

            return new ResponseDto()
            {
                Result = null,
                Message = $"There is no Provider in DB with Id - {providerBlockDto.Id}",
                HttpStatusCode = HttpStatusCode.NotFound,
                IsSuccess = false,
            };
        }

        var isUserHasRights = await IsCurrentUserIsAdminOfDistrictOrMinistryOfProvider(provider);

        if (!isUserHasRights)
        {
            logger.LogInformation("The user {UserId} doesn't have rights to block/unblock the provider {ProviderId}", currentUserService.UserId, providerBlockDto.Id);

            return new ResponseDto()
            {
                Result = null,
                Message = $"The user {currentUserService.UserId} doesn't have the rights to block/unblock the Provider {providerBlockDto.Id}",
                HttpStatusCode = HttpStatusCode.Forbidden,
                IsSuccess = false,
            };
        }

        provider.IsBlocked = providerBlockDto.IsBlocked;
        provider.BlockReason = providerBlockDto.IsBlocked ? providerBlockDto.BlockReason : null;
        provider.BlockPhoneNumber = providerBlockDto.IsBlocked ? providerBlockDto.BlockPhoneNumber : string.Empty;

        await providerRepository.RunInTransaction(async () =>
        {
            await providerRepository.SaveChangesAsync().ConfigureAwait(false);

            var workshops = await workshopServiceCombiner
                               .BlockByProvider(provider)
                               .ConfigureAwait(false);

            foreach (var workshop in workshops)
            {
                logger.LogInformation($"IsBlocked property with povider Id = {provider.Id} " +
                                      $"in workshops with Id = {workshop.Id} updated successfully.");
            }

            logger.LogInformation($"Provider(id) {providerBlockDto.Id} IsBlocked was changed to {provider.IsBlocked}");
        });

        var notificationAction = providerBlockDto.IsBlocked ? NotificationAction.Block : NotificationAction.Unblock;

        await SendNotification(provider, notificationAction, false, false);

        logger.LogInformation("Block/Unblock the particular provider admins and deputy providers belonging to the Provider starts.");

        // TODO: It's need to consider how we might use the result of the blocking provider admins and deputies who belong to the provider.
        _ = await providerAdminService
            .BlockProviderAdminsAndDeputiesByProviderAsync(provider.Id, currentUserService.UserId, token, providerBlockDto.IsBlocked);

        logger.LogInformation("Block/Unblock the particular provider admins and deputy providers belonging to the Provider finished.");

        var blockedStatus = providerBlockDto.IsBlocked ? "blocked" : "unblocked";

        return new ResponseDto()
        {
            Result = providerBlockDto,
            Message = $"The user {currentUserService.UserId} {blockedStatus} the Provider {providerBlockDto.Id}",
            HttpStatusCode = HttpStatusCode.OK,
            IsSuccess = true,
        };
    }

    public async Task<bool?> IsBlocked(Guid providerId)
    {
        return (await providerRepository.GetById(providerId).ConfigureAwait(false))?.IsBlocked;
    }

    public async Task SendNotification(Provider provider, NotificationAction notificationAction, bool addStatusData, bool addLicenseStatusData)
    {
        if (provider == null)
        {
            return;
        }

        var additionalData = new Dictionary<string, string>();

        if (addStatusData)
        {
            additionalData.Add("Status", provider.Status.ToString());
        }

        if (addLicenseStatusData)
        {
            additionalData.Add("LicenseStatus", provider.LicenseStatus.ToString());
        }

        var recipientsIds = await GetNotificationsRecipientIds(notificationAction, additionalData, provider.Id).ConfigureAwait(false);

        await notificationService.Create(
                NotificationType.Provider,
                notificationAction,
                provider.Id,
                recipientsIds,
                additionalData)
            .ConfigureAwait(false);
    }

    public async Task UpdateWorkshopsProviderStatus(Guid providerId, ProviderStatus providerStatus)
    {
        var workshops = await workshopServiceCombiner.UpdateProviderStatus(providerId, providerStatus)
           .ConfigureAwait(false);

        if (workshops != null)
        {
            foreach (var workshop in workshops)
            {
                logger.LogInformation($"Provider's status with Id = {providerId} " +
                                      $"in workshops with Id = {workshop.Id} updated successfully.");
            }
        }
    }

    public async Task<ImportDataValidateResponse> ValidateImportData(ImportDataValidateRequest data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));

        var result = new ImportDataValidateResponse();

        if (data.Edrpous.Count > 0)
        {
            result.Edrpous = await providerRepository.CheckExistsByEdrpous(data.Edrpous);
        }

        if (data.Emails.Count > 0)
        {
            result.Emails = await providerRepository.CheckExistsByEmails(data.Emails);
        }

        return result;
    }

    /// <inheritdoc/>
    public Task<bool> Exists(Guid id)
    {
        logger.LogInformation($"Checking if Provider exists by Id. Looking Id = {id}.");

        return providerRepository.Any(x => x.Id == id);
    }

    private async Task<IEnumerable<string>> GetNotificationsRecipientIds(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
    {
        var recipientIds = new List<string>();

        var provider = await providerRepository.GetById(objectId).ConfigureAwait(false);

        if (provider is null)
        {
            return recipientIds;
        }

        if (action == NotificationAction.Create)
        {
            recipientIds.AddRange(GetTechAdminsIds());
            recipientIds.AddRange(GetMinistryAdminsIds(provider.InstitutionId));
            recipientIds.AddRange(GetRegionAdminsIds(provider.LegalAddress));
            recipientIds.AddRange(GetAreaAdminsIds(provider.LegalAddress));
        }
        else if (action == NotificationAction.Update)
        {
            if (additionalData != null
                && additionalData.TryGetValue("Status", out var statusValue)
                && Enum.TryParse(statusValue, out ProviderStatus status))
            {
                if (status == ProviderStatus.Recheck)
                {
                    recipientIds.AddRange(GetTechAdminsIds());
                    recipientIds.AddRange(GetMinistryAdminsIds(provider.InstitutionId));
                    recipientIds.AddRange(GetRegionAdminsIds(provider.LegalAddress));
                }
                else if (status == ProviderStatus.Editing
                         || status == ProviderStatus.Approved)
                {
                    recipientIds.Add(provider.UserId);
                    recipientIds.AddRange(await providerAdminService.GetProviderDeputiesIds(provider.Id).ConfigureAwait(false));
                }
            }

            if (additionalData != null
                && additionalData.TryGetValue("LicenseStatus", out var licenseStatusValue)
                && Enum.TryParse(licenseStatusValue, out ProviderLicenseStatus licenseStatus))
            {
                if (licenseStatus == ProviderLicenseStatus.Pending)
                {
                    // there should be District admin
                    recipientIds.AddRange(GetTechAdminsIds());
                    recipientIds.AddRange(GetMinistryAdminsIds(provider.InstitutionId));
                }
                else if (licenseStatus == ProviderLicenseStatus.Approved)
                {
                    recipientIds.Add(provider.UserId);
                    recipientIds.AddRange(await providerAdminService.GetProviderDeputiesIds(provider.Id).ConfigureAwait(false));
                }
            }
        }
        else if (action == NotificationAction.Block)
        {
            recipientIds.Add(provider.UserId);
        }
        else if (action == NotificationAction.Unblock)
        {
            recipientIds.Add(provider.UserId);
        }

        return recipientIds.Distinct();
    }

    private protected async Task<ProviderDto> CreateProviderWithActionAfterAsync(ProviderCreateDto providerDto, Func<Provider, Task> actionAfterCreation = null)
    {
        _ = providerDto ?? throw new ArgumentNullException(nameof(providerDto));

        logger.LogDebug("Provider creating was started");

        if (providerRepository.ExistsUserId(providerDto.UserId))
        {
            throw new InvalidOperationException(localizer["You can not create more than one account."]);
        }

        // Note: clear the actual address if it is equal to the legal to avoid data duplication in the database
        if (providerDto.LegalAddress.Equals(providerDto.ActualAddress))
        {
            providerDto.ActualAddress = null;
        }

        var providerDomainModel = mapper.Map<Provider>(providerDto);

        // BUG: concurrency issue:
        //      while first repository with this particular user id is not saved to DB - we can create any number of repositories for this user.
        if (providerRepository.SameExists(providerDomainModel))
        {
            throw new InvalidOperationException(localizer["There is already a provider with such a data"]);
        }

        var users = await usersRepository.GetByFilter(u => u.Id.Equals(providerDto.UserId)).ConfigureAwait(false);
        providerDomainModel.User = users.Single();
        providerDomainModel.User.IsRegistered = true;
        providerDomainModel.User.PhoneNumber = providerDto.PhoneNumber;
        providerDomainModel.Status = ProviderStatus.Pending;
        providerDomainModel.LicenseStatus = providerDomainModel.License == null
            ? ProviderLicenseStatus.NotProvided
            : ProviderLicenseStatus.Pending;

        var newProvider = await providerRepository.Create(providerDomainModel).ConfigureAwait(false);

        if (newProvider is not null)
        {
            await changesLogService.AddCreatingOfEntityToDbContext(newProvider, newProvider.UserId).ConfigureAwait(false);
        }

        if (actionAfterCreation != null)
        {
            await actionAfterCreation(newProvider).ConfigureAwait(false);
            await UpdateProvider().ConfigureAwait(false);
        }

        logger.LogDebug("Provider with Id = {ProviderId} created successfully", newProvider?.Id);

        await SendNotification(newProvider, NotificationAction.Create, true, true);

        return mapper.Map<ProviderDto>(newProvider);
    }

    private protected async Task<ProviderDto> UpdateProviderWithActionBeforeSavingChanges(ProviderUpdateDto providerUpdateDto, string userId, Func<Provider, Task> actionBeforeUpdating = null)
    {
        _ = providerUpdateDto ?? throw new ArgumentNullException(nameof(providerUpdateDto));
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (await ExistsAnotherProviderWithTheSameEdrpouIpn(providerUpdateDto))
        {
            logger.LogTrace("Provider with Id = {providerUpdateDtoId} wasn't updated: Edrpou or Ipn isn't unique.", providerUpdateDto.Id);

            return null;
        }

        logger.LogDebug("Updating Provider with Id = {Id} was started", providerUpdateDto.Id);

        try
        {
            var checkProvider = await providerRepository.GetById(providerUpdateDto.Id).ConfigureAwait(false);

            if (checkProvider?.UserId != userId)
            {
                return null;
            }

            ChangeProviderStatusIfNeeded(providerUpdateDto, checkProvider, out var statusChanged, out var licenseChanged);

            providerUpdateDto.LegalAddress.Id = checkProvider.LegalAddress.Id;

            if (providerUpdateDto.LegalAddress.Equals(providerUpdateDto.ActualAddress))
            {
                providerUpdateDto.ActualAddress = null;
            }

            if (providerUpdateDto.ActualAddress is null && checkProvider.ActualAddress is { })
            {
                var checkProviderActualAddress = checkProvider.ActualAddress;
                checkProvider.ActualAddressId = null;
                checkProvider.ActualAddress = null;
                mapper.Map(providerUpdateDto, checkProvider);
                await addressRepository.Delete(checkProviderActualAddress).ConfigureAwait(false);
            }
            else
            {
                if (providerUpdateDto.ActualAddress != null)
                {
                    providerUpdateDto.ActualAddress.Id = checkProvider.ActualAddress?.Id ?? 0;
                }

                if (IsNeedInRelatedWorkshopsUpdating(providerUpdateDto, checkProvider))
                {
                    checkProvider = await providerRepository.RunInTransaction(async () =>
                    {
                        var workshops = await workshopServiceCombiner
                            .UpdateProviderTitle(providerUpdateDto.Id, providerUpdateDto.FullTitle, providerUpdateDto.FullTitleEn)
                            .ConfigureAwait(false);

                        mapper.Map(providerUpdateDto, checkProvider);
                        LogProviderChanges(checkProvider, userId);
                        await UpdateProvider().ConfigureAwait(false);

                        foreach (var workshop in workshops)
                        {
                            logger.LogInformation($"Provider's properties with Id = {checkProvider?.Id} " +
                                                  $"in workshops with Id = {workshop?.Id} updated successfully.");
                        }

                        return checkProvider;
                    }).ConfigureAwait(false);
                }
                else
                {
                    mapper.Map(providerUpdateDto, checkProvider);
                }

                if (actionBeforeUpdating != null)
                {
                    await actionBeforeUpdating(checkProvider).ConfigureAwait(false);
                }

                LogProviderChanges(checkProvider, userId);
                await UpdateProvider().ConfigureAwait(false);
            }

            logger.LogInformation("Provider with Id = {CheckProviderId} was updated successfully", checkProvider?.Id);

            if (statusChanged)
            {
                await UpdateWorkshopsProviderStatus(providerUpdateDto.Id, providerUpdateDto.Status)
                    .ConfigureAwait(false);
            }

            if (statusChanged || licenseChanged)
            {
                await SendNotification(checkProvider, NotificationAction.Update, statusChanged, licenseChanged);
            }

            return mapper.Map<ProviderDto>(checkProvider);
        }
        finally
        {
            logger.LogTrace("Updating Provider with Id = {Id} was finished", providerUpdateDto.Id);
        }
    }

    private protected async Task<Either<ErrorResponse, ActionResult>> DeleteProviderWithActionBefore(Guid id, string token, Func<Provider, Task> actionBeforeDeleting = null)
    {
        logger.LogInformation("Deleting Provider with Id = {Id} started", id);

        var entity = await providerRepository.GetWithNavigations(id).ConfigureAwait(false);

        if (entity is null)
        {
            var message = $"There is no Provider with Id = {id}";
            logger.LogError(message);
            return new ErrorResponse()
            {
                Message = message,
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (currentUserService.UserId != entity.UserId && !await IsCurrentUserIsAdminOfDistrictOrMinistryOfProvider(entity))
        {
            var message = $"User with userId = {currentUserService.UserId} has no rights to delete user with id = {entity.UserId}";
            logger.LogError(message);
            return new ErrorResponse()
            {
                Message = message,
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        if (actionBeforeDeleting != null)
        {
            await actionBeforeDeleting(entity).ConfigureAwait(false);
        }

        await providerRepository.RunInTransaction(async () =>
        {
            await providerRepository.Delete(entity).ConfigureAwait(false);
            await userService.Delete(entity.UserId).ConfigureAwait(false);
        });

        logger.LogInformation("Provider with Id = {Id} successfully deleted", id);

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(authorizationServerConfig.Authority, "account/deleteuser/" + entity.UserId),
            Token = token,
        };

        logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
            currentUserService.UserId,
            request.Url);

        var response = await communicationService.SendRequest<ResponseDto, ErrorResponse>(request).ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
            ? r
            : new ErrorResponse()
            {
                HttpStatusCode = r.HttpStatusCode,
                Message = r.Message,
            })
            .Map(r => r.Result is not null
            ? JsonSerializerHelper.Deserialize<ActionResult>(r.Result.ToString())
            : null);
    }

    private void ChangeProviderStatusIfNeeded(
        ProviderUpdateDto providerDto,
        Provider checkProvider,
        out bool statusChanged,
        out bool licenseChanged)
    {
        statusChanged = false;
        licenseChanged = false;

        if (checkProvider.Status != ProviderStatus.Pending &&
            !(checkProvider.FullTitle == providerDto.FullTitle
              && checkProvider.EdrpouIpn == providerDto.EdrpouIpn))
        {
            checkProvider.Status = ProviderStatus.Recheck;
            statusChanged = true;
        }

        if (checkProvider.License != providerDto.License)
        {
            checkProvider.LicenseStatus = string.IsNullOrEmpty(providerDto.License)
                ? ProviderLicenseStatus.NotProvided
                : ProviderLicenseStatus.Pending;
            licenseChanged = !string.IsNullOrEmpty(providerDto.License);
        }
    }

    private async Task<bool> IsCurrentUserIsAdminOfDistrictOrMinistryOfProvider(Provider provider)
    {
        if (!currentUserService.IsAdmin())
        {
            return false;
        }

        if (currentUserService.IsMinistryAdmin())
        {
            var minAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId).ConfigureAwait(false);
            return minAdmin.InstitutionId == provider.InstitutionId;
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId).ConfigureAwait(false);
            var listOfCATOTTG = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);
            return regionAdmin.InstitutionId == provider.InstitutionId && listOfCATOTTG.Contains(provider.LegalAddress.CATOTTGId);
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId).ConfigureAwait(false);
            var listOfCATOTTG = await codeficatorService.GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);
            return areaAdmin.InstitutionId == provider.InstitutionId && listOfCATOTTG.Contains(provider.LegalAddress.CATOTTGId);
        }

        return true;
    }

    private bool IsNeedInRelatedWorkshopsUpdating(ProviderUpdateDto providerDto, Provider checkProvider)
    {
        return checkProvider.FullTitle != providerDto.FullTitle || checkProvider.FullTitleEn != providerDto.FullTitleEn;
    }

    private async Task UpdateProvider()
    {
        try
        {
            await providerRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Updating a provider failed");
            throw;
        }
    }

    private void LogProviderChanges(Provider provider, string userId)
    {
        changesLogService.AddEntityChangesToDbContext(provider, userId);
    }

    private Expression<Func<Provider, bool>> PredicateBuild(ProviderFilter filter)
    {
        var predicate = PredicateBuilder.True<Provider>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<Provider>();

            foreach (var word in searchStringService.SplitSearchString(filter.SearchString))
            {
                if (word.Any(c => char.IsLetter(c)))
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.FullTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.ShortTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.FullTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.ShortTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    string searchNumber = string.Join(string.Empty, word.Where(c => char.IsNumber(c)));
                    if (searchNumber.Length > 0)
                    {
                        tempPredicate = tempPredicate.Or(
                            x => x.PhoneNumber.Contains(searchNumber, StringComparison.InvariantCultureIgnoreCase)
                                || x.EdrpouIpn.Contains(searchNumber, StringComparison.InvariantCultureIgnoreCase)
                                || x.Email.Contains(searchNumber, StringComparison.InvariantCultureIgnoreCase));
                    }
                }
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.Status.Any())
        {
            predicate = predicate.And(x => filter.Status.Contains(x.Status));
        }

        if (filter.LicenseStatus.Any())
        {
            predicate = predicate.And(x => filter.LicenseStatus.Contains(x.LicenseStatus));
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => x.InstitutionId == filter.InstitutionId);
        }

        return predicate;
    }

    private async Task FillRatingsForProviders(List<ProviderDto> providersDTO)
    {
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(providersDTO.Select(p => p.Id)).ConfigureAwait(false);

        foreach (var provider in providersDTO)
        {
            var averageRatingsForProvider = averageRatings?.SingleOrDefault(r => r.EntityId == provider.Id);
            provider.Rating = averageRatingsForProvider?.Rate ?? default;
            provider.NumberOfRatings = averageRatingsForProvider?.RateQuantity ?? default;
        }
    }

    private List<string> GetTechAdminsIds()
    {
        var techAdminIds = usersRepository
                        .GetByFilter(u => u.Role == nameof(Role.TechAdmin).ToLower())
                        .Result
                        .Select(u => u.Id)
                        .ToList();
        return techAdminIds;
    }

    private List<string> GetMinistryAdminsIds(Guid? ministryId)
    {
        if (ministryId == null)
        {
            return new List<string>();
        }

        var ministryAdminsIds = institutionAdminRepository
                        .GetByFilterNoTracking(a => a.InstitutionId == ministryId)
                        .Select(a => a.UserId)
                        .ToList();

        return ministryAdminsIds;
    }

    private List<string> GetRegionAdminsIds(Address address)
    {
        var regionAdminsIds = regionAdminRepository
            .GetByFilterNoTracking(a => a.CATOTTGId == address.CATOTTGId)
            .Select(a => a.UserId)
            .ToList();

        return regionAdminsIds;
    }

    private List<string> GetAreaAdminsIds(Address address)
        => areaAdminRepository
            .GetByFilterNoTracking(a => a.CATOTTGId == address.CATOTTGId)
            .Select(a => a.UserId)
            .ToList();

    private async Task<bool> ExistsAnotherProviderWithTheSameEdrpouIpn(ProviderUpdateDto providerUpdateDto)
    {
        var providersWithTheSameEdrpouIpn = await providerRepository
            .GetByFilter(x => x.EdrpouIpn == providerUpdateDto.EdrpouIpn && x.Id != providerUpdateDto.Id)
            .ConfigureAwait(false);

        return providersWithTheSameEdrpouIpn.Any();
    }
}
