using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Range = Moq.Range;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class StatisticControllerTests
    {
        private StatisticController controller;
        private Mock<IStatisticService> service;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<IStatisticService>();
            controller = new StatisticController(service.Object);
        }

        [Test]
        public async Task GetWorkshops_WhenLimitInRange_CityIsNull_ShouldReturnCorrectAmountOfData(
            [Random(3, 10, 3)] int limit, [Values(null)]string city)
        {
            // Arrange
            SetupGetWorkshops();

            // Act
            var result = await controller
                .GetWorkshops(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
        }

        [Test]
        public async Task GetWorkshops_WhenLimitNotInRangeBelow_ShouldReturnCorrectAmountOfData(
            [Random(1, 2, 2)] int limit, [Values(null)] string city)
        {
            // Arrange
            SetupGetWorkshops();

            // Act
            var result = await controller
                .GetWorkshops(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
        }

        [Test]
        public async Task GetWorkshops_WhenLimitNotInRangeAbove_ShouldReturnCorrectAmountOfData(
            [Random(11, 15, 3)] int limit, [Values(null)] string city)
        {
            // Arrange
            SetupGetWorkshops();

            // Act
            var result = await controller
                .GetWorkshops(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
        }

        [Test]
        public async Task GetWorkshops_WhenCityIsEmptyOrWhiteSpace_ShouldReturnCorrectAmountOfData(
            [Random(3, 10, 2)] int limit, [Values("", "  ")]string city)
        {
            // Arrange
            SetupGetWorkshops();

            // Act
            var result = await controller
                .GetWorkshops(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
        }

        [Test]
        public async Task GetWorkshops_WhenCollectionIsEmpty_ShouldReturnNoContent(
            [Random(3, 10, 3)] int limit, [Values(null)]string city)
        {
            // Arrange
            SetupGetWorkshopsEmpty();

            // Act
            var result = await controller
                .GetWorkshops(limit, city)
                .ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().BeAssignableTo<NoContentResult>();
        }

        [Test]
        public async Task GetDirections_WhenLimitInRange_CityIsNull_ShouldReturnCorrectAmountOfData(
            [Random(3, 10, 3)] int limit, [Values(null)]string city)
        {
            // Arrange
            SetupGetDirections();

            // Act
            var result = await controller
                .GetDirections(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
        }

        [Test]
        public async Task GetDirections_WhenLimitNotInRangeBelow_ShouldReturnCorrectAmountOfData(
            [Random(1, 2, 2)] int limit, [Values(null)]string city)
        {
            // Arrange
            SetupGetDirections();

            // Act
            var result = await controller
                .GetDirections(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
        }

        [Test]
        public async Task GetDirections_WhenLimitNotInRangeAbove_ShouldReturnCorrectAmountOfData(
            [Random(11, 15, 3)] int limit, [Values(null)]string city)
        {
            // Arrange
            SetupGetDirections();

            // Act
            var result = await controller
                .GetDirections(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
        }

        [Test]
        public async Task GetDirections_WhenCityIsEmptyOrWhitespace_ShouldReturnCorrectAmountOfData(
            [Random(3, 10, 3)] int limit, [Values("", "  ")]string city)
        {
            // Arrange
            SetupGetDirections();

            // Act
            var result = await controller
                .GetDirections(limit, city)
                .ConfigureAwait(false) as ObjectResult;

            // Assert
            result.Should().BeAssignableTo<OkObjectResult>();
            result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
        }

        [Test]
        public async Task GetDirections_WhenCollectionIsEmpty_ShouldReturnNoContent(
            [Random(3, 10, 3)] int limit, [Values(null)]string city)
        {
            // Arrange
            SetupGetDirectionsEmpty();

            // Act
            var result = await controller
                .GetDirections(limit, city)
                .ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().BeAssignableTo<NoContentResult>();
        }

        [TearDown]
        public void TearDown()
        {
            service.Verify();
        }

        #region Setup

        private void SetupGetWorkshops()
        {
            service.Setup(s =>
                    s.GetPopularWorkshops(
                        It.IsInRange(3, 10, Range.Inclusive), It.IsAny<string>()))
                .ReturnsAsync(WithWorkshopCards())
                .Verifiable();
        }

        private void SetupGetWorkshopsEmpty()
        {
            service.Setup(s =>
                    s.GetPopularWorkshops(
                        It.IsInRange(3, 10, Range.Inclusive), It.IsAny<string>()))
                .ReturnsAsync(new List<WorkshopCard>())
                .Verifiable();
        }

        private void SetupGetDirections()
        {
            service.Setup(s =>
                    s.GetPopularDirections(
                        It.IsInRange(3, 10, Range.Inclusive), It.IsAny<string>()))
                .ReturnsAsync(WithDirectionStatistics())
                .Verifiable();
        }

        private void SetupGetDirectionsEmpty()
        {
            service.Setup(s =>
                    s.GetPopularDirections(
                        It.IsInRange(3, 10, Range.Inclusive), It.IsAny<string>()))
                .ReturnsAsync(new List<DirectionStatistic>())
                .Verifiable();
        }

        #endregion

        #region With

        private List<DirectionStatistic> WithDirectionStatistics()
        {
            return new List<DirectionStatistic>()
            {
                new DirectionStatistic()
                {
                    WorkshopsCount = 2,
                    ApplicationsCount = 2,
                    Direction = new DirectionDto { Id = 1, Title = "c1" },
                },

                new DirectionStatistic()
                {
                    WorkshopsCount = 1,
                    ApplicationsCount = 1,
                    Direction = new DirectionDto { Id = 3, Title = "c3" },
                },
            };
        }

        private List<WorkshopCard> WithWorkshopCards()
        {
            return new List<WorkshopCard>()
            {
                new WorkshopCard()
                {
                    WorkshopId = new Guid("cb40c32f-aed6-478d-bf13-d52d61d52d32"),
                    Title = "w1",
                },

                new WorkshopCard()
                {
                    WorkshopId = new Guid("dae7f9f7-300f-4eac-9909-4a939ecaf8fb"),
                    Title = "w2",
                },
            };
        }

        #endregion

        #region Expected

        private List<WorkshopCard> ExpectedWorkshopCards()
        {
            return new List<WorkshopCard>()
            {
                new WorkshopCard()
                {
                    WorkshopId = new Guid("cb40c32f-aed6-478d-bf13-d52d61d52d32"),
                    Title = "w1",
                },

                new WorkshopCard()
                {
                    WorkshopId = new Guid("dae7f9f7-300f-4eac-9909-4a939ecaf8fb"),
                    Title = "w2",
                },
            };
        }

        private List<DirectionStatistic> ExpectedDirectionStatistics()
        {
            return new List<DirectionStatistic>()
            {
                new DirectionStatistic()
                {
                    WorkshopsCount = 2,
                    ApplicationsCount = 2,
                    Direction = new DirectionDto { Id = 1, Title = "c1" },
                },

                new DirectionStatistic()
                {
                    WorkshopsCount = 1,
                    ApplicationsCount = 1,
                    Direction = new DirectionDto { Id = 3, Title = "c3" },
                },
            };
        }

        #endregion
    }
}
