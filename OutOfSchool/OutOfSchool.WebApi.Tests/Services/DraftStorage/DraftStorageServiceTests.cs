using System;
using System.Text.Json;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.DraftStorage;
using OutOfSchool.BusinessLogic.Services.DraftStorage.Interfaces;
using OutOfSchool.Redis;

namespace OutOfSchool.WebApi.Tests.Services.DraftStorage;

[TestFixture]
public class DraftStorageServiceTests
{
    private const int RANDOMSTRINGSIZE = 50;

    private string key;
    private string cacheKey;
    private Mock<IReadWriteCacheService> readWriteCacheServiceMock;
    private Mock<ILogger<DraftStorageService<WorkshopBaseDto>>> loggerMock;
    private IDraftStorageService<WorkshopBaseDto> draftStorageService;

    [SetUp]
    public void SetUp()
    {
        key = new string(new Faker().Random.Chars(min: (char)0, max: (char)127, count: RANDOMSTRINGSIZE));
        cacheKey = GetCacheKey(key, typeof(WorkshopBaseDto));
        loggerMock = new Mock<ILogger<DraftStorageService<WorkshopBaseDto>>>();
        readWriteCacheServiceMock = new Mock<IReadWriteCacheService>();
        draftStorageService = new DraftStorageService<WorkshopBaseDto>(readWriteCacheServiceMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task RestoreAsync_WhenDraftExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        // Arrange
        var workshopDraft = GetWorkshopFakeDraft();
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(JsonSerializer.Serialize(workshopDraft)))
            .Verifiable(Times.Once);

        // Act
        var result = await draftStorageService.RestoreAsync(key).ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<WorkshopBaseDto>();
        result.Title.Should().Be(workshopDraft.Title);
        result.Email.Should().Be(workshopDraft.Email);
        result.Phone.Should().Be(workshopDraft.Phone);
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task RestoreAsync_WhenDraftIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        var workshopDraft = default(WorkshopBaseDto);
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(JsonSerializer.Serialize(workshopDraft)))
            .Verifiable(Times.Once);

        // Act
        var result = await draftStorageService.RestoreAsync(key).ConfigureAwait(false);

        // Assert
        result.Should().Be(workshopDraft);
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public void CreateAsync_ShouldCallWriteAsyncOnce()
    {
        // Arrange
        var workshopDraft = GetWorkshopFakeDraft();
        var workshopJsonString = JsonSerializer.Serialize(workshopDraft);
        readWriteCacheServiceMock.Setup(c => c.WriteAsync(
            cacheKey,
            workshopJsonString,
            null,
            null))
            .Verifiable(Times.Once);

        // Act
        var result = draftStorageService.CreateAsync(key, workshopDraft).ConfigureAwait(false);

        // Assert
        readWriteCacheServiceMock.VerifyAll();
    }

    [Test]
    public async Task RemoveAsync_WhenDataExistsInCache_ShouldCallRemoveAsyncAndReadAsyncOnce()
    {
        // Arrange
        var workshopJsonString = JsonSerializer.Serialize(GetWorkshopFakeDraft());
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(cacheKey))
            .Returns(() => Task.FromResult(workshopJsonString))
            .Verifiable(Times.Once);
        readWriteCacheServiceMock.Setup(c => c.RemoveAsync(cacheKey))
            .Returns(() => Task.FromResult(workshopJsonString))
            .Verifiable(Times.Once);

        // Act
        await draftStorageService.RemoveAsync(key).ConfigureAwait(false);

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
            .Returns(() => Task.FromResult(string.Empty)).Verifiable(Times.Never);

        // Act
        await draftStorageService.RemoveAsync(key).ConfigureAwait(false);

        // Assert
        readWriteCacheServiceMock.VerifyAll();
    }

    private static WorkshopBaseDto GetWorkshopFakeDraft()
    {
        var workshopFacker = new Faker<WorkshopBaseDto>()
            .RuleFor(w => w.Title, f => f.Name.LastName())
            .RuleFor(w => w.Email, f => f.Internet.Email())
            .RuleFor(w => w.Phone, f => f.Phone.PhoneNumber());
        return workshopFacker.Generate();
    }

    private static string GetCacheKey(string key, Type type)
    {
        return $"{key}_{type.Name}";
    }
}
