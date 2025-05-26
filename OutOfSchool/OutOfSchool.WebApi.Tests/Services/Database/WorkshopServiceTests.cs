using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Services.Util;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class WorkshopServiceTests
{
    private IWorkshopService workshopService;
    private Mock<IWorkshopRepository> workshopRepository;
    private Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>> dateTimeRangeRepository;
    private Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>> roomRepository;
    private Mock<ITeacherService> teacherService;
    private Mock<ILogger<WorkshopService>> logger;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<ILanguageService> languageServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ITagService> tagServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IFeatureManager> featureManagerMock;
    private Mock<IEntityRepository<long, Tag>> tagRepository;
    private Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>> contactsServiceMock;
    private Mock<IApplicationRepository> applicationRepository;
    private Mock<IFeatureManager> featureManager;
    private Guid providerId;
    private Guid studySubjectId;


    [SetUp]
    public void SetUp()
    {
        workshopRepository = new Mock<IWorkshopRepository>();
        dateTimeRangeRepository = new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>();
        roomRepository = new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        languageServiceMock = new Mock<ILanguageService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        tagServiceMock = new Mock<ITagService>();
        featureManagerMock = new Mock<IFeatureManager>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        tagRepository = new Mock<IEntityRepository<long, Tag>>();
        contactsServiceMock = new Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>>();
        applicationRepository = new Mock<IApplicationRepository>();
        featureManager = new Mock<IFeatureManager>();
        providerId = Guid.NewGuid();
        studySubjectId = Guid.NewGuid();

    workshopService =
                new WorkshopService(
                    workshopRepository.Object,
                    languageServiceMock.Object,
                    tagRepository.Object,
                    dateTimeRangeRepository.Object,
                    roomRepository.Object,
                    teacherService.Object,
                    logger.Object,
                    workshopImagesMediator.Object,
                    averageRatingServiceMock.Object,
                    providerRepositoryMock.Object,
                    currentUserServiceMock.Object,
                    ministryAdminServiceMock.Object,
                    regionAdminServiceMock.Object,
                    codeficatorServiceMock.Object,
                    tagServiceMock.Object,
                    searchStringServiceMock.Object,
                    contactsServiceMock.Object,
                    applicationRepository.Object,
                    featureManager.Object
                    );
        languageServiceMock.Setup(s => s.GetById(It.IsAny<long>()))
            .ReturnsAsync((long id) => new LanguageDto { Id = id, Name = "English" });
    }

    #region Create
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Create_Whenever_ShouldRunInTransaction(bool isMemberOfWorkshopIdExisted)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreate(createdEntity, isMemberOfWorkshopIdExisted);

        // Act
        var result = await workshopService.Create(WorkshopCreateRequestDtoGenerator.FromModel(createdEntity)).ConfigureAwait(false);

        // Assert
        workshopRepository.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once);
    }

    [Test]
    public async Task Create_WhenEntityIsValidAndAvailableSeatsIsNotNull_ShouldReturnThisEntity(
        [Random(2, 5, 1)] int teachersInWorkshop,
        [Random(2, 25, 1)] int availableSeats,
        [Random(2, 8, 1)] int tagNumber)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(createdEntity);
        createdEntity.Teachers = teachers;
        var tags = TagsGenerator.Generate(tagNumber).WithWorkshop(createdEntity);
        createdEntity.Tags = tags;
        createdEntity.AvailableSeats = (uint)availableSeats;
        var expectedTeachers = teachers.ToDto();
        var expectedTags = tags.ToDto();
        SetupCreate(createdEntity);

        // Act
        var result = await workshopService.Create(WorkshopCreateRequestDtoGenerator.FromModel(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.Tags.Should().BeEquivalentTo(expectedTags);
        result.AvailableSeats.Should().Be((uint)availableSeats);
    }

    [Test]
    public async Task Create_WhenEntityIsValidAndAvailableSeatsIsNull_ShouldReturnThisEntity(
        [Random(2, 5, 1)] int teachersInWorkshop,
        [Random(2, 8, 1)] int tagNumber)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        createdEntity.Teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(createdEntity);
        createdEntity.Tags = TagsGenerator.Generate(tagNumber).WithWorkshop(createdEntity);
        createdEntity.AvailableSeats = uint.MaxValue;
        SetupCreate(createdEntity);

        var createRequest = WorkshopCreateRequestDtoGenerator.FromModel(createdEntity);
        createRequest.AvailableSeats = 0;

        // Act
        var result = await workshopService.Create(createRequest).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(createdEntity.Teachers.ToDto());
        result.Tags.Should().BeEquivalentTo(createdEntity.Tags.ToDto());
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task Create_WithInvalidLanguageId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = WorkshopCreateRequestDtoGenerator.Generate();
        dto.LanguageOfEducationId = 99999;

        languageServiceMock.Setup(x => x.GetById(dto.LanguageOfEducationId))
            .ReturnsAsync((LanguageDto)null); // language not found

        // Act & Assert
        await workshopService
           .Invoking(s => s.Create(dto))
           .Should()
           .ThrowAsync<InvalidOperationException>()
           .WithMessage($"*Language with ID = {dto.LanguageOfEducationId}*");
    }

    [Test]
    public async Task Create_WhenDirectionsIdsAreWrong_ShouldReturnEntitiesWithRightDirectionsIds()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        createdEntity.InstitutionHierarchyId = Guid.NewGuid();
        SetupCreate(createdEntity);

        // Act
        var result = await workshopService.Create(WorkshopCreateRequestDtoGenerator.FromModel(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createdEntity.ToDto());
    }

    [Test]
    public async Task Create_WhenThereIsNotParentProvider_ShouldThrowNullReferenceException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        SetupCreate(createdEntity);

        // Act and Assert
        await workshopService.Invoking(w => w.Create(WorkshopCreateRequestDtoGenerator.FromModel(createdEntity)))
            .Should().ThrowAsync<NullReferenceException>();
    }

    [Test]
    public async Task Create_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act and Assert
        await workshopService.Invoking(w => w.Create(null))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Create_WhenThereIsNotExistedMemberOfWorkshopId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        createdEntity.ParentWorkshopId = Guid.NewGuid();
        SetupCreate(createdEntity);

        // Act and Assert
        await workshopService.Invoking(w => w.Create(WorkshopCreateRequestDtoGenerator.FromModel(createdEntity)))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task Create_WhenParentWorkshopIsMemberOfAnotherWorkshop_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        var guid = Guid.NewGuid();
        createdEntity.ParentWorkshopId = guid;
        createdEntity.ParentWorkshop = WorkshopGenerator.Generate().WithId(guid);
        SetupCreate(createdEntity, true);

        // Act and Assert
        await workshopService.Invoking(w => w.Create(WorkshopCreateRequestDtoGenerator.FromModel(createdEntity)))
            .Should().ThrowAsync<InvalidOperationException>();
    }
    #endregion

    #region CreateV2
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CreateV2_Whenever_ShouldRunInTransaction(bool isMemberOfWorkshopIdExisted)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreateV2(createdEntity, isMemberOfWorkshopIdExisted);

        // Act
        var result = await workshopService.CreateV2(WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity)).ConfigureAwait(false);

        // Assert
        workshopRepository.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<(Workshop, MultipleImageUploadingResult, Result<string>)>>>()), Times.Once);
    }

    [Test]
    public async Task CreateV2_WhenLanguageDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = WorkshopV2CreateRequestDtoGenerator.Generate();
        dto.LanguageOfEducationId = 88888;

        languageServiceMock.Setup(x => x.GetById(dto.LanguageOfEducationId))
            .ReturnsAsync((LanguageDto)null);

        // Act & Assert
        await workshopService
            .Invoking(s => s.CreateV2(dto))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Language with ID = {dto.LanguageOfEducationId}*");
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndAvailableSeatsIsNotNull_ShouldReturnThisEntity(
        [Random(2, 5, 1)] int teachersInWorkshop,
        [Random(2, 25, 1)] int availableSeats,
        [Random(2, 8, 1)] int tagNumber)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(createdEntity);
        createdEntity.DateTimeRanges = new List<DateTimeRange>();
        createdEntity.Teachers = teachers;
        var tags = TagsGenerator.Generate(tagNumber).WithWorkshop(createdEntity);
        createdEntity.Tags = tags;
        createdEntity.AvailableSeats = (uint)availableSeats;
        var expectedTeachers = teachers.ToDto();
        var expectedTags = tags.ToDto();
        SetupCreateV2(createdEntity);

        // Act
        var result = await workshopService.CreateV2(WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Workshop.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.Workshop.Tags.Should().BeEquivalentTo(expectedTags);
        result.Workshop.AvailableSeats.Should().Be((uint)availableSeats);
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndAvailableSeatsIsNull_ShouldReturnThisEntity(
        [Random(2, 5, 1)] int teachersInWorkshop,
        [Random(2, 8, 1)] int tagNumber)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        createdEntity.DateTimeRanges = new List<DateTimeRange>();
        createdEntity.Teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(createdEntity);
        createdEntity.Tags = TagsGenerator.Generate(tagNumber).WithWorkshop(createdEntity);
        createdEntity.AvailableSeats = uint.MaxValue;
        SetupCreateV2(createdEntity);

        var createRequest = WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity);
        createRequest.AvailableSeats = 0;

        // Act
        var result = await workshopService.CreateV2(createRequest).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Workshop.Teachers.Should().BeEquivalentTo(createdEntity.Teachers.ToDto());
        result.Workshop.Tags.Should().BeEquivalentTo(createdEntity.Tags.ToDto());
        result.Workshop.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task CreateV2_WhenDirectionsIdsAreWrong_ShouldReturnEntitiesWithRightDirectionsIds()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        createdEntity.InstitutionHierarchyId = Guid.NewGuid();
        SetupCreateV2(createdEntity);

        // Act
        var result = await workshopService.CreateV2(WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Workshop.Should().BeEquivalentTo(createdEntity.ToV2Dto());
    }

    [Test]
    public async Task CreateV2_WhenThereIsNotParentProvider_ShouldThrowNullReferenceException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        SetupCreateV2(createdEntity);

        // Act and Assert
        await workshopService.Invoking(w => w.CreateV2(WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity)))
            .Should().ThrowAsync<NullReferenceException>();
    }

    [Test]
    public async Task CreateV2_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act and Assert
        await workshopService.Invoking(w => w.CreateV2(null))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task CreateV2_WhenThereIsNotExistedMemberOfWorkshopId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        createdEntity.ParentWorkshopId = Guid.NewGuid();
        SetupCreateV2(createdEntity);

        // Act and Assert
        await workshopService.Invoking(w => w.CreateV2(WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity)))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task CreateV2_WhenParentWorkshopIsMemberOfAnotherWorkshop_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        var guid = Guid.NewGuid();
        createdEntity.ParentWorkshopId = guid;
        createdEntity.ParentWorkshop = WorkshopGenerator.Generate().WithId(guid);
        SetupCreateV2(createdEntity, true);

        // Act and Assert
        await workshopService.Invoking(w => w.CreateV2(WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity)))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndImagesIsNotNull_ShouldReturnThisEntityWithUploadingImagesResultsEqualNumberOfImages([Random(1, 8, 1)] int numberOfImages)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreateV2(createdEntity, true, numberOfImages);
        var dto = WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity);
        var file = new Mock<IFormFile>().Object;
        dto.ImageFiles = new List<IFormFile>();
        for (int i = 1; i <= numberOfImages; i++)
        {
            dto.ImageFiles.Add(file);
        }

        // Act
        var result = await workshopService.CreateV2(dto).ConfigureAwait(false);

        // Assert
        workshopImagesMediator.Verify(m => m.AddManyImagesAsync(It.IsAny<Workshop>(), It.IsAny<IList<IFormFile>>()), Times.Once());
        result.Should().NotBeNull();
        result.UploadingImagesResults.Results.Count.Should().Be(numberOfImages);
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndImagesIsNull_ShouldReturnThisEntityWithUploadingImagesResultsAreNull()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreateV2(createdEntity, true);
        var dto = WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity);
        var file = new Mock<IFormFile>().Object;

        // Act
        var result = await workshopService.CreateV2(dto).ConfigureAwait(false);

        // Assert
        workshopImagesMediator.Verify(m => m.AddManyImagesAsync(It.IsAny<Workshop>(), It.IsAny<IList<IFormFile>>()), Times.Never());
        result.Should().NotBeNull();
        result.UploadingImagesResults.Should().BeNull();
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndCoverImageIsNotNull_ShouldReturnThisEntityWithUploadingCoverImageResultEqualsTrue()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreateV2(createdEntity);
        var dto = WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity);
        var file = new Mock<IFormFile>().Object;
        dto.CoverImage = file;

        // Act
        var result = await workshopService.CreateV2(dto).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.UploadingCoverImageResult.Succeeded.Should().BeTrue();
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndCoverImageIsNull_ShouldReturnThisEntityWithUploadingCoverImageResultIsNull()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreateV2(createdEntity);
        var dto = WorkshopV2CreateRequestDtoGenerator.FromModel(createdEntity);

        // Act
        var result = await workshopService.CreateV2(dto).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.UploadingCoverImageResult.Should().BeNull();
    }
    #endregion

    #region GetAll
    [Test]
    public async Task GetAll_WhenCalled_ShouldReturnAllEntities()
    {
        // Arrange
        var workshops = WithWorkshopsList();
        var guids = workshops.Select(w => w.Id);
        SetupGetAll(workshops, WithAvarageRatings(guids));
        var filter = new OffsetFilter();

        // Act
        var result = await workshopService.GetAll(filter).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(new SearchResult<WorkshopDto>() { Entities = workshops.ToDto().AsReadOnly(), TotalAmount = workshops.Count() });
    }

    #endregion

    #region GetById

    [Test]
    public async Task GetById_WhenEntityWithThisIdExists_ShouldReturnEntity()
    {
        // Arrange
        var id = new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43");
        var workshop = WorkshopGenerator.Generate().WithId(id).WithAddress();
        SetupGetById(workshop);

        // Act
        var result = await workshopService.GetById(id, false).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(workshop.ToDto());
    }

    [Test]
    public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull()
    {
        // Arrange
        var id = new Guid("2a9fcc6d-6c7d-4711-849d-aa8991337185");
        var workshop = WorkshopGenerator.Generate().WithId(id).WithAddress();
        SetupGetById(workshop);

        // Act
        var result = await workshopService.GetById(It.IsAny<Guid>(), false).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }
    #endregion

    #region GetByProviderId
    [Test]
    public async Task GetByProviderId_WhenThereIsNoEntityWithId_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyListWorkshopProviderViewCards = new List<WorkshopProviderViewCard>();
        SetupGetRepositoryCount(0);
        SetupGetByProviderById(new List<Workshop>(), new List<ChatRoomWorkshop>());

        // Act
        var result = await workshopService.GetByProviderId(It.IsAny<Guid>(), It.IsAny<WorkshopFilterTitle>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        roomRepository.VerifyAll();
        result.TotalAmount.Should().Be(0);
        result.Entities.Should().BeEmpty();
    }

    [Test]
    public async Task GetByProviderId_WhenProviderWithIdExists_ShouldReturnEntitiesWithCountedUnreadMessages()
    {
        // Arrange
        var numberOfWorkshops = 10;
        var numberOfChatMessages = 5;
        var directions = InstitutionHierarchyGenerator.Generate();
        var workshops = WorkshopGenerator.Generate(numberOfWorkshops).WithProvider().WithApplications()
            .WithInstitutionHierarchy(directions);
        var workshopsProviderViewCards = workshops.ToProviderViewCard();

        var chatrooms = new List<ChatRoomWorkshop>()
        {
            new ChatRoomWorkshop
            {
                Id = Guid.NewGuid(),
                WorkshopId = workshops[workshops.Count > 1 ? 1 : 0].Id,
            },
        };
        var chatmessages = ChatMessagesGenerator.Generate(numberOfChatMessages).WithSenderRoleIsProvider(false)
            .WithReadDateTime(null).WithChatRoom(chatrooms[0]);
        chatmessages[chatmessages.Count > 1 ? 1 : 0].SenderRoleIsProvider = true;
        chatrooms[0].ChatMessages = chatmessages;

        var expectedUnreadMessages = workshops.Select(d => chatmessages.Count(m =>
            m.ChatRoom.WorkshopId == d.Id &&
            m.ReadDateTime == null &&
            !m.SenderRoleIsProvider)).ToList();

        SetupGetRepositoryCount(workshops.Count);
        SetupGetByProviderById(workshops, chatrooms);

        // Act
        var result = await workshopService.GetByProviderId(It.IsAny<Guid>(), It.IsAny<WorkshopFilterTitle>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        roomRepository.VerifyAll();
        result.TotalAmount.Should().Be(workshops.Count);
        result.Entities.Count.Should().Be(workshops.Count);
        result.Entities.Select(x => x.ProviderId).Should().Equal(workshops.Select(w => w.ProviderId));
        result.Entities.Select(x => x.Id).Should().Equal(workshops.Select(w => w.Id));
        result.Entities.Select(x => x.UnreadMessages).Should().Equal(expectedUnreadMessages);
    }
    #endregion

    #region GetAttachedWorkshops

    [Test]
    public async Task GetAttachedWorkshops_ReturnsCorrectIsAttachedFlags()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        var studySubject = new StudySubject { Id = studySubjectId };

        var workshops = new List<Workshop>
        {
            new Workshop
            {
                Id = Guid.NewGuid(),
                Title = "Math Basics",
                ProviderId = providerId,
                StudySubjects = new List<StudySubject> { studySubject }
            },
            new Workshop
            {
                Id = Guid.NewGuid(),
                Title = "Science Explorers",
                ProviderId = providerId,
                StudySubjects = new List<StudySubject>()
            },
        };

        SetupRepoForAttachedWorkshops(workshops);

        // Act
        var result = await workshopService.GetAttachedWorkshops(studySubjectId, providerId, page, pageSize);

        // Assert
        var resultList = result.Items.ToList();
        Assert.AreEqual(2, resultList.Count);

        Assert.AreEqual("Math Basics", resultList[0].Title);
        Assert.IsTrue(resultList[0].IsAttached);

        Assert.AreEqual("Science Explorers", resultList[1].Title);
        Assert.IsFalse(resultList[1].IsAttached);
    }

    [Test]
    public async Task GetAttachedWorkshops_FiltersByProvider()
    {
        // Arrange
        var otherProviderId = Guid.NewGuid();

        var workshops = new List<Workshop>
        {
            new Workshop
            {
                Id = Guid.NewGuid(),
                Title = "Art Fun",
                ProviderId = providerId,
                StudySubjects = new List<StudySubject> { new StudySubject { Id = studySubjectId } }
            },
            new Workshop
            {
                Id = Guid.NewGuid(),
                Title = "Other Provider Workshop",
                ProviderId = otherProviderId,
                StudySubjects = new List<StudySubject> { new StudySubject { Id = studySubjectId } }
            },
        };

        SetupRepoForAttachedWorkshops(workshops.Where(w => w.ProviderId == providerId).ToList());

        // Act
        var result = await workshopService.GetAttachedWorkshops(studySubjectId, providerId, 1, 10);

        // Assert
        var items = result.Items.ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("Art Fun", items[0].Title);
        Assert.IsTrue(items[0].IsAttached);
    }

    [Test]
    public async Task GetAttachedWorkshops_ReturnsEmpty_WhenNoWorkshops()
    {
        // Arrange
        SetupRepoForAttachedWorkshops(new List<Workshop>());

        // Act
        var result = await workshopService.GetAttachedWorkshops(Guid.NewGuid(), Guid.NewGuid(), 1, 10);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result.Items);
    }

    [Test]
    public void GetAttachedWorkshops_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        workshopRepository
            .Setup(m => m.GetByFilterNoTracking(
                It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>(), null))
            .Throws(new Exception("DB Failure"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            workshopService.GetAttachedWorkshops(studySubjectId, providerId, 1, 10));
        Assert.That(ex.Message, Is.EqualTo("DB Failure"));
    }

    [Test]
    public async Task GetAttachedWorkshops_RespectsPagination()
    {
        // Arrange
        var workshops = new List<Workshop>
    {
        new Workshop { Id = Guid.NewGuid(), Title = "A", ProviderId = providerId, StudySubjects = new List<StudySubject>() },
        new Workshop { Id = Guid.NewGuid(), Title = "B", ProviderId = providerId, StudySubjects = new List<StudySubject>() },
        new Workshop { Id = Guid.NewGuid(), Title = "C", ProviderId = providerId, StudySubjects = new List<StudySubject>() },
    };

        SetupRepoForAttachedWorkshops(workshops);

        // Act
        var result = await workshopService.GetAttachedWorkshops(Guid.NewGuid(), providerId, page: 2, pageSize: 1);

        // Assert
        var items = result.Items.ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("B", items[0].Title);
        Assert.AreEqual(3, result.TotalCount);
        Assert.AreEqual(3, result.TotalPages);
        Assert.AreEqual(2, result.Page);
    }

    [Test]
    public async Task GetAttachedWorkshops_SortsByTitleAscending()
    {
        // Arrange
        var workshops = new List<Workshop>
    {
        new Workshop { Id = Guid.NewGuid(), Title = "Zebra Workshop", ProviderId = providerId, StudySubjects = new List<StudySubject>() },
        new Workshop { Id = Guid.NewGuid(), Title = "Apple Workshop", ProviderId = providerId, StudySubjects = new List<StudySubject>() },
        new Workshop { Id = Guid.NewGuid(), Title = "Math Club", ProviderId = providerId, StudySubjects = new List<StudySubject>() },
    };

        SetupRepoForAttachedWorkshops(workshops);

        // Act
        var result = await workshopService.GetAttachedWorkshops(Guid.NewGuid(), providerId, 1, 10);

        // Assert
        var titles = result.Items.Select(x => x.Title).ToList();
        CollectionAssert.AreEqual(new[] { "Apple Workshop", "Math Club", "Zebra Workshop" }, titles);
    }

    #endregion

    #region GetWorkshopListByProviderId
    [Test]
    public async Task GetWorkshopListByProviderId_WhenProviderWithIdExists_ShouldReturnEntities()
    {
        // Arrange
        var workshops = WithWorkshopsList().ToList();
        SetupGetWorkshopsByProviderById(workshops);
        var expectedWorkshops = workshops.Select(w => new ShortEntityDto() { Id = w.Id, Title = w.Title }).OrderBy(x => x.Title).ToList();

        // Act
        var result = await workshopService.GetWorkshopListByProviderId(It.IsAny<Guid>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        result.Should().BeEquivalentTo(expectedWorkshops);
    }

    [Test]
    public async Task GetWorkshopListByProviderId_WhenThereIsNoEntityWithId_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyListWorkshops = new List<ShortEntityDto>();
        SetupGetWorkshopsByProviderById(new List<Workshop>());

        // Act
        var result = await workshopService.GetWorkshopListByProviderId(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        result.Should().BeEmpty();
    }

    [Test]
    [Ignore("Frontend asked to refactor without pagination, leave test for now maybe pagination will return")]
    public async Task GetWorkshopListByProviderId_WhenInvalidFilter_ShouldReturnException()
    {
        // Arrange
        var invalidFilter = new OffsetFilter() { From = -1, Size = -1 };

        // Act and Assert
        await workshopService.Invoking(w => w.GetWorkshopListByProviderId(Guid.NewGuid())).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    [Ignore("Frontend asked to refactor without pagination, leave test for now maybe pagination will return")]
    public async Task GetWorkshopListByProviderId_WhenFilterNull_ShouldReturnDefaultSize()
    {
        // Arrange
        var workshops = WithWorkshopsList().ToList();
        var expectedCount = 10;
        SetupGetWorkshopsByProviderById(workshops);
        SetupGetRepositoryCount(expectedCount);
        var expectedWorkshops = workshops.Select(w => new ShortEntityDto() { Id = w.Id, Title = w.Title }).Take(8).OrderBy(x => x.Title).ToList();

        // Act
        var result = await workshopService.GetWorkshopListByProviderId(It.IsAny<Guid>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        result.Should().BeEquivalentTo(expectedWorkshops);
    }
    #endregion

    #region Update
    [Test]
    public async Task Update_WhenEntityIsValid_ShouldReturnUpdatedEntity([Random(2, 5, 1)] int teachersInWorkshop)
    {
        // Arrange
        var changeFirstEntityDto = WorkshopCreateUpdateDtoGenerator.Generate();
        changeFirstEntityDto.DateTimeRanges = [];
        changeFirstEntityDto.AvailableSeats = 0;
        var changedFirstEntity = changeFirstEntityDto.SetToModel(WorkshopGenerator.Generate().WithApplications().WithAddress()); 
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity);
        var provider = ProvidersGenerator.Generate();
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.Provider = provider;
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.ToDto();

        applicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>())).ReturnsAsync(new List<WorkshopTakenSeats>());

        // Act
        var result = await workshopService.Update(changeFirstEntityDto).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task Update_WhenEntityIsValidAvailableSeatsIsNull_ShouldReturnUpdatedEntity([Random(2, 5, 1)] int teachersInWorkshop)
    {
        // Arrange
        var changeFirstEntityDto = WorkshopCreateUpdateDtoGenerator.Generate();
        changeFirstEntityDto.DateTimeRanges = [];
        var changedFirstEntity = changeFirstEntityDto.SetToModel(WorkshopGenerator.Generate().WithApplications().WithAddress());
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity);
        var provider = ProvidersGenerator.Generate();
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.Provider = provider;
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.ToDto();

        changeFirstEntityDto.AvailableSeats = null;

        applicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>())).ReturnsAsync(new List<WorkshopTakenSeats>());

        // Act
        var result = await workshopService.Update(changeFirstEntityDto).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    [TestCase(5U, uint.MaxValue, 5, WorkshopStatus.Closed, WorkshopStatus.Open)]
    [TestCase(5U, 5U, 5, WorkshopStatus.Open, WorkshopStatus.Closed)]
    [TestCase(5U, 3U, 5, WorkshopStatus.Open, WorkshopStatus.Closed)]
    public async Task Update_WhenDtoIsValid_ShouldChangeStatusAndInvokeUpdate(
        uint currentAvailableSeats,
        uint newAvailableSeats,
        int currentTakenSeats,
        WorkshopStatus currentWorkshopStatus,
        WorkshopStatus expectedWorkshopStatus)
    {
        // Arrange
        var changeFirstEntityDto = WorkshopCreateUpdateDtoGenerator.Generate();
        changeFirstEntityDto.DateTimeRanges = [];
        var changedFirstEntity = changeFirstEntityDto.SetToModel(WorkshopGenerator.Generate().WithAddress());
        changedFirstEntity.Applications = SetupApplications(changedFirstEntity, currentTakenSeats);
        changedFirstEntity.Teachers = TeachersGenerator.Generate(3).WithWorkshop(changedFirstEntity);
        changedFirstEntity.DateTimeRanges = [];
        changedFirstEntity.Provider = ProvidersGenerator.Generate();
        changedFirstEntity.Status = currentWorkshopStatus;
        changedFirstEntity.AvailableSeats = currentAvailableSeats;
        changedFirstEntity.Tags = TagsGenerator.Generate(6);

        SetupUpdate(changedFirstEntity);

        changeFirstEntityDto.AvailableSeats = newAvailableSeats;

        workshopRepository.Setup(x => x.Update(changedFirstEntity)).ReturnsAsync(changedFirstEntity);
        var workshopStatusDto = new WorkshopStatusDto()
        {
            WorkshopId = changedFirstEntity.Id,
            Status = expectedWorkshopStatus,
        };

        applicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>())).ReturnsAsync([new(changedFirstEntity.Id, currentTakenSeats)]);

        // Act
        var result = await workshopService.Update(changeFirstEntityDto).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        result.Should().NotBeNull();
        workshopRepository.Verify(x => x.Update(changedFirstEntity), Times.Once);
        changedFirstEntity.Status.Should().Be(expectedWorkshopStatus);
    }

    [Test]
    public async Task Update_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        WorkshopCreateUpdateDto workshopBaseDto = null;

        // Act and Assert
        await workshopService
            .Awaiting(m => m.Update(workshopBaseDto))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    [TestCase(4, false)]
    [TestCase(2, true)]
    public async Task Update_WhenTeachersWereDeletedBefore_ShouldReturnUpdatedEntity(int teachersInWorkshop, bool isDeleted)
    {
        // Arrange
        var changedFirstEntityDto = WorkshopCreateUpdateDtoGenerator.Generate();
        changedFirstEntityDto.DateTimeRanges = [];
        var changedFirstEntity = changedFirstEntityDto.SetToModel(WorkshopGenerator.Generate().WithApplications().WithAddress());
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity).WithIsDeleted(true);
        teachers.AddRange(TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity).WithIsDeleted(isDeleted));
        var provider = ProvidersGenerator.Generate();
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.DateTimeRanges = new List<DateTimeRange>();
        changedFirstEntity.Provider = provider;
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.ToNotDeletedDto();

        applicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>())).ReturnsAsync(new List<WorkshopTakenSeats>());

        // Act
        var result = await workshopService.Update(changedFirstEntityDto).ConfigureAwait(false);

        // Assert
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
    }

    [Test]
    public async Task Update_WithInvalidLanguageId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = WorkshopCreateUpdateDtoGenerator.Generate();
        dto.LanguageOfEducationId = 44444;

        languageServiceMock.Setup(x => x.GetById(dto.LanguageOfEducationId))
            .ReturnsAsync((LanguageDto)null);

        // Act & Assert
        await workshopService
            .Invoking(s => s.Update(dto))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Language with ID = {dto.LanguageOfEducationId}*");
    }

    [Test]
    public async Task UpdateV2_WithInvalidLanguageId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = WorkshopV2DtoGenerator.Generate();
        dto.LanguageOfEducationId = 77777;

        languageServiceMock.Setup(x => x.GetById(dto.LanguageOfEducationId))
            .ReturnsAsync((LanguageDto)null);

        // Act & Assert
        await workshopService
            .Invoking(s => s.UpdateV2(dto))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Language with ID = {dto.LanguageOfEducationId}*");
    }
    #endregion

    #region UpdateStatus

    [Test]
    public async Task UpdateStatus_FromOpenToClosed_WhenEntityIsValid_ShouldReturnUpdatedEntity()
    {
        // Arrange
        var workshopStatusDtoMock = WithWorkshopsList()
            .FirstOrDefault(w => w.Status == WorkshopStatus.Open
                                 && w.AvailableSeats != uint.MaxValue);
        var workshopStatusDto = new WorkshopStatusDto()
        {
            WorkshopId = Guid.NewGuid(),
            Status = WorkshopStatus.Closed,
        };

        workshopRepository.Setup(w => w.GetById(It.IsAny<Guid>())).ReturnsAsync(workshopStatusDtoMock);
        workshopRepository.Setup(w => w.Update(It.IsAny<Workshop>())).ReturnsAsync(workshopStatusDtoMock);

        // Act
        var result = await workshopService.UpdateStatus(workshopStatusDto).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
    }

    [Test]
    public void UpdateStatus_WhenEntityIsInvalid_ShouldReturn_ArgumentException()
    {
        // Arrange
        var workshopStatusDtoMock = WithWorkshopsList()
            .FirstOrDefault(w => w.Status == WorkshopStatus.Open
                                 && w.AvailableSeats == uint.MaxValue);
        var workshopStatusDto = new WorkshopStatusDto()
        {
            WorkshopId = Guid.NewGuid(),
            Status = WorkshopStatus.Closed,
        };

        workshopRepository.Setup(w => w.GetById(It.IsAny<Guid>())).ReturnsAsync(workshopStatusDtoMock);

        // Act and Assert
        workshopService.Invoking(w => w.UpdateStatus(workshopStatusDto)).Should().ThrowAsync<ArgumentException>();
        workshopRepository.VerifyAll();
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_WhenEntityWithIdExists_ShouldTryToDelete()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();
        SetupDelete(workshop);

        // Act
        await workshopService.Delete(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopRepository.Verify(w => w.Delete(It.IsAny<Workshop>()), Times.Once);
    }
    #endregion

    #region GetByFilter
    [Test]
    public async Task GetByFilter_WhenFilterIsNull_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var workshops = WithWorkshopsList();
        var guids = workshops.Select(w => w.Id);
        SetupGetByFilter(workshops, WithAvarageRatings(guids));

        // Act
        var result = await workshopService.GetByFilter(null).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(new SearchResult<WorkshopCard>()
        {
            Entities = workshops.ToCard().AsReadOnly(),
            TotalAmount = workshops.Count()
        });
    }

    [Test]
    public async Task GetByFilter_WhenFilterIsNotNull_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var workshops = WithWorkshopsList();
        var guids = workshops.Select(w => w.Id);
        SetupGetByFilter(workshops, WithAvarageRatings(guids));

        var filter = new WorkshopFilter()
        {
            Statuses = [WorkshopStatus.Open],
            IsSelfFinanced = true,
            IsInclusive = true,
            AreThereBenefits = true,
            AgeComposition = [AgeComposition.SameAge, AgeComposition.DifferentAge],
            EducationalShift = [EducationalShift.First],
            SpecialNeedsType = [SpecialNeedsType.Intelligence],
            Coverage = [Coverage.International],
        };

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(new SearchResult<WorkshopCard>() { 
            Entities = workshops.ToCard().AsReadOnly(), 
            TotalAmount = workshops.Count() 
        });
    }

    [Test]
    public async Task GetByFilter_WhenFilteredBySearchString_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var filter = new WorkshopFilter()
        {
            SearchText = "хореографічний, атлетика",
        };

        var workshops = WithWorkshopsList()
            .ToList();

        var expectedEntities = new List<Workshop>() { workshops[0], workshops[1] };
        var workshopIds = expectedEntities.Select(w => w.Id);
        SetupGetByFilter(expectedEntities, WithAvarageRatings(workshopIds));

        // Act
        var result = await workshopService.GetByFilter(filter)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(new SearchResult<WorkshopCard>()
            { Entities = expectedEntities.ToCard().AsReadOnly(), TotalAmount = expectedEntities.Count() });

        workshopRepository.VerifyAll();
        averageRatingServiceMock.VerifyAll();
    }

    #endregion

    #region GetPriceRange

    [Test]
    public async Task GetPriceRange_WhenFilterIsNull_ReturnsPriceRange()
    {
        // Arrange
        var workshops = WithWorkshopsList();
        SetupGetPriceRange(workshops);

        // Act
        var result = await workshopService.GetPriceRange(null).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedPriceRange(workshops));
    }

    [Test]
    public async Task GetPriceRange_WhenQueryIsEmpty_ReturnsDefaultPriceRange()
    {
        // Arrange
        var workshops = new List<Workshop>();
        var filter = new WorkshopFilter()
        {
            Statuses = [WorkshopStatus.Open],
            IsSelfFinanced = true,
            IsInclusive = true,
            AreThereBenefits = true,
            AgeComposition = [AgeComposition.SameAge, AgeComposition.DifferentAge],
            EducationalShift = [EducationalShift.First],
            SpecialNeedsType = [SpecialNeedsType.Intelligence],
            Coverage = [Coverage.International],
            PayRate = PayRateType.Day
        };
        SetupGetPriceRange(workshops);

        // Act
        var result = await workshopService.GetPriceRange(filter).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedPriceRange(workshops));
    }

    #endregion

    #region With
    private static IEnumerable<Workshop> WithWorkshopsList()
    {
        return new List<Workshop>()
        {
            new Workshop()
            {
                Id = new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                ProviderOwnership = OwnershipType.Private,
                Status = WorkshopStatus.Open,
                AvailableSeats = 30,
                Title = "10",
                DateTimeRanges = new List<DateTimeRange>(),
                Keywords = "хореографічний",
                Price = 100
            },
            new Workshop()
            {
                Id = new Guid("8c14044b-e30d-4b14-a18b-5b3b859ad676"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                ProviderOwnership = OwnershipType.State,
                Status = WorkshopStatus.Open,
                AvailableSeats = 30,
                Title = "9",
                Keywords = "атлетика",
                Price = 200
            },
            new Workshop()
            {
                Id = new Guid("3e8845a8-1359-4676-b6d6-5a6b29c122ea"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "1",
            },
            new Workshop()
            {
                Id = new Guid("7a8b0f29-28a5-48f8-bb7f-94dd9fec28c1"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "2",
            },
            new Workshop()
            {
                Id = new Guid("d17c1234-be9f-427d-a35b-59481becabd1"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "3",
            },
            new Workshop()
            {
                Id = new Guid("89a987a7-f2b4-4271-99d9-0ed532b0f18b"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "4",
            },
            new Workshop()
            {
                Id = new Guid("bbe56f28-321d-4bc9-84f9-4d8766aee70b"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "5",
            },
            new Workshop()
            {
                Id = new Guid("c0082ac7-9ea7-4acc-b6c5-9f1dddf395b9"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "6",
            },
            new Workshop()
            {
                Id = new Guid("6bf96311-ce4f-4a8a-aa7a-33dad46df4a6"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "7",
            },
            new Workshop()
            {
                Id = new Guid("9c8f8932-6eb9-4dd9-8661-cb2c0f9234e4"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Title = "8",
            },
        };
    }

    private static IEnumerable<AverageRatingDto> WithAvarageRatings(IEnumerable<Guid> workshopGuids)
    {
        return RatingsGenerator.GetAverageRatings(workshopGuids);
    }
    #endregion

    #region Setup
    private void SetupCreate(Workshop workshop, bool isMemberOfWorkshopIdExisted = false)
    {
        providerRepositoryMock.Setup(p => p.GetById(It.IsAny<Guid>()))
            .Returns(Task.FromResult(workshop.Provider));
        workshopRepository.Setup(w => w.Create(It.IsAny<Workshop>()))
           .ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());
        workshopRepository.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns((Func<Task<Workshop>> f) => f.Invoke());
    }

    private void SetupCreateV2(Workshop workshop, bool isMemberOfWorkshopIdExisted = false, int numberOfImages = 0)
    {
        if (isMemberOfWorkshopIdExisted)
        {
            workshopRepository.Setup(w => w.Any(It.IsAny<Expression<Func<Workshop, bool>>>()))
                .ReturnsAsync(true);
        }
        else
        {
            workshopRepository.Setup(w => w.Any(It.IsAny<Expression<Func<Workshop, bool>>>()))
                .ReturnsAsync(false);
        }

        workshopRepository.Setup(w => w.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.GetWithNavigations(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshop);
        providerRepositoryMock.Setup(p => p.GetById(It.IsAny<Guid>()))
            .Returns(Task.FromResult(workshop.Provider));
        workshopRepository.Setup(w => w.Create(It.IsAny<Workshop>()))
            .ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());

        languageServiceMock.Setup(s => s.GetById(It.IsAny<long>()))
            .ReturnsAsync((long id) => new LanguageDto { Id = id, Name = "English" });

        var multipleImageUploadingResult = new MultipleImageUploadingResult()
        {
            MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult(),
        };

        for (short i = 1; i <= numberOfImages; i++)
        {
            multipleImageUploadingResult.MultipleKeyValueOperationResult.Results
                .Add(new KeyValuePair<short, OperationResult>(i, OperationResult.Success));
        }

        workshopImagesMediator.Setup(i => i.AddManyImagesAsync(It.IsAny<Workshop>(), It.IsAny<IList<IFormFile>>()))
            .ReturnsAsync(multipleImageUploadingResult);

        var result = Result<string>.Success("string");
        workshopImagesMediator.Setup(i => i.AddCoverImageAsync(It.IsAny<Workshop>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(result);

        workshopRepository.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<ValueTuple<Workshop, MultipleImageUploadingResult, Result<string>>>>>()))
           .Returns((Func<Task<(Workshop, MultipleImageUploadingResult, Result<string>)>> f) => f.Invoke());
    }

    private void SetupGetAll(IEnumerable<Workshop> workshops, IEnumerable<AverageRatingDto> ratings)
    {
        var mockWorkshops = workshops.AsQueryable().BuildMock();
        var workshopGuids = workshops.Select(w => w.Id);

        workshopRepository.Setup(w => w.Get(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Expression<Func<Workshop, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
            .Returns(mockWorkshops);
        workshopRepository.Setup(
            w => w
                .Count(It.IsAny<Expression<Func<Workshop, bool>>>())).ReturnsAsync(workshops.Count());
        averageRatingServiceMock.Setup(r => r.GetByEntityIdsAsync(workshopGuids))
            .ReturnsAsync(ratings);
    }

    private void SetupGetById(Workshop workshop)
    {
        var workshopId = new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43");
        workshopRepository
            .Setup(
                w => w.GetById(workshopId))
            .ReturnsAsync(workshop);
        workshopRepository
            .Setup(
                w => w.GetWithNavigations(workshopId, It.IsAny<bool>()))
            .ReturnsAsync(workshop);
        averageRatingServiceMock.Setup(r => r.GetByEntityIdAsync(workshopId)).ReturnsAsync(new AverageRatingDto() { EntityId = workshop.Id });
    }

    private void SetupGetByProviderById(List<Workshop> workshopBaseCardsList, List<ChatRoomWorkshop> chatRoomsList)
    {
        var workshopGuids = workshopBaseCardsList.Select(w => w.Id);

        workshopRepository
            .Setup(
                w => w.Get(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
            .Returns(workshopBaseCardsList.AsTestAsyncEnumerableQuery);

        roomRepository
            .Setup(r => r.Get(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<ChatRoomWorkshop, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<ChatRoomWorkshop, object>>, SortDirection>>()))
            .Returns(chatRoomsList.AsTestAsyncEnumerableQuery);

        averageRatingServiceMock.Setup(r => r.GetByEntityIdsAsync(workshopGuids)).ReturnsAsync(WithAvarageRatings(workshopGuids));
    }

    private void SetupGetWorkshopsByProviderById(List<Workshop> workshopBaseCardsList)
    {
        workshopRepository
            .Setup(
                w => w.GetByFilter(
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<Func<IQueryable<Workshop>, IQueryable<Workshop>>>()))
            .ReturnsAsync(workshopBaseCardsList);
    }

    private void SetupGetRepositoryCount(int count)
    {
        workshopRepository
           .Setup(repo => repo.Count(It.IsAny<Expression<Func<Workshop, bool>>>()))
            .Returns(Task.FromResult(count));
    }

    private void SetupUpdate(Workshop workshop)
    {
        workshopRepository.Setup(w => w.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.GetWithNavigations(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(It.IsAny<int>());

        workshopRepository.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns((Func<Task<Workshop>> f) => f.Invoke());
    }

    private void SetupDelete(Workshop workshop)
    {
        workshopRepository.Setup(w => w.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.Delete(It.IsAny<Workshop>())).Returns(Task.CompletedTask);
    }

    private void SetupGetByFilter(IEnumerable<Workshop> workshops, IEnumerable<AverageRatingDto> ratings)
    {
        var queryableWorkshops = workshops.AsQueryable();
        workshopRepository.Setup(w => w
                .Count(It.IsAny<Expression<Func<Workshop, bool>>>()))
            .ReturnsAsync(workshops.Count());
        workshopRepository.Setup(w => w
            .Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
            .Returns(queryableWorkshops).Verifiable();
        averageRatingServiceMock.Setup(r => r
                .GetByEntityIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(ratings).Verifiable();
    }

    private List<Application> SetupApplications(Workshop workshop, int approvedApplications)
    {
        var allApplications = approvedApplications + 3;
        var applications = ApplicationGenerator.Generate(allApplications)
            .WithWorkshop(workshop)
            .WithParent(ParentGenerator.Generate())
            .WithChild(ChildGenerator.Generate());
        for (int i = 0; i < allApplications; i++)
        {
            applications[i].Status = i < approvedApplications
                ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
        }

        return applications;
    }

    private void SetupGetPriceRange(IEnumerable<Workshop> workshops)
    {
        var queryableWorkshops = workshops.AsQueryable().BuildMock();

        workshopRepository.Setup(w => w
        .Get(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Expression<Func<Workshop, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
        .Returns(queryableWorkshops).Verifiable();
    }

    private void SetupRepoForAttachedWorkshops(List<Workshop> workshops)
    {
        // Create an IQueryable<Workshop> to mock the repository behavior:
        // If the list is null or empty, return an empty async-compatible query,
        // otherwise, wrap the list in TestAsyncEnumerableQuery to simulate an EF Core async query.
        IQueryable<Workshop> queryable = workshops == null || workshops.Count == 0
        ? QueryableExtensions.AsEmptyTestAsyncEnumerableQuery<Workshop>()
        : new TestAsyncEnumerableQuery<Workshop>(workshops);

        workshopRepository
            .Setup(r => r.GetByFilterNoTracking(
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Workshop>, IQueryable<Workshop>>>()))
            .Returns(queryable);
    }

    #endregion

    #region Expected

    private PriceRange ExpectedPriceRange(IEnumerable<Workshop> workshops)
    {
        if (!workshops.Any())
        {
            return new PriceRange();
        }

        var minPrice = workshops.Min(w => w.Price);
        var maxPrice = workshops.Max(w => w.Price);
        return new PriceRange() { MinPrice = minPrice, MaxPrice = maxPrice };
    }

    #endregion
}