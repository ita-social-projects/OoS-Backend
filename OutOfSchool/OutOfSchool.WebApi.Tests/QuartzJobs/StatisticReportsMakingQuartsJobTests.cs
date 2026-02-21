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
    [Test]
    public async Task Execute_ShouldCallStatisticReportsMakingServiceCreateStatisticReports()
    {
        // Arrange
        var statisticReportsMakingServiceMock = new Mock<IStatisticReportsMakingService>();
        var loggerMock = new Mock<ILogger<StatisticReportsMakingQuartsJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new StatisticReportsMakingQuartsJob(statisticReportsMakingServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        statisticReportsMakingServiceMock.Verify(x => x.CreateStatisticReports(It.IsAny<CancellationToken>()), Times.Once);
    }
}
