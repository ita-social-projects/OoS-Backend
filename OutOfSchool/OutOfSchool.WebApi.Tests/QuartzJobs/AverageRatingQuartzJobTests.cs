using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class AverageRatingQuartzJobTests
{
    [Test]
    public async Task Execute_ShouldCallAverageRatingServiceCalculateAsync()
    {
        // Arrange
        var averageRatingServiceMock = new Mock<IAverageRatingService>();
        var loggerMock = new Mock<ILogger<AverageRatingQuartzJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new AverageRatingQuartzJob(averageRatingServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        averageRatingServiceMock.Verify(x => x.CalculateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
