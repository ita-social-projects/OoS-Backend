using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Provider entity.
/// </summary>
public class ProviderService : IProviderService, INotificationReciever
{
    private readonly IProviderRepository providerRepository;
    private readonly IProviderAdminRepository providerAdminRepository;
    private readonly ILogger<ProviderService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly IEntityRepository<long, Address> addressRepository;
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

    // TODO: It should be removed after models revision.
    //       Temporary instance to fill 'Provider' model 'User' property
    private readonly IEntityRepository<string, User> usersRepository;

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
    /// <param name="regionAdminRepository">RegionAdminRepository</param>
    /// <param name="averageRatingService">Average rating service.</param>
    public ProviderService(
        IProviderRepository providerRepository,
        IEntityRepository<string, User> usersRepository,
        ILogger<ProviderService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        IEntityRepository<long, Address> addressRepository,
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
        IAverageRatingService averageRatingService)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }

    private protected IImageDependentEntityImagesInteractionService<Provider> ProviderImagesService { get; }

    /// <inheritdoc/>
    public async Task<ProviderDto> Create(ProviderDto providerDto)
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

        int count = await providerRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Provider, object>>, SortDirection>
        {
            { x => x.IsBlocked, SortDirection.Ascending },
            { x => x.Status, SortDirection.Ascending },
        };

        var providers = await providerRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: string.Empty,
                where: filterPredicate,
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
        var provider = await providerRepository.GetById(id).ConfigureAwait(false);

        if (provider == null)
        {
            return null;
        }

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
    public async Task<ProviderDto?> GetByUserId(string id, bool isDeputyOrAdmin = false)
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
    public async Task<ProviderDto> Update(ProviderUpdateDto providerUpdateDto, string userId)
        => await UpdateProviderWithActionBeforeSavingChanges(providerUpdateDto, userId).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task Delete(Guid id) => await DeleteProviderWithActionBefore(id).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Guid> GetProviderIdForWorkshopById(Guid workshopId) =>
        await workshopServiceCombiner.GetWorkshopProviderId(workshopId).ConfigureAwait(false);

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

    /// <inheritdoc/>
    public async Task<ResponseDto> Block(ProviderBlockDto providerBlockDto)
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

        var isUserHasRights = await IsCurrentUserHasRightsToBlockProvider(provider);

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

        await providerRepository.RunInTransaction(async () =>
        {
            await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);

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

        await SendNotification(
            provider,
            providerBlockDto.IsBlocked ? NotificationAction.Block : NotificationAction.Unblock,
            false,
            false);

        var blockedStatus = providerBlockDto.IsBlocked ? "blocked" : "unblocked";
        return new ResponseDto()
        {
            Result = providerBlockDto,
            Message = $"The user {currentUserService.UserId} {blockedStatus} the Provider {providerBlockDto.Id}",
            HttpStatusCode = HttpStatusCode.OK,
            IsSuccess = true,
        };
    }

    public async Task<bool> IsProviderBlocked(Guid providerId)
    {
        return (await providerRepository.GetById(providerId).ConfigureAwait(false)).IsBlocked;
    }

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

    async Task<IEnumerable<string>> INotificationReciever.GetNotificationsRecipientIds(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
    {
        var recipientIds = new List<string>();

        var provider = await providerRepository.GetById(objectId).ConfigureAwait(false);

        if (provider is null)
        {
            return recipientIds;
        }

        if (action == NotificationAction.Create)
        {
            recipientIds.AddRange(await GetTechAdminsIds().ConfigureAwait(false));
            recipientIds.AddRange(await GetMinistryAdminsIds(provider.InstitutionId).ConfigureAwait(false));
            recipientIds.AddRange(await GetRegionAdminsIds(provider.LegalAddress).ConfigureAwait(false));
        }
        else if (action == NotificationAction.Update)
        {
            if (additionalData != null
                && additionalData.TryGetValue("Status", out var statusValue)
                && Enum.TryParse(statusValue, out ProviderStatus status))
            {
                if (status == ProviderStatus.Recheck)
                {
                    recipientIds.AddRange(await GetTechAdminsIds().ConfigureAwait(false));
                    recipientIds.AddRange(await GetMinistryAdminsIds(provider.InstitutionId).ConfigureAwait(false));
                    recipientIds.AddRange(await GetRegionAdminsIds(provider.LegalAddress).ConfigureAwait(false));
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
                    recipientIds.AddRange(await GetTechAdminsIds().ConfigureAwait(false));
                    recipientIds.AddRange(await GetMinistryAdminsIds(provider.InstitutionId).ConfigureAwait(false));
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

    private protected async Task<ProviderDto> CreateProviderWithActionAfterAsync(ProviderDto providerDto, Func<Provider, Task> actionAfterCreation = null)
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
        providerDomainModel.Status = ProviderStatus.Pending;
        providerDomainModel.LicenseStatus = providerDomainModel.License == null
            ? ProviderLicenseStatus.NotProvided
            : ProviderLicenseStatus.Pending;

        var newProvider = await providerRepository.Create(providerDomainModel).ConfigureAwait(false);

        if (actionAfterCreation != null)
        {
            await actionAfterCreation(newProvider).ConfigureAwait(false);
            await UpdateProvider().ConfigureAwait(false);
        }

        logger.LogDebug("Provider with Id = {ProviderId} created successfully", newProvider?.Id);

        await SendNotification(newProvider, NotificationAction.Create, true, true).ConfigureAwait(false);

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
                            .UpdateProviderTitle(providerUpdateDto.Id, providerUpdateDto.FullTitle)
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
                await SendNotification(checkProvider, NotificationAction.Update, statusChanged, licenseChanged)
                    .ConfigureAwait(false);
            }

            return mapper.Map<ProviderDto>(checkProvider);
        }
        finally
        {
            logger.LogTrace("Updating Provider with Id = {Id} was finished", providerUpdateDto.Id);
        }
    }

    private void ChangeProviderStatusIfNeeded(
        ProviderUpdateDto providerDto,
        Provider checkProvider,
        out bool statusChanged,
        out bool licenseChanged)
    {
        statusChanged = false;
        licenseChanged = false;

        if (!(checkProvider.FullTitle == providerDto.FullTitle
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

    private protected async Task DeleteProviderWithActionBefore(Guid id, Func<Provider, Task> actionBeforeDeleting = null)
    {
        // BUG: Possible bug with deleting provider not owned by the user itself.
        // TODO: add unit tests to check ownership functionality
        logger.LogInformation("Deleting Provider with Id = {Id} started", id);

        try
        {
            var entity = await providerRepository.GetWithNavigations(id).ConfigureAwait(false);

            if (entity is null)
            {
                throw new ArgumentException($"There is no Provider in DB with Id - {id}");
            }

            if (actionBeforeDeleting != null)
            {
                await actionBeforeDeleting(entity).ConfigureAwait(false);
            }

            await providerRepository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation("Provider with Id = {Id} successfully deleted", id);
        }
        catch (ArgumentNullException ex)
        {
            logger.LogError(ex, "Deleting failed. Provider with Id = {Id} doesn't exist in the system", id);
            throw;
        }
    }

    private static bool IsNeedInRelatedWorkshopsUpdating(ProviderUpdateDto providerDto, Provider checkProvider)
    {
        return checkProvider.FullTitle != providerDto.FullTitle;
    }

    private async Task UpdateProvider()
    {
        try
        {
            await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
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

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Any(c => char.IsLetter(c)))
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.FullTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.ShortTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase));
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

    private async Task SendNotification(
        Provider provider,
        NotificationAction notificationAction,
        bool addStatusData,
        bool addLicenseStatusData)
    {
        if (provider != null)
        {
            var additionalData = new Dictionary<string, string>();

            if (addStatusData)
            {
                additionalData.Add("Status", provider.Status.ToString());
            }

            if (addLicenseStatusData)
            {
                additionalData.Add("LicenseStatus", provider.LicenseStatus.ToString());
            }

            await notificationService.Create(
                    NotificationType.Provider,
                    notificationAction,
                    provider.Id,
                    this,
                    additionalData)
                .ConfigureAwait(false);
        }
    }

    private async Task<IEnumerable<string>> GetTechAdminsIds()
    {
        var techAdminIds = (await usersRepository
                        .GetByFilter(u => u.Role == nameof(Role.TechAdmin).ToLower())
                        .ConfigureAwait(false))
                        .Select(u => u.Id);
        return techAdminIds;
    }

    private async Task<IEnumerable<string>> GetMinistryAdminsIds(Guid? ministryId)
    {
        if (ministryId == null)
        {
            return new List<string>();
        }

        var ministryAdminsIds = await institutionAdminRepository
                        .GetByFilterNoTracking(a => a.InstitutionId == ministryId)
                        .Select(a => a.UserId)
                        .ToListAsync()
                        .ConfigureAwait(false);
        return ministryAdminsIds;
    }

    private async Task<IEnumerable<string>> GetRegionAdminsIds(Address address)
    {
        var regionAdminsIds = await regionAdminRepository
            .GetByFilterNoTracking(a => a.CATOTTGId == address.CATOTTGId)
            .Select(a => a.UserId)
            .ToListAsync()
            .ConfigureAwait(false);

        return regionAdminsIds;
    }

    private async Task UpdateWorkshopsProviderStatus(Guid providerId, ProviderStatus providerStatus)
    {
        var workshops = await workshopServiceCombiner.UpdateProviderStatus(providerId, providerStatus)
           .ConfigureAwait(false);

        foreach (var workshop in workshops)
        {
            logger.LogInformation($"Provider's status with Id = {providerId} " +
                                  $"in workshops with Id = {workshop.Id} updated successfully.");
        }
    }

    private async Task<bool> ExistsAnotherProviderWithTheSameEdrpouIpn(ProviderUpdateDto providerUpdateDto)
    {
        var providersWithTheSameEdrpouIpn = await providerRepository
            .GetByFilter(x => x.EdrpouIpn == providerUpdateDto.EdrpouIpn && x.Id != providerUpdateDto.Id)
            .ConfigureAwait(false);

        return providersWithTheSameEdrpouIpn.Any();
    }

    private async Task<bool> IsCurrentUserHasRightsToBlockProvider(Provider provider)
    {
        if (currentUserService.IsAdmin() && !currentUserService.IsAreaAdmin())
        {
            if (currentUserService.IsMinistryAdmin())
            {
                var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);

                if (ministryAdmin.InstitutionId != provider.InstitutionId)
                {
                    return false;
                }
            }

            if (currentUserService.IsRegionAdmin())
            {
                var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);

                var subSettlementsIds = await codeficatorService
                    .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

                if (regionAdmin.InstitutionId != provider.InstitutionId || !subSettlementsIds.Contains(provider.LegalAddress.CATOTTGId))
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}
