using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public class WorkshopServicesProvider : IWorkshopServicesProvider
    {
        private readonly IWorkshopService workshopService;
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;

        public WorkshopServicesProvider(IWorkshopService workshopService, IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService)
        {
            this.workshopService = workshopService;
            this.elasticsearchService = elasticsearchService;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            var workshop = await workshopService.Create(dto).ConfigureAwait(false);

            await elasticsearchService.Index(workshop.ToESModel()).ConfigureAwait(false);

            return workshop;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            var workshop = await workshopService.GetById(id).ConfigureAwait(false);

            return workshop;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            var workshop = await workshopService.Update(dto).ConfigureAwait(false);

            await elasticsearchService.Update(workshop.ToESModel()).ConfigureAwait(false);

            return workshop;
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            await workshopService.Delete(id).ConfigureAwait(false);

            await elasticsearchService.Delete(id).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopES>> GetAll()
        {
            var workshops = await elasticsearchService.Search(null).ConfigureAwait(false);

            return workshops;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopES>> GetByFilter(WorkshopFilterES filter)
        {
            var workshops = await elasticsearchService.Search(filter).ConfigureAwait(false);

            return workshops;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id)
        {
            var workshop = await workshopService.GetWorkshopsByProviderId(id).ConfigureAwait(false);

            return workshop;
        }

        /// <inheritdoc/>
        public Task<List<WorkshopDTO>> GetPage(WorkshopFilter filter, int size, int pageNumber)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<int> GetPagesCount(WorkshopFilter filter, int size)
        {
            throw new NotImplementedException();
        }
    }
}
