using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class CompetitiveEventAccountingTypeControllerTests
{
    private CompetitiveEventAccountingTypeController controller;
    private Mock<ICompetitiveEventAccountingTypeService> service;
    private IMapper mapper;

    [SetUp]
    public void Setup()
    {
        service = new Mock<ICompetitiveEventAccountingTypeService>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        controller = new CompetitiveEventAccountingTypeController(service.Object);
    }

    [Test]
    public async Task GetAll_WhenValidLocalization_ReturnsOkWithResult()
    {
        // Arrange
        var localization = LocalizationType.Ua;
        var accountingTypesDto = FakeCompetitiveEventAccountingTypesDto();

        service.Setup(s => s.GetAll(localization)).ReturnsAsync(accountingTypesDto);

        // Act
        var result = await controller.GetAll();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(accountingTypesDto, okResult.Value);
    }

    [Test]
    public async Task GetAll_WhenEmptyCollection_ReturnsNoContent()
    {
        // Arrange
        service.Setup(s => s.GetAll(It.IsAny<LocalizationType>()))
            .ReturnsAsync(new List<CompetitiveEventAccountingTypeDto>());

        // Act
        var result = await controller.GetAll().ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    private IEnumerable<CompetitiveEventAccountingTypeDto> FakeCompetitiveEventAccountingTypesDto()
    {
        return new List<CompetitiveEventAccountingTypeDto>()
        {
            new CompetitiveEventAccountingTypeDto { Id = 1, Title = "Тип 1"},
            new CompetitiveEventAccountingTypeDto { Id = 2, Title = "Тип 2"},
        };
    }
}