using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class ElasticsearchSynchronizationService : IElasticsearchSynchronizationService
    {
        private readonly IWorkshopService databaseService;
        private readonly IEntityRepository<ElasticsearchSyncRecord> repository;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;

        public ElasticsearchSynchronizationService(IWorkshopService workshopService, IEntityRepository<ElasticsearchSyncRecord> repository, IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider)
        {
            this.databaseService = workshopService;
            this.repository = repository;
            this.esProvider = esProvider;
        }

        public async Task<bool> Synchronize()
        {
            {
                var sourceDto = await databaseService.GetWorkshopsForUpdate().ConfigureAwait(false);

                List<WorkshopES> source = new List<WorkshopES>();
                foreach (var entity in sourceDto)
                {
                    source.Add(entity.ToESModel());
                }

                var result = await esProvider.IndexAll(source).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    return false;
                }

                return true;
            }
        }

        public async Task<ElasticsearchSyncRecordDto> Create(ElasticsearchSyncRecordDto dto)
        {
            var elasticsearchSyncRecord = dto.ToDomain();
            var newElasticsearchSyncRecord = await repository.Create(elasticsearchSyncRecord).ConfigureAwait(false);
            return newElasticsearchSyncRecord.ToModel();
        }
    }
}
