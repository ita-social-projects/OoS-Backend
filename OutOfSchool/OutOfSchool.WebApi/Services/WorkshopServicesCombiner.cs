using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Images;
using Ubiety.Dns.Core.Records.NotUsed;

namespace OutOfSchool.WebApi.Services
{
    public class WorkshopServicesCombiner : IWorkshopServicesCombiner, INotificationReciever
    {
        private protected readonly IWorkshopService workshopService; // make it private after removing v2 version
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
        private readonly ILogger<WorkshopServicesCombiner> logger;
        private protected readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService; // make it private after removing v2 version
        private readonly INotificationService notificationService;
        private readonly IEntityRepository<Favorite> favoriteRepository;
        private readonly IApplicationRepository applicationRepository;


        public WorkshopServicesCombiner(
            IWorkshopService workshopService,
            IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService,
            ILogger<WorkshopServicesCombiner> logger,
            IElasticsearchSynchronizationService elasticsearchSynchronizationService,
            INotificationService notificationService,
            IEntityRepository<Favorite> favoriteRepository,
            IApplicationRepository applicationRepository)
        {
            this.workshopService = workshopService;
            this.elasticsearchService = elasticsearchService;
            this.logger = logger;
            this.elasticsearchSynchronizationService = elasticsearchSynchronizationService;
            this.notificationService = notificationService;
            this.favoriteRepository = favoriteRepository;
            this.applicationRepository = applicationRepository;
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

        /// <inheritdoc/>
        public async Task<WorkshopStatusDto> UpdateStatus(WorkshopStatusDto dto)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));

            var workshopDto = await workshopService.UpdateStatus(dto).ConfigureAwait(false);

            var additionalData = new Dictionary<string, string>()
            {
                { "Status", workshopDto.Status.ToString() },
            };

            await notificationService.Create(
                NotificationType.Workshop,
                NotificationAction.Update,
                workshopDto.WorkshopId,
                this,
                additionalData).ConfigureAwait(false);

            return dto;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<Guid> GetWorkshopProviderId(Guid workshopId) =>
            await workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);

        public async Task<IEnumerable<string>> GetNotificationsRecipientIds(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
        {
            var recipientIds = new List<string>();

            Expression<Func<Favorite, bool>> filter = x => x.WorkshopId == objectId;
            var favoriteWorkshopUsersIds = favoriteRepository.Get(where: filter).Select(x => x.UserId).ToList();

            Expression<Func<Application, bool>> predicate =
                x => x.Status != ApplicationStatus.Left &&
                     x.WorkshopId == objectId;
            var applications = await applicationRepository.GetByFilter(predicate).ConfigureAwait(false);
            var appliedUsersIds = applications.Select(x => x.Parent.UserId);

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
    }
}
