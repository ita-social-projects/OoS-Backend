using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Range = Moq.Range;

namespace OutOfSchool.WebApi.Tests.Controllers;

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
        [Random(3, 10, 3)] int limit, [Values(0)]long catottgId)
    {
        // Arrange
        SetupGetWorkshops();

        // Act
        var result = await controller
            .GetWorkshops(limit, catottgId)
            .ConfigureAwait(false) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
    }

    [Test]
    public async Task GetWorkshops_WhenLimitNotInRangeBelow_ShouldReturnCorrectAmountOfData(
        [Random(1, 2, 2)] int limit, [Values(0)] long catottgId)
    {
        // Arrange
        SetupGetWorkshops();

        // Act
        var result = await controller
            .GetWorkshops(limit, catottgId)
            .ConfigureAwait(false) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
    }

    [Test]
    public async Task GetWorkshops_WhenLimitNotInRangeAbove_ShouldReturnCorrectAmountOfData(
        [Random(11, 15, 3)] int limit, [Values(0)] long catottgId)
    {
        // Arrange
        SetupGetWorkshops();

        // Act
        var result = await controller
            .GetWorkshops(limit, catottgId)
            .ConfigureAwait(false) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeEquivalentTo(ExpectedWorkshopCards());
    }

    [Test]
    public async Task GetWorkshops_WhenCollectionIsEmpty_ShouldReturnNoContent(
        [Random(3, 10, 3)] int limit, [Values(0)]long catottgId)
    {
        // Arrange
        SetupGetWorkshopsEmpty();

        // Act
        var result = await controller
            .GetWorkshops(limit, catottgId)
            .ConfigureAwait(false) as NoContentResult;

        // Assert
        result.Should().BeAssignableTo<NoContentResult>();
    }

    [Test]
    public async Task GetDirections_WhenLimitInRange_CityIsNull_ShouldReturnCorrectAmountOfData(
        [Random(3, 10, 3)] int limit, [Values(0)]long catottgId)
    {
        // Arrange
        SetupGetDirections();

        // Act
        var result = await controller
            .GetDirections(limit, catottgId)
            .ConfigureAwait(false) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
    }

    [Test]
    public async Task GetDirections_WhenLimitNotInRangeBelow_ShouldReturnCorrectAmountOfData(
        [Random(1, 2, 2)] int limit, [Values(0)]long catottgId)
    {
        // Arrange
        SetupGetDirections();

        // Act
        var result = await controller
            .GetDirections(limit, catottgId)
            .ConfigureAwait(false) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
    }

    [Test]
    public async Task GetDirections_WhenLimitNotInRangeAbove_ShouldReturnCorrectAmountOfData(
        [Random(11, 15, 3)] int limit, [Values(0)]long catottgId)
    {
        // Arrange
        SetupGetDirections();

        // Act
        var result = await controller
            .GetDirections(limit, catottgId)
            .ConfigureAwait(false) as ObjectResult;

        // Assert
        result.Should().BeAssignableTo<OkObjectResult>();
        result?.Value.Should().BeEquivalentTo(ExpectedDirectionStatistics());
    }

    [Test]
    public async Task GetDirections_WhenCollectionIsEmpty_ShouldReturnNoContent(
        [Random(3, 10, 3)] int limit, [Values(0)]long catottgId)
    {
        // Arrange
        SetupGetDirectionsEmpty();

        // Act
        var result = await controller
            .GetDirections(limit, catottgId)
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
                    It.IsInRange(3, 10, Range.Inclusive), It.IsAny<long>()))
            .ReturnsAsync(WithWorkshopCards())
            .Verifiable();
    }

    private void SetupGetWorkshopsEmpty()
    {
        service.Setup(s =>
                s.GetPopularWorkshops(
                    It.IsInRange(3, 10, Range.Inclusive), It.IsAny<long>()))
            .ReturnsAsync(new List<WorkshopCard>())
            .Verifiable();
    }

    private void SetupGetDirections()
    {
        service.Setup(s =>
                s.GetPopularDirections(
                    It.IsInRange(3, 10, Range.Inclusive), It.IsAny<long>()))
            .ReturnsAsync(WithDirectionStatistics())
            .Verifiable();
    }

    private void SetupGetDirectionsEmpty()
    {
        service.Setup(s =>
                s.GetPopularDirections(
                    It.IsInRange(3, 10, Range.Inclusive), It.IsAny<long>()))
            .ReturnsAsync(new List<DirectionDto>())
            .Verifiable();
    }

    #endregion

    #region With

    private List<DirectionDto> WithDirectionStatistics()
    {
        return new List<DirectionDto>()
        {
            new DirectionDto { Id = 1, Title = "c1", WorkshopsCount = 2 },

            new DirectionDto { Id = 3, Title = "c3",  WorkshopsCount = 1, },
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

    private List<DirectionDto> ExpectedDirectionStatistics()
    {
        return new List<DirectionDto>()
        {
            new DirectionDto { Id = 1, Title = "c1", WorkshopsCount = 2, },

            new DirectionDto { Id = 3, Title = "c3", WorkshopsCount = 1, },
        };
    }

    #endregion
}