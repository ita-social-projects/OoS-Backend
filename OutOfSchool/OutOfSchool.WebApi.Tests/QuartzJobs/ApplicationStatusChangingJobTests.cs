using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class ApplicationStatusChangingJobTests
{
    [Test]
    public async Task Execute_ShouldCallApplicationServiceChangeApprovedStatusesToStudying()
    {
        // Arrange
        var applicationStatusChangingServiceMock = new Mock<IApplicationService>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new ApplicationStatusChangingJob(applicationStatusChangingServiceMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        applicationStatusChangingServiceMock.Verify(x => x.ChangeApprovedStatusesToStudying(), Times.Once);
    }
}
