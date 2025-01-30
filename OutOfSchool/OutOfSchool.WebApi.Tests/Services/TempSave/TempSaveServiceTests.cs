using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
using OutOfSchool.BusinessLogic.Services.TempSave;
using OutOfSchool.Common;
using OutOfSchool.Redis;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.TempSave;

[TestFixture]
public class TempSaveServiceTests
{
    private const int RANDOMSTRINGSIZE = 50;

    private string key;
    private string cacheKey;
    private Mock<IReadWriteCacheService> readWriteCacheServiceMock;
    private Mock<ILogger<TempSaveService<WorkshopMainRequiredPropertiesDto>>> loggerMock;
    private ITempSaveService<WorkshopMainRequiredPropertiesDto> tempSaveService;
    private Mock<IOptions<RedisForTempSaveConfig>> redisConfigMock;

    [SetUp]
    public void SetUp()
    {
        key = new string(new Faker().Random.Chars(min: (char)0, max: (char)127, count: RANDOMSTRINGSIZE));
        cacheKey = GetCacheKey(key, typeof(WorkshopMainRequiredPropertiesDto));
        loggerMock = new Mock<ILogger<TempSaveService<WorkshopMainRequiredPropertiesDto>>>();
        readWriteCacheServiceMock = new Mock<IReadWriteCacheService>();
        redisConfigMock = new Mock<IOptions<RedisForTempSaveConfig>>();
        redisConfigMock.Setup(c => c.Value).Returns(new RedisForTempSaveConfig
        {
            AbsoluteExpirationRelativeToNowInterval = TimeSpan.FromMinutes(1)
        });
        tempSaveService = new TempSaveService<WorkshopMainRequiredPropertiesDto>(readWriteCacheServiceMock.Object, loggerMock.Object, redisConfigMock.Object);
    }

    [Test]
    public async Task RestoreAsync_WhenDraftExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        // Arrange
        var workshopDraft = GetWorkshopFakeDraft();
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(JsonSerializerHelper.Serialize(workshopDraft)))
            .Verifiable(Times.Once);

        // Act
        var result = await tempSaveService.RestoreAsync(key).ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<WorkshopMainRequiredPropertiesDto>();
        result.Should().BeEquivalentTo(workshopDraft);
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task RestoreAsync_WhenDraftIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        var workshopDraft = default(WorkshopMainRequiredPropertiesDto);
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(string.Empty))
            .Verifiable(Times.Once);

        // Act
        var result = await tempSaveService.RestoreAsync(key).ConfigureAwait(false);

        // Assert
        result.Should().Be(workshopDraft);
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public void StoreAsync_ShouldCallWriteAsyncOnce()
    {
        // Arrange
        var workshopDraft = GetWorkshopFakeDraft();
        var workshopJsonString = JsonSerializerHelper.Serialize(workshopDraft);
        readWriteCacheServiceMock.Setup(c => c.WriteAsync(
            cacheKey,
            workshopJsonString,
            redisConfigMock.Object.Value.AbsoluteExpirationRelativeToNowInterval,
            TimeSpan.Zero))
            .Verifiable(Times.Once);

        // Act
        var result = tempSaveService.StoreAsync(key, workshopDraft).ConfigureAwait(false);

        // Assert
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task RemoveAsync_WhenDataExistsInCache_ShouldCallRemoveAsyncAndReadAsyncOnce()
    {
        // Arrange
        var workshopJsonString = JsonSerializerHelper.Serialize(GetWorkshopFakeDraft());
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(workshopJsonString))
            .Verifiable(Times.Once);
        readWriteCacheServiceMock.Setup(c => c.RemoveAsync(cacheKey))
            .Verifiable(Times.Once);

        // Act
        await tempSaveService.RemoveAsync(key).ConfigureAwait(false);

        // Assert
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task RemoveAsync_WhenDataIsAbsentInCache_ShouldNeverCallRemoveAsync()
    {
        // Arrange
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(string.Empty)).Verifiable(Times.Once);
        readWriteCacheServiceMock.Setup(c => c.RemoveAsync(cacheKey))
            .Verifiable(Times.Never);

        // Act
        await tempSaveService.RemoveAsync(key).ConfigureAwait(false);

        // Assert
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task GetTimeToLiveAsync_WhenDraftExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        // Arrange
        TimeSpan? timeToLive = TimeSpan.FromMinutes(1);
        readWriteCacheServiceMock.Setup(c => c.GetTimeToLiveAsync(cacheKey))
            .Returns(() => Task.FromResult(timeToLive))
            .Verifiable(Times.Once);

        // Act
        var result = await tempSaveService.GetTimeToLiveAsync(key).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(timeToLive);
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task GetTimeToLiveAsync_WhenDraftIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        TimeSpan? timeToLive = null;
        readWriteCacheServiceMock.Setup(c => c.GetTimeToLiveAsync(cacheKey))
            .Returns(() => Task.FromResult(timeToLive))
            .Verifiable(Times.Once);

        // Act
        var result = await tempSaveService.GetTimeToLiveAsync(key).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
        readWriteCacheServiceMock.VerifyAll();
    }

    private static WorkshopMainRequiredPropertiesDto GetWorkshopFakeDraft() =>
        WorkshopMainRequiredPropertiesDtoGenerator.Generate();

    private static string GetCacheKey(string key, Type type)
    {
        return $"{key}_{type.Name}";
    }
}