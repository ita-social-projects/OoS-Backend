using Quartz;

namespace OutOfSchool.WebApi.Services.AverageRatings;

public class AverageRatingQuartzJob : IJob
{
    private readonly IAverageRatingService averageRatingService;
    private readonly ILogger<AverageRatingQuartzJob> logger;

    public AverageRatingQuartzJob(IAverageRatingService averageRatingService, ILogger<AverageRatingQuartzJob> logger)
    {
        this.averageRatingService = averageRatingService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("The average rating calculation Quartz job started.");

        await averageRatingService.CalculateAsync().ConfigureAwait(false);

        logger.LogInformation("The average rating calculation Quartz job finished.");
    }
}
