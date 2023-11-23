using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ProvidersInfo;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

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
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        mockExternalProviderService = new Mock<IExternalExportProviderService>();
        controller = new ExternalExportProviderController(mockExternalProviderService.Object);
    }

    [Test]
    public async Task GetByFilter_ReturnsOkWithResults()
    {
        // Arrange
        var fakeProviders = ProvidersGenerator.Generate(5).WithWorkshops();

        _ = mockExternalProviderService
            .Setup(x => x.GetProvidersWithWorkshops(It.IsAny<DateTime>(), It.IsAny<SizeFilter>()))
            .ReturnsAsync(new SearchResult<ProviderInfoBaseDto> { Entities = mapper.Map<List<ProviderInfoBaseDto>>(fakeProviders) });

        // Act
        var actionResult = await controller.GetByFilter(DateTime.UtcNow, new SizeFilter { Size = 10 });

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okObjectResult = (OkObjectResult)actionResult.Result;
        Assert.IsInstanceOf<SearchResult<ProviderInfoBaseDto>>(okObjectResult.Value);
        var result = (SearchResult<ProviderInfoBaseDto>)okObjectResult.Value;
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
    }

    [Test]
    public async Task GetByFilter_ReturnsNoContent()
    {
        // Arrange
        mockExternalProviderService
            .Setup(x => x.GetProvidersWithWorkshops(It.IsAny<DateTime>(), It.IsAny<SizeFilter>()))
            .ReturnsAsync(new SearchResult<ProviderInfoBaseDto> { Entities = new List<ProviderInfoBaseDto>() });

        // Act
        var actionResult = await controller.GetByFilter(DateTime.UtcNow, new SizeFilter { Size = 10 });

        // Assert
        Assert.IsInstanceOf<NoContentResult>(actionResult.Result);
    }

    [Test]
    public async Task GetByFilter_ExceptionInService_ReturnsInternalServerError()
    {
        // Arrange
        mockExternalProviderService
            .Setup(x => x.GetProvidersWithWorkshops(It.IsAny<DateTime>(), It.IsAny<SizeFilter>()))
            .ThrowsAsync(new Exception("Simulated exception"));
        // Act
        var actionResult = await controller.GetByFilter(DateTime.UtcNow, new SizeFilter { Size = 10 });

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result);
        var objectResult = (ObjectResult)actionResult.Result;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("An error occurred: Simulated exception", objectResult.Value);
    }
}
