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
    private readonly Mock<IAverageRatingService> averageRatingServiceMock = new();
    private readonly Mock<ILogger<AverageRatingQuartzJob>> loggerMock = new();
    private readonly Mock<IJobExecutionContext> jobExecutionContextMock = new();

    [Test]
    public async Task Execute_ShouldCallAverageRatingServiceCalculateAsync()
    {
        // Arrange
        var job = new AverageRatingQuartzJob(averageRatingServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        averageRatingServiceMock.Verify(x => x.CalculateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
