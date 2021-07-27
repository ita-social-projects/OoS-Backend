using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    public class BackupOperationService : IBackupOperationService
    {
        private readonly IEntityRepository<BackupOperation> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localazer;

        public BackupOperationService(IEntityRepository<BackupOperation> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.repository = repository;
            this.logger = logger;
            this.localazer = localizer;
        }

        public async Task<BackupOperationDto> Create(BackupOperationDto dto)
        {
            var backupOperation = dto.ToDomain();
            var newBackupOperation = await repository.Create(backupOperation).ConfigureAwait(false);
            return newBackupOperation.ToModel();
        }
    }
}
