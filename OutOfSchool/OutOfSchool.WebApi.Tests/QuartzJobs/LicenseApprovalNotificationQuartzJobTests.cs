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
    private readonly Mock<ILicenseApprovalNotificationService> licenseApprovalNotificationServiceMock = new();
    private readonly Mock<ILogger<LicenseApprovalNotificationQuartzJob>> loggerMock = new();
    private readonly Mock<IJobExecutionContext> jobExecutionContextMock = new();

    [Test]
    public async Task Execute_ShouldCallLicenseApprovalNotificationServiceGenerate()
    {
        // Arrange
        var job = new LicenseApprovalNotificationQuartzJob(licenseApprovalNotificationServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        licenseApprovalNotificationServiceMock.Verify(x => x.Generate(It.IsAny<CancellationToken>()), Times.Once);
    }
}
