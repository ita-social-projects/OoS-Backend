using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Redis;

namespace OutOfSchool.WebApi.Tests.Redis;

[TestFixture]
public class CacheServiceTests
{
    private Mock<IDistributedCache> distributedCacheMock;
    private Mock<IOptions<RedisConfig>> redisConfigMock;
    private ICacheService cacheService;

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
            .Returns(Encoding.UTF8.GetBytes(JsonSerializerHelper.Serialize(expected)));

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
            .Returns((byte[]) null);

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
    public async Task RemoveAsync_ShouldCallCacheRemove()
    {
        // Arrange & Act
        await cacheService.RemoveAsync("Example");

        // Assert
        distributedCacheMock.Verify(
            c => c.RemoveAsync("Example", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}