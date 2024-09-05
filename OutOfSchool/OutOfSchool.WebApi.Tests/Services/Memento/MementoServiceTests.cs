using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;
using OutOfSchool.BusinessLogic.Services.Memento;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.Redis;

namespace OutOfSchool.WebApi.Tests.Services.Memento;

[TestFixture]
public class MementoServiceTests
{
    private Mock<IReadWriteCacheService> readWriteCacheServiceMock;
    private Mock<ILogger<MementoService<WorkshopWithRequiredPropertiesDto>>> loggerMock;
    private IMementoService<WorkshopWithRequiredPropertiesDto> mementoService;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger<MementoService<WorkshopWithRequiredPropertiesDto>>>();
        readWriteCacheServiceMock = new Mock<IReadWriteCacheService>();
        mementoService = new MementoService<WorkshopWithRequiredPropertiesDto>(readWriteCacheServiceMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        // Arrange
        var workshopMemento = new WorkshopWithRequiredPropertiesDto()
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
        var result = await mementoService.RestoreAsync("ExpectedKey");

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        Assert.AreEqual(workshopMemento.Title, result.Title);
        Assert.AreEqual(workshopMemento.Email, result.Email);
        Assert.AreEqual(workshopMemento.Phone, result.Phone);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        var expectedMemento = default(WorkshopWithRequiredPropertiesDto);

        // Act
        var result = await mementoService.RestoreAsync("ExpectedKey");

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        Assert.AreEqual(expectedMemento, result);
    }

    [Test]
    public void CreateAsync_ShouldCallWriteAsyncOnce()
    {
        // Arrange
        var workshopMemento = new WorkshopWithRequiredPropertiesDto()
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
        var result = mementoService.CreateAsync("ExpectedKey", workshopMemento);

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
        await mementoService.RemoveAsync("ExpectedKey");

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
        await mementoService.RemoveAsync("ExpectedKey");

        // Assert
        readWriteCacheServiceMock.Verify(
            c => c.ReadAsync(It.IsAny<string>()),
            Times.Once);
        readWriteCacheServiceMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Never);
    }
}
