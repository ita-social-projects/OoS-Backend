using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender.Quartz;
using OutOfSchool.EmailSender.Services;
using Quartz;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class EmailSenderJobTriggerListenerTests
{
    private EmailSenderJobTriggerListener _emailSenderJobTriggerListener;
    private Mock<ISendGridAccessibilityService> _sendGridAccessibilityServiceMock;
    private Mock<ILogger<EmailSenderJobTriggerListener>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _sendGridAccessibilityServiceMock = new Mock<ISendGridAccessibilityService>();
        _loggerMock = new Mock<ILogger<EmailSenderJobTriggerListener>>();
        _emailSenderJobTriggerListener = new EmailSenderJobTriggerListener(
            _sendGridAccessibilityServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Test]
    public void NameProperty_ShouldReturnName()
    {
        Assert.AreEqual("Email Sender Job Trigger Listener", _emailSenderJobTriggerListener.Name);
    }

    [Test]
    public void TriggerComplete_ShouldReturnCompletedTask()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>().Object;
        var trigger = new Mock<ITrigger>().Object;

        // Act
        var result = _emailSenderJobTriggerListener.TriggerComplete(trigger, context, It.IsAny<SchedulerInstruction>());

        // Assert
        Assert.AreEqual(Task.CompletedTask, result);
    }

    [Test]
    public void TriggerFired_ShouldReturnCompletedTask()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>().Object;
        var trigger = new Mock<ITrigger>().Object;

        // Act
        var result = _emailSenderJobTriggerListener.TriggerFired(trigger, context);

        // Assert
        Assert.AreEqual(Task.CompletedTask, result);
    }

    [Test]
    public void TriggerMisfired_ShouldReturnCompletedTask()
    {
        // Arrange
        var trigger = new Mock<ITrigger>().Object;

        // Act
        var result = _emailSenderJobTriggerListener.TriggerMisfired(trigger);

        // Assert
        Assert.AreEqual(Task.CompletedTask, result);
    }

    [Test]
    public async Task VetoJobExecution_SendGridAccessible_ReturnsFalse()
    {
        // Arrange
        _sendGridAccessibilityServiceMock
            .Setup(service => service.IsSendGridAccessible(It.IsAny<DateTimeOffset>()))
            .Returns(true);

        // Act
        var result = await _emailSenderJobTriggerListener.VetoJobExecution(
            It.IsAny<ITrigger>(),
            It.IsAny<IJobExecutionContext>(),
            It.IsAny<CancellationToken>()
        );

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task VetoJobExecution_SendGridInaccessible_ReturnsTrue()
    {
        // Arrange
        _sendGridAccessibilityServiceMock
            .Setup(service => service.IsSendGridAccessible(It.IsAny<DateTimeOffset>()))
            .Returns(false);

        // Act
        var result = await _emailSenderJobTriggerListener.VetoJobExecution(
            It.IsAny<ITrigger>(),
            It.IsAny<IJobExecutionContext>(),
            It.IsAny<CancellationToken>()
        );

        // Assert
        Assert.IsTrue(result);
    }
}
