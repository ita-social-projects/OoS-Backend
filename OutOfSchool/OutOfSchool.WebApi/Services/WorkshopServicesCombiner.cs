using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Images;

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

        public async Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider)
        {
            var workshops = await workshopService.PartialUpdateByProvider(provider).ConfigureAwait(false);

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

        // <inheritdoc/>
        public async Task<Guid> GetWorkshopProviderId(Guid workshopId) =>
            await workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);

        public async Task<OperationResult> UploadImageAsync(Guid entityId, IFormFile image) =>
            await workshopService.UploadImageAsync(entityId, image).ConfigureAwait(false);

        public async Task<OperationResult> RemoveImageAsync(Guid entityId, string imageId) =>
            await workshopService.RemoveImageAsync(entityId, imageId).ConfigureAwait(false);

        public async Task<MultipleKeyValueOperationResult> UploadManyImagesAsync(Guid entityId, IList<IFormFile> images) =>
            await workshopService.UploadManyImagesAsync(entityId, images).ConfigureAwait(false);

        public async Task<MultipleKeyValueOperationResult> RemoveManyImagesAsync(Guid entityId, IList<string> imageIds) =>
            await workshopService.RemoveManyImagesAsync(entityId, imageIds).ConfigureAwait(false);

        public async Task<ImageChangingResult> ChangeImagesAsync(Guid entityId, IList<string> oldImageIds, IList<IFormFile> newImages) =>
            await workshopService.ChangeImagesAsync(entityId, oldImageIds, newImages).ConfigureAwait(false);

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
    }
}
