using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;
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
    private readonly IRatingService ratingService;
    private readonly ILogger<ProviderService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly IEntityRepository<long, Address> addressRepository;
    private readonly IWorkshopServicesCombiner workshopServiceCombiner;
    private readonly IChangesLogService changesLogService;
    private readonly INotificationService notificationService;
    private readonly IProviderAdminService providerAdminService;

    // TODO: It should be removed after models revision.
    //       Temporary instance to fill 'Provider' model 'User' property
    private readonly IEntityRepository<string, User> usersRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderService"/> class.
    /// </summary>
    /// <param name="providerRepository">Provider repository.</param>
    /// <param name="usersRepository">UsersRepository.</param>
    /// <param name="ratingService">Rating service.</param>
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
    public ProviderService(
        IProviderRepository providerRepository,
        IEntityRepository<string, User> usersRepository,
        IRatingService ratingService,
        ILogger<ProviderService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        IEntityRepository<long, Address> addressRepository,
        IWorkshopServicesCombiner workshopServiceCombiner,
        IProviderAdminRepository providerAdminRepository,
        IImageDependentEntityImagesInteractionService<Provider> providerImagesService,
        IChangesLogService changesLogService,
        INotificationService notificationService,
        IProviderAdminService providerAdminService)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        this.ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.workshopServiceCombiner = workshopServiceCombiner ?? throw new ArgumentNullException(nameof(workshopServiceCombiner));
        this.providerAdminRepository = providerAdminRepository ?? throw new ArgumentNullException(nameof(providerAdminRepository));
        ProviderImagesService = providerImagesService ?? throw new ArgumentNullException(nameof(providerImagesService));
        this.changesLogService = changesLogService ?? throw new ArgumentNullException(nameof(changesLogService));
        this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.providerAdminService = providerAdminService;
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

        int count = await providerRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Provider, object>>, SortDirection>
    {
        { x => x.User.FirstName, SortDirection.Ascending },
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
        FillRatingsForProviders(providersDTO);

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

        var rating = ratingService.GetAverageRating(providerDTO.Id, RatingType.Provider);

        providerDTO.Rating = rating?.Item1 ?? default;
        providerDTO.NumberOfRatings = rating?.Item2 ?? default;

        return providerDTO;
    }

    /// <inheritdoc/>
    public async Task<ProviderDto> GetByUserId(string id, bool isDeputyOrAdmin = false)
    {
        logger.LogInformation($"Getting Provider by UserId started. Looking UserId is {id}.");
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

        if (provider == null)
        {
            throw new ArgumentException(localizer["There is no Provider in the Db with such User id"], nameof(id));
        }

        logger.LogInformation($"Successfully got a Provider with UserId = {id}.");

        return mapper.Map<ProviderDto>(provider);
    }

    /// <inheritdoc/>
    public async Task<ProviderDto> Update(ProviderDto providerDto, string userId)
        => await UpdateProviderWithActionBeforeSavingChanges(providerDto, userId).ConfigureAwait(false);

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
        await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);

        logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.Status}");

        await SendNotification(provider, NotificationAction.Update, true, false).ConfigureAwait(false);

        return dto;
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
            // TODO: approval request is sent to district admin
            // AND Service provider waits until(district admin / ministry admin / tech admin) approves the profile
        }
        else if (action == NotificationAction.Update)
        {
            if (additionalData != null
                && additionalData.TryGetValue("Status", out var statusValue)
                && Enum.TryParse(statusValue, out ProviderStatus status))
            {
                if (status == ProviderStatus.Pending)
                {
                    // TODO: create a NEW approve request to District admin
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
                    // TODO: create a NEW approve request to District admin
                }
                else if (licenseStatus == ProviderLicenseStatus.Approved)
                {
                    recipientIds.Add(provider.UserId);
                    recipientIds.AddRange(await providerAdminService.GetProviderDeputiesIds(provider.Id).ConfigureAwait(false));
                }
            }
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

    private protected async Task<ProviderDto> UpdateProviderWithActionBeforeSavingChanges(ProviderDto providerDto, string userId, Func<Provider, Task> actionBeforeUpdating = null)
    {
        _ = providerDto ?? throw new ArgumentNullException(nameof(providerDto));
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }

        logger.LogDebug("Updating Provider with Id = {Id} was started", providerDto.Id);

        try
        {
            var checkProvider = await providerRepository.GetById(providerDto.Id).ConfigureAwait(false);

            if (checkProvider?.UserId != userId)
            {
                return null;
            }

            ChangeProviderStatusIfNeeded(providerDto, checkProvider, out var statusChanged, out var licenseChanged);

            providerDto.LegalAddress.Id = checkProvider.LegalAddress.Id;

            if (providerDto.LegalAddress.Equals(providerDto.ActualAddress))
            {
                providerDto.ActualAddress = null;
            }

            if (providerDto.ActualAddress is null && checkProvider.ActualAddress is { })
            {
                var checkProviderActualAddress = checkProvider.ActualAddress;
                checkProvider.ActualAddressId = null;
                checkProvider.ActualAddress = null;
                mapper.Map(providerDto, checkProvider);
                await addressRepository.Delete(checkProviderActualAddress).ConfigureAwait(false);
            }
            else
            {
                if (providerDto.ActualAddress != null)
                {
                    providerDto.ActualAddress.Id = checkProvider.ActualAddress?.Id ?? 0;
                }

                if (IsNeedInRelatedWorkshopsUpdating(providerDto, checkProvider))
                {
                    checkProvider = await providerRepository.RunInTransaction(async () =>
                    {
                        var workshops = await workshopServiceCombiner
                            .PartialUpdateByProvider(mapper.Map<Provider>(providerDto))
                            .ConfigureAwait(false);

                        mapper.Map(providerDto, checkProvider);
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
                    mapper.Map(providerDto, checkProvider);
                }

                if (actionBeforeUpdating != null)
                {
                    await actionBeforeUpdating(checkProvider).ConfigureAwait(false);
                }

                LogProviderChanges(checkProvider, userId);
                await UpdateProvider().ConfigureAwait(false);
            }

            logger.LogInformation("Provider with Id = {CheckProviderId} was updated successfully", checkProvider?.Id);

            if (statusChanged || licenseChanged)
            {
                await SendNotification(checkProvider, NotificationAction.Update, statusChanged, licenseChanged)
                    .ConfigureAwait(false);
            }

            return mapper.Map<ProviderDto>(checkProvider);
        }
        finally
        {
            logger.LogTrace("Updating Provider with Id = {Id} was finished", providerDto.Id);
        }
    }

    private void ChangeProviderStatusIfNeeded(
        ProviderDto providerDto,
        Provider checkProvider,
        out bool statusChanged,
        out bool licenseChanged)
    {
        statusChanged = false;
        licenseChanged = false;

        if (!(checkProvider.FullTitle == providerDto.FullTitle
              && checkProvider.EdrpouIpn == providerDto.EdrpouIpn))
        {
            checkProvider.Status = ProviderStatus.Pending;
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
            var entity = await providerRepository.GetById(id).ConfigureAwait(false);

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

    private static bool IsNeedInRelatedWorkshopsUpdating(ProviderDto providerDto, Provider checkProvider)
    {
        return checkProvider.FullTitle != providerDto.FullTitle
               || checkProvider.Ownership != providerDto.Ownership;
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
                tempPredicate = tempPredicate.Or(
                    x => x.FullTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.ShortTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.ActualAddress.City.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.LegalAddress.City.StartsWith(word, StringComparison.InvariantCulture)
                        || x.EdrpouIpn.StartsWith(word, StringComparison.InvariantCultureIgnoreCase));
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

        return predicate;
    }

    private void FillRatingsForProviders(List<ProviderDto> providersDTO)
    {
        var averageRatings =
            ratingService.GetAverageRatingForRange(providersDTO.Select(p => p.Id), RatingType.Provider);

        foreach (var provider in providersDTO)
        {
            var averageRatingsForProvider = averageRatings.FirstOrDefault(r => r.Key == provider.Id);
            if (averageRatingsForProvider.Key != Guid.Empty)
            {
                var (_, (rating, numberOfVotes)) = averageRatingsForProvider;
                provider.Rating = rating;
                provider.NumberOfRatings = numberOfVotes;
            }
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
}