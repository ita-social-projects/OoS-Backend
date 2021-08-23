using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatisticServiceTest
    {
        private StatisticService service;
        private IApplicationRepository applicationRepository;
        private IWorkshopRepository workshopRepository;
        private IEntityRepository<Direction> directionRepository;
        private OutOfSchoolDbContext context;
        private Mock<ILogger> logger;

        [SetUp]
        public void SetUp()
        {
            context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
            applicationRepository = new ApplicationRepository(context);
            workshopRepository = new WorkshopRepository(context);
            directionRepository = new EntityRepository<Direction>(context);

            logger = new Mock<ILogger>();
            service = new StatisticService(applicationRepository, workshopRepository, directionRepository, logger.Object);
        }

        [Test]
        public async Task GetPopularWorkshops_ShouldReturnWorkshops()
        {
            var directions = context.Directions;

            // Arrange
            var expected = new WorkshopDTO
            {
                Id = 1,
                Title = "w1",
                DirectionId = 1,
                Direction = directions.First().Title,
                Teachers = null,
                Keywords = new List<string>() {string.Empty},
            };

            // Act
            var result = await service.GetPopularWorkshops(2).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected);
        }

        [Test]
        public async Task GetPopularCategories_ShouldReturnCategories()
        {
            // Arrange
            var categories = context.Directions;

            var expected = new DirectionStatistic
            {
                Direction = categories.First().ToModel(),
                WorkshopsCount = 2,
                ApplicationsCount = 2,
            };

            // Act
            var result = await service.GetPopularDirections(2).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected);
        }
    }
}
