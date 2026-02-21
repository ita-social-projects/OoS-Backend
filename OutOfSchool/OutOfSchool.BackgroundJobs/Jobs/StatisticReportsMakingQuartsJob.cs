using Microsoft.Extensions.Logging;
using OutOfSchool.BusinessLogic.Services.StatisticReports;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class StatisticReportsMakingQuartsJob : IJob
{
    private readonly IStatisticReportsMakingService statisticReportsMakingService;
    private readonly ILogger<StatisticReportsMakingQuartsJob> logger;

    public StatisticReportsMakingQuartsJob(
        IStatisticReportsMakingService statisticReportsMakingService,
        ILogger<StatisticReportsMakingQuartsJob> logger)
    {
        this.statisticReportsMakingService = statisticReportsMakingService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Creating statistic reports job was started");

        await statisticReportsMakingService.CreateStatisticReports().ConfigureAwait(false);

        logger.LogInformation("Creating statistic reports job was finished");
    }
}
