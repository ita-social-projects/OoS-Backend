using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ExternalExportControllerTests
{
    private ExternalExportController controller;
    private Mock<IExternalExportService> mockExternalExportService;
    private IMapper mapper;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile, ExternalExportMappingProfile>();
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
                {TotalAmount = fakeProviders.Count, Entities = mapper.Map<List<ProviderInfoBaseDto>>(fakeProviders)});

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

        _ = mockExternalExportService
            .Setup(x => x.GetWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<WorkshopInfoBaseDto>
                {TotalAmount = fakeWorkshops.Count, Entities = mapper.Map<List<WorkshopInfoBaseDto>>(fakeWorkshops)});

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
                Id = Guid.Parse("b7e1322e-7575-48c1-a444-4effb8f4d083"),
                Title = "A",
                DirectionIds = []
            },
            new()
            {
                Id = Guid.Parse("a042661d-9be8-4bfb-adcd-06cbe91388a0"),
                Title = "B",
                DirectionIds = [1, 2]
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