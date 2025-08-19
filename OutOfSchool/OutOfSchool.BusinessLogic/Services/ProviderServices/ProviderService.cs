using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Individual;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

/// <summary>
/// Implements the interface with CRUD functionality for Provider entity.
/// </summary>
/// <param name="providerRepository">Provider repository.</param>
/// <param name="usersRepository">UsersRepository.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
/// <param name="addressRepository">AddressRepository.</param>
/// <param name="individualRepository">IndividualRepository.</param>
/// <param name="officialRepository">OfficialRepository.</param>
/// <param name="positionRepository">PositionRepository.</param>
/// <param name="workshopServiceCombiner">WorkshopServiceCombiner.</param>
/// <param name="workshopDraftRepository">WorkshopDraftRepository.</param>
/// <param name="providerImagesService">Images service.</param>
/// <param name="changesLogService">ChangesLogService.</param>
/// <param name="notificationService">Notification service.</param>
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
/// <param name="contactsService">Service for handling contacts.</param>
public class ProviderService(
    IProviderRepository providerRepository,
    IEntityRepositorySoftDeleted<string, User> usersRepository,
    ILogger<ProviderService> logger,
    IStringLocalizer<SharedResource> localizer,
    IEntityRepositorySoftDeleted<long, Address> addressRepository,
    ISensitiveEntityRepositorySoftDeleted<Individual> individualRepository,
    IOfficialRepository officialRepository,
    IPositionRepository positionRepository,
    IWorkshopServicesCombiner workshopServiceCombiner,
    IImageDependentEntityImagesInteractionService<Provider> providerImagesService,
    IChangesLogService changesLogService,
    INotificationService notificationService,
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
    ISearchStringService searchStringService,
    IContactsService<Provider, IHasContactsDto<Provider>> contactsService
) : IProviderService, ISensitiveProviderService
    {
    // TODO: It should be removed after models revision.
    //       Temporary instance to fill 'Provider' model 'User' property
    private readonly IEntityRepositorySoftDeleted<string, User> usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));

    private protected IImageDependentEntityImagesInteractionService<Provider> ProviderImagesService { get; } = providerImagesService ?? throw new ArgumentNullException(nameof(providerImagesService));

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

            filterPredicate = filterPredicate.And(x => x.Contacts.Any(c => c.IsDefault && childSettlementsIds.Contains(c.Address.CATOTTGId)));
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
                tempPredicate = tempPredicate.Or(x => x.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == item));
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
                tempPredicate = tempPredicate.Or(x => x.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == item));
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
                whereExpression: filterPredicate,
                orderBy: sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!providers.Any()
            ? "Parents table is empty."
            : $"All {providers.Count} records were successfully received from the Parent table");

        var providersDTO = providers.ToDto();
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

        Func<IQueryable<Provider>, IQueryable<Provider>> includeFunc =
            p => p.Include(p => p.ProviderSectionItems.OrderBy(psi => psi.Name))
                  .Include(p => p.Type)
                  .Include(p => p.Institution)
                  .Include(p => p.Images)
                  .IncludeContactsWithCodeficatorHierarchy();

        Expression<Func<Provider, bool>> providerFilter = p => p.Id == id;
        var provider = await providerRepository
            .Get(whereExpression: providerFilter)
            .IncludeProperties(includeFunc)
            .AsNoTracking()
            .FirstAsync()
            .ConfigureAwait(false);

        logger.LogInformation($"Successfully got a Provider with Id = {id}.");

        var providerDTO = provider.ToDto();

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

        return provider.ToStatusDto();
    }

    /// <inheritdoc/>
    public async Task<ProviderDto> Update(ProviderUpdateDto providerUpdateDto, string userId)
        => await UpdateProviderWithActionBeforeSavingChanges(providerUpdateDto, userId).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<Either<ErrorResponse, bool>> Delete(Guid id)
    {
        return Task.FromResult<Either<ErrorResponse, bool>>(
            new ErrorResponse()
            {
                Message = "Deleting provider is not allowed.",
                HttpStatusCode = HttpStatusCode.Forbidden,
            });
        // TODO: do not allow provider deletion while requirements are updated
        // await DeleteProviderWithActionBefore(id).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Guid> GetProviderIdForWorkshopById(Guid workshopId) =>
        await workshopServiceCombiner.GetWorkshopProviderId(workshopId).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<ResponseDto> Block(ProviderBlockDto providerBlockDto, string token = default)
    {
        logger.LogInformation($"Block/Unblock Provider by Id started.");

        _ = providerBlockDto ?? throw new ArgumentNullException(nameof(providerBlockDto));

        var provider = await providerRepository.GetByIdWithDetails(
            id: providerBlockDto.Id,
            includeExpression: q => q.Include(p => p.Contacts).ThenInclude(c => c.Address)).ConfigureAwait(false);

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

        if (!recipientsIds.Any())
        {
            logger.LogInformation("No recipients found for notification action {Action} for provider with Id {ProviderId}.", notificationAction, provider.Id);
            return;
        }

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

        return result;
    }

    /// <inheritdoc/>
    public Task<bool> Exists(Guid id)
    {
        logger.LogDebug("Checking if Provider exists by Id. Looking Id = {Id}", id);

        return providerRepository.Any(x => x.Id == id);
    }

    public async Task<UploadEmployeeResponse> UploadEmployeesForProvider(Guid id, UploadEmployeeRequestDto[] data)
    {
        var isProviderExists = await Exists(id).ConfigureAwait(false);

        if (!isProviderExists)
        {
            logger.LogError("User has no rights to perform operation. Provider with Id = {id} doesn't exist.", id);
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

        await currentUserService.UserHasRights(new ProviderRights(id));

        // Check list of Employees for uploading.
        CheckListOfEmployeesForUploading(data);

        foreach (var employee in data)
        {
            employee.PositionType = employee.AssignedRole.Contains(Constants.UploadEmployees.DeputyDirector)
                ? PositionType.DeputyDirector
                : PositionType.Employee;
        }

        var uploadResponse = new UploadEmployeeResponse();

        async Task UploadEmployeesIntoDb()
        {
            // Add individuals to DB and populate the Dictionary for uploading employees.
            var uploadDictionary = await AddIndividualsToDb(data, uploadResponse).ConfigureAwait(false);

            // Fill DB with new Employees on certain Positions.
            await FillDbWithNewEmployeesOnPositions(uploadDictionary, id, uploadResponse).ConfigureAwait(false);
        }

        await providerRepository.RunInTransaction(UploadEmployeesIntoDb).ConfigureAwait(false);

        logger.LogDebug("Upload employees for provider finished successfully.");

        return uploadResponse;
    }

    /// <inheritdoc />
    public async Task<Tuple<ProviderLicenseStatus, OwnershipType>> GetLicenseStatusAndOwnershipAsync(Guid providerId) =>
        await providerRepository.Get(whereExpression: x => x.Id == providerId)
            .Select(p => new Tuple<ProviderLicenseStatus, OwnershipType>(p.LicenseStatus, p.Ownership))
            .SingleOrDefaultAsync();

    private async Task<IEnumerable<string>> GetNotificationsRecipientIds(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
    {
        var recipientIds = new List<string>();

        var provider = await providerRepository.GetByIdWithDetails(
                id: objectId, includeExpression: q => q.Include(p => p.Contacts).ThenInclude(c => c.Address))
            .ConfigureAwait(false);

        if (provider is null)
        {
            return recipientIds;
        }

        if (action == NotificationAction.Create)
        {
            recipientIds.AddRange(GetTechAdminsIds());
            recipientIds.AddRange(GetMinistryAdminsIds(provider.InstitutionId));
            recipientIds.AddRange(GetRegionAdminsIds(provider.Contacts.Where(c => c.IsDefault).Select(c => c.Address).Single()));
            recipientIds.AddRange(GetAreaAdminsIds(provider.Contacts.Where(c => c.IsDefault).Select(c => c.Address).Single()));
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
                    recipientIds.AddRange(GetRegionAdminsIds(provider.Contacts.Where(c => c.IsDefault).Select(c => c.Address).Single()));
                }
                else if (status == ProviderStatus.Editing
                         || status == ProviderStatus.Approved)
                {
                    var providerEmployeeUserIds = await officialRepository
                        .GetActiveOfficialUserIdsByProviderId(provider.Id);
                    recipientIds.AddRange(providerEmployeeUserIds);
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
                    var providerEmployeeUserIds = await officialRepository
                        .Get(whereExpression: o => !o.IsDeleted && o.Position.ProviderId == provider.Id && o.Individual.UserId != null)
                        .Include(o => o.Individual)
                        .Select(o => o.Individual.UserId)
                        .ToListAsync()
                        .ConfigureAwait(false);
                    recipientIds.AddRange(providerEmployeeUserIds);
                }
            }
        }
        else if (action == NotificationAction.Block || action == NotificationAction.Unblock)
        {
            var directorUserId = await officialRepository
                .GetDirectorOfficialUserIdByProviderIdAsync(provider.Id);

            if (string.IsNullOrEmpty(directorUserId))
            {
                logger.LogWarning("Director user ID is null or empty for provider with Id {ProviderId}.", provider.Id);
            }
            else
            {
                logger.LogInformation("Director user ID {DirectorUserId} found for provider with Id {ProviderId}.", directorUserId, provider.Id);
                recipientIds.Add(directorUserId);
            }
        }

        return recipientIds.Distinct();
    }

    private protected async Task<ProviderDto> CreateProviderWithActionAfterAsync(ProviderCreateDto providerDto, Func<Provider, Task> actionAfterCreation = null)
    {
        _ = providerDto ?? throw new ArgumentNullException(nameof(providerDto));

        logger.LogDebug("Provider creating was started");

        var providerDomainModel = providerDto.ToModel();

        contactsService.PrepareNewContacts(providerDomainModel, providerDto);

        // BUG: concurrency issue:
        //      while first repository with this particular user id is not saved to DB - we can create any number of repositories for this user.
        if (providerRepository.SameExists(providerDomainModel))
        {
            throw new InvalidOperationException(localizer["There is already a provider with such a data"]);
        }

        providerDomainModel.Status = ProviderStatus.Pending;
        providerDomainModel.LicenseStatus = providerDomainModel.License == null
            ? ProviderLicenseStatus.NotProvided
            : ProviderLicenseStatus.Pending;

        var newProvider = await providerRepository.Create(providerDomainModel).ConfigureAwait(false);

        if (newProvider is not null)
        {
            await changesLogService.AddCreatingOfEntityToDbContext(newProvider, currentUserService.UserId).ConfigureAwait(false);
        }

        if (actionAfterCreation != null)
        {
            await actionAfterCreation(newProvider).ConfigureAwait(false);
            await UpdateProvider().ConfigureAwait(false);
        }

        logger.LogDebug("Provider with Id = {ProviderId} created successfully", newProvider?.Id);

        await SendNotification(newProvider, NotificationAction.Create, true, true);

        return newProvider.ToDto();
    }

    private protected async Task<ProviderDto> UpdateProviderWithActionBeforeSavingChanges(ProviderUpdateDto providerUpdateDto, string userId, Func<Provider, Task> actionBeforeUpdating = null)
    {
        _ = providerUpdateDto ?? throw new ArgumentNullException(nameof(providerUpdateDto));
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }

        logger.LogDebug("Updating Provider with Id = {Id} was started", providerUpdateDto.Id);

        try
        {
            await currentUserService.UserHasRights(new ProviderRights(providerUpdateDto.Id), new DeputyDirectorRights(providerUpdateDto.Id)).ConfigureAwait(false);

            var checkProvider = await providerRepository.GetWithNavigations(providerUpdateDto.Id).ConfigureAwait(false);

            ChangeProviderStatusIfNeeded(providerUpdateDto, checkProvider, out var statusChanged, out var licenseChanged);

            contactsService.PrepareUpdatedContacts(checkProvider, providerUpdateDto);

            if (IsNeedInRelatedWorkshopsUpdating(providerUpdateDto, checkProvider))
            {
                checkProvider = await providerRepository.RunInTransaction(async () =>
                {
                    var workshops = await workshopServiceCombiner
                        .UpdateProviderTitleES(providerUpdateDto.Id, providerUpdateDto.FullTitle,
                            providerUpdateDto.FullTitleEn)
                        .ConfigureAwait(false);

                    providerUpdateDto.SetToModel(checkProvider);
                    LogProviderChanges(checkProvider, userId);
                    await UpdateProvider().ConfigureAwait(false);

                    foreach (var workshop in workshops)
                    {
                        logger.LogDebug("Provider's properties with Id = {ProviderId} " +
                                              "in workshops with Id = {WorkshopId} updated successfully in ElasticSearch", checkProvider?.Id, workshop?.Id);
                    }

                    return checkProvider;
                }).ConfigureAwait(false);
            }
            else
            {
                providerUpdateDto.SetToModel(checkProvider);
            }

            if (actionBeforeUpdating != null)
            {
                await actionBeforeUpdating(checkProvider).ConfigureAwait(false);
            }

            LogProviderChanges(checkProvider, userId);
            await UpdateProvider().ConfigureAwait(false);

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

            return checkProvider.ToDto();
        }
        finally
        {
            logger.LogTrace("Updating Provider with Id = {Id} was finished", providerUpdateDto.Id);
        }
    }

    private protected async Task<Either<ErrorResponse, bool>> DeleteProviderWithActionBefore(Guid id, Func<Provider, Task> actionBeforeDeleting = null)
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

        var currentUserProviderId = currentUserService.ProviderId;
        
        if (currentUserProviderId != entity.Id && !await IsCurrentUserIsAdminOfDistrictOrMinistryOfProvider(entity))
        {
            var message = $"User with userId = {currentUserService.UserId} has no rights to delete Provider with id = {entity.Id}";
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
            // TODO: if we really add deletion - need to delete everything provider has linked to it (positions, officials, etc.)
        });

        logger.LogInformation("Provider with Id = {Id} successfully deleted", id);
        return true;
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
              && checkProvider.ShortTitle == providerDto.ShortTitle))
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
            return regionAdmin.InstitutionId == provider.InstitutionId && provider.Contacts.Any(c => c.IsDefault && listOfCATOTTG.Contains(c.Address.CATOTTGId));
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId).ConfigureAwait(false);
            var listOfCATOTTG = await codeficatorService.GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);
            return areaAdmin.InstitutionId == provider.InstitutionId && provider.Contacts.Any(c => c.IsDefault && listOfCATOTTG.Contains(c.Address.CATOTTGId));
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
                if (word.Any(char.IsLetter))
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.FullTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.ShortTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.Contacts.Any(c => c.Emails.Any(e => e.Address.Contains(word, StringComparison.InvariantCultureIgnoreCase)))
                            || x.FullTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.ShortTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    string searchNumber = string.Join(string.Empty, word.Where(c => char.IsNumber(c)));
                    if (searchNumber.Length > 0)
                    {
                        tempPredicate = tempPredicate.Or(
                            x => x.Contacts.Any(c => c.Phones.Any(p =>
                                     p.Number.Contains(searchNumber, StringComparison.InvariantCultureIgnoreCase)))
                                 || x.Edrpou.Contains(searchNumber, StringComparison.InvariantCultureIgnoreCase)
                                 || x.Contacts.Any(c => c.Emails.Any(e =>
                                     e.Address.Contains(searchNumber, StringComparison.InvariantCultureIgnoreCase))));
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

    private List<string> GetRegionAdminsIds(ContactsAddress address)
    {
        var regionAdminsIds = regionAdminRepository
            .GetByFilterNoTracking(a => a.CATOTTGId == address.CATOTTGId)
            .Select(a => a.UserId)
            .ToList();

        return regionAdminsIds;
    }

    private List<string> GetAreaAdminsIds(ContactsAddress address)
        => areaAdminRepository
            .GetByFilterNoTracking(a => a.CATOTTGId == address.CATOTTGId)
            .Select(a => a.UserId)
            .ToList();

    private void CheckListOfEmployeesForUploading(UploadEmployeeRequestDto[] data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));

        logger.LogDebug("Upload employees for provider was started");

        if (data.Length == 0)
        {
            var errorMessage = "The number of entries to upload should be greater than 0";
            logger.LogError(errorMessage);
            throw new ArgumentOutOfRangeException(errorMessage);
        }

        if (data.Length > Constants.MaxNumberOfEmployeesToUpload)
        {
            var errorMessage = $"The number of entries should not exceed {Constants.MaxNumberOfEmployeesToUpload}.";
            logger.LogError("The number of entries should not exceed {MaxNumberOfEmployeesToUpload}", Constants.MaxNumberOfEmployeesToUpload);
            throw new ArgumentOutOfRangeException(errorMessage);
        }

        var uploadEmployeesRnokpps = data.Select(e => e.Rnokpp).ToList();

        // Check if the Rnokpp property values are unique?
        if (uploadEmployeesRnokpps.Distinct().Count() != data.Length)
        {
            var errorMessage = "The Rnokpp property values are not unique";
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    private async Task<Dictionary<Guid, UploadEmployeeRequestDto>> AddIndividualsToDb(UploadEmployeeRequestDto[] data,
                                                                                      UploadEmployeeResponse uploadResponse)
    {
        // Dictionary for uploading employees
        var uploadDictionary = new Dictionary<Guid, UploadEmployeeRequestDto>();
        // Create a list of Rnokpps.
        // The number of members in this list is limited by a constant - MaxNumberOfEmployeesToUpload.
        var listOfRnokpps = data.Select(e => e.Rnokpp).ToList();
        var existingIndividuals = (await individualRepository.GetByFilter(i => listOfRnokpps.Contains(i.Rnokpp))
                                                                          .ConfigureAwait(false))
                                                                          .Select(i => new { i.Rnokpp, i.Id })
                                                                          .ToDictionary(e => e.Rnokpp);

        // Loop to add individuals to DB and populate the Dictionary for uploading employees
        foreach (var employee in data)
        {
            if (existingIndividuals.TryGetValue(employee.Rnokpp, out var individual))
            {
                uploadDictionary.Add(individual.Id, employee);
            }
            else // Add an Individual to DB if it has not already existed in DB
            {
                var newIndividual = await individualRepository.Create(employee.ToModel());
                uploadResponse.CountOfCreatedIndividuals++;
                uploadDictionary.Add(newIndividual.Id, employee);
            }
        }

        return uploadDictionary;
    }

    private async Task FillDbWithNewEmployeesOnPositions(Dictionary<Guid, UploadEmployeeRequestDto> uploadDictionary,
                                                         Guid providerId,
                                                         UploadEmployeeResponse uploadResponse)
    {
        // Get a dictionary (Lookup) with keys - IndividualId and values - Official.Position.FullName for a certain Provider.
        var existingOfficialsForProvider = (await officialRepository.GetByFilter(o =>
                                                                                 o.Position.ProviderId == providerId
                                                                                 && uploadDictionary.Keys.Contains(o.IndividualId)
                                                                                 && (o.DismissalOrder == null || o.DismissalOrder == string.Empty)
                                                                                 , includeExpression: q => q.Include(o => o.Position))
                                                                                 .ConfigureAwait(false))
                                                                                 .ToLookup(o => o.IndividualId, o => o?.Position?.FullName);

        //Loop for filling the DB with new employees on certain positions.
        foreach (var key in uploadDictionary.Keys)
        {
            // If this Employee already exists and occupies the same Position
            if (existingOfficialsForProvider.Contains(key)
                && existingOfficialsForProvider[key].Contains(uploadDictionary[key].AssignedRole))
            {
                continue;
            }

            // Create a new Position if it doesn't exist
            var position = (await positionRepository.GetByFilter(
                                                                 p => p.ProviderId == providerId
                                                                 && p.FullName == uploadDictionary[key].AssignedRole
                                                                 ).ConfigureAwait(false))
                                                                 .FirstOrDefault();

            if (position == default)
            {
                position = await positionRepository.Create(
                new Position
                {
                    ProviderId = providerId,
                    FullName = uploadDictionary[key].AssignedRole,
                    PositionType = uploadDictionary[key].PositionType,
                }).ConfigureAwait(false);
                uploadResponse.CountOfCreatedPositions++;
            }

            // Create a new Official
            await officialRepository.Create(
                new Official()
                {
                    IndividualId = key,
                    PositionId = position.Id
                }).ConfigureAwait(false);
            uploadResponse.CountOfCreatedOfficials++;
        }
    }
}