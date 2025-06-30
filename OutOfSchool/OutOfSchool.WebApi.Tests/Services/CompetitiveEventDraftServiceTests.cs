using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CompetitiveEventDraftServiceTests
{
    private ICompetitiveEventDraftService competitiveEventDraftService;
    private Mock<ILogger<CompetitiveEventDraftService>> mockLogger;
    private Mock<ICurrentUserService> mockUserService;
    private Mock<ICompetitiveEventServiceV2> mockCompetitiveEventService;
    private Mock<ICompetitiveEventDraftRepository> mockCompetitiveEventDraftRepository;
    private Mock<IImageDependentEntityImagesInteractionService<CompetitiveEventDraft>> mockImageService;
    private Mock<ICodeficatorRepository> mockCodeficatorRepository;
    private Mock<IChangesLogService> mockChangesLogService;


[SetUp]
    public void SetUp()
    {
        mockLogger = new Mock<ILogger<CompetitiveEventDraftService>>();
        mockUserService = new Mock<ICurrentUserService>();
        mockCompetitiveEventService = new Mock<ICompetitiveEventServiceV2>();
        mockCompetitiveEventDraftRepository = new Mock<ICompetitiveEventDraftRepository>();
        mockImageService = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEventDraft>>();
        mockCodeficatorRepository = new Mock<ICodeficatorRepository>();
        mockChangesLogService = new Mock<IChangesLogService>();

        competitiveEventDraftService = new CompetitiveEventDraftService(
            mockLogger.Object,
            mockUserService.Object,
            mockCompetitiveEventService.Object,
            mockCompetitiveEventDraftRepository.Object,
            mockImageService.Object,
            mockChangesLogService.Object,
            mockCodeficatorRepository.Object);
    }

    #region Create

    [Test]
    public async Task Create_ReturnsNull_WhenDtoIsNull()
    {
        // Arrange
        CompetitiveEventV2Dto dto = null;

        // Act
        var result = await competitiveEventDraftService.Create(dto);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task Create_ReturnsDraftResultDto_WhenDtoIsValid()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto() { Id = Guid.NewGuid()};
        var draft = dto.ToDraft();
        var catottgs = new List<CATOTTG>
        {
            new CATOTTG { Id = 1, Name = "Test Codeficator" }
        };
        mockCompetitiveEventService.Setup(service => service.GetById(dto.Id))
            .ReturnsAsync((CompetitiveEventDto)null);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<CompetitiveEventDraft>>>()))
            .ReturnsAsync(draft);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Create(draft))
            .ReturnsAsync(draft);
        mockImageService.Setup(service => service.AddManyImagesAsync(draft, dto.ImageFiles))
            .ReturnsAsync(new MultipleImageUploadingResult());
        mockImageService.Setup(service => service.AddCoverImageAsync(draft, dto.CoverImage))
            .ReturnsAsync(Result<string>.Success("some text"));
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery);

        // Act
        var result = await competitiveEventDraftService.Create(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventDraftResultDto>(result);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_ReturnsFailedResult_WhenDtoIsNull()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        CompetitiveEventDraftUpdateDto dto = null;

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Does.Contain("Dto can't be null"));
    }

    [Test]
    public async Task Update_ReturnsFailedResult_IfIdIsEmpty()
    {
        // Arrange
        Guid id = Guid.Empty;
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto()
        };

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Does.Contain("Dto's id can't be empty"));
    }

    [Test]
    public async Task Update_ReturnsFailedResult_WhenIdsAreDifferent()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = Guid.NewGuid(),
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto()
        };

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Does.Contain("ID in route and DTO do not match"));
    }

    [Test]
    public async Task Update_ReturnsCompetitiveEventDraftResultDto_WhenSuccess()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto()
        };
        var draft = new CompetitiveEventDraft()
        {
            Id = id
        };
        var catottgs = new List<CATOTTG>
        {
            new CATOTTG { Id = 1, Name = "Test Codeficator" }
        };
        var coverImageResult = new ImageChangingResult();
        var imagesResult = new MultipleImageChangingResult();
        var expectedResult = Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>
            .Success((draft, coverImageResult, imagesResult));

        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>>>()))
            .ReturnsAsync(expectedResult);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetById(id))
            .ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventService.Setup(service => service.GetById(id))
            .ReturnsAsync((CompetitiveEventDto)null);
        mockImageService.Setup(service => service.ChangeCoverImageAsync(draft, dto.CompetitiveEventV2Dto.CoverImageId, dto.CompetitiveEventV2Dto.CoverImage))
            .ReturnsAsync(coverImageResult);
        mockImageService.Setup(service => service.ChangeImagesAsync(draft, dto.CompetitiveEventV2Dto.ImageIds, dto.CompetitiveEventV2Dto.ImageFiles))
            .ReturnsAsync(imagesResult);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery);

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.CompetitiveEventDraft.CompetitiveEventDraftId, Is.EqualTo(draft.ToResponseDto().CompetitiveEventDraftId));
    }

    [Test]
    public async Task Update_ReturnsFailedResult_WhenImageUpdateFails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto()
        };
        var draft = new CompetitiveEventDraft()
        {
            Id = id
        };
        var coverImageResult = new ImageChangingResult();
        var imagesResult = new MultipleImageChangingResult();
        var expectedResult = Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>.Failed(new OperationError() { 
            Code = "500",
            Description = "Some error occurred" });

        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>>>()))
            .ReturnsAsync(expectedResult);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetById(id))
            .ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventService.Setup(service => service.GetById(id))
            .ReturnsAsync((CompetitiveEventDto)null);
        mockImageService.Setup(service => service.ChangeCoverImageAsync(draft, dto.CompetitiveEventV2Dto.CoverImageId, dto.CompetitiveEventV2Dto.CoverImage))
            .ReturnsAsync(coverImageResult);
        mockImageService.Setup(service => service.ChangeImagesAsync(draft, dto.CompetitiveEventV2Dto.ImageIds, dto.CompetitiveEventV2Dto.ImageFiles))
            .ReturnsAsync(imagesResult);

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("500"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Is.EqualTo("Some error occurred"));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsFailedResult_WhenIdIsEmpty()
    {
        // Arrange
        Guid id = Guid.Empty;

        // Act
        var result = await competitiveEventDraftService.Delete(id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.Errors.FirstOrDefault().Description, Does.Contain("Id cannot be empty"));
    }

    [Test]
    public async Task Delete_ReturnsFailedResult_WhenDraftDoesNotExist()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync((CompetitiveEventDraft)null);

        // Act
        var result = await competitiveEventDraftService.Delete(id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.FirstOrDefault().Code, Is.EqualTo("404"));
        Assert.That(result.Errors.FirstOrDefault().Description, Does.Contain("Competitive event draft not found"));
    }

    [Test]
    public async Task Delete_ReturnsFailedResult_WhenDraftStatusIsPendingModeration()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var draft = new CompetitiveEventDraft { Id = id, DraftStatus = CompetitiveEventDraftStatus.PendingModeration };
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await competitiveEventDraftService.Delete(id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.Errors.FirstOrDefault().Description, Does.Contain("Competitive event draft can only be deleted when it is not in PendingModeration status"));
    }

    [Test]
    public async Task Delete_ReturnsSuccessResult_WhenEntityIsDeleted()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var draft = new CompetitiveEventDraft { Id = id, DraftStatus = CompetitiveEventDraftStatus.Draft };
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Delete(draft)).Returns(Task.CompletedTask);

        // Act
        var result = await competitiveEventDraftService.Delete(id);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    #endregion

    #region SendForModeration

    [Test]
    public async Task SendForModeration_ReturnsFailedResult_WhenIdIsEmpty()
    {
        // Arrange
        Guid id = Guid.Empty;

        // Act
        var result = await competitiveEventDraftService.SendForModeration(id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.Errors.FirstOrDefault().Description, Does.Contain("Id cannot be empty"));
    }

    [Test]
    public async Task SendForModeration_ReturnsFailedResult_WhenDraftDoesNotExist()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync((CompetitiveEventDraft)null);

        // Act
        var result = await competitiveEventDraftService.SendForModeration(id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.FirstOrDefault().Code, Is.EqualTo("404"));
        Assert.That(result.Errors.FirstOrDefault().Description, Does.Contain("Competitive event draft not found"));
    }

    [Test]
    public async Task SendForModeration_ReturnsFailedResult_WhenDraftIsNotInDraftStatus()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var draft = new CompetitiveEventDraft { Id = id, DraftStatus = CompetitiveEventDraftStatus.PendingModeration };
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await competitiveEventDraftService.SendForModeration(id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.Errors.FirstOrDefault().Description, Does.Contain("Competitive event draft can only be sent for moderation when it is in Draft status"));
    }

    [Test]
    public async Task SendForModeration_ReturnsSuccessResult_WhenDraftSentForModeration()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var draft = new CompetitiveEventDraft { Id = id, DraftStatus = CompetitiveEventDraftStatus.Draft };
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Update(draft)).ReturnsAsync(draft);

        // Act
        var result = await competitiveEventDraftService.SendForModeration(id);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(draft.DraftStatus, Is.EqualTo(CompetitiveEventDraftStatus.PendingModeration));
    }

    #endregion

    #region GetByProviderId

    [Test]
    public async Task GetByProviderId_ReturnsSearchResult_WhenIdIsValid()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();
        var filter = new ExcludeIdFilter();
        var drafts = new List<CompetitiveEventDraft>
        {
            new CompetitiveEventDraft
            {
                Id = Guid.NewGuid(),
                DraftStatus = CompetitiveEventDraftStatus.Draft,
                CompetitiveEventDraftContent = new CompetitiveEventDraftContent { Title = "Test Event" },
                CoverImageId = "coverImageId"
            }
        };
        var searchResult = new SearchResult<CompetitiveEventDraftViewCardDto>
        {
            Entities = new[] { drafts.FirstOrDefault().ToCardDto() },
            TotalAmount = 1
        };

        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Count(It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>()))
            .ReturnsAsync(searchResult.TotalAmount);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Get(filter.From, filter.Size, It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns(drafts.AsTestAsyncEnumerableQuery);

        // Act
        var result = await competitiveEventDraftService.GetByProviderId(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<SearchResult<CompetitiveEventDraftViewCardDto>>(result);
        Assert.AreEqual(1, result.TotalAmount);
    }

    #endregion GetByProviderId

    #region GetCompetitiveEventDraftByIdMapped

    [Test]
    public async Task GetCompetitiveEventDraftByIdMapped_ReturnsNull_IfDraftDoesNotExist()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync((CompetitiveEventDraft)null);

        // Act
        var result = await competitiveEventDraftService.GetCompetitiveEventDraftByIdMapped(id);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetCompetitiveEventDraftByIdMapped_ReturnsDraftResponseDto_IfDraftExists()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var draft = new CompetitiveEventDraft
        {
            Id = id,
            DraftStatus = CompetitiveEventDraftStatus.Draft,
            CompetitiveEventDraftContent = new CompetitiveEventDraftContent { Title = "Test Event" },
            CoverImageId = "coverImageId"
        };
        var catottgs = new List<CATOTTG>
        {
            new CATOTTG { Id = 1, Name = "Test Codeficator" }
        };

        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(id)).ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG,object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery);

        // Act
        var result = await competitiveEventDraftService.GetCompetitiveEventDraftByIdMapped(id);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventDraftResponseDto>(result);
        Assert.AreEqual(draft.Id, result.CompetitiveEventDraftId);
    }

    #endregion
}
