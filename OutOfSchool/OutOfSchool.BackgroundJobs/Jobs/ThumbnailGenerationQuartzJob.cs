using Microsoft.Extensions.Logging;
using OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;
public class ThumbnailGenerationQuartzJob : IJob
{
    private readonly IThumbnailProcessingService thumbnailService;
    private readonly ILogger<ThumbnailGenerationQuartzJob> logger;

    public ThumbnailGenerationQuartzJob(IThumbnailProcessingService thumbnailService, ILogger<ThumbnailGenerationQuartzJob> logger)
    {
        this.thumbnailService = thumbnailService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Started thumbnail generation job for all unprocessed images");

        await thumbnailService.ProcessAllUnprocessedThumbnailsAsync(context.CancellationToken);

        logger.LogInformation("Finished thumbnail generation job for all unprocessed images");
    }
}

