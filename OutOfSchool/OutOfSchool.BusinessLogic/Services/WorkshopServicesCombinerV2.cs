using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class WorkshopServicesCombinerV2(
    IWorkshopService workshopService,
    IElasticsearchSynchronizationService<IWorkshopService, Workshop> elasticsearchSynchronizationService,
    ISensitiveWorkshopsService sensitiveWorkshopService,
    INotificationService notificationService,
    IEntityRepositorySoftDeleted<long, Favorite> favoriteRepository,
    IApplicationRepository applicationRepository,
    IWorkshopStrategy workshopStrategy,
    ICurrentUserService currentUserServicse,
    IMinistryAdminService ministryAdminService,
    IRegionAdminService regionAdminService,
    ICodeficatorService codeficatorService,
    IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider
) : WorkshopServicesCombiner(
        workshopService,
        elasticsearchSynchronizationService,
        sensitiveWorkshopService,
        notificationService,
        favoriteRepository,
        applicationRepository,
        workshopStrategy,
        currentUserServicse,
        ministryAdminService,
        regionAdminService,
        codeficatorService,
        esProvider
    ), IWorkshopServicesCombinerV2
{
    public async Task<WorkshopResultDto> Create(WorkshopV2CreateRequestDto dto)
    {
        var creationResult = await workshopService.CreateV2(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                creationResult.Workshop.Id,
                ElasticsearchSyncOperation.Create)
            .ConfigureAwait(false);

        return creationResult;
    }

    public async Task<Result<WorkshopResultDto>> Update(WorkshopV2Dto dto, bool fromDraft = false)
    {
        var currentWorkshop = await GetById(dto.Id, true).ConfigureAwait(false);
        if (currentWorkshop is null)
        {
            return Result<WorkshopResultDto>.Failed(new OperationError
            {
                Code = nameof(HttpStatusCode.BadRequest),
                Description = Constants.WorkshopNotFoundErrorMessage,
            });
        }

        if (currentWorkshop.Status == WorkshopStatus.Archived)
        {
            return Result<WorkshopResultDto>.Failed(new OperationError
            {
                Code = nameof(HttpStatusCode.BadRequest),
                Description = "Workshop is archived and cannot be updated.",
            });
        }

        if (!IsAvailableSeatsValidForWorkshop(dto.AvailableSeats, currentWorkshop))
        {
            return Result<WorkshopResultDto>.Failed(new OperationError
            {
                Code = nameof(HttpStatusCode.BadRequest),
                Description = Constants.InvalidAvailableSeatsForWorkshopErrorMessage,
            });
        }

        var updatedWorkshop = await workshopService.UpdateV2(dto, fromDraft).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                updatedWorkshop.Workshop.Id,
                ElasticsearchSyncOperation.Update)
            .ConfigureAwait(false);

        return Result<WorkshopResultDto>.Success(updatedWorkshop);
    }

    public new async Task Delete(Guid id)
    {
        await sensitiveWorkshopService.DeleteV2(id).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                id,
                ElasticsearchSyncOperation.Delete)
            .ConfigureAwait(false);
    }
}