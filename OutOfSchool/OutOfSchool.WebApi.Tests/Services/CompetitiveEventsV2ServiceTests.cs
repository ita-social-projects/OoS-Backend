using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

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

    private CompetitiveEventService service;

    [SetUp]
    public void Setup()
    {
        repoMock = new Mock<ICompetitiveEventRepository>();
        loggerMock = new Mock<ILogger<CompetitiveEventService>>();
        localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        currentUserMock = new Mock<ICurrentUserService>();
        contactsServiceMock = new Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>>();
        imageServiceMock = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>>();

        service = new CompetitiveEventService(
            repoMock.Object,
            Mock.Of<IEntityRepository<Guid, CompetitiveEventDescriptionItem>>(),
            loggerMock.Object,
            localizerMock.Object,
            currentUserMock.Object,
            contactsServiceMock.Object,
            imageServiceMock.Object);
    }

    [Test]
    public async Task CreateV2_ReturnsMappedDto_WhenSuccessful()
    {
        // Arrange
        var entity = new CompetitiveEvent { Id = Guid.NewGuid() };
        var dto = new CompetitiveEventV2CreateRequestDto();

        repoMock.Setup(r => r.Create(It.IsAny<CompetitiveEvent>())).ReturnsAsync(entity);
        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>(f => f());

        // Act
        var result = await service.CreateV2(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result.CompetitiveEventV2.Id);
    }

    [Test]
    public async Task CreateV2_HandlesNullImagesCorrectly()
    {
        var id = Guid.NewGuid();
        var dto = new CompetitiveEventV2CreateRequestDto
        {
            ImageFiles = null,
            CoverImage = null
        };
        var entity = new CompetitiveEvent { Id = id };
        var resultDto = new CompetitiveEventV2Dto { Id = id };

        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageUploadingResult, Result<string>)>>>(f => f());

        repoMock.Setup(r => r.Create(It.IsAny<CompetitiveEvent>())).ReturnsAsync(entity);

        var result = await service.CreateV2(dto);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.CompetitiveEventV2);
        Assert.That(result.CompetitiveEventV2.Id, Is.EqualTo(id));
    }

    [Test]
    public void UpdateV2_ThrowsConcurrencyException_WhenEntityDoesNotExist()
    {
        var dto = new CompetitiveEventV2CreateRequestDto { Id = Guid.NewGuid() };

        repoMock.Setup(r => r.GetByIdWithDetails(dto.Id, It.IsAny<string>(), It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync((CompetitiveEvent)null);

        repoMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>()))
            .Returns<Func<Task<(CompetitiveEvent, MultipleImageChangingResult, ImageChangingResult)>>>(f => f());

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await service.UpdateV2(dto));
    }

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
}
