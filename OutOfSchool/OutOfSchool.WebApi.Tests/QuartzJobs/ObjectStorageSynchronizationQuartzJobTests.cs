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
    public async Task Execute_ShouldCallStorageSynchronizationServiceSynchronizeAsync()
    {
        // Arrange
        var storageSynchronizationServiceMock = new Mock<IObjectStorageSynchronizationService>();
        var loggerMock = new Mock<ILogger<ObjectStorageSynchronizationQuartzJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new ObjectStorageSynchronizationQuartzJob(storageSynchronizationServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        storageSynchronizationServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
