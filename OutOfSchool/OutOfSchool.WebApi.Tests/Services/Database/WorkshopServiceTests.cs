using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Nest;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class WorkshopServiceTests
    {
        private IWorkshopService workshopService;
        private Mock<IWorkshopRepository> workshopRepository;
        private Mock<IClassRepository> classRepository;
        private Mock<IRatingService> ratingService;
        private Mock<ITeacherService> teacherService;
        private Mock<ILogger<WorkshopService>> logger;
        private Mock<IMapper> mapper;
        private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;

        [SetUp]
        public void SetUp()
        {
            workshopRepository = new Mock<IWorkshopRepository>();
            classRepository = new Mock<IClassRepository>();
            ratingService = new Mock<IRatingService>();
            teacherService = new Mock<ITeacherService>();
            logger = new Mock<ILogger<WorkshopService>>();
            mapper = new Mock<IMapper>();
            workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
            workshopService =
                new WorkshopService(
                    workshopRepository.Object,
                    classRepository.Object,
                    ratingService.Object,
                    teacherService.Object,
                    logger.Object,
                    mapper.Object,
                    workshopImagesMediator.Object);
        }

        #region Create
        [Test]
        public async Task Create_Whenever_ShouldRunInTransaction([Random(1, 100, 1)] long id)
        {
            // Arrange
            SetupCreate(WithClassEntity(id));
            var newWorkshop = new Workshop();

            // Act
            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

            // Assert
            workshopRepository.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once);
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ShouldReturnThisEntity([Random(1, 100, 1)] long id)
        {
            // Arrange
            SetupCreate(WithClassEntity(id));
            var newWorkshop = new Workshop()
            {
                Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e"),
                DepartmentId = 1,
                DirectionId = 1,
            };

            // Act
            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(newWorkshop));
        }

        [Test]
        public async Task Create_WhenDirectionsIdsAreWrong_ShouldReturnEntitiesWithRightDirectionsIds([Random(1, 100, 1)] long id)
        {
            // Arrange
            SetupCreate(WithClassEntity(id));
            var newWorkshop = new Workshop() { Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e") };
            newWorkshop.DepartmentId = 10;
            newWorkshop.DirectionId = 10;

            // Act
            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedWorkshopDtoCreateSuccess(newWorkshop));
        }

        [Test]
        public void Create_WhenThereIsNoClassId_ShouldThrowArgumentException()
        {
            // Arrange
            SetupCreate(WithNullClassEntity());
            var newWorkshop = new Workshop()
            {
                DepartmentId = 1,
                DirectionId = 1,
            };

            // Act and Assert
            workshopService.Invoking(w => w.Create(newWorkshop.ToModel())).Should().Throw<ArgumentException>();
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
            var id = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174");
            SetupGetByProviderId(WithWorkshopsList());

            // Act
            var result = await workshopService.GetByProviderId(id).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedWorkshopsGetByProviderId());
        }


        [Test]
        public async Task GetByProviderId_WhenThereIsNoEntityWithId_ShouldReturnEmptyList()
        {
            // Arrange
            var id = new Guid("db32b84c-18dd-4cbc-b7f9-fe29647a6aba");
            SetupGetByProviderIdWithEmptyList();

            // Act
            var result = await workshopService.GetByProviderId(id).ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region Update
        [Test]
        public async Task Update_WhenEntityIsValid_ShouldReturnUpdatedEntity([Random(2, 5, 1)] int teachersInWorkshop, [Random(1, 100, 1)] long classId)
        {
            // Arrange
            var id = new Guid("ca2cc30c-419c-4b00-a344-b23f0cbf18d8");
            var changedFirstEntity = WithWorkshop(id);
            var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshopId(changedFirstEntity.Id);
            changedFirstEntity.Teachers = teachers;
            SetupUpdate(changedFirstEntity, WithClassEntity(classId));
            var expectedTeachers = teachers.Select(s => s.ToModel());

            // Act
            var result = await workshopService.Update(changedFirstEntity.ToModel()).ConfigureAwait(false);

            // Assert
            result.Teachers.Should().BeEquivalentTo(expectedTeachers);
        }

        [Test]
        public void Update_WhenClassIdIsInvalid_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var id = new Guid("f1b73d56-ce9f-47fc-85fe-94bf72ebd3e4");
            var changedEntity = WithWorkshop(id);
            SetupUpdate(changedEntity, WithNullClassEntity());

            // Act and Assert
            workshopService.Invoking(w => w.Update(changedEntity.ToModel())).Should().Throw<ArgumentException>();
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

        #region Setup

        private void SetupCreate(Class classEntity)
        {
            workshopRepository.Setup(
                    w => w.Create(It.IsAny<Workshop>()))
                .Returns(Task.FromResult(It.IsAny<Workshop>()));
            classRepository.Setup(
                c => c.GetById(It.IsAny<long>()))
                .ReturnsAsync(classEntity);
            workshopRepository.Setup(
                    w => w.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
                .ReturnsAsync(new Workshop() { Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e") });
            mapper.Setup(m => m.Map<WorkshopDTO>(It.IsAny<Workshop>()))
                .Returns(new WorkshopDTO() { Id = new Guid("8f91783d-a68f-41fa-9ded-d879f187a94e") });
        }

        private void SetupGetAll(IEnumerable<Workshop> workshops, Dictionary<Guid, Tuple<float, int>> ratings)
        {
            var mockWorkshops = workshops.AsQueryable().BuildMock();
            var workshopGuids = workshops.Select(w => w.Id);
            var mappedDtos = workshops.Select(w => new WorkshopDTO() { Id = w.Id }).ToList();

            workshopRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<bool>())).Returns(mockWorkshops.Object);
            workshopRepository.Setup(
                w => w
                    .Count(It.IsAny<Expression<Func<Workshop, bool>>>())).ReturnsAsync(workshops.Count());
            mapper.Setup(m => m.Map<List<WorkshopDTO>>(It.IsAny<List<Workshop>>())).Returns(mappedDtos);
            ratingService.Setup(r => r.GetAverageRatingForRange(workshopGuids, RatingType.Workshop))
                .Returns(ratings);
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
            mapper.Setup(m => m.Map<WorkshopDTO>(workshop)).Returns(new WorkshopDTO() { Id = workshop.Id });

        }

        private void SetupGetByProviderId(IEnumerable<Workshop> workshops)
        {
            var mappedDtos = workshops.Select(w => new WorkshopCard() { ProviderId = w.ProviderId }).ToList();
            workshopRepository
                .Setup(
                    w => w.GetByFilter(
                        It.IsAny<Expression<Func<Workshop, bool>>>(),
                        It.IsAny<string>()))
                .ReturnsAsync(workshops);
            mapper.Setup(m => m.Map<List<WorkshopCard>>(It.IsAny<List<Workshop>>())).Returns(mappedDtos);
        }

        private void SetupGetByProviderIdWithEmptyList()
        {
            var emptylistWorkshops = new List<Workshop>();
            var emptylistWorkshopCards = new List<WorkshopCard>();
            workshopRepository
                .Setup(
                    w => w.GetByFilter(
                        It.IsAny<Expression<Func<Workshop, bool>>>(),
                        It.IsAny<string>()))
                .ReturnsAsync(emptylistWorkshops);
            mapper.Setup(m => m.Map<List<WorkshopCard>>(It.IsAny<List<Workshop>>())).Returns(emptylistWorkshopCards);
        }

        private void SetupUpdate(Workshop workshop, Class classentity)
        {
            classRepository.Setup(c => c.GetById(It.IsAny<long>())).ReturnsAsync(classentity);
            workshopRepository.Setup(w => w.GetWithNavigations(It.IsAny<Guid>())).ReturnsAsync(workshop);
            workshopRepository.Setup(w => w.UnitOfWork.CompleteAsync()).ReturnsAsync(It.IsAny<int>());
            mapper.Setup(m => m.Map<WorkshopDTO>(workshop))
                .Returns(workshop.ToModel());
        }

        private void SetupDelete(Workshop workshop)
        {
            workshopRepository.Setup(w => w.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
            workshopRepository.Setup(w => w.Delete(It.IsAny<Workshop>())).Returns(Task.CompletedTask);
        }

        private void SetupGetByFilter(IEnumerable<Workshop> workshops, Dictionary<Guid, Tuple<float, int>> ratings)
        {
            var queryableWorkshops = workshops.AsQueryable();
            workshopRepository.Setup(w => w
                    .Count(It.IsAny<Expression<Func<Workshop, bool>>>()))
                .ReturnsAsync(workshops.Count());
            workshopRepository.Setup(w => w
                .Get<dynamic>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Expression<Func<Workshop, dynamic>>>(),
                    It.IsAny<bool>())).Returns(queryableWorkshops).Verifiable();
            ratingService.Setup(r => r
                    .GetAverageRatingForRange(It.IsAny<IEnumerable<Guid>>(), RatingType.Workshop))
                .Returns(ratings).Verifiable();
            mapper
                .Setup(m => m.Map<List<WorkshopCard>>(workshops))
                .Returns(workshops
                .Select(w => new WorkshopCard() { ProviderId = w.ProviderId }).ToList());
        }

        #endregion

        #region With

        private Class WithClassEntity(long id)
        {
            return new Class()
            {
                Id = id,
                DepartmentId = 1,
                Department = new Department() { Id = 1, DirectionId = 1 },
            };
        }

        private Class WithNullClassEntity()
        {
            return null;
        }

        private IEnumerable<Workshop> WithWorkshopsList()
        {
            return new List<Workshop>()
            {
                new Workshop()
                {
                    Id = new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                },
                new Workshop()
                {
                    Id = new Guid("8c14044b-e30d-4b14-a18b-5b3b859ad676"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                },
                new Workshop()
                {
                    Id = new Guid("3e8845a8-1359-4676-b6d6-5a6b29c122ea"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                },
            };
        }

        private Dictionary<Guid, Tuple<float, int>> WithAvarageRatings(IEnumerable<Guid> workshopGuids)
        {
            var dictionary = new Dictionary<Guid, Tuple<float, int>>();
            foreach (var guid in workshopGuids)
            {
                dictionary.Add(guid, new Tuple<float, int>(4.2f, 5));
            }
            return dictionary;
        }

        private Workshop WithWorkshop(Guid id)
        {
            return new Workshop()
            {
                Id = id,
                Title = "ChangedTitle",
                Phone = "1111111111",
                Description = "Desc1",
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
                CoverImageId = "image1",
                ProviderId = new Guid("65eb933f-6502-4e89-a7cb-65901e51d119"),
                DirectionId = 1,
                ClassId = 1,
                DepartmentId = 1,
                AddressId = 55,
                Address = new Address
                {
                    Id = 55,
                    Region = "Region55",
                    District = "District55",
                    City = "City55",
                    Street = "Street55",
                    BuildingNumber = "BuildingNumber55",
                    Latitude = 10,
                    Longitude = 10,
                },
            };
        }

        private Workshop WithNullWorkshopEntity()
        {
            return null;
        }

        #endregion

        #region Expected

        private WorkshopDTO ExpectedWorkshopDtoCreateSuccess(Workshop workshop)
        {
            return mapper.Object.Map<WorkshopDTO>(workshop);
        }

        private SearchResult<WorkshopDTO> ExpectedWorkshopsGetAll(IEnumerable<Workshop> workshops)
        {
            var mappeddtos = workshops
                .Select(w => new WorkshopDTO()
                {
                    Id = w.Id,
                    Rating = 4.2f,
                    NumberOfRatings = 5,
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
}
