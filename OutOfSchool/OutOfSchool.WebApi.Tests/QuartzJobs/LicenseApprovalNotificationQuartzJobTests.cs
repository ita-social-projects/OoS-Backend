using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services.LicenseApprovalNotification;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class LicenseApprovalNotificationQuartzJobTests
{
    [Test]
    public async Task Execute_ShouldCallLicenseApprovalNotificationServiceGenerate()
    {
        // Arrange
        var licenseApprovalNotificationServiceMock = new Mock<ILicenseApprovalNotificationService>();
        var loggerMock = new Mock<ILogger<LicenseApprovalNotificationQuartzJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new LicenseApprovalNotificationQuartzJob(licenseApprovalNotificationServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        licenseApprovalNotificationServiceMock.Verify(x => x.Generate(It.IsAny<CancellationToken>()), Times.Once);
    }
}
