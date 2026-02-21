using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ExternalExportControllerTests
{
    private ExternalExportController controller;
    private Mock<IExternalExportService> mockExternalExportService;

    [SetUp]
    public void Setup()
    {
        mockExternalExportService = new Mock<IExternalExportService>();
        controller = new ExternalExportController(mockExternalExportService.Object);
    }

    [Test]
    public async Task GetProviderByFilter_ReturnsOkWithResults()
    {
        // Arrange
        var fakeProviders = ProvidersGenerator.Generate(5).WithWorkshops();

        _ = mockExternalExportService
            .Setup(x => x.GetProviders(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<ProviderInfoBaseDto>
                {TotalAmount = fakeProviders.Count, Entities = fakeProviders.ToBaseOrInfoDto()});

        // Act
        var actionResult = await controller.GetProvidersByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okObjectResult = (OkObjectResult) actionResult;
        Assert.IsInstanceOf<SearchResult<ProviderInfoBaseDto>>(okObjectResult.Value);
        var result = (SearchResult<ProviderInfoBaseDto>) okObjectResult.Value;
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
    }

    [Test]
    public async Task GetProviderByFilter_ReturnsNoContent()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetProviders(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<ProviderInfoBaseDto> {Entities = new List<ProviderInfoBaseDto>()});

        // Act
        var actionResult = await controller.GetProvidersByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<NoContentResult>(actionResult);
    }

    [Test]
    public async Task GetProviderByFilter_ExceptionInService_ReturnsInternalServerError()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetProviders(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        // Act
        var actionResult = await controller.GetProvidersByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var objectResult = (ObjectResult) actionResult;
        Assert.AreEqual(500, objectResult.StatusCode);
    }

    [Test]
    public async Task GetWorkshopByFilter_ReturnsOkWithResults()
    {
        // Arrange
        var fakeWorkshops = WorkshopGenerator.Generate(5);
        fakeWorkshops.ForEach(w => w.LanguageOfEducation = new ()
        {
            Name = "test"
        });

        _ = mockExternalExportService
            .Setup(x => x.GetWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<WorkshopInfoBaseDto>
                {TotalAmount = fakeWorkshops.Count, Entities = fakeWorkshops.ToBaseOrInfoDto()});

        // Act
        var actionResult = await controller.GetWorkshopsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okObjectResult = (OkObjectResult) actionResult;
        Assert.IsInstanceOf<SearchResult<WorkshopInfoBaseDto>>(okObjectResult.Value);
        var result = (SearchResult<WorkshopInfoBaseDto>) okObjectResult.Value;
        Assert.AreEqual(fakeWorkshops.Count, result.Entities.Count);
    }

    [Test]
    public async Task GetWorkshopByFilter_ReturnsNoContent()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<WorkshopInfoBaseDto> {Entities = new List<WorkshopInfoBaseDto>()});

        // Act
        var actionResult = await controller.GetWorkshopsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<NoContentResult>(actionResult);
    }

    [Test]
    public async Task GetWorkshopByFilter_ExceptionInService_ReturnsInternalServerError()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        // Act
        var actionResult = await controller.GetWorkshopsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var objectResult = (ObjectResult) actionResult;
        Assert.AreEqual(500, objectResult.StatusCode);
    }
    
    [Test]
    public async Task GetDirectionsByFilter_ReturnsOkWithResults()
    {
        // Arrange
        var fakeDirections = new List<DirectionInfoDto>
        {
            new()
            {
                Id = 1,
                Title = "A"
            },
            new()
            {
                Id = 1,
                Title = "B"
            }
        };

        _ = mockExternalExportService
            .Setup(x => x.GetDirections(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<DirectionInfoBaseDto>
                {TotalAmount = fakeDirections.Count, Entities = fakeDirections});

        // Act
        var actionResult = await controller.GetDirectionsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okObjectResult = (OkObjectResult) actionResult;
        Assert.IsInstanceOf<SearchResult<DirectionInfoBaseDto>>(okObjectResult.Value);
        var result = (SearchResult<DirectionInfoBaseDto>) okObjectResult.Value;
        Assert.AreEqual(fakeDirections.Count, result.Entities.Count);
    }

    [Test]
    public async Task GetDirectionsByFilter_ReturnsNoContent()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetDirections(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<DirectionInfoBaseDto> {Entities = new List<DirectionInfoDto>()});

        // Act
        var actionResult = await controller.GetDirectionsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<NoContentResult>(actionResult);
    }

    [Test]
    public async Task GetDirectionsByFilter_ExceptionInService_ReturnsInternalServerError()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetDirections(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        // Act
        var actionResult = await controller.GetDirectionsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var objectResult = (ObjectResult) actionResult;
        Assert.AreEqual(500, objectResult.StatusCode);
    }
    
    [Test]
    public async Task GetSubDirectionsByFilter_ReturnsOkWithResults()
    {
        // Arrange
        var fakeSubDirections = new List<SubDirectionsInfoDto>
        {
            new()
            {
                Id = 1,
                Title = "A",
                DirectionId = 1
            },
            new()
            {
                Id = 2,
                Title = "B",
                DirectionId = 2
            },
        };

        _ = mockExternalExportService
            .Setup(x => x.GetSubDirections(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<SubDirectionsInfoBaseDto>
                {TotalAmount = fakeSubDirections.Count, Entities = fakeSubDirections});

        // Act
        var actionResult = await controller.GetSubDirectionsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okObjectResult = (OkObjectResult) actionResult;
        Assert.IsInstanceOf<SearchResult<SubDirectionsInfoBaseDto>>(okObjectResult.Value);
        var result = (SearchResult<SubDirectionsInfoBaseDto>) okObjectResult.Value;
        Assert.AreEqual(fakeSubDirections.Count, result.Entities.Count);
    }

    [Test]
    public async Task GetSubDirectionsByFilter_ReturnsNoContent()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetSubDirections(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<SubDirectionsInfoBaseDto> {Entities = new List<SubDirectionsInfoDto>()});

        // Act
        var actionResult = await controller.GetSubDirectionsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<NoContentResult>(actionResult);
    }

    [Test]
    public async Task GetSubDirectionsByFilter_ExceptionInService_ReturnsInternalServerError()
    {
        // Arrange
        mockExternalExportService
            .Setup(x => x.GetSubDirections(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        // Act
        var actionResult = await controller.GetSubDirectionsByFilter(DateTime.UtcNow, new OffsetFilter {Size = 10});

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var objectResult = (ObjectResult) actionResult;
        Assert.AreEqual(500, objectResult.StatusCode);
    }
}