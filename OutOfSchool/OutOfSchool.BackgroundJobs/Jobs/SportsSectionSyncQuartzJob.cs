using Microsoft.Extensions.Logging;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

[DisallowConcurrentExecution]
public class SportsSectionSyncQuartzJob : IJob
{
    private readonly ISportsSectionSyncService sportsSectionSyncService;
    private readonly ILogger<SportsSectionSyncQuartzJob> logger;

    public SportsSectionSyncQuartzJob(
        ISportsSectionSyncService sportsSectionSyncService,
        ILogger<SportsSectionSyncQuartzJob> logger)
    {
        this.sportsSectionSyncService = sportsSectionSyncService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("SportsSection sync Quartz job started");

        try
        {
            var changedCount = await sportsSectionSyncService.SyncSportsSectionsAsync().ConfigureAwait(false);
            logger.LogInformation("SportsSection sync finished. {Count} entities changed.", changedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SportsSection sync failed");
            throw;
        }
    }
}
