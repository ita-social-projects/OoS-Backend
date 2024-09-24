using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
internal class CompetitiveEventControllerTests
{
    private CompetitiveEventController controller;
    private Mock<ICompetitiveEventService> competitiveEventService;
    private Mock<IStringLocalizer<SharedResource>> localizer;

    private IEnumerable<CompetitiveEventDto> competitiveEvents;

    [SetUp]
    public void Setup()
    {
        competitiveEventService = new Mock<ICompetitiveEventService>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();

        competitiveEvents = FakeCompetitiveEvents();

        controller = new CompetitiveEventController(
            competitiveEventService.Object,
            localizer.Object);
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var competitiveEvent = competitiveEvents.FirstOrDefault();
        competitiveEventService.Setup(x => x.GetById(competitiveEvent.Id)).ReturnsAsync(competitiveEvent);

        // Act
        var result = await controller.GetById(competitiveEvent.Id).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task GetById_WhenIdIsInvalid_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventService.Setup(x => x.GetById(id)).ReturnsAsync(competitiveEvents.SingleOrDefault(x => x.Id == id));

        var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

        //// Act and Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task Create_WhenModelIsValid_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var competitiveEvent = competitiveEvents.First();
        competitiveEventService.Setup(x => x.Create(competitiveEvent)).ReturnsAsync(competitiveEvent);

        // Act
        var result = await controller.Create(competitiveEvent).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(201, result.StatusCode);
    }

    [Test]
    public async Task Update_WhenModelIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        var competitiveEvent = competitiveEvents.First();
        competitiveEventService.Setup(s => s.Update(competitiveEvent)).ReturnsAsync(competitiveEvent);

        // Act
        var result = await controller.Update(competitiveEvent).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Delete_WhenIdIsValid_ReturnsNoContentResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventService.Setup(x => x.Delete(id));

        // Act
        var response = await controller.Delete(id);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(response);
    }

    private IEnumerable<CompetitiveEventDto> FakeCompetitiveEvents()
    {
        return new List<CompetitiveEventDto>()
        {
            new CompetitiveEventDto()
            {
                Id = Guid.NewGuid(),
                Title = "Test1",
                Description = "Test1",
            },
            new CompetitiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = "Test2",
                Description = "Test2",
            },
            new CompetitiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = "Test3",
                Description = "Test3",
            },
        };
    }
}
