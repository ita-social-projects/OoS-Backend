using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bogus;
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
    private const int RANDOMSTRINGSIZE = 50;

    private string expectedValue;
    private string expectedKey;
    private Mock<IDistributedCache> distributedCacheMock;
    private Mock<IOptions<RedisConfig>> redisConfigMock;
    private ICacheService cacheService;
    private IReadWriteCacheService readWriteCacheService;

    [SetUp]
    public void SetUp()
    {
        expectedValue = new string(new Faker().Random.Chars(min: (char)0, max: (char)127, count: RANDOMSTRINGSIZE));
        expectedKey = new string(new Faker().Random.Chars(min: (char)0, max: (char)127, count: RANDOMSTRINGSIZE));
        distributedCacheMock = new Mock<IDistributedCache>();
        redisConfigMock = new Mock<IOptions<RedisConfig>>();
        redisConfigMock.Setup(c => c.Value).Returns(new RedisConfig
        {
            Enabled = true,
            AbsoluteExpirationRelativeToNowInterval = TimeSpan.FromMinutes(1),
            SlidingExpirationInterval = TimeSpan.FromMinutes(1),
        });
        cacheService = new CacheService(distributedCacheMock.Object, redisConfigMock.Object);
        readWriteCacheService = new CacheService(distributedCacheMock.Object, redisConfigMock.Object);
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
        await cacheService.RemoveAsync(expectedValue);

        // Assert
        distributedCacheMock.VerifyAll();
        distributedCacheMock.Verify(
            c => c.Remove(expectedValue),
            Times.Once); // called only once
    }

    [Test]
    public async Task ReadAsync_WhenDataExistsInCacheAndNotExpired_ShouldReturnData()
    {
        // Arrange
        distributedCacheMock.Setup(c => c.Get(expectedKey))
            .Returns(Encoding.UTF8.GetBytes(expectedValue));

        // Act
        var result = await readWriteCacheService.ReadAsync(expectedKey);

        // Assert
        result.Should().Be(expectedValue);
        distributedCacheMock.VerifyAll();
        distributedCacheMock.Verify(
            c => c.Get(expectedKey),
            Times.Once); // called only once
    }

    [Test]
    public async Task ReadAsync_WhenDataNotExistsOrExpired_ShouldReturnNull()
    {
        // Arrange
        distributedCacheMock.Setup(c => c.Get(expectedKey))
            .Returns(Encoding.UTF8.GetBytes(string.Empty));

        // Act
        var result = await readWriteCacheService.ReadAsync(expectedKey);

        // Assert
        result.Should().Be(string.Empty);
        distributedCacheMock.VerifyAll();
        distributedCacheMock.Verify(
            c => c.Get(expectedKey),
            Times.Once); // called only once
    }

    [Test]
    public async Task WriteAsync_ShouldCallCacheSetOnce()
    {
        // Arrange & Act
        await readWriteCacheService.WriteAsync(expectedKey, expectedValue);

        // Assert
        distributedCacheMock.VerifyAll();
        distributedCacheMock.Verify(
            c => c.Set(
                expectedKey,
                Encoding.UTF8.GetBytes(expectedValue),
                It.IsAny<DistributedCacheEntryOptions>()),
            Times.Once); // called only once
    }
}