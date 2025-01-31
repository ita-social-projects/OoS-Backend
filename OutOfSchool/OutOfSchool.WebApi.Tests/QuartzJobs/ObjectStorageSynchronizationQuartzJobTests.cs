using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.ExternalFileStore;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class ObjectStorageSynchronizationQuartzJobTests
{
    [Test]
    public async Task Execute_ShouldCallGcpStorageSynchronizationServiceSynchronizeAsync()
    {
        // Arrange
        var gcpStorageSynchronizationServiceMock = new Mock<IObjectStorageSynchronizationService>();
        var loggerMock = new Mock<ILogger<ObjectStorageSynchronizationQuartzJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new ObjectStorageSynchronizationQuartzJob(gcpStorageSynchronizationServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        gcpStorageSynchronizationServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
