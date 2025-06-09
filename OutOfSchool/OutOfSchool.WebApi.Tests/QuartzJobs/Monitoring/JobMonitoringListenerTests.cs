using Moq;
using NUnit.Framework;
using OutOfSchool.QuartzJobs.Api.Listeners;
using OutOfSchool.QuartzJobs.Api.Logging;
using OutOfSchool.QuartzJobs.Api.Models;
using Quartz;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Monitoring;

[TestFixture]
public class JobMonitoringListenerTests
{
    private Mock<IJobExecutionLogger> _loggerMock;
    private JobMonitoringListener _listener;
    private Mock<IJobExecutionContext> _contextMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<IJobExecutionLogger>();
        _listener = new JobMonitoringListener(_loggerMock.Object);

        var mergedJobDataMap = new JobDataMap();

        _contextMock = new Mock<IJobExecutionContext>();
        _contextMock.Setup(c => c.MergedJobDataMap).Returns(mergedJobDataMap);

        var jobDetailMock = new Mock<IJobDetail>();
        jobDetailMock.Setup(j => j.Key).Returns(new JobKey("TestJob", "TestGroup"));
        _contextMock.Setup(c => c.JobDetail).Returns(jobDetailMock.Object);

        var triggerMock = new Mock<ITrigger>();
        triggerMock.Setup(t => t.Key).Returns(new TriggerKey("TestTrigger", "TestTriggerGroup"));
        _contextMock.Setup(c => c.Trigger).Returns(triggerMock.Object);

        _contextMock.Setup(c => c.RefireCount).Returns(0);
        _contextMock.Setup(c => c.JobRunTime).Returns(TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Name_ShouldReturnCorrectName()
    {
        // Assert
        Assert.That(_listener.Name, Is.EqualTo("JobMonitoringListener"));
    }

    [Test]
    public async Task JobToBeExecuted_ShouldSaveStartTimeToContext()
    {
        // Arrange
        DateTime beforeCall = DateTime.UtcNow;

        // Act
        await _listener.JobToBeExecuted(_contextMock.Object, CancellationToken.None);

        // Assert
        Assert.IsTrue(_contextMock.Object.MergedJobDataMap.ContainsKey("__StartTime"));
        DateTime savedTime = (DateTime)_contextMock.Object.MergedJobDataMap["__StartTime"];
        Assert.That(savedTime, Is.GreaterThanOrEqualTo(beforeCall));
        Assert.That(savedTime, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task JobExecutionVetoed_ShouldDoNothing()
    {
        // Act
        await _listener.JobExecutionVetoed(_contextMock.Object, CancellationToken.None);

        // Assert - no interactions should occur
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task JobWasExecuted_WithoutException_ShouldLogSuccessfulExecution()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddSeconds(-1);
        _contextMock.Object.MergedJobDataMap.Put("__StartTime", startTime);
        _contextMock.Object.MergedJobDataMap.Put("TestParam", "TestValue");

        var concreteTrigger = new Mock<ISimpleTrigger>();
        concreteTrigger.Setup(t => t.Key).Returns(new TriggerKey("TestTrigger", "TestTriggerGroup"));
        _contextMock.Setup(c => c.Trigger).Returns(concreteTrigger.Object);

        JobExecutionInfo capturedInfo = null;
        _loggerMock.Setup(l => l.LogAsync(It.IsAny<JobExecutionInfo>()))
            .Callback<JobExecutionInfo>(info => capturedInfo = info)
            .Returns(Task.CompletedTask);

        // Act
        await _listener.JobWasExecuted(_contextMock.Object, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogAsync(It.IsAny<JobExecutionInfo>()), Times.Once);

        Assert.That(capturedInfo, Is.Not.Null);
        Assert.That(capturedInfo.JobName, Is.EqualTo("TestJob"));
        Assert.That(capturedInfo.JobGroup, Is.EqualTo("TestGroup"));
        Assert.That(capturedInfo.TriggerName, Is.EqualTo("TestTrigger"));
        Assert.That(capturedInfo.TriggerGroup, Is.EqualTo("TestTriggerGroup"));
        Assert.That(capturedInfo.WasSuccessful, Is.True);
        Assert.That(capturedInfo.RetryCount, Is.EqualTo(0));
        Assert.That(capturedInfo.TriggerType, Is.Not.Null);
        Assert.That(capturedInfo.ErrorMessage, Is.EqualTo("No error message available"));
        Assert.That(capturedInfo.StackTrace, Is.EqualTo("No exception details available"));
        Assert.That(capturedInfo.Parameters, Does.ContainKey("TestParam"));
        Assert.That(capturedInfo.Parameters["TestParam"], Is.EqualTo("TestValue"));
        Assert.That(capturedInfo.Parameters, Does.Not.ContainKey("__StartTime"));
    }

    [Test]
    public async Task JobWasExecuted_WithException_ShouldLogFailedExecution()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddSeconds(-1);
        _contextMock.Object.MergedJobDataMap.Put("__StartTime", startTime);

        var exception = new JobExecutionException("Test exception");

        JobExecutionInfo capturedInfo = null;
        _loggerMock.Setup(l => l.LogAsync(It.IsAny<JobExecutionInfo>()))
            .Callback<JobExecutionInfo>(info => capturedInfo = info)
            .Returns(Task.CompletedTask);

        // Act
        await _listener.JobWasExecuted(_contextMock.Object, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogAsync(It.IsAny<JobExecutionInfo>()), Times.Once);

        Assert.That(capturedInfo, Is.Not.Null);
        Assert.That(capturedInfo.WasSuccessful, Is.False);
        Assert.That(capturedInfo.ErrorMessage, Is.EqualTo("Test exception"));
        Assert.That(capturedInfo.StackTrace, Does.Contain("Test exception"));
    }

    [Test]
    public async Task JobWasExecuted_WithoutStartTime_ShouldUseCurrentTime()
    {
        // Arrange
        DateTime beforeCall = DateTime.UtcNow;

        JobExecutionInfo capturedInfo = null;
        _loggerMock.Setup(l => l.LogAsync(It.IsAny<JobExecutionInfo>()))
            .Callback<JobExecutionInfo>(info => capturedInfo = info)
            .Returns(Task.CompletedTask);

        // Act
        await _listener.JobWasExecuted(_contextMock.Object, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogAsync(It.IsAny<JobExecutionInfo>()), Times.Once);

        Assert.That(capturedInfo, Is.Not.Null);
        // Start time should be current time (converted to local)
        Assert.That(capturedInfo.StartTime.ToUniversalTime(), Is.GreaterThanOrEqualTo(beforeCall));
    }

    [Test]
    public async Task JobWasExecuted_WithParameters_ShouldLogParameters()
    {
        // Arrange
        _contextMock.Object.MergedJobDataMap.Put("__StartTime", DateTime.UtcNow);
        _contextMock.Object.MergedJobDataMap.Put("Param1", "Value1");
        _contextMock.Object.MergedJobDataMap.Put("Param2", 123);
        _contextMock.Object.MergedJobDataMap.Put("Param3", null);

        JobExecutionInfo capturedInfo = null;
        _loggerMock.Setup(l => l.LogAsync(It.IsAny<JobExecutionInfo>()))
            .Callback<JobExecutionInfo>(info => capturedInfo = info)
            .Returns(Task.CompletedTask);

        // Act
        await _listener.JobWasExecuted(_contextMock.Object, null, CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogAsync(It.IsAny<JobExecutionInfo>()), Times.Once);

        Assert.That(capturedInfo, Is.Not.Null);
        Assert.That(capturedInfo.Parameters, Is.Not.Null);
        Assert.That(capturedInfo.Parameters.Count, Is.EqualTo(3));
        Assert.That(capturedInfo.Parameters["Param1"], Is.EqualTo("Value1"));
        Assert.That(capturedInfo.Parameters["Param2"], Is.EqualTo("123"));
        Assert.That(capturedInfo.Parameters["Param3"], Is.EqualTo("null"));
        Assert.That(capturedInfo.Parameters, Does.Not.ContainKey("__StartTime"));
    }
}