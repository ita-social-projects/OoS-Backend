using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.WebApi.Controllers.V2;

namespace OutOfSchool.WebApi.Tests.Controllers;
public class CompetitiveEventsV2ControllerTests
{
    private Mock<ICompetitiveEventServiceV2> competitiveEventServiceMock;
    private Mock<IUserService> userServiceMock;
    private Mock<ILogger<CompetitiveEventController>> loggerMock;
    private CompetitiveEventController controller;
    private Mock<ICompetitiveEventDraftService> competitiveEventDraftServiceMock;
    private Mock<IProviderService> providerServiceMock;

    [SetUp]
    public void Setup()
    {
        competitiveEventServiceMock = new Mock<ICompetitiveEventServiceV2>();
        userServiceMock = new Mock<IUserService>();
        loggerMock = new Mock<ILogger<CompetitiveEventController>>();
        competitiveEventDraftServiceMock = new Mock<ICompetitiveEventDraftService>();
        providerServiceMock = new Mock<IProviderService>();

        controller = new CompetitiveEventController(
            competitiveEventServiceMock.Object,
            userServiceMock.Object,
            loggerMock.Object,
            competitiveEventDraftServiceMock.Object,
            providerServiceMock.Object);
    }

    #region V2 tests

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
        var filter = new ExcludeIdFilter();
        var resultDto = new SearchResult<CompetitiveEventViewCardDto> { TotalAmount = 1, Entities = new List<CompetitiveEventViewCardDto> { new() } };

        competitiveEventServiceMock.Setup(s => s.GetByProviderId(id, filter)).ReturnsAsync(resultDto);

        var result = await controller.GetByProviderId(id, filter);

        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetByProviderId_ReturnsNoContent_WhenNoResultsFound()
    {
        var id = Guid.NewGuid();
        var filter = new ExcludeIdFilter();
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
        var dto = new CompetitiveEventV2CreateRequestDto();
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
        var dto = new CompetitiveEventV2CreateRequestDto();
        userServiceMock.Setup(s => s.IsBlocked(It.IsAny<string>())).ReturnsAsync(false);
        competitiveEventServiceMock.Setup(s => s.CreateV2(dto)).ThrowsAsync(new InvalidOperationException("error"));

        var result = await controller.Create(dto);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("error"));
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        var dto = new CompetitiveEventV2CreateRequestDto();
        var expectedId = Guid.NewGuid();
        var resultDto = new CompetitiveEventResultDto
        {
            CompetitiveEventV2 = new CompetitiveEventV2Dto { Id = expectedId }
        };

        competitiveEventServiceMock.Setup(s => s.UpdateV2(dto)).ReturnsAsync(resultDto);

        var result = await controller.Update(dto);

        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        var responseDto = okResult.Value as CompetitiveEventResponseDto;
        Assert.That(responseDto, Is.Not.Null);
        Assert.That(responseDto.CompetitiveEventV2.Id, Is.EqualTo(expectedId));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenResultIsNull()
    {
        var dto = new CompetitiveEventV2CreateRequestDto();

        competitiveEventServiceMock.Setup(s => s.UpdateV2(dto))
            .ReturnsAsync(new CompetitiveEventResultDto { CompetitiveEventV2 = null });

        var result = await controller.Update(dto);

        Assert.IsInstanceOf<BadRequestResult>(result);
        var badRequest = result as BadRequestResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsInvalidOperation()
    {
        var dto = new CompetitiveEventV2CreateRequestDto();

        competitiveEventServiceMock.Setup(s => s.UpdateV2(dto))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var result = await controller.Update(dto);

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

    #endregion
}
