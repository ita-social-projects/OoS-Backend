using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.NotificationImplementations;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using StackExchange.Redis;
using System.Threading.Tasks;
using System;

namespace OutOfSchool.WebApi.Tests.MinioNotifications;

[TestFixture]
public class RedisStorageNotificationHandlerTests
{
    private Mock<IConnectionMultiplexer> _redisConnectionMock;
    private Mock<ILogger<RedisStorageNotificationHandler>> _loggerMock;    
    private Mock<IProcessNotificationService> _processNotificationServiceMock;
    private Mock<ISubscriber> _subscriberMock;
    private RedisStorageNotificationHandler _handler;

    [SetUp]
    public void Setup()
    {
        _redisConnectionMock = new Mock<IConnectionMultiplexer>();
        _loggerMock = new Mock<ILogger<RedisStorageNotificationHandler>>();
        _processNotificationServiceMock = new Mock<IProcessNotificationService>();
        _subscriberMock = new Mock<ISubscriber>();

        _redisConnectionMock.Setup(x => x.GetSubscriber(It.IsAny<object>()))
            .Returns(_subscriberMock.Object);

        _handler = new RedisStorageNotificationHandler(
            _redisConnectionMock.Object,
            _loggerMock.Object,
            _processNotificationServiceMock.Object);
    }

    [Test]
    public async Task Subscribe_ValidParameters_SubscribesToRedisChannel()
    {
        // Arrange
        var bucket = "test-bucket";
        var filter = new NotificationFilter { Prefix = "prefix", Suffix = "suffix" };
        var expectedChannelName = $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";

        _subscriberMock.Setup(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == expectedChannelName),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Subscribe(bucket, filter);

        // Assert
        _redisConnectionMock.Verify(x => x.GetSubscriber(It.IsAny<object>()), Times.Once);
        _subscriberMock.Verify(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == expectedChannelName),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task Subscribe_WhenRedisConnectionThrowsException_LogsErrorAndRethrows()
    {
        // Arrange
        var bucket = "test-bucket";
        var filter = new NotificationFilter { Prefix = "prefix", Suffix = "suffix" };
        var expectedChannelName = $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";

        _subscriberMock.Setup(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == expectedChannelName),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _handler.Subscribe(bucket, filter));
        Assert.That(ex.Message, Is.EqualTo("Test exception"));
    }

    [Test]
    public async Task Subscribe_WhenRedisConnectionExceptionOccurs_LogsErrorButDoesNotRethrow()
    {
        // Arrange
        var bucket = "test-bucket";
        var filter = new NotificationFilter { Prefix = "prefix", Suffix = "suffix" };
        var expectedChannelName = $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";

        _subscriberMock.Setup(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == expectedChannelName),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection error"));

        // Act & Assert
        await _handler.Subscribe(bucket, filter);        
    }

    [Test]
    public async Task Unsubscribe_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var bucket = "test-bucket";
        var filter = new NotificationFilter { Prefix = "prefix", Suffix = "suffix" };
        var expectedChannelName = $"minio-notifications:{bucket}:{filter.Prefix}:{filter.Suffix}";
        
        _subscriberMock.Setup(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == expectedChannelName),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        await _handler.Subscribe(bucket, filter);

        _subscriberMock.Setup(x => x.UnsubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == expectedChannelName),
            null,
            It.IsAny<CommandFlags>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _handler.Unsubscribe(bucket, filter));
        Assert.That(ex.Message, Is.EqualTo("Test exception"));
    }
}
