using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Memento;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.Redis;

namespace OutOfSchool.WebApi.Tests.Services.Memento;

[TestFixture]
public class StorageTests
{
    private Mock<ICrudCacheService> crudCacheServiceMock;
    private Mock<ILogger<Storage>> loggerMock;
    private IStorage storage;

    [SetUp]
    public void SetUp()
    {
        crudCacheServiceMock = new Mock<ICrudCacheService>();
        loggerMock = new Mock<ILogger<Storage>>();

        storage = new Storage(crudCacheServiceMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task SetMementoValueAsync_ShouldCallOnce_SetValueToCacheAsync()
    {
        // Arrange & Act
        await storage.SetMementoValueAsync(new KeyValuePair<string, string?>("ExpectedKey", "ExpectedValue"));

        // Assert
        crudCacheServiceMock.Verify(
            c => c.SetValueToCacheAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                null),
            Times.Once);
    }

    [Test]
    public async Task GetMementoValueAsync_WhenDataExistsInCacheAndNotExpired_ShouldReturnData()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", "ExpectedValue"},
            };
        crudCacheServiceMock.Setup(c => c.GetValueFromCacheAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        var result = await storage.GetMementoValueAsync("ExpectedKey");

        // Assert
        result.Value.Contains("ExpectedValue");
        crudCacheServiceMock.Verify(
            c => c.GetValueFromCacheAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task GetMementoValueAsync_WhenDataNotExistsOrExpired_ShouldReturnNull()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", null},
            };
        crudCacheServiceMock.Setup(c => c.GetValueFromCacheAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        var result = await storage.GetMementoValueAsync("ExpectedKey");

        // Assert
        Assert.IsNull(result.Value);
        crudCacheServiceMock.Verify(
            c => c.GetValueFromCacheAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task RemoveMementoAsync_WhenDataExistsInCache_ShouldCallRemoveFromCacheAsyncOnce()
    {
        // Arrange
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", "ExpectedValue"},
            };
        crudCacheServiceMock.Setup(c => c.GetValueFromCacheAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        await storage.RemoveMementoAsync("ExpectedKey");

        // Assert
        crudCacheServiceMock.Verify(
            c => c.RemoveFromCacheAsync("ExpectedKey"),
            Times.Once);
    }

    [Test]
    public void RemoveMementoAsync_WhenDataNotExistsInCache_ShouldThrowInvalidOperationExceptionAndCallRemoveFromCacheAsyncOnce()
    {
        // Arrange & Act & Assert
        Assert.ThrowsAsync(
            typeof(InvalidOperationException),
            async () => await storage.RemoveMementoAsync("NotExistedKey"));

        crudCacheServiceMock.Verify(
            c => c.RemoveFromCacheAsync("NotExistedKey"),
            Times.Never);
    }
}