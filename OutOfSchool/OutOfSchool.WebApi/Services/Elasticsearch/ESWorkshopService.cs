using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Services
{
    /// <inheritdoc/>
    public class ESWorkshopService : IElasticsearchService<WorkshopES, WorkshopFilterES>
    {
        private readonly IWorkshopService workshopService;
        private readonly IRatingService ratingService;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ESWorkshopService"/> class.
        /// </summary>
        /// <param name="workshopService">Service that provides access to Workshops in the database.</param>
        /// <param name="ratingService">Service that provides access to Ratings in the database.</param>
        /// <param name="esProvider">Povider to the Elasticsearch workshops index.</param>
        public ESWorkshopService(IWorkshopService workshopService, IRatingService ratingService, IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider)
        {
            this.workshopService = workshopService;
            this.ratingService = ratingService;
            this.esProvider = esProvider;
        }

        /// <inheritdoc/>
        public async Task Index(WorkshopES entity)
        {
            await esProvider.IndexEntityAsync(entity).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Update(WorkshopES entity)
        {
            entity.Rating = ratingService.GetAverageRating(entity.Id, RatingType.Workshop);

            await esProvider.UpdateEntityAsync(entity).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            await esProvider.DeleteEntityAsync(new WorkshopES() { Id = id }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ReIndex()
        {
            var sourceDto = await workshopService.GetAll().ConfigureAwait(false);

            List<WorkshopES> source = new List<WorkshopES>();
            foreach (var entity in sourceDto)
            {
                entity.Rating = ratingService.GetAverageRating(entity.Id, RatingType.Workshop);
                source.Add(entity.ToESModel());
            }

            await esProvider.ReIndexAll(source).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopES>> Search(WorkshopFilterES filter)
        {
            var res = await esProvider.Search(filter).ConfigureAwait(false);

            return res;
        }
    }
}
