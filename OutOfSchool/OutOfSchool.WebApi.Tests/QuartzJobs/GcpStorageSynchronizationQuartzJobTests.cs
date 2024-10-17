using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services.Gcp;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class GcpStorageSynchronizationQuartzJobTests
{
    [Test]
    public async Task Execute_ShouldCallGcpStorageSynchronizationServiceSynchronizeAsync()
    {
        // Arrange
        var gcpStorageSynchronizationServiceMock = new Mock<IGcpStorageSynchronizationService>();
        var loggerMock = new Mock<ILogger<GcpStorageSynchronizationQuartzJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new GcpStorageSynchronizationQuartzJob(gcpStorageSynchronizationServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        gcpStorageSynchronizationServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
