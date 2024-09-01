using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using OutOfSchool.Redis;

namespace OutOfSchool.WebApi.Tests.Redis;

[TestFixture]
public class CacheServiceTests
{
    private Mock<IDistributedCache> distributedCacheMock;
    private Mock<IOptions<RedisConfig>> redisConfigMock;
    private ICacheService cacheService;
    private ICrudCacheService crudCacheService;

    [SetUp]
    public void SetUp()
    {
        distributedCacheMock = new Mock<IDistributedCache>();
        redisConfigMock = new Mock<IOptions<RedisConfig>>();
        redisConfigMock.Setup(c => c.Value).Returns(new RedisConfig
        {
            Enabled = true,
            AbsoluteExpirationRelativeToNowInterval = TimeSpan.FromMinutes(1),
            SlidingExpirationInterval = TimeSpan.FromMinutes(1),
        });
        cacheService = new CacheService(distributedCacheMock.Object, redisConfigMock.Object);
        crudCacheService = new CacheService(distributedCacheMock.Object, redisConfigMock.Object);
    }

    [Test]
    public async Task GetOrAddAsync_WhenDataExistsAndNotExpired_ShouldReturnData()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
        {
            {"ExpectedKey", "ExpectedValue"},
        };
        distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
            .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expected)));

        // Act
        var result = await cacheService.GetOrAddAsync("Example", () => Task.FromResult(expected));

        // Assert
        result.Keys.Should().Contain("ExpectedKey");
        result.Values.Should().Contain("ExpectedValue");
        distributedCacheMock.Verify(
            c => c.Set(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>()),
            Times.Never);
    }

    [Test]
    public async Task GetOrAddAsync_WhenDataNotExistsOrExpired_ShouldSaveNewData()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
        {
            {"ExpectedKey", "ExpectedValue"},
        };
        distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
            .Returns((byte[])null);

        // Act
        var result = await cacheService.GetOrAddAsync("Example", () => Task.FromResult(expected));

        // Assert
        result.Keys.Should().Contain("ExpectedKey");
        result.Values.Should().Contain("ExpectedValue");
        distributedCacheMock.Verify(
            c => c.Set(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>()),
            Times.Once);
    }

    [Test]
    public async Task RemoveAsync_ShouldCallCacheRemoveOnce()
    {
        // Arrange & Act
        await cacheService.RemoveAsync("Example");

        // Assert
        distributedCacheMock.Verify(
            c => c.Remove("Example"),
            Times.Once);
    }

    [Test]
    public async Task GetValueAsync_WhenDataExistsInCacheAndNotExpired_ShouldReturnData()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
        {
            {"ExpectedKey", "ExpectedValue"},
        };
        distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
            .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expected)));

        // Act
        var result = await crudCacheService.GetValueAsync("ExpectedKey");

        // Assert
        result.Should().Contain("ExpectedValue");
        distributedCacheMock.Verify(
            c => c.Get(It.IsAny<string>()),
            Times.Once);
    }

    public async Task GetValueAsync_WhenDataNotExistsOrExpired_ShouldReturnNull()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
        {
            {"ExpectedKey", null},
        };
        distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
            .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expected)));

        // Act
        var result = await crudCacheService.GetValueAsync("ExpectedKey");

        // Assert
        result.Should().Contain("{\"ExpectedKey\":null}");
        distributedCacheMock.Verify(
            c => c.Get(It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task UpsertValueAsync_ShouldCallCacheSetOnce()
    {
        // Arrange & Act
        await crudCacheService.UpsertValueAsync("ExpectedKey", "ExpectedValue");

        // Assert
        distributedCacheMock.Verify(
            c => c.Set(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>()),
            Times.Once);
    }
}