using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class WorkshopServicesCombiner(
    IWorkshopService workshopService,
    IElasticsearchSynchronizationService<IWorkshopService, Workshop> elasticsearchSynchronizationService,
    ISensitiveWorkshopsService sensitiveWorkshopService,
    INotificationService notificationService,
    IEntityRepositorySoftDeleted<long, Favorite> favoriteRepository,
    IApplicationRepository applicationRepository,
    IWorkshopStrategy workshopStrategy,
    ICurrentUserService currentUserService,
    IMinistryAdminService ministryAdminService,
    IRegionAdminService regionAdminService,
    ICodeficatorService codeficatorService,
    IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider
) : IWorkshopServicesCombiner
{
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

        if (currentWorkshop.Status == WorkshopStatus.Archived)
        {
            return Result<WorkshopDto>.Failed(new OperationError
            {
                Code = nameof(HttpStatusCode.BadRequest),
                Description = "Workshop is archived and cannot be updated.",
            });
        }

        if (!IsAvailableSeatsValidForWorkshop(dto.AvailableSeats, currentWorkshop))
        {
            return Result<WorkshopDto>.Failed(new OperationError
            {
                Code = nameof(HttpStatusCode.BadRequest),
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
    public async Task<OperationResult> Archive(Guid id)
    {
        return await HandleDeletionOrArchival(
            id,
            () => workshopService.Archive(id));
    }

    /// <inheritdoc/>
    public async Task<OperationResult> Delete(Guid id)
    {
        return await HandleDeletionOrArchival(
            id,
            () => sensitiveWorkshopService.Delete(id));
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

        var settlementsFilter = filter.ToFilterWithSettlements();

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
    public Task<SearchResult<WorkshopProviderViewCard>> GetByProviderId(Guid id, WorkshopFilterTitle filter)
        => workshopService.GetByProviderId(id, filter);

    /// <inheritdoc/>
    public async Task<Guid> GetWorkshopProviderId(Guid workshopId) =>
        await workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<PaginatedResult<WorkshopAttachmentStatusDto>> GetAttachedWorkshops(
           Guid studySubjectId,
           Guid providerId,
           int page,
           int pageSize)
    {
        return workshopService.GetAttachedWorkshops(studySubjectId, providerId, page, pageSize);
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

    /// <inheritdoc/>
    public async Task<PriceRange> GetPriceRangeAsync(WorkshopFilter filter)
    {
        if (!IsFilterValid(filter))
        {
            return new PriceRange();
        }

        return await workshopStrategy.GetPriceRangeAsync(filter);
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

    private async Task<OperationResult> HandleDeletionOrArchival(Guid id, Func<Task<OperationResult>> operationFunc)
    {
        var workshopDto = await workshopService.GetById(id).ConfigureAwait(false);

        var result = await operationFunc().ConfigureAwait(false);

        if (result == null)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = HttpStatusCode.BadRequest.ToString(),
                Description = "Returned result was null.",
            });
        }

        if (!result.Succeeded)
        {
            return OperationResult.Failed(new OperationError
            {
                Code = result.Errors.FirstOrDefault()?.Code,
                Description = result.Errors.FirstOrDefault()?.Description,
            });
        }

        var notificationsRecipientIds = await GetNotificationsRecipientIds(id).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
            ElasticsearchSyncEntity.Workshop,
            id,
            ElasticsearchSyncOperation.Delete).ConfigureAwait(false);

        await SendNotification(workshopDto, NotificationAction.Delete, false, notificationsRecipientIds).ConfigureAwait(false);

        return OperationResult.Success;
    }
}
