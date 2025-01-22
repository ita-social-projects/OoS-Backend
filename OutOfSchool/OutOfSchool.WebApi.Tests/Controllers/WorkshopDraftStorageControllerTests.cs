using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Workshops.Drafts;
using OutOfSchool.BusinessLogic.Services.DraftStorage;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopDraftStorageControllerTests
{
    private readonly string userId = "someUserId";
    private string key;
    private Mock<IDraftStorageService<WorkshopMainRequiredPropertiesDto>> draftStorageService;
    private WorkshopDraftStorageController controller;
    private ClaimsPrincipal user;
    private WorkshopMainRequiredPropertiesDto baseDtoDraft;
    private WorkshopRequiredPropertiesDto derivedDtoDraft;

    [SetUp]
    public void Setup()
    {
        user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId)]));
        baseDtoDraft = GetBaseWorkshopDtoFakeDraft();
        derivedDtoDraft = GetDerivedWorkshopDtoFakeDraft();
        draftStorageService = new Mock<IDraftStorageService<WorkshopMainRequiredPropertiesDto>>();
        controller = new WorkshopDraftStorageController(draftStorageService.Object);
        key = GettingUserProperties.GetUserId(user);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }

    [Test]
    public async Task StoreDraft_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.CreateAsync(key, baseDtoDraft))
            .Verifiable(Times.Once);
        var resultValue = $"{baseDtoDraft.GetType().Name} is stored";

        // Act
        var result = await controller.StoreDraft(baseDtoDraft).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(resultValue);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task StoreDerivedDraft_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.CreateAsync(key, derivedDtoDraft))
            .Verifiable(Times.Once);
        var resultValue = $"{derivedDtoDraft.GetType().Name} is stored";

        // Act
        var result = await controller.StoreDraft(derivedDtoDraft).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(resultValue);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task StoreDraft_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var errorKey = "DraftStorage";
        var errorMessage = "Invalid model state";
        controller.ModelState.AddModelError(errorKey, errorMessage);
        draftStorageService.Setup(ds => ds.CreateAsync(key, baseDtoDraft))
            .Verifiable(Times.Never);

        // Act
        var result = await controller.StoreDraft(baseDtoDraft).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status400BadRequest);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task StoreDerivedDraft_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var errorKey = "DraftStorage";
        var errorMessage = "Invalid model state";
        controller.ModelState.AddModelError(errorKey, errorMessage);
        draftStorageService.Setup(ds => ds.CreateAsync(key, derivedDtoDraft))
            .Verifiable(Times.Never);

        // Act
        var result = await controller.StoreDraft(derivedDtoDraft).ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status400BadRequest);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task RestoreDraft_WhenDraftExistsInCache_ReturnsDraftAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(baseDtoDraft)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.RestoreDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
             .BeOfType<OkObjectResult>()
             .Which.Value.Should().Be(baseDtoDraft);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task RestoreDerivedDraft_WhenDraftExistsInCache_ReturnsDerivedDraftAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(derivedDtoDraft)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.RestoreDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
             .BeOfType<OkObjectResult>()
             .Which.Value.Should().Be(derivedDtoDraft);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task RestoreDraft_WhenDraftIsAbsentInCache_ReturnsDefaultDraftAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(default(WorkshopMainRequiredPropertiesDto))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.RestoreDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task RestoreDrivedDraft_WhenDraftIsAbsentInCache_ReturnsDefaultDrivedDraftAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(default(WorkshopRequiredPropertiesDto))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.RestoreDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task GetTimeToLiveOfDraft_WhenDraftExistsInCache_ReturnsDraftAtActionResult()
    {
        // Arrange
        TimeSpan? timeToLive = TimeSpan.FromMinutes(1);
        draftStorageService.Setup(ds => ds.GetTimeToLiveAsync(key))
            .ReturnsAsync(timeToLive)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.GetTimeToLiveOfDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
             .BeOfType<OkObjectResult>()
             .Which.Value.Should().Be(timeToLive);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task GetTimeToLiveOfDraft_WhenDraftIsAbsentInCache_ReturnsDefaultDraftAtActionResult()
    {
        // Arrange
        TimeSpan? timeToLive = null;
        draftStorageService.Setup(ds => ds.GetTimeToLiveAsync(key))
            .ReturnsAsync(timeToLive)
            .Verifiable(Times.Once);

        // Act
        var result = await controller.GetTimeToLiveOfDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task RemoveDraft_ReturnsStatus204NoContent()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.RemoveAsync(key))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.RemoveDraft().ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
        draftStorageService.VerifyAll();
    }

    private static WorkshopMainRequiredPropertiesDto GetBaseWorkshopDtoFakeDraft() =>
        WorkshopMainRequiredPropertiesDtoGenerator.Generate();

    private static WorkshopRequiredPropertiesDto GetDerivedWorkshopDtoFakeDraft() =>
        WorkshopRequiredPropertiesDtoGenerator.Generate();
}