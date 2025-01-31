using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class GcpStorageSynchronizationQuartzJob : IJob
{
    private readonly IObjectStorageSynchronizationService objectStorageSynchronizationService;
    private readonly ILogger<GcpStorageSynchronizationQuartzJob> logger;

    public GcpStorageSynchronizationQuartzJob(
        IObjectStorageSynchronizationService objectStorageSynchronizationService,
        ILogger<GcpStorageSynchronizationQuartzJob> logger)
    {
        this.gcpStorageSynchronizationService = gcpStorageSynchronizationService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Gcp storage synchronization job was started");

        await gcpStorageSynchronizationService.SynchronizeAsync().ConfigureAwait(false);

        logger.LogInformation("Gcp storage synchronization job was finished");
    }
}