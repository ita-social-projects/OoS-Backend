using Moq;
using NUnit.Framework;
using OutOfSchool.QuartzJobs.Api.Logging;
using OutOfSchool.QuartzJobs.Api.Models;
using OutOfSchool.QuartzJobs.Api.Services;
using OutOfSchool.QuartzJobs.Api.Util;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Monitoring;

[TestFixture]
public class QuartzMonitoringServiceTests
{
    private Mock<ISchedulerFactory> _schedulerFactoryMock;
    private Mock<IScheduler> _schedulerMock;
    private Mock<IJobExecutionLogger> _loggerMock;
    private QuartzMonitoringService _service;

    [SetUp]
    public void Setup()
    {
        _schedulerFactoryMock = new Mock<ISchedulerFactory>();
        _schedulerMock = new Mock<IScheduler>();
        _loggerMock = new Mock<IJobExecutionLogger>();

        _schedulerFactoryMock
            .Setup(sf => sf.GetScheduler(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(_schedulerMock.Object);

        _service = new QuartzMonitoringService(_schedulerFactoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetAllJobsAsync_ShouldReturnEmptyList_WhenNoJobsExist()
    {
        // Arrange
        var emptyJobKeys = new List<JobKey>();
        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(emptyJobKeys);

        // Act
        var result = await _service.GetAllJobsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllJobsAsync_ShouldReturnJobsWithCorrectInfo()
    {
        // Arrange
        var jobKeys = new List<JobKey>
            {
                new JobKey("job1", "group1"),
                new JobKey("job2", "group2")
            };

        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobKeys);

        var jobDetails = new Dictionary<string, IJobDetail>
        {
            ["job1"] = CreateMockJobDetail("job1", "group1", typeof(TestJob), "Job 1 description"),
            ["job2"] = CreateMockJobDetail("job2", "group2", typeof(TestJob), "Job 2 description")
        };

        foreach (var jobKey in jobKeys)
        {
            _schedulerMock
                .Setup(s => s.GetJobDetail(jobKey, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(jobDetails[jobKey.Name]);

            // Mock the trigger state to be Normal for all jobs
            _schedulerMock
                .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new List<ITrigger> { CreateMockTrigger($"trigger-{jobKey.Name}", jobKey.Group) });
        }

        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext>());

        // Act
        var result = await _service.GetAllJobsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var job1 = result.FirstOrDefault(j => j.Name == "job1");
        Assert.That(job1, Is.Not.Null);
        Assert.That(job1.Group, Is.EqualTo("group1"));
        Assert.That(job1.JobType, Is.EqualTo(typeof(TestJob).FullName));
        Assert.That(job1.Status, Is.EqualTo(QuartzJobStatus.Scheduled.ToString()));
        Assert.That(job1.Description, Is.EqualTo("Job 1 description"));

        var job2 = result.FirstOrDefault(j => j.Name == "job2");
        Assert.That(job2, Is.Not.Null);
        Assert.That(job2.Group, Is.EqualTo("group2"));
        Assert.That(job2.JobType, Is.EqualTo(typeof(TestJob).FullName));
        Assert.That(job2.Status, Is.EqualTo(QuartzJobStatus.Scheduled.ToString()));
        Assert.That(job2.Description, Is.EqualTo("Job 2 description"));
    }

    [Test]
    public async Task GetRunningJobsAsync_ShouldReturnEmptyList_WhenNoJobsRunning()
    {
        // Arrange
        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext>());

        // Act
        var result = await _service.GetRunningJobsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRunningJobsAsync_ShouldReturnRunningJobs()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var runningJobs = new List<IJobExecutionContext>
            {
                CreateMockJobExecutionContext("job1", "group1", "trigger1", "triggerGroup1", now),
                CreateMockJobExecutionContext("job2", "group2", "trigger2", "triggerGroup2", now.AddMinutes(-5))
            };

        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(runningJobs);

        // Act
        var result = await _service.GetRunningJobsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var job1 = result.FirstOrDefault(j => j.JobName == "job1");
        Assert.That(job1, Is.Not.Null);
        Assert.That(job1.Group, Is.EqualTo("group1"));
        Assert.That(job1.Trigger, Is.EqualTo("trigger1"));
        Assert.That(job1.TriggerGroup, Is.EqualTo("triggerGroup1"));
        Assert.That(job1.StartedAt, Is.EqualTo(now.ToLocalTime()));

        var job2 = result.FirstOrDefault(j => j.JobName == "job2");
        Assert.That(job2, Is.Not.Null);
        Assert.That(job2.Group, Is.EqualTo("group2"));
        Assert.That(job2.Trigger, Is.EqualTo("trigger2"));
        Assert.That(job2.TriggerGroup, Is.EqualTo("triggerGroup2"));
        Assert.That(job2.StartedAt, Is.EqualTo(now.AddMinutes(-5).ToLocalTime()));
    }

    [Test]
    public async Task GetJobDetailsAsync_ShouldReturnNull_WhenJobDoesNotExist()
    {
        // Arrange
        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey>());

        // Act
        var result = await _service.GetJobDetailsAsync("non-existent-job");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetJobDetailsAsync_ShouldReturnJobDetails_WithNormalTriggers()
    {
        // Arrange
        var jobKey = new JobKey("testJob", "testGroup");
        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey> { jobKey });

        var jobDetail = CreateMockJobDetail("testJob", "testGroup", typeof(TestJob), "Test job description");
        _schedulerMock
            .Setup(s => s.GetJobDetail(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobDetail);

        var triggers = new List<ITrigger>
            {
                CreateMockTrigger("trigger1", "triggerGroup1", true),
                CreateMockTrigger("trigger2", "triggerGroup2", false)
            };

        _schedulerMock
            .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(triggers);

        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext>());

        // Mock trigger states
        foreach (var trigger in triggers)
        {
            _schedulerMock
                .Setup(s => s.GetTriggerState(trigger.Key, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(TriggerState.Normal);
        }

        // Act
        var result = await _service.GetJobDetailsAsync("testJob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("testJob"));
        Assert.That(result.Group, Is.EqualTo("testGroup"));
        Assert.That(result.JobType, Is.EqualTo(typeof(TestJob).FullName));
        Assert.That(result.Status, Is.EqualTo(QuartzJobStatus.Scheduled.ToString()));
        Assert.That(result.Description, Is.EqualTo("Test job description"));
        Assert.That(result.Triggers.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetJobHistoryAsync_ShouldReturnHistoryFromLogger()
    {
        // Arrange
        var jobName = "testJob";
        var history = new List<JobExecutionInfo>
            {
                new JobExecutionInfo { JobName = jobName, WasSuccessful = true },
                new JobExecutionInfo { JobName = jobName, WasSuccessful = false }
            };

        _loggerMock
            .Setup(l => l.GetHistoryAsync(jobName))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetJobHistoryAsync(jobName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        _loggerMock.Verify(l => l.GetHistoryAsync(jobName), Times.Once);
    }

    [Test]
    public async Task GetFailedJobsAsync_ShouldReturnFailedJobsFromLogger()
    {
        // Arrange
        var jobName = "testJob";
        var failedJobs = new List<JobExecutionInfo>
            {
                new JobExecutionInfo { JobName = jobName, WasSuccessful = false },
                new JobExecutionInfo { JobName = jobName, WasSuccessful = false }
            };

        _loggerMock
            .Setup(l => l.GetFailedAsync(jobName))
            .ReturnsAsync(failedJobs);

        // Act
        var result = await _service.GetFailedJobsAsync(jobName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        _loggerMock.Verify(l => l.GetFailedAsync(jobName), Times.Once);
    }

    [Test]
    public async Task GetNextExecutionsForAllJobsAsync_ShouldReturnEmptyList_WhenNoJobsExist()
    {
        // Arrange
        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey>());

        // Act
        var result = await _service.GetNextExecutionsForAllJobsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetNextExecutionsForAllJobsAsync_ShouldReturnNextExecutions()
    {
        // Arrange
        var jobKeys = new List<JobKey>
            {
                new JobKey("job1", "group1"),
                new JobKey("job2", "group2")
            };

        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobKeys);

        foreach (var jobKey in jobKeys)
        {
            var triggers = new List<ITrigger>
                {
                    CreateMockTrigger($"trigger-{jobKey.Name}", jobKey.Group, true)
                };

            _schedulerMock
                .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(triggers);
        }

        // Act
        var result = await _service.GetNextExecutionsForAllJobsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var job1 = result.FirstOrDefault(j => j.JobName == "job1");
        Assert.That(job1, Is.Not.Null);
        Assert.That(job1.JobGroup, Is.EqualTo("group1"));
        Assert.That(job1.NextExecutions.Count, Is.EqualTo(1));
        Assert.That(job1.NextExecutions[0].TriggerName, Is.EqualTo("trigger-job1"));

        var job2 = result.FirstOrDefault(j => j.JobName == "job2");
        Assert.That(job2, Is.Not.Null);
        Assert.That(job2.JobGroup, Is.EqualTo("group2"));
        Assert.That(job2.NextExecutions.Count, Is.EqualTo(1));
        Assert.That(job2.NextExecutions[0].TriggerName, Is.EqualTo("trigger-job2"));
    }

    [Test]
    public async Task GetNextExecutionsForJobAsync_ShouldReturnEmptyList_WhenJobDoesNotExist()
    {
        // Arrange
        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey>());

        // Act
        var result = await _service.GetNextExecutionsForJobAsync("non-existent-job");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetNextExecutionsForJobAsync_ShouldReturnNextExecutions()
    {
        // Arrange
        var jobKey = new JobKey("testJob", "testGroup");
        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey> { jobKey });

        var triggers = new List<ITrigger>
            {
                CreateMockTrigger("trigger1", "triggerGroup1", true),
                CreateMockTrigger("trigger2", "triggerGroup2", false)
            };

        _schedulerMock
            .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(triggers);

        // Act
        var result = await _service.GetNextExecutionsForJobAsync("testJob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].JobName, Is.EqualTo("testJob"));
        Assert.That(result[0].JobGroup, Is.EqualTo("testGroup"));
        Assert.That(result[0].NextExecutions.Count, Is.EqualTo(2));
        Assert.That(result[0].NextExecutions[0].TriggerName, Is.EqualTo("trigger1"));
        Assert.That(result[0].NextExecutions[1].TriggerName, Is.EqualTo("trigger2"));
    }

    [Test]
    public async Task GetJobStatus_ShouldReturnRunning_WhenJobIsExecuting()
    {
        // Arrange
        var jobKey = new JobKey("runningJob", "testGroup");

        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey> { jobKey });

        var jobDetail = CreateMockJobDetail("runningJob", "testGroup", typeof(TestJob), "Test job");
        _schedulerMock
            .Setup(s => s.GetJobDetail(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobDetail);

        var triggers = new List<ITrigger> { CreateMockTrigger("trigger1", "triggerGroup1") };
        _schedulerMock
            .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(triggers);

        _schedulerMock
            .Setup(s => s.GetTriggerState(triggers[0].Key, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(TriggerState.Normal);

        // Set up a running job execution context
        var runningContext = CreateMockJobExecutionContext("runningJob", "testGroup", "trigger1", "triggerGroup1", DateTimeOffset.Now);
        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext> { runningContext });

        // Act
        var result = await _service.GetJobDetailsAsync("runningJob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(QuartzJobStatus.Running.ToString()));
    }

    [Test]
    public async Task GetJobStatus_ShouldReturnPaused_WhenAllTriggersArePaused()
    {
        // Arrange
        var jobKey = new JobKey("pausedJob", "testGroup");

        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey> { jobKey });

        var jobDetail = CreateMockJobDetail("pausedJob", "testGroup", typeof(TestJob), "Test job");
        _schedulerMock
            .Setup(s => s.GetJobDetail(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobDetail);

        var triggers = new List<ITrigger> { CreateMockTrigger("trigger1", "triggerGroup1") };
        _schedulerMock
            .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(triggers);

        _schedulerMock
            .Setup(s => s.GetTriggerState(triggers[0].Key, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(TriggerState.Paused);

        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext>());

        // Act
        var result = await _service.GetJobDetailsAsync("pausedJob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(QuartzJobStatus.Paused.ToString()));
    }

    [Test]
    public async Task GetJobStatus_ShouldReturnMixed_WhenTriggersHaveDifferentStates()
    {
        // Arrange
        var jobKey = new JobKey("mixedJob", "testGroup");

        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey> { jobKey });

        var jobDetail = CreateMockJobDetail("mixedJob", "testGroup", typeof(TestJob), "Test job");
        _schedulerMock
            .Setup(s => s.GetJobDetail(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobDetail);

        var trigger1 = CreateMockTrigger("trigger1", "triggerGroup1");
        var trigger2 = CreateMockTrigger("trigger2", "triggerGroup2");

        var triggers = new List<ITrigger> { trigger1, trigger2 };
        _schedulerMock
            .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(triggers);

        _schedulerMock
            .Setup(s => s.GetTriggerState(trigger1.Key, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(TriggerState.Normal);

        _schedulerMock
            .Setup(s => s.GetTriggerState(trigger2.Key, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(TriggerState.Paused);

        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext>());

        // Act
        var result = await _service.GetJobDetailsAsync("mixedJob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(QuartzJobStatus.Mixed.ToString()));
    }

    [Test]
    public async Task GetJobStatus_ShouldReturnUnknown_WhenNoTriggersExist()
    {
        // Arrange
        var jobKey = new JobKey("unknownJob", "testGroup");

        _schedulerMock
            .Setup(s => s.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<JobKey> { jobKey });

        var jobDetail = CreateMockJobDetail("unknownJob", "testGroup", typeof(TestJob), "Test job");
        _schedulerMock
            .Setup(s => s.GetJobDetail(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(jobDetail);

        // No triggers for this job
        _schedulerMock
            .Setup(s => s.GetTriggersOfJob(jobKey, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<ITrigger>());

        _schedulerMock
            .Setup(s => s.GetCurrentlyExecutingJobs(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new List<IJobExecutionContext>());

        // Act
        var result = await _service.GetJobDetailsAsync("unknownJob");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(QuartzJobStatus.Unknown.ToString()));
    }

    #region Helper Methods

    private static IJobDetail CreateMockJobDetail(string name, string group, Type jobType, string description)
    {
        var jobDetailMock = new Mock<IJobDetail>();
        jobDetailMock.Setup(jd => jd.Key).Returns(new JobKey(name, group));
        jobDetailMock.Setup(jd => jd.JobType).Returns(jobType);
        jobDetailMock.Setup(jd => jd.Description).Returns(description);
        return jobDetailMock.Object;
    }

    private static ITrigger CreateMockTrigger(string name, string group, bool isCronTrigger = false)
    {
        if (isCronTrigger)
        {
            var cronTriggerMock = new Mock<ICronTrigger>();
            cronTriggerMock.Setup(t => t.Key).Returns(new TriggerKey(name, group));
            cronTriggerMock.Setup(t => t.CronExpressionString).Returns("0 0 12 * * ?"); // Noon every day
            cronTriggerMock.Setup(t => t.GetNextFireTimeUtc()).Returns(DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(12));
            return cronTriggerMock.Object;
        }
        else
        {
            var triggerMock = new Mock<ITrigger>();
            triggerMock.Setup(t => t.Key).Returns(new TriggerKey(name, group));
            triggerMock.Setup(t => t.GetNextFireTimeUtc()).Returns(DateTimeOffset.UtcNow.AddHours(1));
            return triggerMock.Object;
        }
    }

    private static IJobExecutionContext CreateMockJobExecutionContext(
        string jobName,
        string jobGroup,
        string triggerName,
        string triggerGroup,
        DateTimeOffset fireTime)
    {
        var jobDetailMock = new Mock<IJobDetail>();
        jobDetailMock.Setup(jd => jd.Key).Returns(new JobKey(jobName, jobGroup));

        var triggerMock = new Mock<ITrigger>();
        triggerMock.Setup(t => t.Key).Returns(new TriggerKey(triggerName, triggerGroup));

        var contextMock = new Mock<IJobExecutionContext>();
        contextMock.Setup(c => c.JobDetail).Returns(jobDetailMock.Object);
        contextMock.Setup(c => c.Trigger).Returns(triggerMock.Object);
        contextMock.Setup(c => c.FireTimeUtc).Returns(fireTime);

        return contextMock.Object;
    }

    private class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }

    #endregion
}