using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class ObjectStorageSynchronizationQuartzJob : IJob
{
    private readonly IObjectStorageSynchronizationService objectStorageSynchronizationService;
    private readonly ILogger<ObjectStorageSynchronizationQuartzJob> logger;

    public ObjectStorageSynchronizationQuartzJob(
        IObjectStorageSynchronizationService objectStorageSynchronizationService,
        ILogger<ObjectStorageSynchronizationQuartzJob> logger)
    {
        this.objectStorageSynchronizationService = objectStorageSynchronizationService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Object storage synchronization job was started");

        await objectStorageSynchronizationService.SynchronizeAsync().ConfigureAwait(false);

        logger.LogInformation("Object storage synchronization job was finished");
    }
}