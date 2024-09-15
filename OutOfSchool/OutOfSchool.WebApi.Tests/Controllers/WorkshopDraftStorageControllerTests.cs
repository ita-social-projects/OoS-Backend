using System.Security.Claims;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.DraftStorage.Interfaces;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopDraftStorageControllerTests
{
    private const int RANDOMSTRINGSIZE = 50;

    private string key;
    private Mock<IDraftStorageService<WorkshopBaseDto>> draftStorageService;
    private WorkshopDraftStorageController controller;
    private ClaimsPrincipal user;
    private WorkshopBaseDto draft;

    [SetUp]
    public void Setup()
    {
        draft = GetWorkshopFakeDraft();
        draftStorageService = new Mock<IDraftStorageService<WorkshopBaseDto>>();
        controller = new WorkshopDraftStorageController(draftStorageService.Object);
        user = new ClaimsPrincipal(new ClaimsIdentity());
        key = GettingUserProperties.GetUserId(user);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }

    [Test]
    public async Task StoreDraft_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.CreateAsync(key, draft))
            .Verifiable(Times.Once);
        var resultValue = $"{draft.GetType().Name} is stored";

        // Act
        var result = await controller.StoreDraft(draft).ConfigureAwait(false);

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
        draftStorageService.Setup(ds => ds.CreateAsync(key, draft))
            .Verifiable(Times.Never);

        // Act
        var result = await controller.StoreDraft(draft).ConfigureAwait(false);

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
            .ReturnsAsync(draft)
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
             .Which.Value.Should().Be(draft);
        draftStorageService.VerifyAll();
    }

    [Test]
    public async Task RestoreDraft_WhenDraftIsAbsentInCache_ReturnsDefaultDraftAtActionResult()
    {
        // Arrange
        draftStorageService.Setup(ds => ds.RestoreAsync(key))
            .ReturnsAsync(default(WorkshopBaseDto))
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
             .Which.Value.Should().Be(default(WorkshopBaseDto));
        draftStorageService.VerifyAll();
    }

    private WorkshopBaseDto GetWorkshopFakeDraft()
    {
        var workshopFacker = new Faker<WorkshopBaseDto>()
            .RuleFor(w => w.Title, f => f.Name.LastName())
            .RuleFor(w => w.Email, f => f.Internet.Email())
            .RuleFor(w => w.Phone, f => f.Phone.PhoneNumber());
        return workshopFacker.Generate();
    }
}