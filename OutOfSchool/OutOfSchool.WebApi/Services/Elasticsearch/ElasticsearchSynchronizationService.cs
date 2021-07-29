using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.ElasticsearchData.Models;

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
            var backupTrackerRecords = await GetAll().ConfigureAwait(false);
            foreach (var backupTrackerRecord in backupTrackerRecords)
            {
                var workshop = await databaseService.GetById(backupTrackerRecord.RecordId).ConfigureAwait(false);
                var esResultIsValid = await elasticsearchService.Index(workshop.ToESModel()).ConfigureAwait(false);
            };
            return true;
        }

        public async Task<IEnumerable<BackupTrackerDto>> GetAll()
        {
            var backupTracker = await repository.GetAll().ConfigureAwait(false);
            return backupTracker.Select(backupTracker => backupTracker.ToModel()).ToList();
        }
    }
}
