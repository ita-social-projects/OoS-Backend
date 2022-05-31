using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OutOfSchool.WebApi.Services.Gcp;
using Quartz;

namespace OutOfSchool.WebApi.Services.Images
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