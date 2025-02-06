using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class SubDirectionControllerTests
{
    private SubDirectionController controller;
    private Mock<ISubDirectionService> service;
    private Mock<IStringLocalizer<SharedResource>> localizer;

    private IEnumerable<SubDirectionDto> subDirections;
    private SubDirectionDto subDirection;

    [SetUp]
    public void Setup()
    {
        service = new Mock<ISubDirectionService>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();

        controller = new SubDirectionController(service.Object, localizer.Object);

        subDirections = FakeSubDirections();
        subDirection = FakeSubDirection();
    }

    [Test]
    public async Task GetByFilter_WhenSearchResultIsNotNullOrEmpty_ReturnOkObjectResult()
    {
        // Arrange
        var data = new SearchResult<SubDirectionDto>()
        {
            Entities = new List<SubDirectionDto>() { new SubDirectionDto() },
            TotalAmount = 1,
        };
        var directionId = 1;

        var filter = new SearchStringFilter();

        service.Setup(x => x.GetByFilter(directionId, filter)).ReturnsAsync(data);

        // Act
        var result = await controller.GetByFilter(directionId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<OkObjectResult>(result);
        Assert.AreEqual(StatusCodes.Status200OK, (result as OkObjectResult).StatusCode);
    }

    [Test]
    public async Task GetByFilter_WhenSearchResultIsNullOrEmpty_ReturnNoContentObjectResult()
    {
        // Arrange
        var data = new SearchResult<SubDirectionDto>()
        {

        };
        var directionId = 1;

        var filter = new SearchStringFilter();

        service.Setup(x => x.GetByFilter(directionId, filter)).ReturnsAsync(data);

        // Act
        var result = await controller.GetByFilter(directionId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<NoContentResult>(result);
        Assert.AreEqual(StatusCodes.Status204NoContent, (result as NoContentResult).StatusCode);
    }

    [Test]
    [TestCase(1)]
    public async Task GetById_WhenIdIsValid_ReturnsOkObjectResult(long id)
    {
        // Arrange
        service.Setup(x => x.GetById(id)).ReturnsAsync(subDirections.SingleOrDefault(x => x.Id == id));

        // Act
        var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    [TestCase(-1)]
    public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Arrange
        service.Setup(x => x.GetById(id)).ReturnsAsync(subDirections.SingleOrDefault(x => x.Id == id));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await controller.GetById(id).ConfigureAwait(false));
    }

    [Test]
    [TestCase(10)]
    public async Task GetById_WhenIdIsInvalid_ReturnsNotFound(long id)
    {
        // Arrange
        service.Setup(x => x.GetById(id)).ReturnsAsync((long id) => subDirections.SingleOrDefault(x => x.Id == id));

        // Act
        var result = await controller.GetById(id).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(404, result.StatusCode);
    }

    [Test]
    public async Task Create_WhenModelIsValid_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var directionId = 1;
        var returnedResult = Result<SubDirectionDto>.Success(subDirection);
        service.Setup(x => x.Create(directionId, subDirection)).ReturnsAsync(returnedResult);

        // Act
        var result = await controller.Create(directionId, subDirection).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(201, result.StatusCode);
    }

    [Test]
    public async Task Create_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var directionId = 1;
        controller.ModelState.AddModelError("CreateSubDirection", "Invalid model state.");

        // Act
        var result = await controller.Create(directionId, subDirection).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_WhenModelTitleIsDuplicated_ReturnsBadRequestObjectResult()
    {
        //Arrange
        var returnedResult = Result<SubDirectionDto>.Failed(new OperationError()
        {
            Code = "400",
            Description = "There is already a Direction with such a data.",
        });
        var directionId = 1;
        service.Setup(x => x.Create(directionId, subDirection)).ReturnsAsync(returnedResult);

        // Act
        var result = await controller.Create(directionId, subDirection).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    private SubDirectionDto FakeSubDirection()
    {
        return new SubDirectionDto()
        {
            Title = "Test1",
            Description = "Test1",
        };
    }

    private IEnumerable<SubDirectionDto> FakeSubDirections()
    {
        return new List<SubDirectionDto>()
        {
            new SubDirectionDto()
            {
                Id = 1,
                Title = "Test1",
                Description = "Test1",
            },
            new SubDirectionDto
            {
                Id = 2,
                Title = "Test2",
                Description = "Test2",
            },
            new SubDirectionDto
            {
                Id = 3,
                Title = "Test3",
                Description = "Test3",
            },
        };
    }
}
