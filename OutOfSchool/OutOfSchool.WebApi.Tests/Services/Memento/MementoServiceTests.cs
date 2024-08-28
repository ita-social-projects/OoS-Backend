using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Memento;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;
using OutOfSchool.Redis;

namespace OutOfSchool.WebApi.Tests.Services.Memento;

[TestFixture]
public class MementoServiceTests
{
    private Mock<ICrudCacheService> crudCacheServiceMock;
    private Mock<ILogger<MementoService<RequiredWorkshopMemento>>> loggerMock;
    private IMementoService<RequiredWorkshopMemento> mementoService;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger<MementoService<RequiredWorkshopMemento>>>();
        crudCacheServiceMock = new Mock<ICrudCacheService>();
        mementoService = new MementoService<RequiredWorkshopMemento>(crudCacheServiceMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        // Arrange
        var workshopMemento = new RequiredWorkshopMemento()
        {
            Title = "title",
            Email = "myemail@gmail.com",
            Phone = "+380670000000",
        };
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", "{\"Title\":\"title\",\"Email\":\"myemail@gmail.com\",\"Phone\":\"+380670000000\"}"},
            };

        crudCacheServiceMock.Setup(c => c.GetValueAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        var result = await mementoService.RestoreAsync("ExpectedKey");

        // Assert
        crudCacheServiceMock.Verify(
            c => c.GetValueAsync(It.IsAny<string>()),
            Times.Once);
        Assert.AreEqual(workshopMemento.Title, result.Title);
        Assert.AreEqual(workshopMemento.Email, result.Email);
        Assert.AreEqual(workshopMemento.Phone, result.Phone);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        var expectedMemento = default(RequiredWorkshopMemento);

        // Arrange & Act
        var result = await mementoService.RestoreAsync("ExpectedKey");

        // Assert
        crudCacheServiceMock.Verify(
            c => c.GetValueAsync(It.IsAny<string>()),
            Times.Once);
        Assert.AreEqual(expectedMemento, result);
    }

    [Test]
    public void CreateAsync_ShouldReturnCreatedMemento()
    {
        // Arrange
        var workshopMemento = new RequiredWorkshopMemento()
        {
            Title = "title",
            Email = "myemail@gmail.com",
            Phone = "+380670000000",
        };

        crudCacheServiceMock.Setup(c => c.SetValueAsync(
            It.IsAny<string>(),
            It.IsAny<RequiredWorkshopMemento>(),
            null,
            null));

        // Act
        var result = mementoService.CreateAsync("ExpectedKey", workshopMemento);

        // Assert
        crudCacheServiceMock.Verify(
            c => c.SetValueAsync(
            It.IsAny<string>(),
            It.IsAny<RequiredWorkshopMemento>(),
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
        crudCacheServiceMock.Setup(c => c.GetValueAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        await mementoService.RemoveAsync("ExpectedKey");

        // Assert
        crudCacheServiceMock.Verify(
            c => c.GetValueAsync(It.IsAny<string>()),
            Times.Once);
        crudCacheServiceMock.Verify(
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
        crudCacheServiceMock.Setup(c => c.GetValueAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

        // Act
        await mementoService.RemoveAsync("ExpectedKey");

        // Assert
        crudCacheServiceMock.Verify(
            c => c.GetValueAsync(It.IsAny<string>()),
            Times.Once);
        crudCacheServiceMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }
}
