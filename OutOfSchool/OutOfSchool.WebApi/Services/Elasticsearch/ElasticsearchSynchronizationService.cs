using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        // TODO: move to config
        private const int NumberOfOperation = 2;

        private readonly IWorkshopService databaseService;
        private readonly IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
        private readonly ILogger<ElasticsearchSynchronizationService> logger;

        public ElasticsearchSynchronizationService(
            IWorkshopService workshopService,
            IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
            IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
            ILogger<ElasticsearchSynchronizationService> logger)
        {
            this.databaseService = workshopService;
            this.elasticsearchSyncRecordRepository = elasticsearchSyncRecordRepository;
            this.esProvider = esProvider;
            this.logger = logger;
        }

        public async Task<bool> Synchronize()
        {
            logger.LogInformation($"Synchronization of elasticsearch has started.");

            var elasticsearchSyncRecords = await elasticsearchSyncRecordRepository.GetByEntity(
                ElasticsearchSyncEntity.Workshop,
                NumberOfOperation).ConfigureAwait(false);

            var resultCreate = await Synchronize(elasticsearchSyncRecords, ElasticsearchSyncOperation.Create).ConfigureAwait(false);
            if (resultCreate)
            {
                await elasticsearchSyncRecordRepository.DeleteRange(
                    elasticsearchSyncRecords.Where(es => es.Operation == ElasticsearchSyncOperation.Create)
                    .Select(es => es.Id)).ConfigureAwait(false);
            }

            var resultUpdate = await Synchronize(elasticsearchSyncRecords, ElasticsearchSyncOperation.Update).ConfigureAwait(false);
            if (resultUpdate)
            {
                await elasticsearchSyncRecordRepository.DeleteRange(
                    elasticsearchSyncRecords.Where(es => es.Operation == ElasticsearchSyncOperation.Update)
                    .Select(es => es.Id)).ConfigureAwait(false);
            }

            var resultDelete = await Synchronize(elasticsearchSyncRecords, ElasticsearchSyncOperation.Delete).ConfigureAwait(false);
            if (resultDelete)
            {
                await elasticsearchSyncRecordRepository.DeleteRange(
                    elasticsearchSyncRecords.Where(es => es.Operation == ElasticsearchSyncOperation.Delete)
                    .Select(es => es.Id)).ConfigureAwait(false);
            }

            logger.LogInformation($"Synchronization of elasticsearch has finished successfully.");

            return true;
        }

        public async Task Create(ElasticsearchSyncRecordDto dto)
        {
            var elasticsearchSyncRecord = dto.ToDomain();
            try
            {
                await elasticsearchSyncRecordRepository.Create(elasticsearchSyncRecord).ConfigureAwait(false);

                logger.LogInformation("ElasticsearchSyncRecord created successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Creating new record to ElasticserchSyncRecord failed.");
                throw;
            }
        }

        public async Task AddNewRecordToElasticsearchSynchronizationTable(ElasticsearchSyncEntity entity, Guid id, ElasticsearchSyncOperation operation)
        {
            ElasticsearchSyncRecordDto elasticsearchSyncRecordDto = new ElasticsearchSyncRecordDto()
            {
                Entity = entity,
                RecordId = id,
                OperationDate = DateTimeOffset.UtcNow,
                Operation = operation,
            };

            await Create(elasticsearchSyncRecordDto).ConfigureAwait(false);
        }

        public async Task Synchronize(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Elasticsearch synchronization started");

                await Synchronize().ConfigureAwait(false);

                logger.LogInformation("Elasticsearch synchronization finished");

                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<bool> Synchronize(IEnumerable<ElasticsearchSyncRecord> elasticsearchSyncRecords, ElasticsearchSyncOperation elasticsearchSyncOperation)
        {
            var ids = elasticsearchSyncRecords.Where(es => es.Operation == elasticsearchSyncOperation).Select(es => es.RecordId);

            if (!ids.Any())
            {
                return true;
            }

            if (elasticsearchSyncOperation == ElasticsearchSyncOperation.Delete)
            {
                var result = await esProvider.DeleteRangeOfEntitiesByIdsAsync(ids).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    logger.LogError($"Error happend while trying to delete indexes in Elasticsearch.");
                    return false;
                }
            }
            else
            {
                var workshops = await databaseService.GetByIds(ids).ConfigureAwait(false);

                var sourse = workshops.Select(entity => entity.ToESModel());

                var result = await esProvider.IndexAll(sourse).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    logger.LogError($"Error happend while trying to update indexes in Elasticsearch.");
                    return false;
                }
            }

            return true;
        }
    }
}
