﻿using System.Collections.Generic;
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

        private List<DirectionStatistic> categories;
        private List<WorkshopCard> workshops;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<IStatisticService>();
            controller = new StatisticController(service.Object);

            categories = FakeDirectionStatistics();
            workshops = FakeWorkshopCards();
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
            service.Setup(s => s.GetPopularWorkshops(limit)).ReturnsAsync(new List<WorkshopCard>());

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
        public async Task GetDirections_WhenLimitIsValid_ShouldReturnOkResultObject(int limit)
        {
            // Arrange
            service.Setup(s => s.GetPopularDirections(It.IsInRange(3, 10, Range.Inclusive))).ReturnsAsync(categories);

            // Act
            var result = await controller.GetDirections(limit).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(5)]
        public async Task GetDirections_WhenCollectionIsEmpty_ShouldReturnNoContent(int limit)
        {
            // Arrange
            service.Setup(s => s.GetPopularDirections(limit)).ReturnsAsync(new List<DirectionStatistic>());

            // Act
            var result = await controller.GetDirections(limit).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        private List<DirectionStatistic> FakeDirectionStatistics()
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

        private List<WorkshopCard> FakeWorkshopCards()
        {
            return new List<WorkshopCard>()
            {
                new WorkshopCard()
                {
                    WorkshopId = 1,
                    Title = "w1",
                },

                new WorkshopCard()
                {
                    WorkshopId = 2,
                    Title = "w2",
                },
            };
        }
    }
}
