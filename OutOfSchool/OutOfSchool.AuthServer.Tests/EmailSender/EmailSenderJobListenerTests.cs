using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender.Quartz;
using OutOfSchool.EmailSender.Services;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class EmailSenderJobListenerTests
{
    private Mock<ISendGridAccessibilityService> _mockSendGridAccessibilityService;
    private Mock<ISchedulerFactory> _mockSchedulerFactory;
    private Mock<IScheduler> _mockScheduler;
    private Mock<ILogger<EmailSenderJobListener>> _mockLogger;
    private EmailSenderJobListener _emailSenderJobListener;

    [SetUp]
    public void Setup()
    {
        _mockSendGridAccessibilityService = new Mock<ISendGridAccessibilityService>();
        _mockSchedulerFactory = new Mock<ISchedulerFactory>();
        _mockScheduler = new Mock<IScheduler>();
        _mockLogger = new Mock<ILogger<EmailSenderJobListener>>();
        _emailSenderJobListener = new EmailSenderJobListener(
            _mockSendGridAccessibilityService.Object,
            _mockSchedulerFactory.Object,
            _mockLogger.Object);
    }

    [Test]
    public void NameProperty_ShouldReturnName()
    {
        Assert.AreEqual("Email Sender Job Listener", _emailSenderJobListener.Name);
    }

    [Test]
    public void JobExecutionVetoed_ShouldReturnCompletedTask()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>().Object;

        // Act
        var result = _emailSenderJobListener.JobExecutionVetoed(context);

        // Assert
        Assert.AreEqual(Task.CompletedTask, result);
    }

    [Test]
    public void JobToBeExecuted_ShouldReturnCompletedTask()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>().Object;

        // Act
        var result = _emailSenderJobListener.JobToBeExecuted(context);

        // Assert
        Assert.AreEqual(Task.CompletedTask, result);
    }

    [Test]
    public async Task JobWasExecuted_WithJobException_ShouldCallSetSendGridInaccessibleAndRescheduleJob()
    {
        // Arrange
        var mockContext = new Mock<IJobExecutionContext>();
        mockContext.Setup(x => x.Trigger).Returns(TriggerBuilder.Create().Build());
        var jobException = new JobExecutionException();
        _mockSchedulerFactory.Setup(f => f.GetScheduler(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockScheduler.Object);
        _mockSendGridAccessibilityService.Setup(x => x.GetAccessibilityTime(It.IsAny<DateTimeOffset>())).Returns(DateTimeOffset.Now);
        _mockScheduler.Setup(x => x.RescheduleJob(It.IsAny<TriggerKey>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>())).ReturnsAsync(DateTimeOffset.Now);

        // Act
        await _emailSenderJobListener.JobWasExecuted(mockContext.Object, jobException);

        // Assert
        _mockSendGridAccessibilityService.Verify(
            s => s.SetSendGridInaccessible(It.IsAny<DateTimeOffset>()),
            Times.Once
        );

        _mockScheduler.Verify(
            s => s.RescheduleJob(It.IsAny<TriggerKey>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task JobWasExecuted_WithoutJobException_ShouldNotCallSetSendGridInaccessible()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>().Object;

        // Act
        await _emailSenderJobListener.JobWasExecuted(context, null);

        // Assert
        _mockSendGridAccessibilityService.Verify(
            s => s.SetSendGridInaccessible(It.IsAny<DateTimeOffset>()),
            Times.Never
        );
    }
}
