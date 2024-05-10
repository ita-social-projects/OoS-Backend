using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Services.NotificationsClearing;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs;

[TestFixture]
public class NotificationsClearingQuartsJobTests
{
    private readonly Mock<INotificationsClearingService> notificationsClearingServiceMock = new();
    private readonly Mock<ILogger<NotificationsClearingQuartsJob>> loggerMock = new();
    private readonly Mock<IJobExecutionContext> jobExecutionContextMock = new();

    [Test]
    public async Task Execute_ShouldCallNotificationsClearingServiceClearNotifications()
    {
        // Arrange
        var job = new NotificationsClearingQuartsJob(notificationsClearingServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        notificationsClearingServiceMock.Verify(x => x.ClearNotifications(It.IsAny<CancellationToken>()), Times.Once);
    }
}
