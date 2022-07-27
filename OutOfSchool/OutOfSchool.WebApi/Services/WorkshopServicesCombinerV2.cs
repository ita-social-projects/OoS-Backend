using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Strategies;

namespace OutOfSchool.WebApi.Services;

public class WorkshopServicesCombinerV2 : WorkshopServicesCombiner, IWorkshopServicesCombinerV2
{
    public WorkshopServicesCombinerV2(
        IWorkshopService workshopService,
        IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService,
        ILogger<WorkshopESStrategy> logger,
        IElasticsearchSynchronizationService elasticsearchSynchronizationService,
        INotificationService notificationService,
        IEntityRepository<long, Favorite> favoriteRepository,
        IApplicationRepository applicationRepository)
        : base(workshopService, elasticsearchService, logger, elasticsearchSynchronizationService, notificationService, favoriteRepository, applicationRepository)
    {
    }

    public new async Task<WorkshopCreationResultDto> Create(WorkshopDTO dto)
    {
        var creationResult = await workshopService.CreateV2(dto).ConfigureAwait(false);

        await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                creationResult.Workshop.Id,
                ElasticsearchSyncOperation.Create)
            .ConfigureAwait(false);

        return creationResult;
    }

    public new async Task<WorkshopUpdateResultDto> Update(WorkshopDTO dto)
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