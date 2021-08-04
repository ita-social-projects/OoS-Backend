using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IBackupTrackerService backupTrackerService;
        private readonly IEntityRepository<BackupTracker> repository;
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;

        public ElasticsearchSynchronizationService(IWorkshopService workshopService, IBackupTrackerService backupTrackerService, IEntityRepository<BackupTracker> repository, IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService)
        {
            this.databaseService = workshopService;
            this.backupTrackerService = backupTrackerService;
            this.repository = repository;
            this.elasticsearchService = elasticsearchService;
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
                    case BackupOperation.Create:
                        workshop = await databaseService.GetById(backupTrackerRecord.RecordId).ConfigureAwait(false);
                        esResultIsValid = await elasticsearchService.Index(workshop.ToESModel()).ConfigureAwait(false);
                        break;
                    case BackupOperation.Update:
                        workshop = await databaseService.GetById(backupTrackerRecord.RecordId).ConfigureAwait(false);
                        esResultIsValid = await elasticsearchService.Update(workshop.ToESModel()).ConfigureAwait(false);
                        break;
                    case BackupOperation.Delete:
                        esResultIsValid = await elasticsearchService.Delete(backupTrackerRecord.RecordId).ConfigureAwait(false);
                        break;
                }
            }

            return true;
        }

        public async Task<IEnumerable<BackupTrackerDto>> GetAll()
        {
            var backupTracker = await repository.GetAll().ConfigureAwait(false);

            return backupTracker.Select(backupTracker => backupTracker.ToModel());
        }
    }
}
