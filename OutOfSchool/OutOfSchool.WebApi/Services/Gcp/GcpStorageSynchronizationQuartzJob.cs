using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace OutOfSchool.WebApi.Services.Gcp
{
    public class GcpStorageSynchronizationQuartzJob : IJob
    {
        private readonly IGcpStorageSynchronizationService gcpStorageSynchronizationService;
        private readonly ILogger<GcpStorageSynchronizationQuartzJob> logger;

        public GcpStorageSynchronizationQuartzJob(
            IGcpStorageSynchronizationService gcpStorageSynchronizationService,
            ILogger<GcpStorageSynchronizationQuartzJob> logger)
        {
            this.gcpStorageSynchronizationService = gcpStorageSynchronizationService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogDebug("Gcp storage synchronization was started");

            await gcpStorageSynchronizationService.SynchronizeAsync().ConfigureAwait(false);

            logger.LogDebug("Gcp storage synchronization was finished");
        }
    }
}