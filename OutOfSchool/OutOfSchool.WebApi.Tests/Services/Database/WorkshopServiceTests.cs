using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
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
    private Mock<IProviderAdminRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;

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
        providerAdminRepository = new Mock<IProviderAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();

        workshopService =
            new WorkshopService(
                workshopRepository.Object,
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
                codeficatorServiceMock.Object);
    }

    #region Create
    [Test]
    public async Task Create_Whenever_ShouldRunInTransaction([Random(1, 100, 1)] long id)
    {
        // Arrange
        SetupCreate();
        var newWorkshop = new Workshop();

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopBaseDto>(newWorkshop)).ConfigureAwait(false);

        // Assert
        workshopRepository.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once);
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ShouldReturnThisEntity([Random(1, 100, 1)] long id)
    {
        // Arrange
        SetupCreate();
        var newWorkshop = new Workshop()
        {
            Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
            InstitutionHierarchyId = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
        };

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopBaseDto>(newWorkshop)).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(newWorkshop));
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task Create_WhenEntityIsValidAvaliableSeatsIsNull_ShouldReturnThisEntity([Random(1, 100, 1)] long id)
    {
        // Arrange
        SetupCreate();
        var newWorkshop = new Workshop()
        {
            Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
            InstitutionHierarchyId = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
        };

        var workshopDto = mapper.Map<WorkshopBaseDto>(newWorkshop);
        workshopDto.AvailableSeats = null;

        // Act
        var result = await workshopService.Create(workshopDto).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(newWorkshop));
        result.AvailableSeats.Should().Be(uint.MaxValue);
    }

    [Test]
    public async Task Create_WhenDirectionsIdsAreWrong_ShouldReturnEntitiesWithRightDirectionsIds([Random(1, 100, 1)] long id)
    {
        // Arrange
        SetupCreate();
        var newWorkshop = new Workshop
        {
            Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
            InstitutionHierarchyId = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
        };

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopBaseDto>(newWorkshop)).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(newWorkshop));
    }

    [Test]
    public void Create_WhenThereIsNoClassId_ShouldThrowArgumentException()
    {
        // Arrange
        SetupCreate();
        var newWorkshop = new Workshop()
        {
            InstitutionHierarchyId = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
        };

        // Act and Assert
        workshopService.Invoking(w => w.Create(mapper.Map<WorkshopBaseDto>(newWorkshop))).Should().ThrowAsync<ArgumentException>();
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

        // Act
        var result = await workshopService.Update(mapper.Map<WorkshopBaseDto>(changedFirstEntity)).ConfigureAwait(false);

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

        var changeFirstEntityDto = mapper.Map<WorkshopBaseDto>(changedFirstEntity);
        changeFirstEntityDto.AvailableSeats = null;

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

        SetupUpdate(changedFirstEntity);

        var changeFirstEntityDto = mapper.Map<WorkshopBaseDto>(changedFirstEntity);
        changeFirstEntityDto.AvailableSeats = newAvailableSeats;

        workshopRepository.Setup(x => x.Update(changedFirstEntity)).ReturnsAsync(changedFirstEntity);
        var workshopStatusDto = new WorkshopStatusDto()
        {
            WorkshopId = changedFirstEntity.Id,
            Status = expectedWorkshopStatus,
        };
        mapperMock.Setup(m => m.Map<WorkshopStatusWithTitleDto>(It.IsAny<WorkshopStatusDto>()))
            .Returns(mapper.Map<WorkshopStatusWithTitleDto>(workshopStatusDto));

        // Act
        var result = await workshopService.Update(changeFirstEntityDto).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        workshopRepository.Verify(x => x.Update(changedFirstEntity), Times.Once);
        changedFirstEntity.Status.Should().Be(expectedWorkshopStatus);
    }

    [Test]
    public async Task Update_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        WorkshopBaseDto workshopBaseDto = null;

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

        // Act
        var result = await workshopService.Update(mapper.Map<WorkshopBaseDto>(changedFirstEntity)).ConfigureAwait(false);

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

    #endregion

    #region FetchByFilterForAdmins
    

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
            },
            new Workshop()
            {
                Id = new Guid("8c14044b-e30d-4b14-a18b-5b3b859ad676"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                ProviderOwnership = OwnershipType.State,
                Status = WorkshopStatus.Open,
                AvailableSeats = 30,
                Title = "9",
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

    private void SetupCreate()
    {
        var provider = ProvidersGenerator.Generate();
        var id = Guid.NewGuid();

        providerRepositoryMock
            .Setup(p => p.GetById(It.IsAny<Guid>()))
            .Returns(Task.FromResult(provider));
        workshopRepository.Setup(
                w => w.Create(It.IsAny<Workshop>()))
            .ReturnsAsync((Workshop workshop) =>
            {
                return new Workshop
                {
                    Id = id,
                    AvailableSeats = workshop.AvailableSeats,
                };
            });
        workshopRepository.Setup(
                w => w.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns(async (Func<Task<Workshop>> func) =>
            {
                var workshop = await func();
                return new Workshop
                {
                    Id = id,
                    AvailableSeats = workshop.AvailableSeats,
                };
            });
        mapperMock.Setup(m => m.Map<WorkshopBaseDto>(It.IsAny<Workshop>()))
            .Returns((Workshop workshop) =>
            {
                var dto = new WorkshopBaseDto
                {
                    Id = id,
                    AvailableSeats = workshop.AvailableSeats,
                };

                return dto;
            });
        mapperMock.Setup(m => m.Map<Workshop>(It.IsAny<WorkshopBaseDto>()))
            .Returns((WorkshopBaseDto dto) =>
            {
                var workshop = new Workshop
                {
                    Id = id,
                    AvailableSeats = (uint)dto.AvailableSeats,
                };

                return workshop;
            });
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
        workshopRepository.Setup(w => w.UnitOfWork.CompleteAsync()).ReturnsAsync(It.IsAny<int>());
        mapperMock.Setup(m => m.Map<WorkshopBaseDto>(workshop))
            .Returns(mapper.Map<WorkshopBaseDto>(workshop));
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

    private WorkshopBaseDto ExpectedWorkshopDtoCreateSuccess(Workshop workshop)
    {
        return mapperMock.Object.Map<WorkshopBaseDto>(workshop);
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