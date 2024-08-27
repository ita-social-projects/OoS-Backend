using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopMementoControllerTests
{
    private WorkshopMementoController controller;
    private Mock<IMementoService<RequiredWorkshopMemento>> mementoService;
    private Mock<IStorage> storage;
    private ClaimsPrincipal user;

    private IEnumerable<RequiredWorkshopMemento> mementos;
    private RequiredWorkshopMemento memento;

    [SetUp]
    public void Setup()
    {
        mementoService = new Mock<IMementoService<RequiredWorkshopMemento>>();
        controller = new WorkshopMementoController(mementoService.Object, storage.Object);
        user = new ClaimsPrincipal(new ClaimsIdentity());
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        mementos = FakeMementos();
        memento = FakeMemento();
    }

    //[Test]
    //public async Task GetByFilter_WhenSearchResultIsNotNullOrEmpty_ReturnOkObjectResult()
    //{
    //    // Arrange
    //    var data = new SearchResult<DirectionDto>()
    //    {
    //        Entities = new List<DirectionDto>() { new DirectionDto() },
    //        TotalAmount = 1,
    //    };

    //    var directionFilter = new DirectionFilter();

    //    service.Setup(x => x.GetByFilter(directionFilter, true)).ReturnsAsync(data);

    //    // Act
    //    var result = await controller.GetByFilter(directionFilter, true);

    //    // Assert
    //    result.Should().NotBeNull();
    //    result.Should()
    //          .BeOfType<OkObjectResult>()
    //          .Which.StatusCode
    //          .Should()
    //          .Be(StatusCodes.Status200OK);
    //}

    //[Test]
    //public async Task GetByFilter_WhenSearchResultIsNullOrEmpty_ReturnNoContentObjectResult()
    //{
    //    // Arrange
    //    var data = new SearchResult<DirectionDto>()
    //    {

    //    };

    //    var directionFilter = new DirectionFilter();

    //    service.Setup(x => x.GetByFilter(directionFilter, true)).ReturnsAsync(data);

    //    // Act
    //    var result = await controller.GetByFilter(directionFilter, true);

    //    // Assert
    //    result.Should().NotBeNull();
    //    result.Should()
    //          .BeOfType<NoContentResult>()
    //          .Which.StatusCode
    //          .Should()
    //          .Be(StatusCodes.Status204NoContent);
    //}

    //[Test]
    //public async Task Get_WhenCalled_ReturnsOkResultObject()
    //{
    //    // Arrange
    //    service.Setup(x => x.GetAll()).ReturnsAsync(directions);

    //    // Act
    //    var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

    //    // Assert
    //    Assert.That(result, Is.Not.Null);
    //    Assert.AreEqual(200, result.StatusCode);
    //}

    //[Test]
    //[TestCase(1)]
    //public async Task GetById_WhenIdIsValid_ReturnsOkObjectResult(long id)
    //{
    //    // Arrange
    //    service.Setup(x => x.GetById(id)).ReturnsAsync(directions.SingleOrDefault(x => x.Id == id));

    //    // Act
    //    var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

    //    // Assert
    //    Assert.That(result, Is.Not.Null);
    //    Assert.AreEqual(200, result.StatusCode);
    //}

    //[Test]
    //[TestCase(-1)]
    //public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    //{
    //    // Arrange
    //    service.Setup(x => x.GetById(id)).ReturnsAsync(directions.SingleOrDefault(x => x.Id == id));

    //    // Act and Assert
    //    Assert.ThrowsAsync<ArgumentOutOfRangeException>(
    //        async () => await controller.GetById(id).ConfigureAwait(false));
    //}

    //[Test]
    //[TestCase(10)]
    //public async Task GetById_WhenIdIsInvalid_ReturnsNull(long id)
    //{
    //    // Arrange
    //    service.Setup(x => x.GetById(id)).ReturnsAsync(directions.SingleOrDefault(x => x.Id == id));

    //    // Act
    //    var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

    //    // Assert
    //    Assert.That(result, Is.Not.Null);
    //    Assert.AreEqual(200, result.StatusCode);
    //}

    //[Test]
    //public async Task Create_WhenModelIsValid_ReturnsCreatedAtActionResult()
    //{
    //    // Arrange
    //    service.Setup(x => x.Create(direction)).ReturnsAsync(direction);

    //    // Act
    //    var result = await controller.Create(direction).ConfigureAwait(false) as CreatedAtActionResult;

    //    // Assert
    //    Assert.That(result, Is.Not.Null);
    //    Assert.AreEqual(201, result.StatusCode);
    //}

    //[Test]
    //public async Task Create_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    //{
    //    // Arrange
    //    controller.ModelState.AddModelError("CreateDirection", "Invalid model state.");

    //    // Act
    //    var result = await controller.Create(direction).ConfigureAwait(false);

    //    // Assert
    //    Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    //    Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
    //}

    private RequiredWorkshopMemento FakeMemento()
    {
        return new RequiredWorkshopMemento()
        {
            Title = "title1",
            Email = "myemail1@gmail.com",
            Phone = "+380670000001",
        };
    }

    private IEnumerable<RequiredWorkshopMemento> FakeMementos()
    {
        return new List<RequiredWorkshopMemento>()
        {
            new RequiredWorkshopMemento()
            {
                Title = "title1",
                Email = "myemail1@gmail.com",
                Phone = "+380670000001",
            },
            new RequiredWorkshopMemento()
            {
                Title = "title2",
                Email = "myemail2@gmail.com",
                Phone = "+380670000002",
            },
            new RequiredWorkshopMemento()
            {
                Title = "title2",
                Email = "myemail2@gmail.com",
                Phone = "+380670000002",
            },
        };
    }
}