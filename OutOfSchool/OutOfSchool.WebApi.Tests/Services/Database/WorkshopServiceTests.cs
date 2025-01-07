using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
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
    private Mock<IMapper> mapperMock;
    private IMapper mapper;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IEmployeeRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ITagService> tagServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepository;
    private Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>> contactsServiceMock;

    [SetUp]
    public void SetUp()
    {
        workshopRepository = new Mock<IWorkshopRepository>();
        dateTimeRangeRepository = new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>();
        roomRepository = new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        mapperMock = new Mock<IMapper>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        providerAdminRepository = new Mock<IEmployeeRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        tagServiceMock = new Mock<ITagService>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        tagRepository = new Mock<IEntityRepository<long, Tag>>();
        contactsServiceMock = new Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>>();

        workshopService =
                new WorkshopService(
                    workshopRepository.Object,
                    tagRepository.Object,
                    dateTimeRangeRepository.Object,
                    roomRepository.Object,
                    teacherService.Object,
                    logger.Object,
                    mapperMock.Object,
                    workshopImagesMediator.Object,
                    providerAdminRepository.Object,
                    averageRatingServiceMock.Object,
                    providerRepositoryMock.Object,
                    currentUserServiceMock.Object,
                    ministryAdminServiceMock.Object,
                    regionAdminServiceMock.Object,
                    codeficatorServiceMock.Object,
                    tagServiceMock.Object,
                    searchStringServiceMock.Object,
                    contactsServiceMock.Object);
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
        var result = await workshopService.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        workshopRepository.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once);
    }

    [Test]
    public async Task Create_WhenEntityIsValidAndAvaliableSeatsIsNotNull_ShouldReturnThisEntity(
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
        var expectedTeachers = teachers.Select(mapper.Map<TeacherDTO>);
        var expectedTags = tags.Select(mapper.Map<TagDto>);
        SetupCreate(createdEntity);

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.Tags.Should().BeEquivalentTo(expectedTags);
        result.AvailableSeats.Should().Be((uint)availableSeats);
    }

    [Test]
    public async Task Create_WhenEntityIsValidAndAvaliableSeatsIsNull_ShouldReturnThisEntity(
        [Random(2, 5, 1)] int teachersInWorkshop,
        [Random(2, 8, 1)] int tagNumber)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(createdEntity);
        createdEntity.Teachers = teachers;
        var tags = TagsGenerator.Generate(tagNumber).WithWorkshop(createdEntity);
        createdEntity.Tags = tags;
        createdEntity.AvailableSeats = 0;
        var expectedTeachers = teachers.Select(mapper.Map<TeacherDTO>);
        var expectedTags = tags.Select(mapper.Map<TagDto>);
        SetupCreate(createdEntity);

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.Tags.Should().BeEquivalentTo(expectedTags);
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task Create_WhenDirectionsIdsAreWrong_ShouldReturnEntitiesWithRightDirectionsIds()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        createdEntity.InstitutionHierarchyId = Guid.NewGuid();
        SetupCreate(createdEntity);

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(createdEntity));
    }

    [Test]
    public async Task Create_WhenThereIsNotParentProvider_ShouldThrowNullReferenceException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        SetupCreate(createdEntity);

        // Act and Assert
        await workshopService.Invoking(w => w.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)))
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
        await workshopService.Invoking(w => w.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)))
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
        await workshopService.Invoking(w => w.Create(mapper.Map<WorkshopCreateRequestDto>(createdEntity)))
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
        var result = await workshopService.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        workshopRepository.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<(Workshop, MultipleImageUploadingResult, Result<string>)>>>()), Times.Once);
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndAvaliableSeatsIsNotNull_ShouldReturnThisEntity(
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
        var expectedTeachers = teachers.Select(mapper.Map<TeacherDTO>);
        var expectedTags = tags.Select(mapper.Map<TagDto>);
        SetupCreateV2(createdEntity);

        // Act
        var temp = mapper.Map<WorkshopV2CreateRequestDto>(createdEntity);

        var result = await workshopService.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Workshop.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.Workshop.Tags.Should().BeEquivalentTo(expectedTags);
        result.Workshop.AvailableSeats.Should().Be((uint)availableSeats);
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndAvaliableSeatsIsNull_ShouldReturnThisEntity(
        [Random(2, 5, 1)] int teachersInWorkshop,
        [Random(2, 8, 1)] int tagNumber)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(createdEntity);
        createdEntity.DateTimeRanges = new List<DateTimeRange>();
        createdEntity.Teachers = teachers;
        var tags = TagsGenerator.Generate(tagNumber).WithWorkshop(createdEntity);
        createdEntity.Tags = tags;
        createdEntity.AvailableSeats = 0;
        var expectedTeachers = teachers.Select(mapper.Map<TeacherDTO>);
        var expectedTags = tags.Select(mapper.Map<TagDto>);
        SetupCreateV2(createdEntity);

        // Act
        var result = await workshopService.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Workshop.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.Workshop.Tags.Should().BeEquivalentTo(expectedTags);
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
        var result = await workshopService.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Workshop.Should().BeEquivalentTo(ExpectedWorkshopV2DtoCreateSuccess(createdEntity));
    }

    [Test]
    public async Task CreateV2_WhenThereIsNotParentProvider_ShouldThrowNullReferenceException()
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate();
        SetupCreateV2(createdEntity);

        // Act and Assert
        await workshopService.Invoking(w => w.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)))
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
        await workshopService.Invoking(w => w.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)))
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
        await workshopService.Invoking(w => w.CreateV2(mapper.Map<WorkshopV2CreateRequestDto>(createdEntity)))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task CreateV2_WhenEntityIsValidAndImagesIsNotNull_ShouldReturnThisEntityWithUploadingImagesResultsEqualNumberOfImages([Random(1, 8, 1)] int numberOfImages)
    {
        // Arrange
        var createdEntity = WorkshopGenerator.Generate().WithProvider();
        SetupCreateV2(createdEntity, true, numberOfImages);
        var dto = mapper.Map<WorkshopV2CreateRequestDto>(createdEntity);
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
        var dto = mapper.Map<WorkshopV2CreateRequestDto>(createdEntity);
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
        var dto = mapper.Map<WorkshopV2CreateRequestDto>(createdEntity);
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
        var dto = mapper.Map<WorkshopV2CreateRequestDto>(createdEntity);

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
        result.Should().BeEquivalentTo(ExpectedWorkshopsGetAll(workshops));
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
        result.Should().BeEquivalentTo(ExpectedWorkshopGetByIdSuccess(id));
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
        mapperMock.Setup(m => m.Map<List<WorkshopProviderViewCard>>(It.IsAny<List<Workshop>>()))
            .Returns(emptyListWorkshopProviderViewCards);

        // Act
        var result = await workshopService.GetByProviderId(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()).ConfigureAwait(false);

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
        var workshopsProviderViewCards = mapper.Map<List<WorkshopProviderViewCard>>(workshops);

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

        mapperMock.Setup(m => m.Map<List<WorkshopProviderViewCard>>(It.IsAny<List<Workshop>>()))
            .Returns(workshopsProviderViewCards);

        SetupGetRepositoryCount(workshops.Count);
        SetupGetByProviderById(workshops, chatrooms);

        // Act
        var result = await workshopService.GetByProviderId(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        roomRepository.VerifyAll();
        result.TotalAmount.Should().Be(workshops.Count);
        result.Entities.Count.Should().Be(workshops.Count);
        result.Entities.Select(x => x.ProviderId).Should().Equal(workshops.Select(w => w.ProviderId));
        result.Entities.Select(x => x.WorkshopId).Should().Equal(workshops.Select(w => w.Id));
        result.Entities.Select(x => x.UnreadMessages).Should().Equal(expectedUnreadMessages);
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
        mapperMock.Setup(m => m.Map<List<ShortEntityDto>>(It.IsAny<List<Workshop>>())).Returns(expectedWorkshops);

        // Act
        var result = await workshopService.GetWorkshopListByProviderId(It.IsAny<Guid>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        mapperMock.VerifyAll();
        result.Should().BeEquivalentTo(expectedWorkshops);
    }

    [Test]
    public async Task GetWorkshopListByProviderId_WhenThereIsNoEntityWithId_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyListWorkshops = new List<ShortEntityDto>();
        SetupGetWorkshopsByProviderById(new List<Workshop>());
        mapperMock.Setup(m => m.Map<List<ShortEntityDto>>(It.IsAny<List<Workshop>>())).Returns(emptyListWorkshops);

        // Act
        var result = await workshopService.GetWorkshopListByProviderId(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        mapperMock.VerifyAll();
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
        mapperMock.Setup(m => m.Map<List<ShortEntityDto>>(It.IsAny<List<Workshop>>())).Returns(expectedWorkshops);

        // Act
        var result = await workshopService.GetWorkshopListByProviderId(It.IsAny<Guid>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        mapperMock.VerifyAll();
        result.Should().BeEquivalentTo(expectedWorkshops);
    }
    #endregion

    #region Update
    [Test]
    public async Task Update_WhenEntityIsValid_ShouldReturnUpdatedEntity([Random(2, 5, 1)] int teachersInWorkshop)
    {
        // Arrange
        var changedFirstEntity = WorkshopGenerator.Generate().WithApplications().WithAddress();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity);
        var provider = ProvidersGenerator.Generate();
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.DateTimeRanges = new List<DateTimeRange>();
        changedFirstEntity.Provider = provider;
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.Select(s => mapper.Map<TeacherDTO>(s));
        mapperMock.Setup(m => m.Map<WorkshopDto>(It.IsAny<Workshop>())).Returns(mapper.Map<WorkshopDto>(changedFirstEntity));

        // Act
        var result = await workshopService.Update(mapper.Map<WorkshopCreateUpdateDto>(changedFirstEntity)).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task Update_WhenEntityIsValidAvaliableSeatsIsNull_ShouldReturnUpdatedEntity([Random(2, 5, 1)] int teachersInWorkshop)
    {
        // Arrange
        var changedFirstEntity = WorkshopGenerator.Generate().WithApplications().WithAddress();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity);
        var provider = ProvidersGenerator.Generate();
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.DateTimeRanges = new List<DateTimeRange>();
        changedFirstEntity.Provider = provider;
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.Select(s => mapper.Map<TeacherDTO>(s));

        var changeFirstEntityDto = mapper.Map<WorkshopCreateUpdateDto>(changedFirstEntity);
        changeFirstEntityDto.AvailableSeats = null;

        mapperMock.Setup(m => m.Map<WorkshopDto>(It.IsAny<Workshop>())).Returns(mapper.Map<WorkshopDto>(changedFirstEntity));

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
        var changedFirstEntity = WorkshopGenerator.Generate().WithAddress();
        changedFirstEntity.Applications = SetupApplications(changedFirstEntity, currentTakenSeats);
        changedFirstEntity.Teachers = TeachersGenerator.Generate(3).WithWorkshop(changedFirstEntity);
        changedFirstEntity.DateTimeRanges = [];
        changedFirstEntity.Provider = ProvidersGenerator.Generate();
        changedFirstEntity.Status = currentWorkshopStatus;
        changedFirstEntity.AvailableSeats = currentAvailableSeats;
        changedFirstEntity.Tags = TagsGenerator.Generate(6);

        SetupUpdate(changedFirstEntity);

        var changeFirstEntityDto = mapper.Map<WorkshopCreateUpdateDto>(changedFirstEntity);
        changeFirstEntityDto.AvailableSeats = newAvailableSeats;

        workshopRepository.Setup(x => x.Update(changedFirstEntity)).ReturnsAsync(changedFirstEntity);
        var workshopStatusDto = new WorkshopStatusDto()
        {
            WorkshopId = changedFirstEntity.Id,
            Status = expectedWorkshopStatus,
        };
        mapperMock.Setup(m => m.Map<WorkshopStatusWithTitleDto>(It.IsAny<WorkshopStatusDto>()))
            .Returns(mapper.Map<WorkshopStatusWithTitleDto>(workshopStatusDto));
        mapperMock.Setup(m => m.Map<WorkshopDto>(It.IsAny<Workshop>())).Returns(mapper.Map<WorkshopDto>(changedFirstEntity));

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
        var changedFirstEntity = WorkshopGenerator.Generate().WithApplications().WithAddress();
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity).WithIsDeleted(true);
        teachers.AddRange(TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity).WithIsDeleted(isDeleted));
        var provider = ProvidersGenerator.Generate();
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.DateTimeRanges = new List<DateTimeRange>();
        changedFirstEntity.Provider = provider;
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.Where(t => !t.IsDeleted).Select(s => mapper.Map<TeacherDTO>(s));
        var techersToUpdate = mapper.Map<WorkshopCreateUpdateDto>(changedFirstEntity);

        mapperMock.Setup(m => m.Map<WorkshopDto>(It.IsAny<Workshop>())).Returns(mapper.Map<WorkshopDto>(changedFirstEntity));

        // Act
        var result = await workshopService.Update(techersToUpdate).ConfigureAwait(false);

        // Assert
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
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
        mapperMock.Setup(m => m.Map<WorkshopStatusWithTitleDto>(workshopStatusDto))
            .Returns(mapper.Map<WorkshopStatusWithTitleDto>(workshopStatusDto));

        // Act
        var result = await workshopService.UpdateStatus(workshopStatusDto).ConfigureAwait(false);
        var workshopStatusDto2 = mapper.Map<WorkshopStatusDto>(result);

        // Assert
        workshopRepository.VerifyAll();
        workshopStatusDto2.Should().BeEquivalentTo(workshopStatusDto);
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
        var guids = WithWorkshopsList().Select(w => w.Id);
        SetupGetByFilter(WithWorkshopsList(), WithAvarageRatings(guids));

        // Act
        var result = await workshopService.GetByFilter(null).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedSearchResultGetByFilter(WithWorkshopsList()));
    }

    [Test]
    public async Task GetByFilter_WhenFilterIsNotNull_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var guids = WithWorkshopsList().Select(w => w.Id);
        SetupGetByFilter(WithWorkshopsList(), WithAvarageRatings(guids));

        // Act
        var result = await workshopService.GetByFilter(It.IsAny<WorkshopFilter>()).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedSearchResultGetByFilter(WithWorkshopsList()));
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
        var expectedResult = ExpectedSearchResultGetByFilter(expectedEntities);
        SetupGetByFilter(expectedEntities, WithAvarageRatings(workshopIds));

        // Act
        var result = await workshopService.GetByFilter(filter)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedResult);

        workshopRepository.VerifyAll();
        averageRatingServiceMock.VerifyAll();
        mapperMock.VerifyAll();
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
        var workshopCreateUpdateDto = mapper.Map<WorkshopCreateRequestDto>(workshop);
        var workshopDto = mapper.Map<WorkshopDto>(workshop);

        if (workshopCreateUpdateDto.AvailableSeats is 0)
        {
            workshopCreateUpdateDto.AvailableSeats = uint.MaxValue;
            workshopDto.AvailableSeats = uint.MaxValue;
        }

        mapperMock.Setup(m => m.Map<WorkshopCreateRequestDto>(workshop))
            .Returns(workshopCreateUpdateDto);
        mapperMock.Setup(m => m.Map<WorkshopDto>(workshop))
            .Returns(workshopDto);
        mapperMock.Setup(m => m.Map<Workshop>(It.IsAny<WorkshopCreateRequestDto>()))
            .Returns(mapper.Map<Workshop>(workshopCreateUpdateDto));

        providerRepositoryMock.Setup(p => p.GetById(It.IsAny<Guid>()))
            .Returns(Task.FromResult(workshop.Provider));
        workshopRepository.Setup(w => w.Create(It.IsAny<Workshop>()))
           .ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());
        mapperMock.Setup(m => m.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()))
            .Returns(mapper.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()));
        workshopRepository.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns((Func<Task<Workshop>> f) => f.Invoke());
    }

    private void SetupCreateV2(Workshop workshop, bool isMemberOfWorkshopIdExisted = false, int numberOfImages = 0)
    {
        var workshopV2CreateRequestDto = mapper.Map<WorkshopV2CreateRequestDto>(workshop);
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);

        if (workshopV2Dto.AvailableSeats is 0)
        {
            workshopV2Dto.AvailableSeats = uint.MaxValue;
        }

        mapperMock.Setup(m => m.Map<WorkshopV2CreateRequestDto>(workshop))
           .Returns(workshopV2CreateRequestDto);
        mapperMock.Setup(m => m.Map<Workshop>(It.IsAny<WorkshopV2CreateRequestDto>()))
            .Returns(mapper.Map<Workshop>(workshopV2CreateRequestDto));
        mapperMock.Setup(m => m.Map<WorkshopV2Dto>(workshop))
            .Returns(workshopV2Dto);
        mapperMock.Setup(m => m.Map<Workshop>(It.IsAny<WorkshopV2Dto>()))
            .Returns(mapper.Map<Workshop>(workshopV2Dto));

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
        mapperMock.Setup(m => m.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()))
            .Returns(mapper.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()));

        var multipleImageUploadingResult = new MultipleImageUploadingResult()
        {
            MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult(),
        };

        var dictonary = new Dictionary<short, OperationResult>();
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
        var mappedDtos = workshops.Select(w => new WorkshopDto() { Id = w.Id }).ToList();

        workshopRepository.Setup(w => w.Get(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Workshop, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
            It.IsAny<bool>())).Returns(mockWorkshops);
        workshopRepository.Setup(
            w => w
                .Count(It.IsAny<Expression<Func<Workshop, bool>>>())).ReturnsAsync(workshops.Count());
        mapperMock.Setup(m => m.Map<List<WorkshopDto>>(It.IsAny<List<Workshop>>())).Returns(mappedDtos);
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
        mapperMock.Setup(m => m.Map<WorkshopDto>(workshop)).Returns(new WorkshopDto() { Id = workshop.Id });
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
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                    false))
            .Returns(workshopBaseCardsList.AsTestAsyncEnumerableQuery);

        roomRepository
            .Setup(r => r.Get(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<ChatRoomWorkshop, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<ChatRoomWorkshop, object>>, SortDirection>>(),
                    false))
            .Returns(chatRoomsList.AsTestAsyncEnumerableQuery);

        averageRatingServiceMock.Setup(r => r.GetByEntityIdsAsync(workshopGuids)).ReturnsAsync(WithAvarageRatings(workshopGuids));
    }

    private void SetupGetWorkshopsByProviderById(List<Workshop> workshopBaseCardsList)
    {
        workshopRepository
            .Setup(
                w => w.GetByFilter(
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<string>()))
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
        mapperMock.Setup(m => m.Map<WorkshopCreateUpdateDto>(workshop))
            .Returns(mapper.Map<WorkshopCreateUpdateDto>(workshop));
        mapperMock.Setup(m => m.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()))
            .Returns(mapper.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()));

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
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>())).Returns(queryableWorkshops).Verifiable();
        averageRatingServiceMock.Setup(r => r
                .GetByEntityIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(ratings).Verifiable();
        mapperMock
            .Setup(m => m.Map<List<WorkshopCard>>(workshops))
            .Returns(workshops
                .Select(w => new WorkshopCard() { ProviderId = w.ProviderId, WorkshopId = w.Id, }).ToList());
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
    #endregion

    #region Expected

    private WorkshopDto ExpectedWorkshopDtoCreateSuccess(Workshop workshop)
    {
        return mapperMock.Object.Map<WorkshopDto>(workshop);
    }

    private WorkshopV2Dto ExpectedWorkshopV2DtoCreateSuccess(Workshop workshop)
    {
        return mapperMock.Object.Map<WorkshopV2Dto>(workshop);
    }

    private SearchResult<WorkshopDto> ExpectedWorkshopsGetAll(IEnumerable<Workshop> workshops)
    {
        var mappeddtos = workshops
            .Select(w => new WorkshopDto()
            {
                Id = w.Id,
            });

        return new SearchResult<WorkshopDto>() { Entities = mappeddtos.ToList().AsReadOnly(), TotalAmount = workshops.Count() };
    }

    private WorkshopDto ExpectedWorkshopGetByIdSuccess(Guid id)
    {
        return new WorkshopDto() { Id = id };
    }

    private List<WorkshopCard> ExpectedWorkshopsGetByProviderId()
    {
        return new List<WorkshopCard>()
        {
            new WorkshopCard()
            {
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
            },
            new WorkshopCard()
            {
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
            },
            new WorkshopCard()
            {
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
            },
        };
    }

    private SearchResult<WorkshopCard> ExpectedSearchResultGetByFilter(IEnumerable<Workshop> workshops)
    {
        var mappeddtos = workshops
            .Select(w => new WorkshopCard()
            {
                ProviderId = w.ProviderId,
                WorkshopId = w.Id,
            });

        return new SearchResult<WorkshopCard>()
        { Entities = mappeddtos.ToList().AsReadOnly(), TotalAmount = workshops.Count() };
    }

    private Expression<Func<Workshop, bool>> ExpectedPredicateWithIds(WorkshopFilter filter)
    {
        var predicate = PredicateBuilder.True<Workshop>();
        predicate = predicate.And(x => filter.Ids.Any(g => g == x.Id));
        return predicate;
    }

    #endregion
}