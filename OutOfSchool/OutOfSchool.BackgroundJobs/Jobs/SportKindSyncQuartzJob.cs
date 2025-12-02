using Microsoft.Extensions.Logging;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

[DisallowConcurrentExecution] // ! important
public class SportKindSyncQuartzJob : IJob
{
    private readonly ISportKindSyncService sportKindSyncService;
    private readonly ILogger<SportKindSyncQuartzJob> logger;

    public SportKindSyncQuartzJob(
        ISportKindSyncService sportKindSyncService,
        ILogger<SportKindSyncQuartzJob> logger)
    {
        this.sportKindSyncService = sportKindSyncService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("SportKind sync Quartz job started");

        try
        {
            var changedCount = await sportKindSyncService.SyncSportKindsAsync().ConfigureAwait(false);
            logger.LogInformation("SportKind sync finished. {Count} entities changed.", changedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SportKind sync failed");
            throw;
        }
    }
}