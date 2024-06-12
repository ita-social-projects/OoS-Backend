using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ProvidersInfo;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ExternalExportProviderControllerTests
{
    private ExternalExportProviderController controller;
    private Mock<IExternalExportProviderService> mockExternalProviderService;
    private IMapper mapper;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        mockExternalProviderService = new Mock<IExternalExportProviderService>();
        controller = new ExternalExportProviderController(mockExternalProviderService.Object);
    }

    [Test]
    public async Task GetByFilter_ReturnsOkWithResults()
    {
        // Arrange
        var fakeProviders = ProvidersGenerator.Generate(5).WithWorkshops();

        _ = mockExternalProviderService
            .Setup(x => x.GetProvidersWithWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<ProviderInfoBaseDto> { Entities = mapper.Map<List<ProviderInfoBaseDto>>(fakeProviders) });

        // Act
        var actionResult = await controller.GetByFilter(DateTime.UtcNow, new OffsetFilter { Size = 10 });

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okObjectResult = (OkObjectResult)actionResult;
        Assert.IsInstanceOf<SearchResult<ProviderInfoBaseDto>>(okObjectResult.Value);
        var result = (SearchResult<ProviderInfoBaseDto>)okObjectResult.Value;
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
    }

    [Test]
    public async Task GetByFilter_ReturnsNoContent()
    {
        // Arrange
        mockExternalProviderService
            .Setup(x => x.GetProvidersWithWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ReturnsAsync(new SearchResult<ProviderInfoBaseDto> { Entities = new List<ProviderInfoBaseDto>() });

        // Act
        var actionResult = await controller.GetByFilter(DateTime.UtcNow, new OffsetFilter { Size = 10 });

        // Assert
        Assert.IsInstanceOf<NoContentResult>(actionResult);
    }

    [Test]
    public async Task GetByFilter_ExceptionInService_ReturnsInternalServerError()
    {
        // Arrange
        mockExternalProviderService
            .Setup(x => x.GetProvidersWithWorkshops(It.IsAny<DateTime>(), It.IsAny<OffsetFilter>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        // Act
        var actionResult = await controller.GetByFilter(DateTime.UtcNow, new OffsetFilter { Size = 10 });

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var objectResult = (ObjectResult)actionResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("An error occurred: Simulated exception", objectResult.Value);
    }
}
