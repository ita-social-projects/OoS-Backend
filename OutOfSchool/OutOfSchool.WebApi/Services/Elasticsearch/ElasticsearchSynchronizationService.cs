using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the operations for synchronization databases.
    /// </summary>
    public class ElasticsearchSynchronizationService : IElasticsearchSynchronizationService
    {
        private readonly IWorkshopService databaseService;
        private readonly IEntityRepository<ElasticsearchSyncRecord> repository;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
        private readonly ILogger<ElasticsearchSynchronizationService> logger;

        public ElasticsearchSynchronizationService(
            IWorkshopService workshopService,
            IEntityRepository<ElasticsearchSyncRecord> repository,
            IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
            ILogger<ElasticsearchSynchronizationService> logger)
        {
            this.databaseService = workshopService;
            this.repository = repository;
            this.esProvider = esProvider;
            this.logger = logger;
        }

        public async Task<bool> Synchronize()
        {
            {
                Result result;

                logger.LogInformation($"Synchronization of elasticsearch has started.");

                var sourceDtoForCreate = await databaseService.GetWorkshopsForCreate().ConfigureAwait(false);
                var sourceDtoForUpdate = await databaseService.GetWorkshopsForUpdate().ConfigureAwait(false);

                var sourceDto = new List<WorkshopDTO>();
                sourceDto.AddRange(sourceDtoForCreate);
                sourceDto.AddRange(sourceDtoForUpdate);

                var source = sourceDto.Select(entity => entity.ToESModel());

                result = await esProvider.IndexAll(source).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    logger.LogError($"Error happend while trying to update indexes in Elasticsearch.");
                    return false;
                }

                var workshopIds = await databaseService.GetWorkshopsForDelete().ConfigureAwait(false);

                result = await esProvider.DeleteRangeOfEntitiesByIdsAsync(workshopIds).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    logger.LogError($"Error happend while trying to delete indexes in Elasticsearch.");
                    return false;
                }

                try
                {
                    await repository.DeleteAll().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.LogError($"Delete all records in ElasticsearchSyncRecords is failed.");
                    return false;
                }

                logger.LogInformation($"Synchronization of elasticsearch has finished successfully.");

                return true;
            }
        }

        public async Task Create(ElasticsearchSyncRecordDto dto)
        {
            var elasticsearchSyncRecord = dto.ToDomain();
            try
            {
                await repository.Create(elasticsearchSyncRecord).ConfigureAwait(false);

                logger.LogInformation("ElasticsearchSyncRecord created successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Creating new record to ElasticserchSyncRecord failed.");
                throw;
            }
        }

        public async Task AddNewRecordToElasticsearchSynchronizationTable(ElasticsearchSyncEntity entity, long id, ElasticsearchSyncOperation operation)
        {
            ElasticsearchSyncRecordDto elasticsearchSyncRecordDto = new ElasticsearchSyncRecordDto()
            {
                Entity = entity,
                RecordId = id,
                OperationDate = DateTime.UtcNow,
                Operation = operation,
            };
            await Create(elasticsearchSyncRecordDto).ConfigureAwait(false);
        }
    }
}
