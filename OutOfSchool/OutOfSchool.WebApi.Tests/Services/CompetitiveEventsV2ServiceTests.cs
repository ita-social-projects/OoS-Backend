using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CompetitiveEventsV2ServiceTests
{
    private Mock<ICompetitiveEventRepository> repoMock;
    private Mock<ILogger<CompetitiveEventService>> loggerMock;
    private Mock<IStringLocalizer<SharedResource>> localizerMock;
    private Mock<ICurrentUserService> currentUserMock;
    private Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>> contactsServiceMock;
    private Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>> imageServiceMock;
    private Mock<IEntityRepository<long, SubDirection>> mockSubDirectionRepository;


    private CompetitiveEventService service;

    [SetUp]
    public void Setup()
    {
        repoMock = new Mock<ICompetitiveEventRepository>();
        loggerMock = new Mock<ILogger<CompetitiveEventService>>();
        localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(u => u.UserHasRights(It.IsAny<IUserRights>())).Returns(Task.CompletedTask);
        contactsServiceMock = new Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>>();
        imageServiceMock = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>>();
        mockSubDirectionRepository = new Mock<IEntityRepository<long, SubDirection>>();

        service = new CompetitiveEventService(
            repoMock.Object,
            Mock.Of<IEntityRepository<Guid, CompetitiveEventDescriptionItem>>(),
            mockSubDirectionRepository.Object,
            loggerMock.Object,
            localizerMock.Object,
            currentUserMock.Object,
            contactsServiceMock.Object,
            imageServiceMock.Object);
    }

    #region CreateV2

    [Test]
    public async Task CreateV2_ReturnsMappedDto_WhenSuccessful()
    {
        var createdEntity = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = "New Title",
            SubDirections = new List<SubDirection>()
            {
                new() { Id = 1, DirectionId = 1, IsDeleted = false, Direction = new() { Id = 1, IsDeleted = false } }
            },
            CompetitiveEventDescriptionItems =
            [
                new CompetitiveEventDescriptionItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Description 1",
                    SectionName = "Section 1"
                }
            ]
        };

        var dto = new CompetitiveEventV2CreateRequestDto() { SubDirectionIds = [1], OrganizerOfTheEventId = Guid.NewGuid(), };
        var expectedDto = new CompetitiveEventV2Dto { Id = createdEntity.Id };

        repoMock.Setup(r => r.Create(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync(createdEntity)
            .Verifiable(Times.Once);
        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>(f => f())
            .Verifiable(Times.Once);

        // Act
        var result = await service.CreateV2(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(createdEntity.Id, result.CompetitiveEventV2.Id);
        Mock.VerifyAll();
    }

    [Test]
    public async Task CreateV2_HandlesNullImagesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CompetitiveEventV2CreateRequestDto
        {
            ImageFiles = null,
            CoverImage = null,
            SubDirectionIds = [1]
        };
        var entity = new CompetitiveEvent { Id = id };
        var resultDto = new CompetitiveEventV2Dto { Id = id };

        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>(f => f())
            .Verifiable(Times.Once);
        repoMock.Setup(r => r.Create(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync(entity)
            .Verifiable(Times.Once);

        // Act
        var result = await service.CreateV2(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.CompetitiveEventV2);
        Assert.That(result.CompetitiveEventV2.Id, Is.EqualTo(id));
        Mock.VerifyAll();
    }

    #endregion

    #region UpdateV2

    [Test]
    public void UpdateV2_WhenDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        CompetitiveEventV2Dto dto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.UpdateV2(dto).ConfigureAwait(false));
    }

    [Test]
    public void UpdateV2_ThrowsConcurrencyException_WhenEntityDoesNotExist()
    {
        var dto = new CompetitiveEventV2Dto { Id = Guid.NewGuid() };

        repoMock.Setup(r => r.GetByIdWithDetails(dto.Id, It.IsAny<string>(), It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync((CompetitiveEvent)null);

        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>(f => f());

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await service.UpdateV2(dto));
    }

    [Test]
    public void UpdateV2_ThrowsInvalidOperationException_WhenSubDirectionIdsAreEmpty()
    {
        // Arrange
        var dto = CompetitiveEventV2DtoGenerator.Generate();
        var competitiveEvent = dto.SetToModel(new CompetitiveEvent());
        competitiveEvent.Id = dto.Id;

        repoMock.Setup(r => r.GetByIdWithDetails(dto.Id, It.IsAny<string>(), It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync(competitiveEvent);
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>(f => f());

        // Act, Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.UpdateV2(dto));
    }

    [Test]
    public async Task UpdateV2_ReturnResult_WhenEntityExists()
    {
        // Arrange
        var dto = CompetitiveEventV2DtoGenerator.Generate();
        var competitiveEvent = dto.SetToModel(new CompetitiveEvent());
        competitiveEvent.Id = dto.Id;
        competitiveEvent.CompetitiveEventDescriptionItems = dto.CompetitiveEventDescriptionItems.ToModel();
        var subDirections = new List<SubDirection>
        {
            new() { Id = 54, DirectionId = 14, Description = "description1", IsDeleted = false, Title = "title1"  },
            new() { Id = 9, DirectionId = 10, Description = "description2", IsDeleted = true, Title = "title2"  }
        };

        repoMock.Setup(r => r.GetByIdWithDetails(dto.Id, It.IsAny<string>(), It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync(competitiveEvent).Verifiable(Times.Once); ;
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>(f => f()).Verifiable(Times.Once);;
        mockSubDirectionRepository.Setup(repo => repo.GetByFilter(
            It.IsAny<Expression<Func<SubDirection, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(subDirections.Where(sd => !sd.IsDeleted)).Verifiable(Times.Once); ;
        imageServiceMock.Setup(service => service.ChangeImagesAsync(competitiveEvent, It.IsAny<List<string>>(), It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(new MultipleImageChangingResult()).Verifiable(Times.Once); ;
        imageServiceMock.Setup(service => service.ChangeCoverImageAsync(competitiveEvent, It.IsAny<string>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(new ImageChangingResult()).Verifiable(Times.Once); ;
        repoMock.Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>()).Verifiable(Times.Once);

        // Act
        var result = await service.UpdateV2(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(competitiveEvent.Id, result.CompetitiveEventV2.Id);
        Mock.VerifyAll();
    }

    #endregion

    #region DeleteV2
    [Test]
    public async Task DeleteV2_CallsRemoveImagesAndDeletesEntity_WhenImagesExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new CompetitiveEvent
        {
            Id = id,
            Images = new List<Image<CompetitiveEvent>>
            {
                new Image<CompetitiveEvent> { ExternalStorageId = "img1" }
            },
            CoverImageId = "cover"
        };

        repoMock.Setup(r => r.GetById(id)).ReturnsAsync(entity);
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns<Func<Task<Workshop>>>(f => f());

        // Act
        await service.DeleteV2(id);

        // Assert
        imageServiceMock.Verify(s => s.RemoveManyImagesAsync(entity, It.Is<List<string>>(l => l.Contains("img1"))), Times.Once);
        imageServiceMock.Verify(s => s.RemoveCoverImageAsync(entity), Times.Once);
        repoMock.Verify(r => r.Delete(entity), Times.Once);
    }

    [Test]
    public async Task DeleteV2_SkipsImageRemoval_WhenNoImagesOrCover()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new CompetitiveEvent
        {
            Id = id,
            Images = new List<Image<CompetitiveEvent>>(),
            CoverImageId = null
        };

        repoMock.Setup(r => r.GetById(id)).ReturnsAsync(entity);
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns<Func<Task<Workshop>>>(f => f());

        // Act
        await service.DeleteV2(id);

        // Assert
        imageServiceMock.Verify(s => s.RemoveManyImagesAsync(It.IsAny<CompetitiveEvent>(), It.IsAny<List<string>>()), Times.Never);
        imageServiceMock.Verify(s => s.RemoveCoverImageAsync(It.IsAny<CompetitiveEvent>()), Times.Never);
        repoMock.Verify(r => r.Delete(entity), Times.Once);
    }

    #endregion
}
