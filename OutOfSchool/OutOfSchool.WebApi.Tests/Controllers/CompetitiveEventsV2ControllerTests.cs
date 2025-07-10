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
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.Services.Common.Exceptions;

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

    #region CreateDraft

    [Test]
    public async Task CreateDraft_ReturnsCreated_WhenSuccessful()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto()
        {
            Id = Guid.NewGuid(),
            OrganizerOfTheEventId = Guid.NewGuid()
        };
        competitiveEventDraftServiceMock.Setup(s => s.Create(dto))
            .ReturnsAsync(new CompetitiveEventDraftResultDto
            {
                CompetitiveEventDraft = new CompetitiveEventDraftResponseDto
                {
                    CompetitiveEventDraftId = dto.Id,
                }
            });
        providerServiceMock.Setup(s => s.IsBlocked(dto.OrganizerOfTheEventId))
            .ReturnsAsync(false);

        // Act
        var result = await controller.CreateDraft(dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());

        var createdResult = result as CreatedAtActionResult;
        Assert.That(createdResult?.Value, Is.InstanceOf<CompetitiveEventDraftResultDto>());

        var responseDto = createdResult.Value as CompetitiveEventDraftResultDto;
        Assert.That(responseDto?.CompetitiveEventDraft.CompetitiveEventDraftId, Is.EqualTo(dto.Id));
    }

    [Test]
    public async Task CreateDraft_ReturnsBadRequest_WhenResultNull()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto();
        providerServiceMock.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);
        competitiveEventDraftServiceMock.Setup(s => s.Create(dto))
            .ReturnsAsync((CompetitiveEventDraftResultDto)null);

        // Act
        var result = await controller.CreateDraft(dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest?.Value.ToString(), Does.Contain("Returned result is null"));
    }

    [Test]
    public async Task CreateDraft_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Act
        var result = await controller.CreateDraft(null).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest?.Value.ToString(), Does.Contain("Dto is null"));
    }

    [Test]
    public async Task CreateDraft_ReturnsBadRequest_WhenProviderDoesNotExist()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto();
        providerServiceMock.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync((bool?)null);

        // Act
        var result = await controller.CreateDraft(dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateDraft_ReturnsForbidden_WhenProviderIsBlocked()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto();
        providerServiceMock.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await controller.CreateDraft(dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(403));
    }

    #endregion

    #region UpdateDraft

    [Test]
    public async Task UpdateDraft_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Act
        var result = await controller.UpdateDraft(Guid.NewGuid(), null).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest?.Value.ToString(), Does.Contain("Dto is null"));
    }

    [Test]
    public async Task UpdateDraft_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto
            {
                Id = id,
                OrganizerOfTheEventId = providerId
            }
        };
        var returnedResult = Result<CompetitiveEventDraftResultDto>.Success(new CompetitiveEventDraftResultDto()
        {
            CompetitiveEventDraft = new CompetitiveEventDraftResponseDto
            {
                CompetitiveEventDraftId = id,
                CompetitiveEventDetails = dto.CompetitiveEventV2Dto
            }
        });

        competitiveEventDraftServiceMock.Setup(s => s.Update(id, dto))
            .ReturnsAsync(returnedResult);
        providerServiceMock.Setup(s => s.IsBlocked(providerId))
            .ReturnsAsync(false);

        // Act
        var result = await controller.UpdateDraft(id, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.InstanceOf<Result<CompetitiveEventDraftResultDto>>());
        var responseResult = okResult.Value as Result<CompetitiveEventDraftResultDto>;
        Assert.That(responseResult.Succeeded, Is.True);
        Assert.That(responseResult.Value.CompetitiveEventDraft.CompetitiveEventDraftId, Is.EqualTo(id));
    }

    [Test]
    public async Task UpdateDraft_ReturnsBadReques_WhenOpeartionResultIsFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto
            {
                Id = id,
                OrganizerOfTheEventId = providerId
            }
        };
        var returnedResult = Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
        {
            Code = "400",
            Description = "An error occurred while updating the draft."
        });

        competitiveEventDraftServiceMock.Setup(s => s.Update(id, dto))
            .ReturnsAsync(returnedResult);
        providerServiceMock.Setup(s => s.IsBlocked(providerId))
            .ReturnsAsync(false);

        // Act
        var result = await controller.UpdateDraft(id, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.StatusCode, Is.EqualTo(400));
        var errorDescription = badRequest.Value;
        Assert.That(errorDescription, Does.Contain("error occurred while updating the draft"));
    }

    [Test]
    public async Task UpdateDraft_ReturnsInternalError_WhenOpeartionResultIsFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto
            {
                Id = id,
                OrganizerOfTheEventId = providerId
            }
        };
        var returnedResult = Result<CompetitiveEventDraftResultDto>.Failed(new OperationError()
        {
            Code = "500",
            Description = "An error occurred while updating the draft."
        });

        competitiveEventDraftServiceMock.Setup(s => s.Update(id, dto))
            .ReturnsAsync(returnedResult);
        providerServiceMock.Setup(s => s.IsBlocked(providerId))
            .ReturnsAsync(false);

        // Act
        var result = await controller.UpdateDraft(id, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var internalError = result as ObjectResult;
        Assert.That(internalError?.StatusCode, Is.EqualTo(500));
        var errorDescription = internalError.Value;
        Assert.That(errorDescription, Does.Contain("error occurred while updating the draft"));
    }

    [Test]
    public async Task UpdateDraft_ReturnsInternalError_WhenServiceThrowsEntityDeletedConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto
            {
                Id = id,
                OrganizerOfTheEventId = providerId
            }
        };
        competitiveEventDraftServiceMock.Setup(s => s.Update(id, dto))
            .ThrowsAsync(new EntityDeletedConflictException("Service error"));
        providerServiceMock.Setup(s => s.IsBlocked(providerId))
            .ReturnsAsync(false);

        // Act
        var result = await controller.UpdateDraft(id, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var internalError = result as ObjectResult;
        Assert.That(internalError?.StatusCode, Is.EqualTo(500));
        var errorDescription = internalError.Value;
        Assert.That(errorDescription, Does.Contain("Service error"));
    }

    [Test]
    public async Task UpdateDraft_ReturnsInternalError_WhenServiceThrowsEntityModifiedConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto
            {
                Id = id,
                OrganizerOfTheEventId = providerId
            }
        };
        competitiveEventDraftServiceMock.Setup(s => s.Update(id, dto))
            .ThrowsAsync(new EntityModifiedConflictException("Service error"));
        providerServiceMock.Setup(s => s.IsBlocked(providerId))
            .ReturnsAsync(false);

        // Act
        var result = await controller.UpdateDraft(id, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var internalError = result as ObjectResult;
        Assert.That(internalError?.StatusCode, Is.EqualTo(500));
        var errorDescription = internalError.Value;
        Assert.That(errorDescription, Does.Contain("Service error"));
    }

    #endregion

    #region DeleteDraft

    [Test]
    public async Task DeleteDraft_ReturnsNoContent_WhenDraftDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Success;
        competitiveEventDraftServiceMock.Setup(s => s.Delete(id)).ReturnsAsync(response);

        // Act
        var result = await controller.DeleteDraft(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task DeleteDraft_ReturnsBadRequest_WhenOpeartionResultFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Failed(new OperationError
        {
            Code = "400",
            Description = "Something gone wrong."
        });
        competitiveEventDraftServiceMock.Setup(s => s.Delete(id)).ReturnsAsync(response);

        // Act
        var result = await controller.DeleteDraft(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("Something gone wrong"));
    }

    [Test]
    public async Task DeleteDraft_ReturnsNotFound_WhenDraftDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Failed(new OperationError
        {
            Code = "404",
            Description = "Draft does not exist."
        });
        competitiveEventDraftServiceMock.Setup(s => s.Delete(id)).ReturnsAsync(response);

        // Act
        var result = await controller.DeleteDraft(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeleteDraft_ReturnsInternalError_WhenOperationResultFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Failed(new OperationError
        {
            Code = "500",
            Description = "An error occurred while deleting the draft."
        });
        competitiveEventDraftServiceMock.Setup(s => s.Delete(id)).ReturnsAsync(response);

        // Act
        var result = await controller.DeleteDraft(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var internalError = result as ObjectResult;
        Assert.That(internalError.StatusCode, Is.EqualTo(500));
        Assert.That(internalError.Value.ToString(), Does.Contain("An error occurred while deleting the draft"));
    }

    [Test]
    public async Task DeleteDraft_ReturnsInternalError_WhenServiceThrowsEntityDeletedConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventDraftServiceMock.Setup(s => s.Delete(id))
            .ThrowsAsync(new EntityDeletedConflictException("Service error"));

        // Act
        var result = await controller.DeleteDraft(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var internalError = result as ObjectResult;
        Assert.That(internalError.StatusCode, Is.EqualTo(500));
        Assert.That(internalError.Value.ToString(), Does.Contain("Service error"));
    }

    [Test]
    public async Task DeleteDraft_ReturnsInternalError_WhenServiceThrowsEntityModifiedConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventDraftServiceMock.Setup(s => s.Delete(id))
            .ThrowsAsync(new EntityModifiedConflictException("Service error"));

        // Act
        var result = await controller.DeleteDraft(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var internalError = result as ObjectResult;
        Assert.That(internalError.StatusCode, Is.EqualTo(500));
        Assert.That(internalError.Value.ToString(), Does.Contain("Service error"));
    }

    #endregion

    #region SendForModeration

    [Test]
    public async Task SendForModeration_ReturnsOkResult_WhenDraftSentForModeration()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Success;
        competitiveEventDraftServiceMock.Setup(s => s.SendForModeration(id)).ReturnsAsync(response);

        // Act
        var result = await controller.SendForModeration(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
        var okResult = result as OkResult;
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task SendForModeration_ReturnsBadRequest_WhenOperationResultFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Failed(new OperationError
        {
            Code = "400",
            Description = "Something gone wrong."
        });
        competitiveEventDraftServiceMock.Setup(s => s.SendForModeration(id)).ReturnsAsync(response);

        // Act
        var result = await controller.SendForModeration(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value.ToString(), Does.Contain("Something gone wrong"));
    }

    [Test]
    public async Task SendForModeration_ReturnsNotFound_WhenDraftDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Failed(new OperationError
        {
            Code = "404",
            Description = "Competitive event draft was not found."
        });
        competitiveEventDraftServiceMock.Setup(s => s.SendForModeration(id)).ReturnsAsync(response);

        // Act
        var result = await controller.SendForModeration(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var badRequest = result as NotFoundObjectResult;
        Assert.That(badRequest.StatusCode, Is.EqualTo(404));
        Assert.That(badRequest.Value.ToString(), Does.Contain("Competitive event draft was not found"));
    }

    [Test]
    public async Task SendForModeration_ReturnsInternalError_WhenOperationResultFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = OperationResult.Failed(new OperationError
        {
            Code = "500",
            Description = "An error occurred while sending the draft for moderation."
        });
        competitiveEventDraftServiceMock.Setup(s => s.SendForModeration(id)).ReturnsAsync(response);

        // Act
        var result = await controller.SendForModeration(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var internalError = result as ObjectResult;
        Assert.That(internalError.StatusCode, Is.EqualTo(500));
        Assert.That(internalError.Value.ToString(), Does.Contain("An error occurred while sending the draft for moderation"));
    }

    [Test]
    public async Task SendForModeration_ReturnsInternalError_WhenServiceThrowsEntityDeletedConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventDraftServiceMock.Setup(s => s.SendForModeration(id))
            .ThrowsAsync(new EntityDeletedConflictException("Service error"));

        // Act
        var result = await controller.SendForModeration(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var internalError = result as ObjectResult;
        Assert.That(internalError.StatusCode, Is.EqualTo(500));
        Assert.That(internalError.Value.ToString(), Does.Contain("Service error"));
    }

    [Test]
    public async Task SendForModeration_ReturnsInternalError_WhenServiceThrowsEntityModifiedConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventDraftServiceMock.Setup(s => s.SendForModeration(id))
            .ThrowsAsync(new EntityModifiedConflictException("Service error"));

        // Act
        var result = await controller.SendForModeration(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var internalError = result as ObjectResult;
        Assert.That(internalError.StatusCode, Is.EqualTo(500));
        Assert.That(internalError.Value.ToString(), Does.Contain("Service error"));
    }

    #endregion

    #region GetDraftByProviderId

    [Test]
    public async Task GetDraftByProviderId_ReturnsOk_WhenResultsExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filter = new ExcludeIdFilter();
        var resultDto = new SearchResult<CompetitiveEventDraftViewCardDto> { TotalAmount = 1, Entities = new List<CompetitiveEventDraftViewCardDto> { new() } };
        competitiveEventDraftServiceMock.Setup(s => s.GetByProviderId(id, filter)).ReturnsAsync(resultDto);

        // Act
        var result = await controller.GetDraftByProviderId(id, filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetDraftByProviderId_ReturnsNoContent_WhenNoEntitiesExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filter = new ExcludeIdFilter();
        var emptyResult = new SearchResult<CompetitiveEventDraftViewCardDto> { TotalAmount = 0, Entities = new List<CompetitiveEventDraftViewCardDto>() };

        competitiveEventDraftServiceMock.Setup(s => s.GetByProviderId(id, filter)).ReturnsAsync(emptyResult);
        // Act
        var result = await controller.GetDraftByProviderId(id, filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    #endregion

    #region GetDraftById

    [Test]
    public async Task GetDraftById_ReturnsOk_WhenDraftExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var draftDto = new CompetitiveEventDraftResponseDto { CompetitiveEventDraftId = id };
        competitiveEventDraftServiceMock.Setup(s => s.GetCompetitiveEventDraftByIdMapped(id)).ReturnsAsync(draftDto);

        // Act
        var result = await controller.GetDraftById(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.InstanceOf<CompetitiveEventDraftResponseDto>());
        var responseDto = okResult.Value as CompetitiveEventDraftResponseDto;
        Assert.That(responseDto.CompetitiveEventDraftId, Is.EqualTo(id));
    }

    [Test]
    public async Task GetDraftById_ReturnsNotFound_WhenDraftDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventDraftServiceMock.Setup(s => s.GetCompetitiveEventDraftByIdMapped(id)).ReturnsAsync((CompetitiveEventDraftResponseDto)null);

        // Act
        var result = await controller.GetDraftById(id).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    #endregion

    #endregion
}
