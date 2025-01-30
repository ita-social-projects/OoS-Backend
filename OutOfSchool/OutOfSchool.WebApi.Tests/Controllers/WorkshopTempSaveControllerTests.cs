using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
using OutOfSchool.BusinessLogic.Services.TempSave;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopTempSaveControllerTests
{
    private readonly string userId = "someUserId";
    private string key;
    private Mock<ITempSaveService<WorkshopMainRequiredPropertiesDto>> tempSaveService;
    private WorkshopTempSaveController controller;
    private ClaimsPrincipal user;
    private WorkshopMainRequiredPropertiesDto baseDto;
    private WorkshopRequiredPropertiesDto derivedDto;

    [SetUp]
    public void Setup()
    {
        user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId)]));
        baseDto = GetBaseWorkshopDtoFakeDraft();
        derivedDto = GetDerivedWorkshopDtoFakeDraft();
        tempSaveService = new Mock<ITempSaveService<WorkshopMainRequiredPropertiesDto>>();
        controller = new WorkshopTempSaveController(tempSaveService.Object);
        key = GettingUserProperties.GetUserId(user);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }

    [Test]
    public async Task Store_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.StoreAsync(key, baseDto))
            .Verifiable(Times.Once);
        var resultValue = $"{baseDto.GetType().Name} is stored";

        // Act
        var result = await controller.Store(baseDto).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(resultValue);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task StoreDerivedDto_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.StoreAsync(key, derivedDto))
            .Verifiable(Times.Once);
        var resultValue = $"{derivedDto.GetType().Name} is stored";

        // Act
        var result = await controller.Store(derivedDto).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(resultValue);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task Store_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var errorKey = "TempSave";
        var errorMessage = "Invalid model state";
        controller.ModelState.AddModelError(errorKey, errorMessage);
        tempSaveService.Setup(ds => ds.StoreAsync(key, baseDto))
            .Verifiable(Times.Never);

        // Act
        var result = await controller.Store(baseDto).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status400BadRequest);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task StoreDerivedDto_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var errorKey = "TempSave";
        var errorMessage = "Invalid model state";
        controller.ModelState.AddModelError(errorKey, errorMessage);
        tempSaveService.Setup(ds => ds.StoreAsync(key, derivedDto))
            .Verifiable(Times.Never);

        // Act
        var result = await controller.Store(derivedDto).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status400BadRequest);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task Restore_WhenDtoValueExistsInCache_ReturnsDtoAtActionResult()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(baseDto)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.Restore().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
             .BeOfType<OkObjectResult>()
             .Which.Value.Should().Be(baseDto);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task RestoreDerivedDto_WhenDtoValueExistsInCache_ReturnsDerivedDtoAtActionResult()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(derivedDto)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.Restore().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
             .BeOfType<OkObjectResult>()
             .Which.Value.Should().Be(derivedDto);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task Restore_WhenDtoValueIsAbsentInCache_ReturnsDefaultDtoAtActionResult()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(default(WorkshopMainRequiredPropertiesDto))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.Restore().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task RestoreDrivedDto_WhenDtoValueIsAbsentInCache_ReturnsDefaultDrivedDtoAtActionResult()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(default(WorkshopRequiredPropertiesDto))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.Restore().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task GetTimeToLive_WhenDtoValueExistsInCache_ReturnsDtoAtActionResult()
    {
        // Arrange
        TimeSpan? timeToLive = TimeSpan.FromMinutes(1);
        tempSaveService.Setup(ds => ds.GetTimeToLiveAsync(key))
            .ReturnsAsync(timeToLive)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.GetTimeToLive().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
             .BeOfType<OkObjectResult>()
             .Which.Value.Should().Be(timeToLive);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task GetTimeToLive_WhenDtoValueIsAbsentInCache_ReturnsDefaultDtoAtActionResult()
    {
        // Arrange
        TimeSpan? timeToLive = null;
        tempSaveService.Setup(ds => ds.GetTimeToLiveAsync(key))
            .ReturnsAsync(timeToLive)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.GetTimeToLive().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        tempSaveService.VerifyAll();
    }

    [Test]
    public async Task Remove_ReturnsStatus204NoContent()
    {
        // Arrange
        tempSaveService.Setup(ds => ds.RemoveAsync(key))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.Remove().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        tempSaveService.VerifyAll();
    }

    private static WorkshopMainRequiredPropertiesDto GetBaseWorkshopDtoFakeDraft() =>
        WorkshopMainRequiredPropertiesDtoGenerator.Generate();

    private static WorkshopRequiredPropertiesDto GetDerivedWorkshopDtoFakeDraft() =>
        WorkshopRequiredPropertiesDtoGenerator.Generate();
}