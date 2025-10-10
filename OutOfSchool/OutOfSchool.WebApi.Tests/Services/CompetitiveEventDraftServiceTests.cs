using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
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
    private Mock<IEntityRepositorySoftDeleted<long, SubDirection>> mockSubDirectionRepository;


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
        mockSubDirectionRepository = new Mock<IEntityRepositorySoftDeleted<long, SubDirection>>();

        competitiveEventDraftService = new CompetitiveEventDraftService(
            mockLogger.Object,
            mockUserService.Object,
            mockCompetitiveEventService.Object,
            mockCompetitiveEventDraftRepository.Object,
            mockImageService.Object,
            mockChangesLogService.Object,
            mockCodeficatorRepository.Object,
            new Mock<IRegionAdminService>().Object,
            new Mock<IMinistryAdminService>().Object,
            new Mock<ICodeficatorService>().Object,
            new Mock<ISearchStringService>().Object,
            mockSubDirectionRepository.Object);
    }

    #region Create

    [Test]
    public void Create_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        // Arrange
        CompetitiveEventV2Dto dto = null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await competitiveEventDraftService.Create(dto));
    }

    [Test]
    public async Task Create_ReturnsDraftResultDto_WhenDtoIsValidWithNewImagesAndCompetitiveEventDoesNotExist()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto()
        {
            Id = Guid.NewGuid(),
            Contacts =
            [
                new ContactsDto
                {
                    IsDefault = true,
                    Address = ContactsAddressDtoGenerator.Generate()
                }
            ],
            ImageFiles = [Mock.Of<IFormFile>()],
            CoverImage = Mock.Of<IFormFile>()
        };
        var draft = dto.ToDraft();
        var catottgs = new List<CATOTTG>
        {
            new() { Id = 1, Name = "Test Codeficator" }
        };

        var directionSubDirectionIds = new List<DirectionSubDirectionIdsDto>
        {
            new() { DirectionId = 14, SubDirectionId = 54  }
        };

        var subDirections = new List<SubDirection>
        {
            new() { Id = 54, DirectionId = 14, Description = "description1", IsDeleted = false, Title = "title1"  },
            new() { Id = 9, DirectionId = 10, Description = "description2", IsDeleted = true, Title = "title2"  }
        };

        mockCompetitiveEventService.Setup(service => service.GetById(dto.Id))
            .ReturnsAsync((CompetitiveEventDto)null)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>>>()))
            .Returns((Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>> f) => f.Invoke())
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Create(It.IsAny<CompetitiveEventDraft>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.AddManyImagesAsync(draft, dto.ImageFiles))
            .ReturnsAsync(new MultipleImageUploadingResult())
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.AddCoverImageAsync(draft, dto.CoverImage))
            .ReturnsAsync(Result<string>.Success("some text"))
            .Verifiable(Times.Once);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);
        mockSubDirectionRepository.Setup(repo => repo.GetByFilter(
            It.IsAny<Expression<Func<SubDirection, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(subDirections.Where(sd => !sd.IsDeleted))
            .Verifiable(Times.Once);

        // Act
        var result = await competitiveEventDraftService.Create(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventDraftResultDto>(result);
        Assert.That(result.CompetitiveEventDraft.CompetitiveEventDetails.DirectionSubDirectionIds, Has.Count.EqualTo(1));
        Assert.That(result.CompetitiveEventDraft.CompetitiveEventDetails.DirectionSubDirectionIds
              .Select(x => (x.DirectionId, x.SubDirectionId)), Is.EqualTo(directionSubDirectionIds.Select(x => (x.DirectionId, x.SubDirectionId))));
        Mock.VerifyAll();
    }

    [Test]
    public async Task Create_ReturnsDraftResultDto_WhenDtoIsValidWithNewImagesAndCompetitiveEventExists()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto()
        {
            Id = Guid.NewGuid(),
            Contacts =
            [
                new ContactsDto
                {
                    IsDefault = true,
                    Address = ContactsAddressDtoGenerator.Generate()
                }
            ],
            ImageFiles = [Mock.Of<IFormFile>()],
            CoverImage = Mock.Of<IFormFile>()
        };
        var draft = dto.ToDraft();
        var catottgs = new List<CATOTTG>
        {
            new() { Id = 1, Name = "Test Codeficator" }
        };

        var directionSubDirectionIds = new List<DirectionSubDirectionIdsDto>
        {
            new() { DirectionId = 14, SubDirectionId = 54  }
        };

        var subDirections = new List<SubDirection>
        {
            new() { Id = 54, DirectionId = 14, Description = "description1", IsDeleted = false, Title = "title1"  },
            new() { Id = 9, DirectionId = 10, Description = "description2", IsDeleted = true, Title = "title2"  }
        };

        var existedCompetitiveEventDto = new CompetitiveEventDto() { State = CompetitiveEventStates.Published };

        mockCompetitiveEventService.Setup(service => service.GetById(dto.Id))
            .ReturnsAsync(existedCompetitiveEventDto)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>>>()))
            .Returns((Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>> f) => f.Invoke())
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Create(It.IsAny<CompetitiveEventDraft>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.AddManyImagesAsync(draft, dto.ImageFiles))
            .ReturnsAsync(new MultipleImageUploadingResult())
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.AddCoverImageAsync(draft, dto.CoverImage))
            .ReturnsAsync(Result<string>.Success("some text"))
            .Verifiable(Times.Once);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);
        mockSubDirectionRepository.Setup(repo => repo.GetByFilter(
            It.IsAny<Expression<Func<SubDirection, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(subDirections.Where(sd => !sd.IsDeleted))
            .Verifiable(Times.Once);

        // Act
        var result = await competitiveEventDraftService.Create(dto, true);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventDraftResultDto>(result);
        Assert.That(result.CompetitiveEventDraft.CompetitiveEventDetails.DirectionSubDirectionIds, Has.Count.EqualTo(1));
        Assert.That(result.CompetitiveEventDraft.CompetitiveEventDetails.DirectionSubDirectionIds
              .Select(x => (x.DirectionId, x.SubDirectionId)), Is.EqualTo(directionSubDirectionIds.Select(x => (x.DirectionId, x.SubDirectionId))));
        Mock.VerifyAll();
    }

    [Test]
    public void Create_ThrowInvalidOperationException_WhenImageUploadFails()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto()
        {
            Id = Guid.NewGuid(),
            Contacts =
            [
                new ContactsDto
                {
                    IsDefault = true,
                    Address = ContactsAddressDtoGenerator.Generate()
                }
            ],
            ImageFiles = [Mock.Of<IFormFile>()],
            CoverImage = Mock.Of<IFormFile>()
        };
        var draft = dto.ToDraft();
        var catottgs = new List<CATOTTG>
        {
            new() { Id = 1, Name = "Test Codeficator" }
        };
        var directionSubDirectionIds = new List<DirectionSubDirectionIdsDto>
        {
            new() { DirectionId = 14, SubDirectionId = 54  }
        };

        var subDirections = new List<SubDirection>
        {
            new() { Id = 54, DirectionId = 14, Description = "description1", IsDeleted = false, Title = "title1"  },
            new() { Id = 9, DirectionId = 10, Description = "description2", IsDeleted = true, Title = "title2"  }
        };
        var existedCompetitiveEventDto = new CompetitiveEventDto() { State = CompetitiveEventStates.Published };
        var expectedResult = Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>.Failed(new OperationError()
        {
            Code = "500",
            Description = "Some error occurred"
        });

        mockCompetitiveEventService.Setup(service => service.GetById(dto.Id))
            .ReturnsAsync(existedCompetitiveEventDto)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>>>()))
            .ReturnsAsync(expectedResult)
            .Verifiable(Times.Once);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);
        mockSubDirectionRepository.Setup(repo => repo.GetByFilter(
            It.IsAny<Expression<Func<SubDirection, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(subDirections.Where(sd => !sd.IsDeleted))
            .Verifiable(Times.Once);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await competitiveEventDraftService.Create(dto, true));
        Assert.That(ex.Message, Does.Contain("Some error occurred"));
        Mock.VerifyAll();
    }

    [Test]
    public void Create_ThrowsInvalidOperationException_WhenCompetitiveEventStateIsArchived()
    {
        // Arrange
        var dto = new CompetitiveEventV2Dto()
        {
            Id = Guid.NewGuid(),
            Contacts =
            [
                new ContactsDto
                {
                    IsDefault = true,
                    Address = ContactsAddressDtoGenerator.Generate()
                }
            ]
        };
        var existedCompetitiveEventDto = new CompetitiveEventDto() { State = CompetitiveEventStates.Archived };
        mockCompetitiveEventService.Setup(service => service.GetById(dto.Id))
            .ReturnsAsync(existedCompetitiveEventDto)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await competitiveEventDraftService.Create(dto));
        Mock.VerifyAll();
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
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Does.Contain("ID in route can't be empty."));
    }

    [Test]
    public async Task Update_ReturnsFailedResult_IfIdIsNotEmptyButDtoIdIsEmpty()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = Guid.Empty,
            CompetitiveEventV2Dto = new CompetitiveEventV2Dto()
        };

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Does.Contain("Dto's id can't be empty."));
    }

    [Test]
    public async Task Update_ReturnsFailedResult_WhenDtoIsNotNullButCompetitiveEventV2DtoIsNull()
    {
        // Arrange
        Guid id = Guid.Empty;
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            CompetitiveEventV2Dto = null
        };

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Does.Contain("Dto can't be null."));
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
    public async Task Update_ReturnsCompetitiveEventDraftResultDto_WhenSuccessAndCompetitiveEventDoesNotExist()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = CompetitiveEventV2DtoGenerator.Generate()
        };
        var draft = new CompetitiveEventDraft()
        {
            Id = id,
            CompetitiveEventDraftContent = new CompetitiveEventDraftContent()
            {
                OrganizerOfTheEventId = Guid.NewGuid()
            }
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
           .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>>>()))
           .Returns((Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>> f) => f.Invoke())
           .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetByIdWithDetails(id, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetById(id))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));
        mockCompetitiveEventService.Setup(service => service.GetById(id))
            .ReturnsAsync((CompetitiveEventDto)null)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.ChangeCoverImageAsync(draft, dto.CompetitiveEventV2Dto.CoverImageId, dto.CompetitiveEventV2Dto.CoverImage))
            .ReturnsAsync(coverImageResult)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.ChangeImagesAsync(draft, dto.CompetitiveEventV2Dto.ImageIds, dto.CompetitiveEventV2Dto.ImageFiles))
            .ReturnsAsync(imagesResult)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.Update(It.IsAny<CompetitiveEventDraft>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.CompetitiveEventDraft.CompetitiveEventDraftId, Is.EqualTo(draft.ToResponseDto().CompetitiveEventDraftId));
        Mock.VerifyAll();
    }

    [Test]
    public async Task Update_ReturnsCompetitiveEventDraftResultDto_WhenSuccessAndCompetitiveEventExists()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var competitiveEventV2Dto = CompetitiveEventV2DtoGenerator.Generate();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = competitiveEventV2Dto
        };
        var draft = new CompetitiveEventDraft()
        {
            Id = id,
            CompetitiveEventDraftContent = new CompetitiveEventDraftContent()
            {
                OrganizerOfTheEventId = Guid.NewGuid()
            }
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
           .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>>>()))
           .Returns((Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>> f) => f.Invoke())
           .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetByIdWithDetails(id, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetById(id))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(3));
        mockCompetitiveEventService.Setup(service => service.GetById(id))
            .ReturnsAsync(competitiveEventV2Dto)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.ChangeCoverImageAsync(draft, dto.CompetitiveEventV2Dto.CoverImageId, dto.CompetitiveEventV2Dto.CoverImage))
            .ReturnsAsync(coverImageResult)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.ChangeImagesAsync(draft, dto.CompetitiveEventV2Dto.ImageIds, dto.CompetitiveEventV2Dto.ImageFiles))
            .ReturnsAsync(imagesResult)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.Update(It.IsAny<CompetitiveEventDraft>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.CompetitiveEventDraft.CompetitiveEventDraftId, Is.EqualTo(draft.ToResponseDto().CompetitiveEventDraftId));
        Mock.VerifyAll();
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
        var expectedResult = Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>.Failed(new OperationError()
        {
            Code = "500",
            Description = "Some error occurred"
        });

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

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("500"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Is.EqualTo("Some error occurred"));
    }

    [Test]
    public async Task Update_ReturnsFailedResult_WhenDraftStatusIsPendingModeration()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var competitiveEventV2Dto = CompetitiveEventV2DtoGenerator.Generate();
        var dto = new CompetitiveEventDraftUpdateDto()
        {
            Id = id,
            CompetitiveEventV2Dto = competitiveEventV2Dto
        };
        var draft = new CompetitiveEventDraft()
        {
            Id = id,
            CompetitiveEventDraftContent = new CompetitiveEventDraftContent()
            {
                OrganizerOfTheEventId = Guid.NewGuid()
            },
            DraftStatus = CompetitiveEventDraftStatus.PendingModeration
        };

        mockCompetitiveEventDraftRepository
           .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>>>()))
           .Returns((Func<Task<Result<(CompetitiveEventDraft, ImageChangingResult, MultipleImageChangingResult)>>> f) => f.Invoke())
           .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.GetById(id))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(3));
        mockCompetitiveEventService.Setup(service => service.GetById(id))
            .ReturnsAsync(competitiveEventV2Dto)
            .Verifiable(Times.Once);

        // Act
        var result = await competitiveEventDraftService.Update(id, dto);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description, Is.EqualTo("Competitive event draft can't be updated when it is in PendingModeration status."));

        Mock.VerifyAll();
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
        var filter = new CompetitiveEventDraftFilterTitle();
        var drafts = new List<CompetitiveEventDraft>
        {
            new CompetitiveEventDraft
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
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
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.Get(
            filter.From,
            filter.Size,
            It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns((int from, int size, Expression<Func<CompetitiveEventDraft, bool>> predicate,
            Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection> sort)
                => drafts.AsQueryable().Where(predicate).AsTestAsyncEnumerableQuery());

        // Act
        var result = await competitiveEventDraftService.GetByProviderId(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<SearchResult<CompetitiveEventDraftViewCardDto>>(result);
        Assert.AreEqual(1, result.TotalAmount);
    }

    [Test]
    public async Task GetByProviderId_ReturnsSearchResult_WhenFilterContainsSearchText()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();
        var filter = new CompetitiveEventDraftFilterTitle { SearchText = "test" };
        var drafts = new List<CompetitiveEventDraft>
        {
            new CompetitiveEventDraft
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
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
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.Get(
            filter.From,
            filter.Size,
            It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns((int from, int size, Expression<Func<CompetitiveEventDraft, bool>> predicate,
            Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection> sort)
                => drafts.AsQueryable().Where(predicate).AsTestAsyncEnumerableQuery());

        // Act
        var result = await competitiveEventDraftService.GetByProviderId(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<SearchResult<CompetitiveEventDraftViewCardDto>>(result);
        Assert.AreEqual(1, result.Entities.Count);
        Assert.AreEqual(result.Entities.First().Title, drafts.First().CompetitiveEventDraftContent.Title);
    }

    [Test]
    public async Task GetByProviderId_ReturnsValidResult_WhenFilterHasExcludedId()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();
        Guid excludedId = Guid.NewGuid();
        var filter = new CompetitiveEventDraftFilterTitle { ExcludedId = excludedId };
        var drafts = new List<CompetitiveEventDraft>
        {
            new CompetitiveEventDraft
            {
                Id = excludedId,
                ProviderId = providerId,
                DraftStatus = CompetitiveEventDraftStatus.Draft,
                CompetitiveEventDraftContent = new CompetitiveEventDraftContent { Title = "Test Event" },
                CoverImageId = "coverImageId"
            },
            new CompetitiveEventDraft
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                DraftStatus = CompetitiveEventDraftStatus.Draft,
                CompetitiveEventDraftContent = new CompetitiveEventDraftContent { Title = "Another Test Event" },
                CoverImageId = "coverImageId2"
            }
        };
        var searchResult = new SearchResult<CompetitiveEventDraftViewCardDto>
        {
            Entities = new[] { drafts.FirstOrDefault(x => x.Id != excludedId).ToCardDto() },
            TotalAmount = 1
        };
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Count(It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>()))
            .ReturnsAsync(searchResult.TotalAmount);
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.Get(
            filter.From,
            filter.Size,
            It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns((int from, int size, Expression<Func<CompetitiveEventDraft, bool>> predicate,
            Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection> sort)
                => drafts.AsQueryable().Where(predicate).AsTestAsyncEnumerableQuery());

        // Act
        var result = await competitiveEventDraftService.GetByProviderId(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<SearchResult<CompetitiveEventDraftViewCardDto>>(result);
        Assert.AreEqual(1, result.Entities.Count);
        Assert.AreEqual(result.Entities.First().Title, drafts.First(x => x.Id != excludedId).CompetitiveEventDraftContent.Title);
        Assert.IsFalse(result.Entities.Any(e => e.CompetitiveEventDraftId == excludedId));
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

        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetByIdWithDetails(id, It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(draft);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery);

        // Act
        var result = await competitiveEventDraftService.GetCompetitiveEventDraftByIdMapped(id);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventDraftResponseDto>(result);
        Assert.AreEqual(draft.Id, result.CompetitiveEventDraftId);
    }

    #endregion

    #region Approve

    [Test]
    public void Approve_ReturnsArgumentException_IfDraftDoesNotExist()
    {
        // Arrange
        Guid draftId = Guid.NewGuid();
        var draft = (CompetitiveEventDraft)null;
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(draftId))
            .ReturnsAsync(draft);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await competitiveEventDraftService.Approve(draftId));
    }

    [Test]
    public void Approve_ReturnsArgumentException_IfDraftStatusIsNotPendingModerationOrEditedByModerator()
    {
        // Arrange
        Guid draftId = Guid.NewGuid();
        CompetitiveEventDraft draft = new() { DraftStatus = CompetitiveEventDraftStatus.Draft };
        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(draftId))
            .ReturnsAsync(draft);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await competitiveEventDraftService.Approve(draftId));
    }

    [Test]
    public async Task Approve_CallCreateV2_IfCompetitiveEventIdIsNull()
    {
        // Arrange
        Guid draftId = Guid.NewGuid();
        CompetitiveEventDraft draft = new()
        {
            DraftStatus = CompetitiveEventDraftStatus.PendingModeration,
            CompetitiveEventId = null,
            CompetitiveEventDraftContent = CompetitiveEventDraftContentGenerator.Generate()
        };

        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(draftId))
            .ReturnsAsync(draft);
        mockCompetitiveEventService.Setup(s => s.CreateV2(It.IsAny<CompetitiveEventV2Dto>()))
           .ReturnsAsync(new CompetitiveEventResultDto());
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Delete(draft))
            .Returns(Task.CompletedTask);

        // Act
        await competitiveEventDraftService.Approve(draftId);

        // Assert
        mockCompetitiveEventService.Verify(s => s.CreateV2(It.IsAny<CompetitiveEventV2Dto>()), Times.Once);
        mockCompetitiveEventDraftRepository.Verify(repo => repo.Delete(draft), Times.Once);
    }

    [Test]
    public async Task Approve_CallUpdateV2_IfCompetitiveEventIdIsNotNull()
    {
        // Arrange
        Guid draftId = Guid.NewGuid();
        CompetitiveEventDraft draft = CompetitiveEventDraftGenerator.Generate();
        draft.DraftStatus = CompetitiveEventDraftStatus.PendingModeration;

        mockCompetitiveEventDraftRepository.Setup(repo => repo.GetById(draftId))
            .ReturnsAsync(draft);
        mockCompetitiveEventService.Setup(s => s.UpdateV2(draft.ToDto(), true))
            .ReturnsAsync(new CompetitiveEventResultDto());
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Delete(draft))
            .Returns(Task.CompletedTask);

        // Act
        await competitiveEventDraftService.Approve(draftId);

        // Assert
        mockCompetitiveEventService.Verify(s => s.UpdateV2(It.IsAny<CompetitiveEventV2Dto>(), true), Times.Once);
        mockCompetitiveEventDraftRepository.Verify(repo => repo.Delete(draft), Times.Once);
    }

    #endregion

    #region UpdateCompetitiveEvent

    [Test]
    public void UpdateCompetitiveEvent_ReturnsInvalidOperationException_IfCompetitiveEventDoesNotExist()
    {
        // Arrange
        CompetitiveEventV2Dto v2dto = CompetitiveEventV2DtoGenerator.Generate();
        CompetitiveEventDto dto = null;

        mockCompetitiveEventService.Setup(repo => repo.GetById(v2dto.Id))
            .ReturnsAsync(dto)
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await competitiveEventDraftService.UpdateCompetitiveEvent(v2dto));
        Mock.VerifyAll();
    }

    [Test]
    public void UpdateCompetitiveEvent_ReturnsInvalidOperationException_IfCompetitiveEventStateIsAchieved()
    {
        // Arrange
        CompetitiveEventV2Dto v2dto = CompetitiveEventV2DtoGenerator.Generate();
        v2dto.State = CompetitiveEventStates.Archived;
        CompetitiveEventDto dto = v2dto;

        mockCompetitiveEventService.Setup(repo => repo.GetById(v2dto.Id))
            .ReturnsAsync(dto)
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await competitiveEventDraftService.UpdateCompetitiveEvent(v2dto));
        Mock.VerifyAll();
    }

    [Test]
    public void UpdateCompetitiveEvent_ReturnsInvalidOperationException_IfDraftExists()
    {
        // Arrange
        CompetitiveEventV2Dto v2dto = CompetitiveEventV2DtoGenerator.Generate();
        CompetitiveEventDto dto = v2dto;
        Guid excludedId = Guid.NewGuid();
        var filter = new CompetitiveEventDraftFilterTitle { ExcludedId = excludedId };
        var drafts = new List<CompetitiveEventDraft>
        {
            new CompetitiveEventDraft
            {
                Id = excludedId,
                DraftStatus = CompetitiveEventDraftStatus.Draft,
                CompetitiveEventDraftContent = new CompetitiveEventDraftContent { Title = "Test Event" },
                CoverImageId = "coverImageId"
            },
            new CompetitiveEventDraft
            {
                Id = Guid.NewGuid(),
                DraftStatus = CompetitiveEventDraftStatus.Draft,
                CompetitiveEventDraftContent = new CompetitiveEventDraftContent { Title = "Another Test Event" },
                CoverImageId = "coverImageId2"
            }
        };

        mockCompetitiveEventService.Setup(repo => repo.GetById(v2dto.Id))
            .ReturnsAsync(dto)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns(drafts.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await competitiveEventDraftService.UpdateCompetitiveEvent(v2dto));
        Mock.VerifyAll();
    }

    [Test]
    public async Task UpdateCompetitiveEvent_CallCreate_IfModeratedFieldsChangedAndDtoWithNewImages()
    {
        // Arrange
        CompetitiveEventV2Dto v2dto = CompetitiveEventV2DtoGenerator.Generate();
        v2dto.ImageFiles = [Mock.Of<IFormFile>()];
        v2dto.CoverImage = Mock.Of<IFormFile>();
        CompetitiveEventDraft draft = v2dto.ToDraft();
        CompetitiveEventDto dto = draft.ToDto();
        dto.Title = "new Title";
        var drafts = new List<CompetitiveEventDraft>();
        var catottgs = new List<CATOTTG>
        {
            new() { Id = 1, Name = "Test Codeficator" }
        };
        var directionSubDirectionIds = new List<DirectionSubDirectionIdsDto>
        {
            new() { DirectionId = 14, SubDirectionId = 54  }
        };
        var subDirections = new List<SubDirection>
        {
            new() { Id = 54, DirectionId = 14, Description = "description1", IsDeleted = false, Title = "title1"  },
            new() { Id = 9, DirectionId = 10, Description = "description2", IsDeleted = true, Title = "title2"  }
        };

        mockCompetitiveEventService.Setup(repo => repo.GetById(v2dto.Id))
            .ReturnsAsync(dto)
            .Verifiable(Times.Exactly(2));
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns(drafts.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);
        mockUserService.Setup(service => service.UserHasRights(It.IsAny<IUserRights[]>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));
        mockCompetitiveEventDraftRepository
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>>>()))
            .Returns((Func<Task<Result<(CompetitiveEventDraft, UploadCompetitiveEventDraftImagesResult)>>> f) => f.Invoke())
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Create(It.IsAny<CompetitiveEventDraft>()))
            .ReturnsAsync(draft)
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.AddManyImagesAsync(draft, v2dto.ImageFiles))
            .ReturnsAsync(new MultipleImageUploadingResult())
            .Verifiable(Times.Once);
        mockImageService.Setup(service => service.AddCoverImageAsync(draft, v2dto.CoverImage))
            .ReturnsAsync(Result<string>.Success("some text"))
            .Verifiable(Times.Once);
        mockCodeficatorRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(catottgs.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);
        mockSubDirectionRepository.Setup(repo => repo.GetByFilter(
            It.IsAny<Expression<Func<SubDirection, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(subDirections.Where(sd => !sd.IsDeleted))
            .Verifiable(Times.Once);

        // Act 
        var result = await competitiveEventDraftService.UpdateCompetitiveEvent(v2dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventV2Dto>(result);
        Assert.That(result.DirectionSubDirectionIds, Has.Count.EqualTo(1));
        Assert.That(result.DirectionSubDirectionIds
              .Select(x => (x.DirectionId, x.SubDirectionId)), Is.EqualTo(directionSubDirectionIds.Select(x => (x.DirectionId, x.SubDirectionId))));
        Mock.VerifyAll();
    }

    [Test]
    public async Task UpdateCompetitiveEvent_CallUpdateV2_IfModeratedFieldsDoesNotChanged()
    {
        // Arrange
        CompetitiveEventV2Dto v2dto = CompetitiveEventV2DtoGenerator.Generate();
        CompetitiveEventDraft draft = v2dto.ToDraft();
        CompetitiveEventDto dto = draft.ToDto();
        var drafts = new List<CompetitiveEventDraft>();

        mockCompetitiveEventService.Setup(repo => repo.GetById(v2dto.Id))
            .ReturnsAsync(dto)
            .Verifiable(Times.Once);
        mockCompetitiveEventDraftRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CompetitiveEventDraft, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CompetitiveEventDraft, object>>, SortDirection>>()))
            .Returns(drafts.AsTestAsyncEnumerableQuery)
            .Verifiable(Times.Once);
        mockCompetitiveEventService.Setup(s => s.UpdateV2(v2dto, false))
            .ReturnsAsync(new CompetitiveEventResultDto() { CompetitiveEventV2 = draft.ToDto() })
            .Verifiable(Times.Once);

        // Act 
        var result = await competitiveEventDraftService.UpdateCompetitiveEvent(v2dto);

        // Assert
        mockCompetitiveEventService.Verify(s => s.UpdateV2(It.IsAny<CompetitiveEventV2Dto>(), false), Times.Once);
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<CompetitiveEventV2Dto>(result);
        Assert.AreEqual(result.Title, v2dto.Title);
        Mock.VerifyAll();
    }

    #endregion
}
