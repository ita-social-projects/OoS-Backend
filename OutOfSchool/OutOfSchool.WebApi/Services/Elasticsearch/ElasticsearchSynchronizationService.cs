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
        private readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService;
        private readonly IEntityRepository<ElasticsearchSyncRecord> repository;
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;

        public ElasticsearchSynchronizationService(IWorkshopService workshopService, IElasticsearchSynchronizationService elasticsearchSynchronizationService, IEntityRepository<ElasticsearchSyncRecord> repository, IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService, IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider)
        {
            this.databaseService = workshopService;
            this.elasticsearchSynchronizationService = elasticsearchSynchronizationService;
            this.repository = repository;
            this.elasticsearchService = elasticsearchService;
            this.esProvider = esProvider;
        }

        public async Task<bool> Synchronize()
        {
            var backupTrackerRecords = (await GetAll().ConfigureAwait(false)).ToList();

            var resultMaxDate = from r in backupTrackerRecords
                                group r by r.RecordId into gr
                                select new { RecordId = gr.Key, MaxOperationDate = gr.Max(r => r.OperationDate) };

            var result = from rMaxDate in resultMaxDate
                         join r in backupTrackerRecords on new { rMaxDate.RecordId, OperationDate = rMaxDate.MaxOperationDate } equals new { r.RecordId, r.OperationDate } into leftJoin
                         from rg in leftJoin
                         select new { rg.RecordId, rg.OperationDate, rg.Operation };

            WorkshopDTO workshop;
            bool esResultIsValid;
            foreach (var backupTrackerRecord in result)
            {
                workshop = null;
                esResultIsValid = false;
                switch (backupTrackerRecord.Operation)
                {
                    case ElasticsearchSyncOperation.Create:
                        workshop = await databaseService.GetById(backupTrackerRecord.RecordId).ConfigureAwait(false);
                        esResultIsValid = await elasticsearchService.Index(workshop.ToESModel()).ConfigureAwait(false);
                        break;
                    case ElasticsearchSyncOperation.Update:
                        workshop = await databaseService.GetById(backupTrackerRecord.RecordId).ConfigureAwait(false);
                        esResultIsValid = await elasticsearchService.Update(workshop.ToESModel()).ConfigureAwait(false);
                        break;
                    case ElasticsearchSyncOperation.Delete:
                        esResultIsValid = await elasticsearchService.Delete(backupTrackerRecord.RecordId).ConfigureAwait(false);
                        break;
                }
            }

            return true;
        }

        public async Task<IEnumerable<ElasticsearchSyncRecordDto>> GetAll()
        {
            var backupTracker = await repository.GetAll().ConfigureAwait(false);

            return backupTracker.Select(backupTracker => backupTracker.ToModel());
        }

        public async Task<bool> Synchronize2()
        {
            var sourceDto = await databaseService.GetWorkshopsForUpdate().ConfigureAwait(false);

            List<WorkshopES> source = new List<WorkshopES>();
            foreach (var entity in sourceDto)
            {
                source.Add(entity.ToESModel());
            }

            var resp = await esProvider.ReIndexAll(source).ConfigureAwait(false);

            if (resp == Result.Error)
            {
                return false;
            }

            return true;
        }

        public async Task<ElasticsearchSyncRecordDto> Create(ElasticsearchSyncRecordDto dto)
        {
            var elasticsearchSyncRecord = dto.ToDomain();
            var newElasticsearchSyncRecord = await repository.Create(elasticsearchSyncRecord).ConfigureAwait(false);
            return newElasticsearchSyncRecord.ToModel();
        }
    }
}
