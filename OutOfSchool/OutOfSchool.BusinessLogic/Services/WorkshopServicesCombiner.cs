using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class WorkshopServicesCombiner : IWorkshopServicesCombiner
{
    private protected readonly IWorkshopService workshopService; // make it private after removing v2 version
    private protected readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService; // make it private after removing v2 version
    private readonly INotificationService notificationService;
    private readonly IEntityRepositorySoftDeleted<long, Favorite> favoriteRepository;
    private readonly IApplicationRepository applicationRepository;
    private readonly IWorkshopStrategy workshopStrategy;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;
    private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
    private readonly IMapper mapper;

    public WorkshopServicesCombiner(
        IWorkshopService workshopService,
        IElasticsearchSynchronizationService elasticsearchSynchronizationService,
        INotificationService notificationService,
        IEntityRepositorySoftDeleted<long, Favorite> favoriteRepository,
        IApplicationRepository applicationRepository,
        IWorkshopStrategy workshopStrategy,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        ICodeficatorService codeficatorService,
        IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
        IMapper mapper)
    {
        this.workshopService = workshopService;
        this.elasticsearchSynchronizationService = elasticsearchSynchronizationService;
        this.notificationService = notificationService;
        this.favoriteRepository = favoriteRepository;
        this.applicationRepository = applicationRepository;
        this.workshopStrategy = workshopStrategy;
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
        this.esProvider = esProvider ?? throw new ArgumentNullException(nameof(esProvider));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<WorkshopDto> Create(WorkshopCreateRequestDto dto)
    {
        var workshop = await workshopService.Create(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                workshop.Id,
                ElasticsearchSyncOperation.Create)
            .ConfigureAwait(false);

        return workshop;
    }

    /// <inheritdoc/>
    public Task<bool> Exists(Guid id)
    {
        return workshopService.Exists(id);
    }

    /// <inheritdoc/>
    public async Task<WorkshopDto> GetById(Guid id, bool asNoTracking = false)
    {
        var workshop = await workshopService.GetById(id, asNoTracking).ConfigureAwait(false);

        return workshop;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkshopDto>> Update(WorkshopCreateUpdateDto dto)
    {
        var currentWorkshop = await GetById(dto.Id, true).ConfigureAwait(false);
        if (currentWorkshop is null)
        {
            return Result<WorkshopDto>.Failed(new OperationError
            {
                Code = HttpStatusCode.BadRequest.ToString(),
                Description = Constants.WorkshopNotFoundErrorMessage,
            });
        }

        if (!IsAvailableSeatsValidForWorkshop(dto.AvailableSeats, currentWorkshop))
        {
            return Result<WorkshopDto>.Failed(new OperationError
            {
                Code = HttpStatusCode.BadRequest.ToString(),
                Description = Constants.InvalidAvailableSeatsForWorkshopErrorMessage,
            });
        }

        var updatedWorkshop = await workshopService.Update(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                updatedWorkshop.Id,
                ElasticsearchSyncOperation.Update).ConfigureAwait(false);

        return Result<WorkshopDto>.Success(updatedWorkshop);
    }

    /// <inheritdoc/>
    public async Task<Result<WorkshopDto>> UpdateTags(WorkshopTagsUpdateDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        var workshop = await workshopService.UpdateTags(dto).ConfigureAwait(false);

        return Result<WorkshopDto>.Success(workshop);
    }

    /// <inheritdoc/>
    public async Task<WorkshopStatusDto> UpdateStatus(WorkshopStatusDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        var workshopDto = await workshopService.UpdateStatus(dto).ConfigureAwait(false);

        var additionalData = new Dictionary<string, string>()
        {
            { "Status", workshopDto.Status.ToString() },
            { "Title", workshopDto.Title },
        };

        var recipientsIds = await GetNotificationsRecipientIds(workshopDto.WorkshopId).ConfigureAwait(false);

        await notificationService.Create(
            NotificationType.Workshop,
            NotificationAction.Update,
            workshopDto.WorkshopId,
            recipientsIds,
            additionalData).ConfigureAwait(false);

        return dto;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle, string providerTitleEn)
    {
        var workshops = await workshopService.UpdateProviderTitle(providerId, providerTitle, providerTitleEn).ConfigureAwait(false);

        foreach (var workshop in workshops)
        {
            await esProvider
                .PartialUpdateEntityAsync(workshop.Id, new WorkshopProviderTitleES { ProviderTitle = providerTitle, ProviderTitleEn = providerTitleEn })
                .ConfigureAwait(false);
        }

        return workshops;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workshop>> BlockByProvider(Provider provider)
    {
        var workshops = await workshopService.BlockByProvider(provider).ConfigureAwait(false);

        foreach (var workshop in workshops)
        {
            await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                    ElasticsearchSyncEntity.Workshop,
                    workshop.Id,
                    ElasticsearchSyncOperation.Update)
                .ConfigureAwait(false);
        }

        return workshops;
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        var notificationsRecipientIds = await GetNotificationsRecipientIds(id).ConfigureAwait(false);

        var workshopDto = await workshopService.GetById(id).ConfigureAwait(false);

        await workshopService.Delete(id).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                id,
                ElasticsearchSyncOperation.Delete)
            .ConfigureAwait(false);

        await SendNotification(workshopDto, NotificationAction.Delete, false, notificationsRecipientIds).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopCard>> GetAll(OffsetFilter offsetFilter)
    {
        if (offsetFilter == null)
        {
            offsetFilter = new OffsetFilter();
        }

        var filter = new WorkshopFilter()
        {
            Size = offsetFilter.Size,
            From = offsetFilter.From,
            OrderByField = OrderBy.Id.ToString(),
        };

        return await workshopStrategy.SearchAsync(filter);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter)
    {
        if (!IsFilterValid(filter))
        {
            return new SearchResult<WorkshopCard> { TotalAmount = 0, Entities = new List<WorkshopCard>() };
        }

        return await workshopStrategy.SearchAsync(filter);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopCard>> GetByFilterForAdmins(WorkshopFilter filter)
    {
        if (!IsFilterValid(filter))
        {
            return new SearchResult<WorkshopCard> { TotalAmount = 0, Entities = new List<WorkshopCard>() };
        }

        var settlementsFilter = mapper.Map<WorkshopFilterWithSettlements>(filter);

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            settlementsFilter.InstitutionId = ministryAdmin.InstitutionId;
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            settlementsFilter.InstitutionId = regionAdmin.InstitutionId;
            settlementsFilter.SettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);
        }

        return await workshopService.GetByFilter(settlementsFilter).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ShortEntityDto>> GetWorkshopListByProviderId(Guid providerId)
    {
        return await workshopService.GetWorkshopListByProviderId(providerId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ShortEntityDto>> GetWorkshopListByEmployeeId(string providerAdminId)
    {
        return await workshopService.GetWorkshopListByEmployeeId(providerAdminId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<SearchResult<WorkshopProviderViewCard>> GetByProviderId(Guid id, ExcludeIdFilter filter)
        => workshopService.GetByProviderId(id, filter);

    /// <inheritdoc/>
    public async Task<Guid> GetWorkshopProviderId(Guid workshopId) =>
        await workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IEnumerable<ShortEntityDto>> UpdateProviderStatus(Guid providerId, ProviderStatus providerStatus)
    {
        var shortWorkshops = await workshopService.GetWorkshopListByProviderId(providerId).ConfigureAwait(false);

        foreach (var workshop in shortWorkshops)
        {
            await esProvider
                .PartialUpdateEntityAsync(workshop.Id, new WorkshopProviderStatusES { ProviderStatus = providerStatus })
                .ConfigureAwait(false);
        }

        return shortWorkshops;
    }

    /// <summary>
    /// Checks if the given available seats value is valid for the specified workshop.
    /// </summary>
    /// <param name="availableSeats">The number of available seats to validate.</param>
    /// <param name="workshop">The workshop for which the available seats value is being validated.</param>
    /// <returns> A boolean value indicating whether the available seats value is valid for the workshop.</returns>
    public bool IsAvailableSeatsValidForWorkshop(uint? availableSeats, WorkshopDto workshop)
    {
        return availableSeats.GetMaxValueIfNullOrZero() >= workshop.TakenSeats;
    }

    private async Task<IEnumerable<string>> GetNotificationsRecipientIds(Guid objectId)
    {
        var recipientIds = new List<string>();

        var favoriteWorkshopUsersIds = await favoriteRepository.Get(whereExpression: x => x.WorkshopId == objectId)
            .Select(x => x.UserId)
            .ToListAsync()
            .ConfigureAwait(false);

        Expression<Func<Application, bool>> predicate =
            x => x.Status != ApplicationStatus.Left
                    && x.WorkshopId == objectId;

        var appliedUsersIds = await applicationRepository.Get(whereExpression: predicate)
            .Select(x => x.Parent.UserId)
            .ToListAsync()
            .ConfigureAwait(false);

        recipientIds.AddRange(favoriteWorkshopUsersIds);
        recipientIds.AddRange(appliedUsersIds);

        return recipientIds.Distinct();
    }

    private bool IsFilterValid(WorkshopFilter filter)
    {
        return filter != null && filter.MaxStartTime >= filter.MinStartTime
                              && filter.MaxAge >= filter.MinAge
                              && filter.MaxPrice >= filter.MinPrice;
    }

    private async Task SendNotification(
        WorkshopDto workshop,
        NotificationAction notificationAction,
        bool addStatusData,
        IEnumerable<string> recipientsIds)
    {
        if (workshop != null)
        {
            var additionalData = new Dictionary<string, string>()
            {
                { "Title", workshop.Title },
            };

            if (addStatusData)
            {
                additionalData.Add("Status", workshop.Status.ToString());
            }

            await notificationService.Create(
                    NotificationType.Workshop,
                    notificationAction,
                    workshop.Id,
                    recipientsIds,
                    additionalData)
                .ConfigureAwait(false);
        }
    }
}
