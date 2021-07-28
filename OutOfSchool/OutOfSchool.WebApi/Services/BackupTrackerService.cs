using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    public class BackupTrackerService : IBackupTrackerService
    {
        private readonly IEntityRepository<BackupTracker> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localazer;

        public BackupTrackerService(IEntityRepository<BackupTracker> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.repository = repository;
            this.logger = logger;
            this.localazer = localizer;
        }

        public async Task<BackupTrackerDto> Create(BackupTrackerDto dto)
        {
            var backupTracker = dto.ToDomain();
            var newBackupTracker = await repository.Create(backupTracker).ConfigureAwait(false);
            return newBackupTracker.ToModel();
        }
    }
}
