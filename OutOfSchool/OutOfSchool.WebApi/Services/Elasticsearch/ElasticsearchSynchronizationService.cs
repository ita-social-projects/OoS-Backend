using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nest;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    public class ElasticsearchSynchronizationService : IElasticsearchSynchronizationService
    {
        private readonly IWorkshopService databaseService;
        private readonly IEntityRepository<ElasticsearchSyncRecord> repository;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
        private readonly ILogger logger;

        public ElasticsearchSynchronizationService(
            IWorkshopService workshopService,
            IEntityRepository<ElasticsearchSyncRecord> repository,
            IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
            ILogger logger)
        {
            this.databaseService = workshopService;
            this.repository = repository;
            this.esProvider = esProvider;
            this.logger = logger;
        }

        public async Task<bool> Synchronize()
        {
            {
                var sourceDtoForCreate = await databaseService.GetWorkshopsForCreate().ConfigureAwait(false);
                var sourceDtoForUpdate = await databaseService.GetWorkshopsForUpdate().ConfigureAwait(false);

                var sourceDto = new List<WorkshopDTO>();
                sourceDto.AddRange(sourceDtoForCreate);
                sourceDto.AddRange(sourceDtoForUpdate);

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

        public async Task Create(ElasticsearchSyncRecordDto dto)
        {
            var elasticsearchSyncRecord = dto.ToDomain();
            try
            {
                await repository.Create(elasticsearchSyncRecord).ConfigureAwait(false);

                logger.Information("ElasticsearchSyncRecord created successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Creating new record to ElasticserchSyncRecord failed.");
                throw;
            }
        }
    }
}
