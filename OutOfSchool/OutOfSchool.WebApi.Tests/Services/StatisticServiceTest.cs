using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
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
        private Mock<IEntityRepository<Direction>> directionRepository;

        private Mock<ILogger<StatisticService>> logger;

        [SetUp]
        public void SetUp()
        {
            applicationRepository = new Mock<IApplicationRepository>();
            workshopRepository = new Mock<IWorkshopRepository>();
            directionRepository = new Mock<IEntityRepository<Direction>>();

            logger = new Mock<ILogger<StatisticService>>();

            service = new StatisticService(
                applicationRepository.Object,
                workshopRepository.Object,
                directionRepository.Object,
                logger.Object);
        }

        [Test]
        public async Task GetPopularWorkshops_WhenCityNotQueried_ShouldReturnCertainWorkshops()
        {
            // Arrange
            SetupGetPopularWorkshops();

            // Act
            var result = await service
                .GetPopularWorkshops(2, string.Empty)
                .ConfigureAwait(false);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    ExpectedWorkshopCardsNoCityFilter(), options => options.WithStrictOrdering());
        }

        [Test]
        public async Task GetPopularWorkshops_WithCityQueried_ShouldReturnCertainWorkshops()
        {
            // Arrange
            SetupGetPopularWorkshops();

            // Act
            var result = await service
                .GetPopularWorkshops(2, "Київ")
                .ConfigureAwait(false);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    ExpectedWorkshopCardsCityFilter(), options => options.WithStrictOrdering());
        }

        [Test]
        public async Task GetPopularDirections_WhenCityNotQueried_ShouldReturnCertainDirections()
        {
            // Arrange
            SetupGetPopularDirections();

            // Act
            var result = await service
                .GetPopularDirections(2, string.Empty)
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
                .GetPopularDirections(2, "Київ")
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
                    Id = 1,
                    Title = "w1",
                    DirectionId = 1,
                    Address = new Address
                    {
                        City = "Київ",
                    },
                    Applications = new List<Application>
                    {
                        new Application { Id = 1 },
                    },
                },
                new Workshop
                {
                    Id = 2,
                    Title = "w2",
                    DirectionId = 2,
                    Address = new Address
                    {
                        City = "Київ",
                    },
                    Applications = new List<Application>
                    {
                        new Application { Id = 2 },
                        new Application { Id = 3 },
                    },
                },
                new Workshop
                {
                    Id = 3,
                    Title = "w3",
                    DirectionId = 3,
                    Address = new Address
                    {
                        City = "Одеса",
                    },
                    Applications = new List<Application>
                    {
                        new Application { Id = 4 },
                        new Application { Id = 5 },
                        new Application { Id = 6 },
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
                new Application { Id = 1, WorkshopId = 1, Workshop = new Workshop { Id = 1, DirectionId = 1 } },
                new Application { Id = 2, WorkshopId = 2, Workshop = new Workshop { Id = 2, DirectionId = 2 } },
                new Application { Id = 3, WorkshopId = 2, Workshop = new Workshop { Id = 2, DirectionId = 2 } },
                new Application { Id = 4, WorkshopId = 3, Workshop = new Workshop { Id = 3, DirectionId = 3 } },
                new Application { Id = 5, WorkshopId = 3, Workshop = new Workshop { Id = 3, DirectionId = 3 } },
                new Application { Id = 6, WorkshopId = 3, Workshop = new Workshop { Id = 3, DirectionId = 3 } },
            };
        }

        #endregion

        #region Expected

        private List<WorkshopCard> ExpectedWorkshopCardsNoCityFilter()
        {
            return new List<WorkshopCard>
            {
                new WorkshopCard {WorkshopId = 3, Title = "w3", Address = new AddressDto {City = "Одеса"}},
                new WorkshopCard {WorkshopId = 2, Title = "w2", Address = new AddressDto {City = "Київ"}},
            };
        }

        private List<WorkshopCard> ExpectedWorkshopCardsCityFilter()
        {
            return new List<WorkshopCard>
            {
                new WorkshopCard {WorkshopId = 2, Title = "w2", Address = new AddressDto {City = "Київ"}},
                new WorkshopCard {WorkshopId = 1, Title = "w1", Address = new AddressDto {City = "Київ"}},
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
