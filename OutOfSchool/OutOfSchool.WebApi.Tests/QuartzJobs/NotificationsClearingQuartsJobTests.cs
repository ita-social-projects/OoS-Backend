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
    [Test]
    public async Task Execute_ShouldCallNotificationsClearingServiceClearNotifications()
    {
        // Arrange
        var notificationsClearingServiceMock = new Mock<INotificationsClearingService>();
        var loggerMock = new Mock<ILogger<NotificationsClearingQuartsJob>>();
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var job = new NotificationsClearingQuartsJob(notificationsClearingServiceMock.Object, loggerMock.Object);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        notificationsClearingServiceMock.Verify(x => x.ClearNotifications(It.IsAny<CancellationToken>()), Times.Once);
    }
}
