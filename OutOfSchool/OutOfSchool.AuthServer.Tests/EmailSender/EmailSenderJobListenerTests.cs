using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender.Quartz;
using OutOfSchool.EmailSender.Services;
using Quartz;
using System;
using System.Threading.Tasks;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class EmailSenderJobListenerTests
{
    private Mock<ISendGridAccessibilityService> _mockSendGridAccessibilityService;
    private EmailSenderJobListener _emailSenderJobListener;

    [SetUp]
    public void Setup()
    {
        _mockSendGridAccessibilityService = new Mock<ISendGridAccessibilityService>();
        _emailSenderJobListener = new EmailSenderJobListener(_mockSendGridAccessibilityService.Object);
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
    public async Task JobWasExecuted_WithJobException_ShouldCallSetSendGridInaccessible()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>().Object;
        var jobException = new JobExecutionException();

        // Act
        await _emailSenderJobListener.JobWasExecuted(context, jobException);

        // Assert
        _mockSendGridAccessibilityService.Verify(
            s => s.SetSendGridInaccessible(It.IsAny<DateTimeOffset>()),
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
