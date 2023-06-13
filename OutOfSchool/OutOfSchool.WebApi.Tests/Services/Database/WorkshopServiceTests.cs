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
//using Nest;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class WorkshopServiceTests
{
    private IWorkshopService workshopService;
    private Mock<IWorkshopRepository> workshopRepository;
    private Mock<IEntityRepository<long, DateTimeRange>> dateTimeRangeRepository;
    private Mock<ITeacherService> teacherService;
    private Mock<ILogger<WorkshopService>> logger;
    private Mock<IMapper> mapperMock;
    private IMapper mapper;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IProviderAdminRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        workshopRepository = new Mock<IWorkshopRepository>();
        dateTimeRangeRepository = new Mock<IEntityRepository<long, DateTimeRange>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        mapperMock = new Mock<IMapper>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        providerAdminRepository = new Mock<IProviderAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();

        workshopService =
            new WorkshopService(
                workshopRepository.Object,
                dateTimeRangeRepository.Object,
                teacherService.Object,
                logger.Object,
                mapperMock.Object,
                workshopImagesMediator.Object,
                providerAdminRepository.Object,
                averageRatingServiceMock.Object,
                providerRepositoryMock.Object);
    }

    #region Create
    [Test]
    public async Task Create_Whenever_ShouldRunInTransaction([Random(1, 100, 1)] long id)
    {
        // Arrange
        SetupCreate();
        var newWorkshop = new Workshop();

        // Act
        var result = await workshopService.Create(mapper.Map<WorkshopDTO>(newWorkshop)).ConfigureAwait(false);

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
        var result = await workshopService.Create(mapper.Map<WorkshopDTO>(newWorkshop)).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(newWorkshop));
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
        var result = await workshopService.Create(mapper.Map<WorkshopDTO>(newWorkshop)).ConfigureAwait(false);

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
        workshopService.Invoking(w => w.Create(mapper.Map<WorkshopDTO>(newWorkshop))).Should().ThrowAsync<ArgumentException>();
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
        SetupGetById(WithWorkshop(id));

        // Act
        var result = await workshopService.GetById(id).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedWorkshopGetByIdSuccess(id));
    }

    [Test]
    public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull()
    {
        // Arrange
        var id = new Guid("2a9fcc6d-6c7d-4711-849d-aa8991337185");
        SetupGetById(WithWorkshop(id));

        // Act
        var result = await workshopService.GetById(It.IsAny<Guid>()).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }
    #endregion

    #region GetByProviderId
    [Test]
    public async Task GetByProviderId_WhenProviderWithIdExists_ShouldReturnEntities()
    {
        // Arrange
        var workshops = WithWorkshopsList().ToList();
        var expectedWorkshopBaseCards = workshops.Select(w => new WorkshopBaseCard() { ProviderId = w.ProviderId, WorkshopId = w.Id, }).ToList();

        SetupGetRepositoryCount(10);
        SetupGetByProviderById(workshops);

        mapperMock.Setup(m => m.Map<List<WorkshopBaseCard>>(It.IsAny<List<Workshop>>())).Returns(expectedWorkshopBaseCards);

        // Act
        var result = await workshopService.GetByProviderId<WorkshopBaseCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        mapperMock.VerifyAll();
        (result as SearchResult<WorkshopBaseCard>).TotalAmount.Should().Be(workshops.Count);
        (result as SearchResult<WorkshopBaseCard>).Entities.Should().BeEquivalentTo(expectedWorkshopBaseCards);
    }

    [Test]
    public async Task GetByProviderId_WhenThereIsNoEntityWithId_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyListWorkshopCards = new List<WorkshopBaseCard>();
        SetupGetRepositoryCount(0);
        SetupGetByProviderById(new List<Workshop>());
        mapperMock.Setup(m => m.Map<List<WorkshopBaseCard>>(It.IsAny<List<Workshop>>())).Returns(emptyListWorkshopCards);

        // Act
        var result = await workshopService.GetByProviderId<WorkshopBaseCard>(Guid.NewGuid(), It.IsAny<ExcludeIdFilter>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        mapperMock.VerifyAll();
        result.TotalAmount.Should().Be(0);
        result.Entities.Should().BeEmpty();
    }

    [Test]
    public async Task GetByProviderId_WhenThereIsExcludedIds_ShouldReturnList()
    {
        // Arrange
        var workshops = WithWorkshopsList().ToList();
        var excludedIds = new List<Guid>() { new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43"), new Guid("8c14044b-e30d-4b14-a18b-5b3b859ad676") };
        var expectedWorkshopBaseCards = workshops.Select(w => new WorkshopBaseCard() { ProviderId = w.ProviderId })
            .Where(w => !excludedIds.Any(excluded => w.WorkshopId != excluded)).ToList();

        SetupGetRepositoryCount(10);
        SetupGetByProviderById(workshops);

        mapperMock.Setup(m => m.Map<List<WorkshopBaseCard>>(It.IsAny<List<Workshop>>())).Returns(expectedWorkshopBaseCards);

        // Act
        var result = await workshopService.GetByProviderId<WorkshopBaseCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        mapperMock.VerifyAll();
        (result as SearchResult<WorkshopBaseCard>).TotalAmount.Should().Be(workshops.Count);
        (result as SearchResult<WorkshopBaseCard>).Entities.Should().BeEquivalentTo(expectedWorkshopBaseCards);
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
        var id = new Guid("ca2cc30c-419c-4b00-a344-b23f0cbf18d8");
        var changedFirstEntity = WithWorkshop(id);
        var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshop(changedFirstEntity);
        changedFirstEntity.Teachers = teachers;
        changedFirstEntity.DateTimeRanges = new List<DateTimeRange>();
        SetupUpdate(changedFirstEntity);
        var expectedTeachers = teachers.Select(s => mapper.Map<TeacherDTO>(s));

        // Act
        var result = await workshopService.Update(mapper.Map<WorkshopDTO>(changedFirstEntity)).ConfigureAwait(false);

        // Assert
        result.Teachers.Should().BeEquivalentTo(expectedTeachers);
    }

    [Test]
    public void Update_WhenClassIdIsInvalid_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = new Guid("f1b73d56-ce9f-47fc-85fe-94bf72ebd3e4");
        var changedEntity = WithWorkshop(id);
        SetupUpdate(changedEntity);

        // Act and Assert
        workshopService.Invoking(w => w.Update(mapper.Map<WorkshopDTO>(changedEntity))).Should().ThrowAsync<ArgumentException>();
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

    [Test]
    public async Task Update_WhenTryUpdateStatus_ShouldReturnEntityWithOldStatus([Random(1, 100, 1)] long classId)
    {
        // Arrange
        var inputWorkshopDto = WithWorkshop(Guid.NewGuid());
        inputWorkshopDto.Status = WorkshopStatus.Closed;
        var expectedStatus = WorkshopStatus.Open;
        var workshopDtoMock = WithWorkshop(Guid.NewGuid());

        workshopRepository.Setup(w => w.GetWithNavigations(It.IsAny<Guid>())).ReturnsAsync(workshopDtoMock);
        workshopRepository.Setup(w => w.UnitOfWork.CompleteAsync()).ReturnsAsync(It.IsAny<int>());
        mapperMock.Setup(m => m.Map<WorkshopDTO>(workshopDtoMock))
            .Returns(mapper.Map<WorkshopDTO>(workshopDtoMock));

        mapperMock.Setup(m => m.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()))
            .Returns(mapper.Map<List<DateTimeRange>>(It.IsAny<List<DateTimeRangeDto>>()));

        workshopRepository.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .Returns((Func<Task<Workshop>> f) => f.Invoke());

        // Act
        var result = await workshopService.Update(mapper.Map<WorkshopDTO>(inputWorkshopDto)).ConfigureAwait(false);

        // Assert
        workshopRepository.VerifyAll();
        Assert.AreEqual(expectedStatus, result.Status);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_WhenEntityWithIdExists_ShouldTryToDelete()
    {
        // Arrange
        var id = Guid.NewGuid();
        SetupDelete(WithWorkshop(id));

        // Act
        await workshopService.Delete(id).ConfigureAwait(false);

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

    private static Workshop WithWorkshop(Guid id) => new Workshop()
    {
        Id = id,
        Title = "ChangedTitle",
        Phone = "1111111111",
        Price = 1000,
        WithDisabilityOptions = true,
        ProviderTitle = "ProviderTitle",
        DisabilityOptionsDesc = "Desc1",
        Website = "website1",
        Instagram = "insta1",
        Facebook = "facebook1",
        Email = "email1@gmail.com",
        MaxAge = 10,
        MinAge = 4,
        ProviderOwnership = OwnershipType.Private,
        Status = WorkshopStatus.Open,
        CoverImageId = "image1",
        ProviderId = new Guid("65eb933f-6502-4e89-a7cb-65901e51d119"),
        InstitutionHierarchyId = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
        AddressId = 55,
        Address = new Address
        {
            Id = 55,
            CATOTTGId = 4970,
            Street = "Street55",
            BuildingNumber = "BuildingNumber55",
            Latitude = 10,
            Longitude = 10,
        },
        WorkshopDescriptionItems = new[]
        {
            new WorkshopDescriptionItem()
            {
                Id = Guid.NewGuid(),
                SectionName = "Workshop description heading 1",
                Description = "Workshop description text 1",
            },
        },
        DateTimeRanges = new List<DateTimeRange>()
        {
            new DateTimeRange
            {
                Id = It.IsAny<long>(),
                EndTime = It.IsAny<TimeSpan>(),
                StartTime = It.IsAny<TimeSpan>(),
                Workdays = default,
            },
        },
        Teachers = new List<Teacher>(),
        //{
        //    new Teacher
        //    {
        //        Id = Guid.NewGuid(),
        //        FirstName = "Alex",
        //        LastName = "Brown",
        //        MiddleName = "SomeMiddleName",
        //        Description = "Description",
        //        DateOfBirth = DateTime.Parse("2000-01-01"),
        //        WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
        //    },
        //},
    };

    private Workshop WithNullWorkshopEntity()
    {
        return null;
    }

    #endregion

    #region Setup

    private void SetupCreate()
    {
        var provider = ProvidersGenerator.Generate();

        providerRepositoryMock
            .Setup(p => p.GetById(It.IsAny<Guid>()))
            .Returns(Task.FromResult(provider));
        workshopRepository.Setup(
                w => w.Create(It.IsAny<Workshop>()))
            .Returns(Task.FromResult(It.IsAny<Workshop>()));
        workshopRepository.Setup(
                w => w.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
            .ReturnsAsync(new Workshop() { Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e") });
        mapperMock.Setup(m => m.Map<WorkshopDTO>(It.IsAny<Workshop>()))
            .Returns(new WorkshopDTO() { Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e") });
        mapperMock.Setup(m => m.Map<Workshop>(It.IsAny<WorkshopDTO>()))
            .Returns(new Workshop() { Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e") });
    }

    private void SetupGetAll(IEnumerable<Workshop> workshops, IEnumerable<AverageRatingDto> ratings)
    {
        var mockWorkshops = workshops.AsQueryable().BuildMock();
        var workshopGuids = workshops.Select(w => w.Id);
        var mappedDtos = workshops.Select(w => new WorkshopDTO() { Id = w.Id }).ToList();

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
        mapperMock.Setup(m => m.Map<List<WorkshopDTO>>(It.IsAny<List<Workshop>>())).Returns(mappedDtos);
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
                w => w.GetWithNavigations(workshopId))
            .ReturnsAsync(workshop);
        mapperMock.Setup(m => m.Map<WorkshopDTO>(workshop)).Returns(new WorkshopDTO() { Id = workshop.Id });
        averageRatingServiceMock.Setup(r => r.GetByEntityIdAsync(workshopId)).ReturnsAsync(new AverageRatingDto() { EntityId = workshop.Id });
    }

    private void SetupGetByProviderById(List<Workshop> workshopBaseCardsList)
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
        workshopRepository.Setup(w => w.GetWithNavigations(It.IsAny<Guid>())).ReturnsAsync(workshop);
        workshopRepository.Setup(w => w.UnitOfWork.CompleteAsync()).ReturnsAsync(It.IsAny<int>());
        mapperMock.Setup(m => m.Map<WorkshopDTO>(workshop))
            .Returns(mapper.Map<WorkshopDTO>(workshop));
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

    #endregion

    #region Expected

    private WorkshopDTO ExpectedWorkshopDtoCreateSuccess(Workshop workshop)
    {
        return mapperMock.Object.Map<WorkshopDTO>(workshop);
    }

    private SearchResult<WorkshopDTO> ExpectedWorkshopsGetAll(IEnumerable<Workshop> workshops)
    {
        var mappeddtos = workshops
            .Select(w => new WorkshopDTO()
            {
                Id = w.Id,
            });

        return new SearchResult<WorkshopDTO>() { Entities = mappeddtos.ToList().AsReadOnly(), TotalAmount = workshops.Count() };
    }

    private WorkshopDTO ExpectedWorkshopGetByIdSuccess(Guid id)
    {
        return new WorkshopDTO() { Id = id };
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