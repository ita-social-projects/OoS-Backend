using System.Collections.Generic;
using System.Threading.Tasks;
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
    private Mock<IReadWriteCacheService> readWriteCacheServiceMock;
    private Mock<ILogger<DraftStorageService<WorkshopBaseDto>>> loggerMock;
    private IDraftStorageService<WorkshopBaseDto> draftStorageService;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger<DraftStorageService<WorkshopBaseDto>>>();
        readWriteCacheServiceMock = new Mock<IReadWriteCacheService>();
        draftStorageService = new DraftStorageService<WorkshopBaseDto>(readWriteCacheServiceMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task RestoreAsync_WhenDraftExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        // Arrange
        var workshopDraft = new WorkshopBaseDto()
        {
            Title = "title",
            Email = "myemail@gmail.com",
            Phone = "+380670000000",
        };
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", "{\"Title\":\"title\",\"Email\":\"myemail@gmail.com\",\"Phone\":\"+380670000000\"}"},
            };

        readWriteCacheServiceMock.Setup(c => c.ReadAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        var result = await draftStorageService.RestoreAsync("ExpectedKey");

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        result.Should().BeOfType<WorkshopBaseDto>();
        Assert.AreEqual(workshopDraft.Title, result.Title);
        Assert.AreEqual(workshopDraft.Email, result.Email);
        Assert.AreEqual(workshopDraft.Phone, result.Phone);
    }

    [Test]
    public async Task RestoreAsync_WhenDraftIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        var expectedDraft = default(WorkshopBaseDto);
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", null},
            };
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        var result = await draftStorageService.RestoreAsync("ExpectedKey");

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        Assert.AreEqual(expectedDraft, result);
    }

    [Test]
    public void CreateAsync_ShouldCallWriteAsyncOnce()
    {
        // Arrange
        var workshopDraft = new WorkshopBaseDto()
        {
            Title = "title",
            Email = "myemail@gmail.com",
            Phone = "+380670000000",
        };

        readWriteCacheServiceMock.Setup(c => c.WriteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            null));

        // Act
        var result = draftStorageService.CreateAsync("ExpectedKey", workshopDraft);

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.WriteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            null),
            Times.Once);
    }

    [Test]
    public async Task RemoveAsync_WhenDataExistsInCache_ShouldCallRemoveAsyncOnce()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
                {
                    {"ExpectedKey", "ExpectedValue"},
                };
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        await draftStorageService.RemoveAsync("ExpectedKey");

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        readWriteCacheServiceMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task RemoveAsync_WhenDataIsAbsentInCache_ShouldCallRemoveAsyncNever()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
                {
                    {"ExpectedKey", null},
                };
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        await draftStorageService.RemoveAsync("ExpectedKey").ConfigureAwait(false);

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        readWriteCacheServiceMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }
}
