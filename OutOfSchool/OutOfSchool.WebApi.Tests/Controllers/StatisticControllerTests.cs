using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class StatisticControllerTests
    {
        private StatisticController controller;
        private Mock<IStatisticService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<CategoryStatistic> categories;
        private IEnumerable<WorkshopDTO> workshops;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<IStatisticService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            controller = new StatisticController(service.Object, localizer.Object);

            categories = FakeCategoryStatistics();
            workshops = FakeWorkshops();
        }

        [Test]
        [TestCase(2)]
        public async Task GetWorkshops_WhenNumberIsValid_ShouldReturnOkResultObject(int number)
        {
            // Arrange
            service.Setup(s => s.GetPopularWorkshops(number)).ReturnsAsync(workshops);

            // Act
            var result = await controller.GetWorkshops(number).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(0)]
        public void GetWorkshops_WhenNumberIsNotValid_ShouldThrowArgumentOutOfRangeException(int number)
        {
            // Act and Assert
            controller.Invoking(c => c.GetWorkshops(number))
                      .Should().ThrowAsync<ArgumentOutOfRangeException>()
                      .WithMessage("The number of entries cannot be less than 1.");
        }

        [Test]
        [TestCase(2)]
        public async Task GetWorkshops_WhenCollectionIsEmpty_ShouldReturnNoContent(int number)
        {
            // Arrange
            service.Setup(s => s.GetPopularWorkshops(number)).ReturnsAsync(new List<WorkshopDTO>());

            // Act
            var result = await controller.GetWorkshops(number).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        [TestCase(2)]
        public async Task GetCategories_WhenNumberIsValid_ShouldReturnOkResultObject(int number)
        {
            // Arrange
            service.Setup(s => s.GetPopularCategories(number)).ReturnsAsync(categories);

            // Act
            var result = await controller.GetCategories(number).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(0)]
        public void GetCategories_WhenNumberIsNotValid_ShouldThrowArgumentOutOfRangeException(int number)
        {
            // Act and Assert
            controller.Invoking(c => c.GetCategories(number))
                      .Should().ThrowAsync<ArgumentOutOfRangeException>()
                      .WithMessage("The number of entries cannot be less than 1.");
        }

        [Test]
        [TestCase(2)]
        public async Task GetCategories_WhenCollectionIsEmpty_ShouldReturnNoContent(int number)
        {
            // Arrange
            service.Setup(s => s.GetPopularCategories(number)).ReturnsAsync(new List<CategoryStatistic>());

            // Act
            var result = await controller.GetCategories(number).ConfigureAwait(false) as NoContentResult;

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
