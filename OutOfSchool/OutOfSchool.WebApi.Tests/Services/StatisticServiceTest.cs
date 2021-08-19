using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatisticServiceTest
    {
        private IStatisticService service;

        private Mock<IApplicationRepository> applicationRepository;
        private Mock<IWorkshopRepository> workshopRepository;
        private Mock<IEntityRepository<Direction>> directionRepository;

        private Mock<ILogger> logger;

        private IEnumerable<Workshop> workshops;
        private IEnumerable<Application> applications;
        private IEnumerable<Direction> directions;

        [SetUp]
        public void SetUp()
        {
            applicationRepository = new Mock<IApplicationRepository>();
            workshopRepository = new Mock<IWorkshopRepository>();
            directionRepository = new Mock<IEntityRepository<Direction>>();

            logger = new Mock<ILogger>();

            service = new StatisticService(
                applicationRepository.Object,
                workshopRepository.Object,
                directionRepository.Object,
                logger.Object);

            workshops = FakeWorkshops();
            applications = FakeApplications();
            directions = FakeDirections();
        }

        [Test]
        public async Task GetPopularWorkshops_ShouldReturnWorkshops()
        {
            // Arrange
            var expected = new List<WorkshopCard>
            {
                new WorkshopCard()
                {
                    WorkshopId = 3,
                    Title = "w3",
                },
                new WorkshopCard()
                {
                    WorkshopId = 2,
                    Title = "w2",
                },
            };

            var workshopsMock = workshops.AsQueryable().BuildMock();

            workshopRepository.Setup(w => w.Get<It.IsAnyType>(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Expression<Func<Workshop, It.IsAnyType>>>(),
                It.IsAny<bool>()))
                .Returns(workshopsMock.Object);

            // Act
            var result = await service.GetPopularWorkshops(2).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        [Ignore("Test must be fixed")]
        public async Task GetPopularCategories_ShouldReturnCategories()
        {
            // Arrange
            var expected = new DirectionStatistic
            {
                WorkshopsCount = 2,
                ApplicationsCount = 2,
            };

            var workshopsMock = workshops.AsQueryable().BuildMock();
            var applicationsMock = applications.AsQueryable().BuildMock();
            var directionsMock = directions.AsQueryable().BuildMock();

            workshopRepository.Setup(w => w.Get<It.IsAnyType>(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Expression<Func<Workshop, It.IsAnyType>>>(),
                It.IsAny<bool>()))
                .Returns(workshopsMock.Object);

            applicationRepository.Setup(w => w.Get<It.IsAnyType>(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                It.IsAny<bool>()))
                .Returns(applicationsMock.Object);

            directionRepository.Setup(w => w.Get<It.IsAnyType>(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Direction, bool>>>(),
                It.IsAny<Expression<Func<Direction, It.IsAnyType>>>(),
                It.IsAny<bool>()))
                .Returns(directionsMock.Object);

            // Act
            var result = await service.GetPopularDirections(2).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected);
        }

        private IEnumerable<Workshop> FakeWorkshops()
        {
            return new List<Workshop>()
            {
                new Workshop()
                {
                    Id = 1,
                    Title = "w1",
                    DirectionId = 1,
                    Applications = new List<Application>()
                    {
                        new Application() { Id = 1},
                    },
                },
                new Workshop()
                {
                    Id = 2,
                    Title = "w2",
                    DirectionId = 2,
                    Applications = new List<Application>()
                    {
                        new Application() { Id = 2},
                        new Application() { Id = 3},
                    },
                },
                new Workshop()
                {
                    Id = 3,
                    Title = "w3",
                    DirectionId = 3,
                    Applications = new List<Application>()
                    {
                        new Application() { Id = 4},
                        new Application() { Id = 5},
                        new Application() { Id = 6},
                    },
                },
            };
        }

        private IEnumerable<Direction> FakeDirections()
        {
            return new List<Direction>()
            {
                new Direction { Id = 1 },
                new Direction { Id = 2 },
                new Direction { Id = 3 },
            };
        }

        private IEnumerable<Application> FakeApplications()
        {
            return new List<Application>()
            {
                new Application { Id = 1, WorkshopId = 1, Workshop = new Workshop { Id = 1 } },
                new Application { Id = 2, WorkshopId = 2, Workshop = new Workshop { Id = 2 } },
                new Application { Id = 3, WorkshopId = 2, Workshop = new Workshop { Id = 2 } },
            };
        }
    }
}
