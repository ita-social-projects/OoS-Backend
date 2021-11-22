using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <inheritdoc/>
    public class ESWorkshopService : IElasticsearchService<WorkshopES, WorkshopFilterES>
    {
        private readonly IWorkshopService workshopService;
        private readonly IRatingService ratingService;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
        private readonly ElasticPinger esPinger;

        /// <inheritdoc/>
        public bool IsElasticAlive => esPinger.IsHealthy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ESWorkshopService"/> class.
        /// </summary>
        /// <param name="workshopService">Service that provides access to Workshops in the database.</param>
        /// <param name="ratingService">Service that provides access to Ratings in the database.</param>
        /// <param name="esProvider">Provider to the Elasticsearch workshops index.</param>
        /// <param name="elasticPinger">Background worker pings the Elasticsearch.</param>
        public ESWorkshopService(IWorkshopService workshopService, IRatingService ratingService, IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider, ElasticPinger elasticPinger)
        {
            this.workshopService = workshopService;
            this.ratingService = ratingService;
            this.esProvider = esProvider;
            this.esPinger = elasticPinger;
        }

        /// <inheritdoc/>
        public async Task<bool> Index(WorkshopES entity)
        {
            NullCheck(entity);

            try
            {
                var resp = await esProvider.IndexEntityAsync(entity).ConfigureAwait(false);

                if (resp == Result.Error)
                {
                    return false;
                }
            }
            catch (ElasticsearchClientException)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> Update(WorkshopES entity)
        {
            NullCheck(entity);

            try
            {
                entity.Rating = ratingService.GetAverageRating(entity.Id, RatingType.Workshop).Item1;

                var resp = await esProvider.UpdateEntityAsync(entity).ConfigureAwait(false);

                if (resp == Result.Error)
                {
                    return false;
                }
            }
            catch (ElasticsearchClientException)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> Delete(Guid id)
        {
            try
            {
                var resp = await esProvider.DeleteEntityAsync(new WorkshopES() { Id = id }).ConfigureAwait(false);

                if (resp == Result.Error)
                {
                    return false;
                }
            }
            catch (ElasticsearchClientException)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ReIndex()
        {
            try
            {
                var filter = new OffsetFilter() { From = 0, Size = 500 };
                var source = new List<WorkshopES>();

                var data = await workshopService.GetAll(filter).ConfigureAwait(false);
                while (data.Entities.Count > 0)
                {
                    foreach (var entity in data.Entities)
                    {
                        entity.Rating = ratingService.GetAverageRating(entity.Id, RatingType.Workshop).Item1;
                        source.Add(entity.ToESModel());
                    }

                    filter.From += filter.Size;
                    data = await workshopService.GetAll(filter).ConfigureAwait(false);
                }

                var resp = await esProvider.ReIndexAll(source).ConfigureAwait(false);

                if (resp == Result.Error)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<SearchResultES<WorkshopES>> Search(WorkshopFilterES filter)
        {
            try
            {
                var res = await esProvider.Search(filter).ConfigureAwait(false);

                return res;
            }
            catch
            {
                return new SearchResultES<WorkshopES>();
            }
        }

        private void NullCheck(WorkshopES entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException($"{entity} is not set to an instance.");
            }
        }
    }
}
