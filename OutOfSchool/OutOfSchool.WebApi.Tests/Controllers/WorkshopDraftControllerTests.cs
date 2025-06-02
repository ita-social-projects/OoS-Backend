using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V2;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopDraftControllerTests
{
    private const int Ok = 200;
    private const int NoContent = 204;
    private const int Create = 201;
    private const int BadRequest = 400;
    private const int Forbidden = 403;

    private static WorkshopV2Dto workshopV2Dto;
    private static WorkshopDraftResultDto workshopDraftResultDto;
    private static ProviderDto provider;    

    private WorkshopDraftController controller;
    private Mock<IProviderService> providerServiceMoq;
    private Mock<IWorkshopDraftService> workshopDraftServiceMoq;
    private Mock<ISensitiveWorkshopDraftService> sensitiveWorkshopDraftServiceMoq;
    private Mock<HttpContext> httpContextMoq;

    private string userId;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        userId = "someUserId";
        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        httpContextMoq.Setup(x => x.User.IsInRole("provider"))
            .Returns(true);

        provider = ProviderDtoGenerator.Generate();

        workshopV2Dto = WorkshopV2DtoGenerator.Generate();
        workshopV2Dto.Address = AddressDtoGenerator.Generate();
        workshopV2Dto.DateTimeRanges = DateTimeRangeDtoGenerator.Generate(5);
        workshopV2Dto.ProviderId = provider.Id;

        workshopDraftResultDto = new WorkshopDraftResultDto()
        {
            WorkshopDraft = workshopV2Dto.ToDraft().ToResponseDto()
        };
    }

    [SetUp]
    public void Setup()
    {
        workshopDraftServiceMoq = new Mock<IWorkshopDraftService>();
        providerServiceMoq = new Mock<IProviderService>();
        sensitiveWorkshopDraftServiceMoq = new Mock<ISensitiveWorkshopDraftService>();

        controller = new WorkshopDraftController(
            providerServiceMoq.Object,
            workshopDraftServiceMoq.Object,
            sensitiveWorkshopDraftServiceMoq.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
        };
    }

    #region Create
    [Test]
    public async Task CreateWorkshopDraft_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
    {
        // Arrange        
        workshopDraftServiceMoq.Setup(x => x.Create(workshopV2Dto))
            .ReturnsAsync(workshopDraftResultDto).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))            
            .ReturnsAsync(false).Verifiable(Times.Once);

        // Act
        var result = await controller.Create(workshopV2Dto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert        
        providerServiceMoq.VerifyAll();
        workshopDraftServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Create, result.StatusCode);
    }
    #endregion    

    #region Update
    [Test]
    public async Task Update_WhenModelIsValid_ShouldReturnOkResult()
    {
        // Arrange        
        var workshopDraftUpdateDto = new WorkshopDraftUpdateDto()
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto,
        };

        workshopDraftServiceMoq.Setup(x => x.Update(workshopDraftUpdateDto))
            .ReturnsAsync(workshopDraftResultDto).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);

        // Act
        var result = await controller.Update(workshopDraftUpdateDto).ConfigureAwait(false) as OkObjectResult;

        // Assert        
        providerServiceMoq.VerifyAll();
        workshopDraftServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
    }
    #endregion 

    #region Delete
    [Test]
    public async Task Delete_WhenModelIsValid_ShouldReturnNoContent()
    {
        // Arrange  
        workshopDraftServiceMoq.Setup(x => x.Delete(workshopV2Dto.Id))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        var result = await controller.Delete(workshopV2Dto.Id).ConfigureAwait(false) as NoContentResult;

        // Assert    
        workshopDraftServiceMoq.VerifyAll();
        Assert.AreEqual(NoContent, result.StatusCode);
    }
    #endregion 

    #region SendForModeration
    [Test]
    public async Task SendForModeration_WhenModelIsValid_ShouldReturnOk()
    {
        // Arrange  
        workshopDraftServiceMoq.Setup(x => x.SendForModeration(workshopV2Dto.Id))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        var result = await controller.SendForModeration(workshopV2Dto.Id).ConfigureAwait(false) as OkResult;

        // Assert        
        workshopDraftServiceMoq.VerifyAll();
        Assert.AreEqual(Ok, result.StatusCode);
    }
    #endregion

    #region Reject
    [Test]
    public async Task Reject_WhenModelIsValid_ShouldReturnOk()
    {
        // Arrange  
        var rejectionDto = new WorkshopDraftRejectionDto
        {            
            RejectionMessage = "I don’t like it"
        };

        workshopDraftServiceMoq.Setup(x => x.Reject(workshopV2Dto.Id, rejectionDto.RejectionMessage))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        var result = await controller.Reject(workshopV2Dto.Id, rejectionDto).ConfigureAwait(false) as OkResult;

        // Assert        
        workshopDraftServiceMoq.VerifyAll();
        Assert.AreEqual(Ok, result.StatusCode);
    }

    [Test]
    public async Task Reject_WhenRejectionMessageIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange        
        var rejectionDto = new WorkshopDraftRejectionDto
        {
            RejectionMessage = string.Empty
        };

        controller.ModelState.AddModelError(nameof(rejectionDto.RejectionMessage), "Rejection message is required");

        // Act
        var result = await controller.Reject(workshopV2Dto.Id, rejectionDto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert             
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.IsInstanceOf<SerializableError>(result.Value);
    }

    #endregion 

    #region Approve
    [Test]
    public async Task Approve_WhenModelIsValid_ShouldReturnOk()
    {
        // Arrange  
        workshopDraftServiceMoq.Setup(x => x.Approve(workshopV2Dto.Id))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        var result = await controller.Approve(workshopV2Dto.Id).ConfigureAwait(false) as OkResult;

        // Assert        
        workshopDraftServiceMoq.VerifyAll();
        Assert.AreEqual(Ok, result.StatusCode);
    }
    #endregion 

    #region GetByProviderId
    [Test]
    public async Task GetByProviderId_WhenThereAreWorkshopDrafts_ShouldReturnOkResultObject()
    {
        // Arrange 
        var searchResult = new SearchResult<WorkshopDraftViewCardDto>()
        {
            TotalAmount = 1,
            Entities = new List<WorkshopDraftViewCardDto>()
            {
                workshopV2Dto.ToDraft().ToCardDto()
            },            
        };

        workshopDraftServiceMoq.Setup(x => x.GetByProviderId(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(searchResult).Verifiable(Times.Once);

        // Act
        var result = await controller.GetByProviderId(Guid.NewGuid(), null).ConfigureAwait(false) as OkObjectResult;

        // Assert        
        workshopDraftServiceMoq.VerifyAll();
        Assert.AreEqual(Ok, result.StatusCode);
    }

    [Test]
    public async Task GetByProviderId_WhenThereIsNoWorkshopDrafts_ShouldReturnNoContentResult()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue };
        var emptySearchResult = new SearchResult<WorkshopDraftViewCardDto>() 
        { 
            TotalAmount = 0, 
            Entities = new List<WorkshopDraftViewCardDto>() 
        };

        workshopDraftServiceMoq.Setup(x => x.GetByProviderId(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(emptySearchResult);

        // Act
        var result = await controller.GetByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as NoContentResult;

        // Assert
        workshopDraftServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }
    #endregion

    #region UpdateAsModerator

    [Test]
    public async Task UpdateAsModerator_ReturnsOkResult_WhenServiceReturnsSuccessResult()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var dto = new ModeratorWorkshopDraftEditDto
        {
            Title = "Updated Title",
            ShortTitle = "Updated Short Title",
            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDto>()
        };

        var expectedResponse = new WorkshopDraftResponseDto
        {
            WorkshopDraftId = draftId,
            DraftStatus = WorkshopDraftStatus.EditedByModerator,
            WorkshopDetails = workshopV2Dto
        };

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.UpdateDraftAsModeratorAsync(draftId, dto))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Success(expectedResponse));

        // Act
        var result = await controller.UpdateAsModerator(draftId, dto);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [Test]
    public async Task UpdateAsModerator_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var dto = new ModeratorWorkshopDraftEditDto();
        var errorMessage = "Invalid input data";

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.UpdateDraftAsModeratorAsync(draftId, dto))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "400", Description = errorMessage }));

        // Act
        var result = await controller.UpdateAsModerator(draftId, dto);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.AreEqual(errorMessage, badRequestResult.Value);
    }

    [Test]
    public async Task UpdateAsModerator_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var dto = new ModeratorWorkshopDraftEditDto();
        var errorMessage = "Draft not found";

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.UpdateDraftAsModeratorAsync(draftId, dto))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "404", Description = errorMessage }));

        // Act
        var result = await controller.UpdateAsModerator(draftId, dto);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = (NotFoundObjectResult)result;
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.AreEqual(errorMessage, notFoundResult.Value);
    }

    [Test]
    public async Task UpdateAsModerator_ReturnsConflict_WhenServiceReturnsConflictError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var dto = new ModeratorWorkshopDraftEditDto();
        var errorMessage = "Draft cannot be edited in its current status";

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.UpdateDraftAsModeratorAsync(draftId, dto))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "409", Description = errorMessage }));

        // Act
        var result = await controller.UpdateAsModerator(draftId, dto);

        // Assert
        Assert.IsInstanceOf<ConflictObjectResult>(result);
        var conflictResult = (ConflictObjectResult)result;
        Assert.AreEqual(StatusCodes.Status409Conflict, conflictResult.StatusCode);
        Assert.AreEqual(errorMessage, conflictResult.Value);
    }

    #endregion

    #region DeleteCoverImageAsModerator

    [Test]
    public async Task DeleteCoverImageAsModerator_ReturnsOkResult_WhenServiceReturnsSuccessResult()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var expectedResponse = new WorkshopDraftResponseDto
        {
            WorkshopDraftId = draftId,
            DraftStatus = WorkshopDraftStatus.EditedByModerator,
            WorkshopDetails = workshopV2Dto
        };

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteCoverImageAsModeratorAsync(draftId))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Success(expectedResponse));

        // Act
        var result = await controller.DeleteCoverImageAsModerator(draftId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [Test]
    public async Task DeleteCoverImageAsModerator_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var errorMessage = "Draft does not have a cover image";

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteCoverImageAsModeratorAsync(draftId))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "400", Description = errorMessage }));

        // Act
        var result = await controller.DeleteCoverImageAsModerator(draftId);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.AreEqual(errorMessage, badRequestResult.Value);
    }

    #endregion

    #region DeleteImageAsModerator

    [Test]
    public async Task DeleteImageAsModerator_ReturnsOkResult_WhenServiceReturnsSuccessResult()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var imageId = "image123";
        var expectedResponse = new WorkshopDraftResponseDto
        {
            WorkshopDraftId = draftId,
            DraftStatus = WorkshopDraftStatus.EditedByModerator,
            WorkshopDetails = workshopV2Dto
        };

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteImageAsModeratorAsync(draftId, imageId))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Success(expectedResponse));

        // Act
        var result = await controller.DeleteImageAsModerator(draftId, imageId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [Test]
    public async Task DeleteImageAsModerator_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var imageId = "image123";
        var errorMessage = "Image not found";

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteImageAsModeratorAsync(draftId, imageId))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "404", Description = errorMessage }));

        // Act
        var result = await controller.DeleteImageAsModerator(draftId, imageId);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = (NotFoundObjectResult)result;
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.AreEqual(errorMessage, notFoundResult.Value);
    }

    #endregion

    #region DeleteManyImagesAsModerator

    [Test]
    public async Task DeleteManyImagesAsModerator_ReturnsOkResult_WhenServiceReturnsSuccessResult()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var imageIds = new List<string> { "image1", "image2", "image3" };
        var expectedResponse = new WorkshopDraftResponseDto
        {
            WorkshopDraftId = draftId,
            DraftStatus = WorkshopDraftStatus.EditedByModerator,
            WorkshopDetails = workshopV2Dto
        };

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteManyImagesAsModeratorAsync(draftId, imageIds))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Success(expectedResponse));

        // Act
        var result = await controller.DeleteManyImagesAsModerator(draftId, imageIds);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [Test]
    public async Task DeleteManyImagesAsModerator_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var imageIds = new List<string>();
        var errorMessage = "No image IDs were provided";

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteManyImagesAsModeratorAsync(draftId, imageIds))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "400", Description = errorMessage }));

        // Act
        var result = await controller.DeleteManyImagesAsModerator(draftId, imageIds);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.AreEqual(errorMessage, badRequestResult.Value);
    }

    [Test]
    public async Task DeleteManyImagesAsModerator_ReturnsForbid_WhenServiceReturnsForbiddenError()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var imageIds = new List<string> { "image1", "image2" };

        sensitiveWorkshopDraftServiceMoq
            .Setup(s => s.DeleteManyImagesAsModeratorAsync(draftId, imageIds))
            .ReturnsAsync(Result<WorkshopDraftResponseDto>.Failed(
                new OperationError { Code = "403", Description = "Forbidden" }));

        // Act
        var result = await controller.DeleteManyImagesAsModerator(draftId, imageIds);

        // Assert
        Assert.IsInstanceOf<ForbidResult>(result);
    }

    #endregion
}
