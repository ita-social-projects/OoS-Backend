using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
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
    private readonly string jsonStringForWorkshopWithDescriptionDto = "{\"$type\":\"withDescription\",\"WorkshopDescriptionItems\":[{\"Id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"SectionName\":\"string\",\"Description\":\"string\",\"WorkshopId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"}],\"WithDisabilityOptions\":false,\"DisabilityOptionsDesc\":\"\",\"InstitutionId\":null,\"Institution\":null,\"InstitutionHierarchyId\":null,\"InstitutionHierarchy\":null,\"DirectionIds\":null,\"Keywords\":null,\"Title\":\"string\",\"ShortTitle\":\"string\",\"Phone\":\"\\u002B340976894523\",\"Email\":\"user@example.com\",\"Website\":\"string\",\"Facebook\":\"string\",\"Instagram\":\"string\",\"MinAge\":12,\"MaxAge\":20,\"DateTimeRanges\":[{\"Id\":0,\"StartTime\":\"12:00:00\",\"EndTime\":\"14:00:00\",\"Workdays\":[4]}],\"FormOfLearning\":10,\"Price\":100000,\"PayRate\":\"None\",\"AvailableSeats\":10,\"CompetitiveSelection\":true,\"CompetitiveSelectionDescription\":\"string\",\"ProviderId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"ProviderTitle\":\"string\",\"ProviderTitleEn\":\"string\",\"ProviderLicenseStatus\":0}";
    private readonly string jsonStringForWorkshopWithContactsDto = "{\"$type\":\"withContacts\",\"AddressId\":0,\"Address\":{\"Id\":0,\"Street\":\"string111\"},\"WorkshopDescriptionItems\":[{\"Id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"SectionName\":\"string\",\"Description\":\"string\",\"WorkshopId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"}],\"WithDisabilityOptions\":false,\"DisabilityOptionsDesc\":\"\",\"InstitutionId\":null,\"Institution\":null,\"InstitutionHierarchyId\":null,\"InstitutionHierarchy\":null,\"DirectionIds\":null,\"Keywords\":null,\"Title\":\"string\",\"ShortTitle\":\"string\",\"Phone\":\"\\u002B340976894523\",\"Email\":\"user@example.com\",\"Website\":\"string\",\"Facebook\":\"string\",\"Instagram\":\"string\",\"MinAge\":12,\"MaxAge\":20,\"DateTimeRanges\":[{\"Id\":0,\"StartTime\":\"12:00:00\",\"EndTime\":\"14:00:00\",\"Workdays\":[4]}],\"FormOfLearning\":10,\"Price\":100000,\"PayRate\":\"None\",\"AvailableSeats\":10,\"CompetitiveSelection\":true,\"CompetitiveSelectionDescription\":\"string\",\"ProviderId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"ProviderTitle\":\"string\",\"ProviderTitleEn\":\"string\",\"ProviderLicenseStatus\":0}";
    private readonly string jsonStringForWorkshopWithTeachersDto = "{\"$type\":\"withTeachers\",\"Teachers\":[],\"AddressId\":0,\"Address\":{\"Id\":0,\"Street\":\"string111\"},\"WorkshopDescriptionItems\":[{\"Id\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"SectionName\":\"string\",\"Description\":\"string\",\"WorkshopId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"}],\"WithDisabilityOptions\":false,\"DisabilityOptionsDesc\":\"\",\"InstitutionId\":null,\"Institution\":null,\"InstitutionHierarchyId\":null,\"InstitutionHierarchy\":null,\"DirectionIds\":null,\"Keywords\":null,\"Title\":\"string\",\"ShortTitle\":\"string\",\"Phone\":\"\\u002B340976894523\",\"Email\":\"user@example.com\",\"Website\":\"string\",\"Facebook\":\"string\",\"Instagram\":\"string\",\"MinAge\":12,\"MaxAge\":20,\"DateTimeRanges\":[{\"Id\":0,\"StartTime\":\"12:00:00\",\"EndTime\":\"14:00:00\",\"Workdays\":[4]}],\"FormOfLearning\":10,\"Price\":100000,\"PayRate\":\"None\",\"AvailableSeats\":10,\"CompetitiveSelection\":true,\"CompetitiveSelectionDescription\":\"string\",\"ProviderId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"ProviderTitle\":\"string\",\"ProviderTitleEn\":\"string\",\"ProviderLicenseStatus\":0}";

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
        result.Should().BeOfType<WorkshopWithRequiredPropertiesDto>();
        Assert.AreEqual(workshopMemento.Title, result.Title);
        Assert.AreEqual(workshopMemento.Email, result.Email);
        Assert.AreEqual(workshopMemento.Phone, result.Phone);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoIsWorkshopWithDescriptionDtoAndExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        var workshopMemento = FakeMemento(jsonStringForWorkshopWithDescriptionDto);

        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", jsonStringForWorkshopWithDescriptionDto },
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
        Assert.AreEqual(
            (workshopMemento as WorkshopWithDescriptionDto).WorkshopDescriptionItems.First().Id,
            (result as WorkshopWithDescriptionDto).WorkshopDescriptionItems.First().Id);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoIsWorkshopWithContactsDtoAndExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        var workshopMemento = FakeMemento(jsonStringForWorkshopWithContactsDto);

        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", jsonStringForWorkshopWithContactsDto },
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
        Assert.AreEqual(
            (workshopMemento as WorkshopWithContactsDto).WorkshopDescriptionItems.First().Id,
            (result as WorkshopWithContactsDto).WorkshopDescriptionItems.First().Id);
        Assert.AreEqual(
            (workshopMemento as WorkshopWithContactsDto).Address.Street,
            (result as WorkshopWithContactsDto).Address.Street);
        Assert.AreEqual(
            (workshopMemento as WorkshopWithContactsDto).AddressId,
            (result as WorkshopWithContactsDto).AddressId);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoIsWorkshopWithTeachersDtoAndExistsInCache_ShouldRestoreAppropriatedEntity()
    {
        var workshopMemento = FakeMemento(jsonStringForWorkshopWithTeachersDto);

        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", jsonStringForWorkshopWithTeachersDto },
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
        Assert.AreEqual(
            (workshopMemento as WorkshopWithTeachersDto).WorkshopDescriptionItems.First().Id,
            (result as WorkshopWithTeachersDto).WorkshopDescriptionItems.First().Id);
        Assert.AreEqual(
            (workshopMemento as WorkshopWithTeachersDto).Address.Street,
            (result as WorkshopWithTeachersDto).Address.Street);
        Assert.AreEqual(
            (workshopMemento as WorkshopWithTeachersDto).AddressId,
            (result as WorkshopWithTeachersDto).AddressId);
        Assert.AreEqual(
            (workshopMemento as WorkshopWithTeachersDto).Teachers,
            (result as WorkshopWithTeachersDto).Teachers);
    }

    [Test]
    public async Task RestoreAsync_WhenMementoIsAbsentInCache_ShouldRestoreDefaultEntity()
    {
        // Arrange
        var expectedMemento = default(WorkshopWithRequiredPropertiesDto);
        var expected = new Dictionary<string, string>()
            {
                {"ExpectedKey", null},
            };
        readWriteCacheServiceMock.Setup(c => c.ReadAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(expected["ExpectedKey"]));

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

    private WorkshopWithRequiredPropertiesDto FakeMemento(string jsonString)
    {
        return JsonSerializer.Deserialize<WorkshopWithRequiredPropertiesDto>(jsonString);
    }
}
