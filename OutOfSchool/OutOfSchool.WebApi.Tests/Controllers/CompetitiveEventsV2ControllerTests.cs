using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.WebApi.Controllers.V2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Controllers;
public class CompetitiveEventsV2ControllerTests
{
    private Mock<ICompetitiveEventServiceV2> competitiveEventServiceMock;
    private Mock<IUserService> userServiceMock;
    private Mock<ILogger<CompetitiveEventController>> loggerMock;
    private CompetitiveEventController controller;
    private Mock<ICompetitiveEventDraftService> competitiveEventDraftServiceMock;

    [SetUp]
    public void Setup()
    {
        competitiveEventServiceMock = new Mock<ICompetitiveEventServiceV2>();
        userServiceMock = new Mock<IUserService>();
        loggerMock = new Mock<ILogger<CompetitiveEventController>>();
        competitiveEventDraftServiceMock = new Mock<ICompetitiveEventDraftService>();

        controller = new CompetitiveEventController(
            competitiveEventServiceMock.Object,
            userServiceMock.Object,
            loggerMock.Object,
            competitiveEventDraftServiceMock.Object);
    }

    #region GetById

    [Test]
    public async Task GetById_ReturnsOk_WhenEntityExists()
    {
        var id = Guid.NewGuid();
        var dto = new CompetitiveEventV2Dto { Id = id };
        competitiveEventServiceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var result = await controller.GetById(id);

        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetById_ReturnsNotFound_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        competitiveEventServiceMock.Setup(s => s.GetById(id)).ReturnsAsync((CompetitiveEventV2Dto)null);

        var result = await controller.GetById(id);

        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    #endregion

    #region GetByProviderId

    [Test]
    public async Task GetByProviderId_ReturnsOk_WhenResultsExist()
    {
        var id = Guid.NewGuid();
        var filter = new CompetitiveEventFilterTitle();
        var resultDto = new SearchResult<CompetitiveEventViewCardDto> { TotalAmount = 1, Entities = new List<CompetitiveEventViewCardDto> { new() } };

        competitiveEventServiceMock.Setup(s => s.GetByProviderId(id, filter)).ReturnsAsync(resultDto);

        var result = await controller.GetByProviderId(id, filter);

        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetByProviderId_ReturnsNoContent_WhenNoResultsFound()
    {
        var id = Guid.NewGuid();
        var filter = new CompetitiveEventFilterTitle();
        var emptyResult = new SearchResult<CompetitiveEventViewCardDto> { TotalAmount = 0, Entities = new List<CompetitiveEventViewCardDto>() };

        competitiveEventServiceMock.Setup(s => s.GetByProviderId(id, filter)).ReturnsAsync(emptyResult);

        var result = await controller.GetByProviderId(id, filter);

        Assert.IsInstanceOf<NoContentResult>(result);
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_ReturnsCreated_WhenSuccessful()
    {
        var dto = new CompetitiveEventV2CreateRequestDto()
        {
            ImageFiles = [Mock.Of<IFormFile>()],
            CoverImage = Mock.Of<IFormFile>()
        };
        var expectedId = Guid.NewGuid();
        var resultDto = new CompetitiveEventResultDto
        {
            CompetitiveEventV2 = new CompetitiveEventV2Dto { Id = expectedId }
        };

        userServiceMock.Setup(s => s.IsBlocked(It.IsAny<string>())).ReturnsAsync(false);
        competitiveEventServiceMock.Setup(s => s.CreateV2(dto)).ReturnsAsync(resultDto);

        var result = await controller.Create(dto);

        Assert.IsInstanceOf<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(controller.GetById)));
        var value = createdResult.Value as CompetitiveEventResponseDto;
        Assert.That(value, Is.Not.Null);
        Assert.That(value.CompetitiveEventV2.Id, Is.EqualTo(expectedId));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenDtoIsNull()
    {
        var result = await controller.Create(null);
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("CompetitiveEvent is null"));
    }

    [Test]
    public async Task Create_ReturnsForbidden_WhenUserIsBlocked()
    {
        var dto = new CompetitiveEventV2CreateRequestDto();
        userServiceMock.Setup(s => s.IsBlocked(It.IsAny<string>())).ReturnsAsync(true);

        var result = await controller.Create(dto);

        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(403));
        Assert.That(objectResult.Value.ToString(), Does.Contain("User is blocked"));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenServiceThrowsInvalidOperation()
    {
        var dto = new CompetitiveEventV2CreateRequestDto
        {
            ImageFiles = [Mock.Of<IFormFile>()],
            CoverImage = Mock.Of<IFormFile>()
        };

        userServiceMock.Setup(s => s.IsBlocked(It.IsAny<string>())).ReturnsAsync(false);
        competitiveEventServiceMock.Setup(s => s.CreateV2(dto)).ThrowsAsync(new InvalidOperationException("error"));

        var result = await controller.Create(dto);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("error"));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenImageFilesIsNullOrEmptyArray()
    {
        // Arrange
        var dto = new CompetitiveEventV2CreateRequestDto();
        userServiceMock.Setup(s => s.IsBlocked(It.IsAny<string>())).ReturnsAsync(false);
        competitiveEventServiceMock.Setup(s => s.CreateV2(dto)).ThrowsAsync(new InvalidOperationException("error"));

        // Act
        var result = await controller.Create(dto);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("When creating CompetitiveEvent, the ImageFiles field must contain a non-empty array of images, and the ImageIds field must be null or an empty array."));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenImageIdsHasValues()
    {
        // Arrange
        var dto = new CompetitiveEventV2CreateRequestDto() { ImageIds = ["image"] };
        userServiceMock.Setup(s => s.IsBlocked(It.IsAny<string>())).ReturnsAsync(false);
        competitiveEventServiceMock.Setup(s => s.CreateV2(dto)).ThrowsAsync(new InvalidOperationException("error"));

        // Act
        var result = await controller.Create(dto);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("When creating CompetitiveEvent, the ImageFiles field must contain a non-empty array of images, and the ImageIds field must be null or an empty array."));
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto();
        var expectedId = Guid.NewGuid();
        var resultDto = new CompetitiveEventV2Dto { Id = expectedId };
        competitiveEventDraftServiceMock.Setup(s => s.UpdateCompetitiveEvent(dto)).ReturnsAsync(resultDto);

        // Act
        var result = await controller.Update(dto);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        var responseDto = okResult.Value as CompetitiveEventV2Dto;
        Assert.That(responseDto, Is.Not.Null);
        Assert.That(responseDto.Id, Is.EqualTo(expectedId));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenResultIsNull()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto();
        var resultDto = (CompetitiveEventV2Dto)null;
        competitiveEventDraftServiceMock.Setup(s => s.UpdateCompetitiveEvent(dto)).ReturnsAsync(resultDto);

        // Act
        var result = await controller.Update(dto);

        // Assert
        Assert.IsInstanceOf<BadRequestResult>(result);
        var badRequest = result as BadRequestResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto();
        competitiveEventDraftServiceMock.Setup(s => s.UpdateCompetitiveEvent(dto))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await controller.Update(dto);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("Service error"));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsNoContent_WhenEntityExists()
    {
        var id = Guid.NewGuid();
        var dto = new CompetitiveEventV2Dto { Id = id };
        competitiveEventServiceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var result = await controller.Delete(id);

        Assert.IsInstanceOf<NoContentResult>(result);
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        competitiveEventServiceMock.Setup(s => s.GetById(id)).ReturnsAsync((CompetitiveEventV2Dto)null);

        var result = await controller.Delete(id);

        Assert.IsInstanceOf<NoContentResult>(result);
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_ReturnsBadRequest_WhenServiceThrows()
    {
        var id = Guid.NewGuid();
        var dto = new CompetitiveEventV2Dto { Id = id };
        competitiveEventServiceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);
        competitiveEventServiceMock.Setup(s => s.DeleteV2(id)).ThrowsAsync(new InvalidOperationException("Test error"));

        var result = await controller.Delete(id);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("Test error"));
    }

    #endregion
}
