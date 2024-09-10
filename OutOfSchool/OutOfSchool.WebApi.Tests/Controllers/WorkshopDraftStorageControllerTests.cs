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
public class WorkshopDraftStorageControllerTests
{
    private Mock<IDraftStorageService<WorkshopWithRequiredPropertiesDto>> draftStorageService;
    private WorkshopDraftStorageController controller;
    private ClaimsPrincipal user;
    private WorkshopWithRequiredPropertiesDto draft;

    [SetUp]
    public void Setup()
    {
        draftStorageService = new Mock<IDraftStorageService<WorkshopWithRequiredPropertiesDto>>();
        controller = new WorkshopDraftStorageController(draftStorageService.Object);
        user = new ClaimsPrincipal(new ClaimsIdentity());
        draft = FakeDraft();
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }

    [Test]
    public async Task StoreDraft_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        draftStorageService.Setup(ms => ms.CreateAsync(It.IsAny<string>(), draft));
        var resulValue = "WorkshopWithRequiredPropertiesDto is stored";

        // Act
        var result = await controller.StoreDraft(draft);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(resulValue);
    }

    [Test]
    public async Task StoreDraft_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        controller.ModelState.AddModelError("StoreMemento", "Invalid model state.");
        var resulValue = "{[\"StoreMemento\"] = {\"Invalid model state.\"}}";

        // Act
        var result = await controller.StoreDraft(draft);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.Value.Equals(resulValue);
    }

    [Test]
    public async Task RestoreDraft_WhenDraftExistsInCache_ReturnsMementoAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(rm => rm.RestoreAsync(It.IsAny<string>())).ReturnsAsync(draft);

        // Act
        var result = await controller.RestoreDraft();

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(draft);
    }

    [Test]
    public async Task RestoreDraft_WhenMementoIsAbsentInCache_ReturnsDefaultMementoAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(ms => ms.RestoreAsync(It.IsAny<string>())).ReturnsAsync(default(WorkshopWithRequiredPropertiesDto));

        // Act
        var result = await controller.RestoreDraft();

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(default(WorkshopWithRequiredPropertiesDto));
    }

    private WorkshopWithRequiredPropertiesDto FakeDraft()
    {
        return new WorkshopWithRequiredPropertiesDto()
        {
            Title = "title1",
            Email = "myemail1@gmail.com",
            Phone = "+380670000001",
        };
    }
}