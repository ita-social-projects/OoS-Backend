using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;

namespace OutOfSchool.WebApi.Services;

public class WorkshopServicesCombiner : IWorkshopServicesCombiner, INotificationReciever
{
    private protected readonly IWorkshopService workshopService; // make it private after removing v2 version
    private protected readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService; // make it private after removing v2 version
    private readonly INotificationService notificationService;
    private readonly IEntityRepository<long, Favorite> favoriteRepository;
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
        IEntityRepository<long, Favorite> favoriteRepository,
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
    public async Task<WorkshopDTO> Create(WorkshopDTO dto)
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
    public async Task<WorkshopDTO> GetById(Guid id)
    {
        var workshop = await workshopService.GetById(id).ConfigureAwait(false);

        return workshop;
    }

    /// <inheritdoc/>
    public async Task<WorkshopDTO> Update(WorkshopDTO dto)
    {
        var workshop = await workshopService.Update(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                workshop.Id,
                ElasticsearchSyncOperation.Update)
            .ConfigureAwait(false);

        return workshop;
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

        await notificationService.Create(
            NotificationType.Workshop,
            NotificationAction.Update,
            workshopDto.WorkshopId,
            this,
            additionalData).ConfigureAwait(false);

        return dto;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle)
    {
        var workshops = await workshopService.UpdateProviderTitle(providerId, providerTitle).ConfigureAwait(false);

        foreach (var workshop in workshops)
        {
            await esProvider
                .PartialUpdateEntityAsync(workshop.Id, new WorkshopProviderTitleES { ProviderTitle = providerTitle })
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
        await workshopService.Delete(id).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                id,
                ElasticsearchSyncOperation.Delete)
            .ConfigureAwait(false);
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

        var settlementsFilter = mapper.Map<WorkshopBySettlementsFilter>(filter);

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
    public async Task<List<ShortEntityDto>> GetWorkshopListByProviderAdminId(string providerAdminId)
    {
        return await workshopService.GetWorkshopListByProviderAdminId(providerAdminId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<T>> GetByProviderId<T>(Guid id, ExcludeIdFilter filter)
        where T : WorkshopBaseCard
    {
        var workshopBaseCards = await workshopService.GetByProviderId<T>(id, filter).ConfigureAwait(false);

        return workshopBaseCards;
    }

    /// <inheritdoc/>
    public async Task<Guid> GetWorkshopProviderId(Guid workshopId) =>
        await workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);

    public async Task<IEnumerable<string>> GetNotificationsRecipientIds(
        NotificationAction action,
        Dictionary<string, string> additionalData,
        Guid objectId)
    {
        var recipientIds = new List<string>();

        var favoriteWorkshopUsersIds = await favoriteRepository.Get(where: x => x.WorkshopId == objectId)
            .Select(x => x.UserId)
            .ToListAsync()
            .ConfigureAwait(false);

        Expression<Func<Application, bool>> predicate =
            x => x.Status != ApplicationStatus.Left
                 && x.WorkshopId == objectId;

        var appliedUsersIds = await applicationRepository.Get(where: predicate)
            .Select(x => x.Parent.UserId)
            .ToListAsync()
            .ConfigureAwait(false);

        recipientIds.AddRange(favoriteWorkshopUsersIds);
        recipientIds.AddRange(appliedUsersIds);

        return recipientIds.Distinct();
    }

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

    private bool IsFilterValid(WorkshopFilter filter)
    {
        return filter != null && filter.MaxStartTime >= filter.MinStartTime
                              && filter.MaxAge >= filter.MinAge
                              && filter.MaxPrice >= filter.MinPrice;
    }
}
