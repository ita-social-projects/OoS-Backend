using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class StatisticControllerTests
    {
        private StatisticController controller;
        private Mock<IStatisticService> service;

        private IEnumerable<CategoryStatistic> categories;
        private IEnumerable<WorkshopDTO> workshops;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<IStatisticService>();
            controller = new StatisticController(service.Object);

            categories = FakeCategoryStatistics();
            workshops = FakeWorkshops();
        }

        [Test]
        [TestCase(5)]
        [TestCase(2)]
        [TestCase(12)]
        public async Task GetWorkshops_WhenLimitIsValid_ShouldReturnOkResultObject(int limit)
        {
            // Arrange
            service.Setup(s => s.GetPopularWorkshops(It.IsInRange(3, 10, Range.Inclusive))).ReturnsAsync(workshops);

            // Act
            var result = await controller.GetWorkshops(limit).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(5)]
        public async Task GetWorkshops_WhenCollectionIsEmpty_ShouldReturnNoContent(int limit)
        {
            // Arrange
            service.Setup(s => s.GetPopularWorkshops(limit)).ReturnsAsync(new List<WorkshopDTO>());

            // Act
            var result = await controller.GetWorkshops(limit).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        [TestCase(5)]
        [TestCase(2)]
        [TestCase(12)]
        public async Task GetCategories_WhenLimitIsValid_ShouldReturnOkResultObject(int limit)
        {
            // Arrange
            service.Setup(s => s.GetPopularCategoriesV1(It.IsInRange(3, 10, Range.Inclusive))).ReturnsAsync(categories);

            // Act
            var result = await controller.GetCategories(limit).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(5)]
        public async Task GetCategories_WhenCollectionIsEmpty_ShouldReturnNoContent(int limit)
        {
            // Arrange
            service.Setup(s => s.GetPopularCategoriesV1(limit)).ReturnsAsync(new List<CategoryStatistic>());

            // Act
            var result = await controller.GetCategories(limit).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        private IEnumerable<CategoryStatistic> FakeCategoryStatistics()
        {
            return new List<CategoryStatistic>()
            {
                new CategoryStatistic()
                {
                    WorkshopsCount = 2,
                    ApplicationsCount = 2,
                    Category = new CategoryDTO { Id = 1, Title = "c1" },
                },

                new CategoryStatistic()
                {
                    WorkshopsCount = 1,
                    ApplicationsCount = 1,
                    Category = new CategoryDTO { Id = 3, Title = "c3" },
                },
            };
        }

        private IEnumerable<WorkshopDTO> FakeWorkshops()
        {
            return new List<WorkshopDTO>()
            {
                new WorkshopDTO()
                {
                    Id = 1,
                    Title = "w1",
                    SubsubcategoryId = 1,
                },

                new WorkshopDTO()
                {
                    Id = 2,
                    Title = "w2",
                    SubsubcategoryId = 2,
                },
            };
        }
    }
}
