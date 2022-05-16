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
using OutOfSchool.Redis;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatisticServiceTest
    {
        private IStatisticService service;

        private Mock<IApplicationRepository> applicationRepository;
        private Mock<IWorkshopRepository> workshopRepository;
        private Mock<IDirectionRepository> directionRepository;

        private Mock<IMapper> mapper;
        private Mock<ICacheService> cache;

        [SetUp]
        public void SetUp()
        {
            applicationRepository = new Mock<IApplicationRepository>();
            workshopRepository = new Mock<IWorkshopRepository>();
            directionRepository = new Mock<IDirectionRepository>();
            var ratingService = new Mock<IRatingService>();
            var logger = new Mock<ILogger<StatisticService>>();
            mapper = new Mock<IMapper>();
            cache = new Mock<ICacheService>();

            service = new StatisticService(
                applicationRepository.Object,
                workshopRepository.Object,
                ratingService.Object,
                directionRepository.Object,
                logger.Object,
                mapper.Object,
                cache.Object);
        }

        [Test]
        public async Task GetPopularWorkshops_WhenCityNotQueried_ShouldReturnCertainWorkshops()
        {
            // Arrange
            List<WorkshopCard> expectedWorkshopCards = ExpectedWorkshopCardsNoCityFilter();

            SetupGetPopularWorkshops();

            mapper.Setup(m => m.Map<List<WorkshopCard>>(It.IsAny<List<Workshop>>()))
                .Returns(expectedWorkshopCards);

            // Act
            var result = await service
                .GetPopularWorkshopsFromDatabase(2, string.Empty)
                .ConfigureAwait(false);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    expectedWorkshopCards, options => options.WithStrictOrdering());
        }

        [Test]
        public async Task GetPopularWorkshops_WithCityQueried_ShouldReturnCertainWorkshops()
        {
            // Arrange
            List<WorkshopCard> expectedWorkshopCards = ExpectedWorkshopCardsCityFilter();

            SetupGetPopularWorkshops();

            mapper.Setup(m => m.Map<List<WorkshopCard>>(It.IsAny<List<Workshop>>()))
                .Returns(expectedWorkshopCards);

            // Act
            var result = await service
                .GetPopularWorkshopsFromDatabase(2, "Київ")
                .ConfigureAwait(false);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    expectedWorkshopCards, options => options.WithStrictOrdering());
        }

        [Test]
        public async Task GetPopularDirections_WhenCityNotQueried_ShouldReturnCertainDirections()
        {
            // Arrange
            SetupGetPopularDirections();

            // Act
            var result = await service
                .GetPopularDirectionsFromDatabase(2, string.Empty)
                .ConfigureAwait(false);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    ExpectedDirectionStatisticsNoCityFilter(), options => options.WithStrictOrdering());
        }

        [Test]
        public async Task GetPopularDirections_WithCityQueried_ShouldReturnCertainDirections()
        {
            // Arrange
            SetupGetPopularDirections();

            // Act
            var result = await service
                .GetPopularDirectionsFromDatabase(2, "Київ")
                .ConfigureAwait(false);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    ExpectedDirectionStatisticsCityFilter(), options => options.WithStrictOrdering());
        }

        [TearDown]
        public void TearDown()
        {
            workshopRepository.Verify();
            applicationRepository.Verify();
            directionRepository.Verify();
        }

        #region Setup

        private void SetupGetPopularWorkshops()
        {
            var workshopsMock = WithWorkshops().AsQueryable().BuildMock();

            workshopRepository
                .Setup(w => w.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Expression<Func<Workshop, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(workshopsMock.Object)
                .Verifiable();
        }

        private void SetupGetPopularDirections()
        {
            var workshopsMock = WithWorkshops().AsQueryable().BuildMock();
            var applicationsMock = WithApplications().AsQueryable().BuildMock();
            var directionsMock = WithDirections().AsQueryable().BuildMock();

            workshopRepository.Setup(w => w.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Expression<Func<Workshop, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(workshopsMock.Object)
                .Verifiable();

            applicationRepository.Setup(w => w.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(applicationsMock.Object)
                .Verifiable();

            directionRepository.Setup(w => w.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Direction, bool>>>(),
                    It.IsAny<Expression<Func<Direction, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(directionsMock.Object)
                .Verifiable();
        }

        #endregion

        #region With

        private IEnumerable<Workshop> WithWorkshops()
        {
            return new List<Workshop>
            {
                new Workshop
                {
                    Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                    Title = "w1",
                    DirectionId = 1,
                    Address = new Address
                    {
                        City = "Київ",
                    },
                    Applications = new List<Application>
                    {
                        new Application { Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb") },
                    },
                },
                new Workshop
                {
                    Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"),
                    Title = "w2",
                    DirectionId = 2,
                    Address = new Address
                    {
                        City = "Київ",
                    },
                    Applications = new List<Application>
                    {
                        new Application { Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be") },
                        new Application { Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd") },
                    },
                },
                new Workshop
                {
                    Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"),
                    Title = "w3",
                    DirectionId = 3,
                    Address = new Address
                    {
                        City = "Одеса",
                    },
                    Applications = new List<Application>
                    {
                        new Application { Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d") },
                        new Application { Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d") },
                        new Application { Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771") },
                    },
                },
            };
        }

        private IEnumerable<Direction> WithDirections()
        {
            return new List<Direction>
            {
                new Direction { Id = 1 },
                new Direction { Id = 2 },
                new Direction { Id = 3 },
            };
        }

        private IEnumerable<Application> WithApplications()
        {
            return new List<Application>
            {
                new Application { Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"), WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"), Workshop = new Workshop { Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"), DirectionId = 1 } },
                new Application { Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be"), WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Workshop = new Workshop { Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), DirectionId = 2 } },
                new Application { Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd"), WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Workshop = new Workshop { Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), DirectionId = 2 } },
                new Application { Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"), WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Workshop = new Workshop { Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), DirectionId = 3 } },
                new Application { Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"), WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Workshop = new Workshop { Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), DirectionId = 3 } },
                new Application { Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"), WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Workshop = new Workshop { Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), DirectionId = 3 } },
            };
        }

        private IEnumerable<Workshop> WithWorkshopsFilteredForCityQuery(int limit, string city)
        {
            var workshops = WithWorkshops();

            if (!string.IsNullOrWhiteSpace(city))
            {
                workshops = workshops
                    .Where(w => string.Equals(w.Address.City, city.Trim()));
            }

            var workshopsWithApplications = workshops.Select(w => new
            {
                Workshop = w,
                Applications = w.Applications.Count,
            });

            var popularWorkshops = workshopsWithApplications
                .OrderByDescending(w => w.Applications)
                .Select(w => w.Workshop)
                .Take(limit);

            return popularWorkshops.ToList();
        }

        #endregion

        #region Expected

        private List<WorkshopCard> ExpectedWorkshopCardsNoCityFilter()
        {
            return new List<WorkshopCard>
            {
                new WorkshopCard {WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Title = "w3", Address = new AddressDto {City = "Одеса"}},
                new WorkshopCard {WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Title = "w2", Address = new AddressDto {City = "Київ"}},
            };
        }

        private List<WorkshopCard> ExpectedWorkshopCardsCityFilter()
        {
            return new List<WorkshopCard>
            {
                new WorkshopCard {WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Title = "w2", Address = new AddressDto {City = "Київ"}},
                new WorkshopCard {WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"), Title = "w1", Address = new AddressDto {City = "Київ"}},
            };
        }

        private List<DirectionStatistic> ExpectedDirectionStatisticsNoCityFilter()
        {
            return new List<DirectionStatistic>
            {
                new DirectionStatistic {ApplicationsCount = 3, Direction = new DirectionDto { Id = 3 }, WorkshopsCount = 1 },
                new DirectionStatistic {ApplicationsCount = 2, Direction = new DirectionDto { Id = 2 }, WorkshopsCount = 1 },
            };
        }

        private List<DirectionStatistic> ExpectedDirectionStatisticsCityFilter()
        {
            return new List<DirectionStatistic>
            {
                new DirectionStatistic {ApplicationsCount = 2, Direction = new DirectionDto { Id = 2 }, WorkshopsCount = 1 },
                new DirectionStatistic {ApplicationsCount = 1, Direction = new DirectionDto { Id = 1 }, WorkshopsCount = 1 },
            };
        }

        #endregion

    }
}
