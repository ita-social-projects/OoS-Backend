using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatisticServiceTest
    {
        private StatisticService service;
        private IApplicationRepository applicationRepository;
        private IEntityRepository<Workshop> workshopRepository;
        private OutOfSchoolDbContext context;

        [SetUp]
        public void SetUp()
        {
            context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
            applicationRepository = new ApplicationRepository(context);
            workshopRepository = new EntityRepository<Workshop>(context);
            service = new StatisticService(applicationRepository, workshopRepository);
        }

        [Test]
        public async Task GetPopularWorkshops_ShouldReturnWorkshops()
        {
            // Arrange
            var expected = context.Workshops;

            // Act
            var result = await service.GetPopularWorkshops(2).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected.First().ToModel());
        }

        [Test]
        public async Task GetPopularCategories_ShouldReturnCategories()
        {
            // Arrange
            var categories = context.Categories;

            var expected = new CategoryStatistic
            {
                Category = categories.First().ToModel(),
                WorkshopsCount = 2,
                ApplicationsCount = 2,
            };

            // Act
            var result = await service.GetPopularCategories(2).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(expected);
        }
    }
}
