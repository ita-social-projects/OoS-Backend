﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public class WorkshopServicesCombiner : IWorkshopServicesCombiner
    {
        private readonly IWorkshopService workshopService;
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
        private readonly ILogger<WorkshopServicesCombiner> logger;
        private readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService;

        public WorkshopServicesCombiner(
            IWorkshopService workshopService,
            IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService,
            ILogger<WorkshopServicesCombiner> logger,
            IElasticsearchSynchronizationService elasticsearchSynchronizationService)
        {
            this.workshopService = workshopService;
            this.elasticsearchService = elasticsearchService;
            this.logger = logger;
            this.elasticsearchSynchronizationService = elasticsearchSynchronizationService;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            var workshop = await workshopService.Create(dto).ConfigureAwait(false);

            var esResultIsValid = await elasticsearchService.Index(workshop.ToESModel()).ConfigureAwait(false);

            if (!esResultIsValid)
            {
                logger.LogWarning($"Error happend while trying to index {nameof(workshop)}:{workshop.Id} in Elasticsearch.");

                await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                    ElasticsearchSyncEntity.Workshop,
                    workshop.Id,
                    ElasticsearchSyncOperation.Create)
                    .ConfigureAwait(false);
            }

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

            var esResultIsValid = await elasticsearchService.Update(workshop.ToESModel()).ConfigureAwait(false);

            if (!esResultIsValid)
            {
                logger.LogWarning($"Error happend while trying to update {nameof(workshop)}:{workshop.Id} in Elasticsearch.");

                await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                    ElasticsearchSyncEntity.Workshop,
                    workshop.Id,
                    ElasticsearchSyncOperation.Update)
                    .ConfigureAwait(false);
            }

            return workshop;
        }

        /// <inheritdoc/>
        public async Task Delete(Guid id)
        {
            await workshopService.Delete(id).ConfigureAwait(false);

            var esResultIsValid = await elasticsearchService.Delete(id).ConfigureAwait(false);

            if (!esResultIsValid)
            {
                logger.LogWarning($"Error happend while trying to delete Workshop:{id} in Elasticsearch.");

                await elasticsearchSynchronizationService.AddNewRecordToElasticsearchSynchronizationTable(
                    ElasticsearchSyncEntity.Workshop,
                    id,
                    ElasticsearchSyncOperation.Delete)
                    .ConfigureAwait(false);
            }
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

            if (elasticsearchService.IsElasticAlive)
            {
                var result = await elasticsearchService.Search(filter.ToESModel()).ConfigureAwait(false);
                if (result.TotalAmount <= 0)
                {
                    logger.LogInformation($"Result was {result.TotalAmount}");
                }

                return result.ToSearchResult();
            }
            else
            {
                var databaseResult = await workshopService.GetByFilter(filter).ConfigureAwait(false);

                return new SearchResult<WorkshopCard>() { TotalAmount = databaseResult.TotalAmount, Entities = databaseResult.Entities };
            }
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter)
        {
            if (!IsFilterValid(filter))
            {
                return new SearchResult<WorkshopCard> { TotalAmount = 0, Entities = new List<WorkshopCard>() };
            }

            if (elasticsearchService.IsElasticAlive)
            {
                var result = await elasticsearchService.Search(filter.ToESModel()).ConfigureAwait(false);
                if (result.TotalAmount <= 0)
                {
                    logger.LogInformation($"Result was {result.TotalAmount}");
                }

                return result.ToSearchResult();
            }
            else
            {
                var databaseResult = await workshopService.GetByFilter(filter).ConfigureAwait(false);

                return new SearchResult<WorkshopCard>() { TotalAmount = databaseResult.TotalAmount, Entities = databaseResult.Entities };
            }
        }

        /// <inheritdoc/>
        public async Task<List<WorkshopCard>> GetByProviderId(Guid id)
        {
            var workshopCards = await workshopService.GetByProviderId(id).ConfigureAwait(false);

            return workshopCards.ToList();
        }

        private List<WorkshopCard> DtoModelsToWorkshopCards(IEnumerable<WorkshopDTO> source)
        {
            return source.Select(currentElement => currentElement.ToCardDto()).ToList();
        }

        private bool IsFilterValid(WorkshopFilter filter)
        {
            return filter != null && filter.MaxStartTime >= filter.MinStartTime
                                  && filter.MaxAge >= filter.MinAge
                                  && filter.MaxPrice >= filter.MinPrice;
        }

        private async void AddRecordToBackupTracker(long id, BackupOperation operation)
        private async void AddNewRecordToElasticsearchSynchronizationTable(long id, ElasticsearchSyncOperation operation)
        private async Task<ElasticsearchSyncRecordDto> AddNewRecordToElasticsearchSynchronizationTable(long id, ElasticsearchSyncOperation operation)
        private async Task AddNewRecordToElasticsearchSynchronizationTable(long id, ElasticsearchSyncOperation operation)
        {
            ElasticsearchSyncRecordDto elasticsearchSyncRecordDto = new ElasticsearchSyncRecordDto()
            {
                Operation = operation,
                OperationDate = DateTime.UtcNow,
                RecordId = id,
            };
            await elasticsearchSynchronizationService.Create(elasticsearchSyncRecordDto).ConfigureAwait(false);
        }
    }
}
