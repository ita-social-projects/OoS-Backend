using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services
{
    public class WorkshopServicesCombinerV2 : WorkshopServicesCombiner, IWorkshopServicesCombinerV2
    {
        public WorkshopServicesCombinerV2(IWorkshopService workshopService, IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService, ILogger<WorkshopServicesCombiner> logger, IElasticsearchSynchronizationService elasticsearchSynchronizationService)
            : base(workshopService, elasticsearchService, logger, elasticsearchSynchronizationService)
        {
        }

        public async Task<WorkshopCreationResultDto> Create(WorkshopCreationDto dto)
        {
            var creationResult = await workshopService.CreateV2(dto).ConfigureAwait(false);

            await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                    ElasticsearchSyncEntity.Workshop,
                    creationResult.Workshop.Id,
                    ElasticsearchSyncOperation.Create)
                .ConfigureAwait(false);

            return creationResult;
        }

        public async Task<WorkshopUpdateResultDto> Update(WorkshopUpdateDto dto)
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
}
