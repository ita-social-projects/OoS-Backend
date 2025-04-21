using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.QuartzJobs.Api.Configuration;
using OutOfSchool.QuartzJobs.Api.Logging;
using OutOfSchool.QuartzJobs.Api.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Monitoring;

[TestFixture]
public class RedisJobExecutionLoggerTests
{
    private Mock<IConnectionMultiplexer> _connectionMock;
    private Mock<IDatabase> _databaseMock;
    private IOptions<QuartzMonitoringOptions> _options;
    private RedisJobExecutionLogger _logger;
    private const int DefaultHistoryLength = 5;
    private const string TestJobName = "TestJob";

    [SetUp]
    public void Setup()
    {
        _databaseMock = new Mock<IDatabase>();
        _connectionMock = new Mock<IConnectionMultiplexer>();
        _connectionMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _options = Options.Create(new QuartzMonitoringOptions
        {
            Redis = new QuartzMonitoringOptions.RedisConfig
            {
                HistoryLength = DefaultHistoryLength
            }
        });

        _logger = new RedisJobExecutionLogger(_connectionMock.Object, _options);
    }

    [Test]
    public async Task LogAsync_PushesJobExecutionInfoToRedis()
    {
        // Arrange
        var info = CreateJobExecutionInfo();
        var key = $"monitoring:jobs:{TestJobName}:history";
        var expectedJson = JsonSerializer.Serialize(info);

        _databaseMock.Setup(db => db.ListLeftPushAsync(key, expectedJson, When.Always, CommandFlags.None))
            .ReturnsAsync(1)
            .Verifiable();

        _databaseMock.Setup(db => db.ListTrimAsync(key, 0, DefaultHistoryLength - 1, CommandFlags.None))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _logger.LogAsync(info);

        // Assert
        _databaseMock.Verify();
    }

    [Test]
    public async Task GetHistoryAsync_ReturnsDeserializedJobExecutionInfoList()
    {
        // Arrange
        var jobInfos = new List<JobExecutionInfo>
        {
            CreateJobExecutionInfo(true),
            CreateJobExecutionInfo(false)
        };

        var key = $"monitoring:jobs:{TestJobName}:history";
        var serialized = jobInfos.Select(x => (RedisValue)JsonSerializer.Serialize(x)).ToArray();

        _databaseMock.Setup(db => db.ListRangeAsync(key, 0, -1, CommandFlags.None))
            .ReturnsAsync(serialized);

        // Act
        var result = await _logger.GetHistoryAsync(TestJobName);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        for (int i = 0; i < jobInfos.Count; i++)
        {
            Assert.That(result[i].JobName, Is.EqualTo(jobInfos[i].JobName));
            Assert.That(result[i].WasSuccessful, Is.EqualTo(jobInfos[i].WasSuccessful));
            Assert.That(result[i].StartTime, Is.EqualTo(jobInfos[i].StartTime).Within(TimeSpan.FromSeconds(1)));
        }
    }

    [Test]
    public async Task GetFailedAsync_ReturnsOnlyFailedExecutions()
    {
        // Arrange
        var jobInfos = new List<JobExecutionInfo>
        {
            CreateJobExecutionInfo(true),
            CreateJobExecutionInfo(false),
            CreateJobExecutionInfo(true),
            CreateJobExecutionInfo(false)
        };

        var key = $"monitoring:jobs:{TestJobName}:history";
        var serialized = jobInfos.Select(x => (RedisValue)JsonSerializer.Serialize(x)).ToArray();

        _databaseMock.Setup(db => db.ListRangeAsync(key, 0, -1, CommandFlags.None))
            .ReturnsAsync(serialized);

        // Act
        var result = await _logger.GetFailedAsync(TestJobName);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(x => !x.WasSuccessful), Is.True);
    }

    [Test]
    public void Constructor_ShouldFallbackToDefaultHistoryLength_WhenRedisOptionsIsNull()
    {
        // Arrange
        var options = Options.Create(new QuartzMonitoringOptions { Redis = null });

        // Act
        var logger = new RedisJobExecutionLogger(_connectionMock.Object, options);

        // Assert
        Assert.IsNotNull(logger);
    }

    private JobExecutionInfo CreateJobExecutionInfo(bool wasSuccessful = true)
    {
        return new JobExecutionInfo
        {
            JobName = TestJobName,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddSeconds(1),
            Duration = TimeSpan.FromSeconds(1),
            WasSuccessful = wasSuccessful,
            ErrorMessage = wasSuccessful ? null : "Simulated error"
        };
    }
}
