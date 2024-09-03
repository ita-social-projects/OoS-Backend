using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopMementoControllerTests
{
    private WorkshopMementoController controller;
    private Mock<IMementoService<WorkshopWithRequiredPropertiesDto>> mementoService;
    private ClaimsPrincipal user;

    private IEnumerable<WorkshopWithRequiredPropertiesDto> mementos;
    private WorkshopWithRequiredPropertiesDto memento;

    [SetUp]
    public void Setup()
    {
        mementoService = new Mock<IMementoService<WorkshopWithRequiredPropertiesDto>>();
        controller = new WorkshopMementoController(mementoService.Object);
        user = new ClaimsPrincipal(new ClaimsIdentity());
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        memento = FakeMemento();
    }

    [Test]
    public async Task StoreMemento_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        mementoService.Setup(ms => ms.CreateAsync(It.IsAny<string>(), memento));

        // Act
        var result = await controller.StoreMemento(memento);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().NotBe(default(WorkshopWithRequiredPropertiesDto));
    }

    [Test]
    public async Task StoreMemento_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        controller.ModelState.AddModelError("StoreMemento", "Invalid model state.");

        // Act
        var result = await controller.StoreMemento(memento);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task RestoreMemento_WhenMementoExistsInCache_ReturnsMementoAtActionResult()
    {
        // Arrange
        mementoService.Setup(rm => rm.RestoreAsync(It.IsAny<string>())).ReturnsAsync(memento);

        // Act
        var result = await controller.RestoreMemento();

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().NotBe(default(string));
    }

    [Test]
    public async Task RestoreMemento_WhenMementoIsAbsentInCache_ReturnsDefaultMementoAtActionResult()
    {
        // Arrange
        mementoService.Setup(ms => ms.RestoreAsync(It.IsAny<string>())).ReturnsAsync(default(WorkshopWithRequiredPropertiesDto));

        // Act
        var result = await controller.RestoreMemento();

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(default(string));
    }

    private WorkshopWithRequiredPropertiesDto FakeMemento()
    {
        return new WorkshopWithRequiredPropertiesDto()
        {
            Title = "title1",
            Email = "myemail1@gmail.com",
            Phone = "+380670000001",
        };
    }
}