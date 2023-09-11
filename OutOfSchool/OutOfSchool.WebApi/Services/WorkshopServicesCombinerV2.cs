using AutoMapper;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;

namespace OutOfSchool.WebApi.Services;

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

    public new async Task<WorkshopResultDto> Create(WorkshopV2Dto dto)
    {
        var creationResult = await workshopService.CreateV2(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                creationResult.Workshop.Id,
                ElasticsearchSyncOperation.Create)
            .ConfigureAwait(false);

        return creationResult;
    }

    public new async Task<WorkshopResultDto> Update(WorkshopV2Dto dto)
    {
        var workshop = await workshopService.UpdateV2(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                workshop.Workshop.Id,
                ElasticsearchSyncOperation.Update)
            .ConfigureAwait(false);

        return workshop;
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