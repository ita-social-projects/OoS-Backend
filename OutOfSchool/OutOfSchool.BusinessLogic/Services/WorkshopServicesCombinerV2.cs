using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class WorkshopServicesCombinerV2 : WorkshopServicesCombiner, IWorkshopServicesCombinerV2
{
    public WorkshopServicesCombinerV2(
        IWorkshopService workshopService,
        IElasticsearchSynchronizationService elasticsearchSynchronizationService,
        INotificationService notificationService,
        IEntityRepositorySoftDeleted<long, Favorite> favoriteRepository,
        IApplicationRepository applicationRepository,
        IWorkshopStrategy workshopStrategy,
        ICurrentUserService currentUserServicse,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        ICodeficatorService codeficatorService,
        IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
        IMapper mapper)
        : base(
            workshopService,
            elasticsearchSynchronizationService,
            notificationService,
            favoriteRepository,
            applicationRepository,
            workshopStrategy,
            currentUserServicse,
            ministryAdminService,
            regionAdminService,
            codeficatorService,
            esProvider,
            mapper)
    {
    }

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

    public async Task<Result<WorkshopResultDto>> Update(WorkshopV2Dto dto)
    {
        var currentWorkshop = await GetById(dto.Id, true).ConfigureAwait(false);
        if (currentWorkshop is null)
        {
            return Result<WorkshopResultDto>.Failed(new OperationError
            {
                Code = HttpStatusCode.BadRequest.ToString(),
                Description = Constants.WorkshopNotFoundErrorMessage,
            });
        }

        if (!IsAvailableSeatsValidForWorkshop(dto.AvailableSeats, currentWorkshop))
        {
            return Result<WorkshopResultDto>.Failed(new OperationError
            {
                Code = HttpStatusCode.BadRequest.ToString(),
                Description = Constants.InvalidAvailableSeatsForWorkshopErrorMessage,
            });
        }

        var updatedWorkshop = await workshopService.UpdateV2(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                updatedWorkshop.Workshop.Id,
                ElasticsearchSyncOperation.Update)
            .ConfigureAwait(false);

        return Result<WorkshopResultDto>.Success(updatedWorkshop);
    }

    public new async Task Delete(Guid id)
    {
        await workshopService.DeleteV2(id).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                id,
                ElasticsearchSyncOperation.Delete)
            .ConfigureAwait(false);
    }
}