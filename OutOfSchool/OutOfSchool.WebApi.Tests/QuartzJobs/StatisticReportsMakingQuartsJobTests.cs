using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services.StatisticReports;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class StatisticReportsMakingQuartsJobTests
{
    private readonly Mock<IStatisticReportsMakingService> statisticReportsMakingServiceMock = new();
    private readonly Mock<ILogger<StatisticReportsMakingQuartsJob>> loggerMock = new();
    private readonly Mock<IJobExecutionContext> jobExecutionContextMock = new();

    [Test]
    public async Task Execute_ShouldCallStatisticReportsMakingServiceCreateStatisticReports()
    {
        // Arrange
        var job = new StatisticReportsMakingQuartsJob(statisticReportsMakingServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        statisticReportsMakingServiceMock.Verify(x => x.CreateStatisticReports(It.IsAny<CancellationToken>()), Times.Once);
    }
}
