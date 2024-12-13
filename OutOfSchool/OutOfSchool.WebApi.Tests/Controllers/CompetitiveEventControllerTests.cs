using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
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
        Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
    }

    [Test]
    public async Task GetById_WhenIdIsInvalid_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        competitiveEventService.Setup(x => x.GetById(id)).ReturnsAsync(competitiveEvents.SingleOrDefault(x => x.Id == id));

        // Act
        var result = await controller.GetById(id).ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual((int)HttpStatusCode.NoContent, result.StatusCode);
    }

    [Test]
    public async Task Create_WhenModelIsValid_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var inputDto = new CompetitiveEventCreateDto()
        {
            Title = "New Event",
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow.AddHours(1),
            Judges = new List<JudgeDto>()
            { 
                new JudgeDto()
                {
                    FirstName ="Judge A",
                    Description = "Description A",
                    IsChiefJudge = true,
                },
                new JudgeDto()
                {
                    FirstName ="Judge B",
                    Description = "Description B"
                },
            }
        };
        competitiveEventService.Setup(x => x.Create(It.IsAny<CompetitiveEventCreateDto>()))
            .ReturnsAsync(
            new CompetitiveEventDto()
            {
                Id = Guid.NewGuid(),
                Title = inputDto.Title,
                ScheduledStartTime = inputDto.ScheduledStartTime,
                ScheduledEndTime = inputDto.ScheduledEndTime,
                Judges = inputDto.Judges,
            });

        // Act
        var result = await controller.Create(inputDto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        var competitiveEventResult = result.Value as CompetitiveEventDto;
        Assert.That(result, Is.Not.Null);

        AssertCompetitiveEventPropertiesAreEqual(inputDto, competitiveEventResult);
        AssertJudgesAreEqual(inputDto.Judges, competitiveEventResult.Judges);

        Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
    }

    [Test]
    public async Task Create_WhenDtoIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await controller.Create(null);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("The request body is empty.", badRequestResult.Value);
    }
       
    [Test]
    public async Task Create_WhenJudgesAreInvalid_ReturnsBadRequest()
    {
        // Arrange
        var invalidDto = new CompetitiveEventCreateDto
        {
            Judges = new List<JudgeDto>
            {
                new JudgeDto { IsChiefJudge = true },
                new JudgeDto { IsChiefJudge = true },
            },
        };

        // Act
        var result = await controller.Create(invalidDto);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("A competitive event can have no more than one chief judge.", badRequestResult.Value);
    }

    [Test]
    public async Task Update_WhenValidDto_ReturnsOkObjectResult()
    {
        // Arrange
        var competitiveEvent = competitiveEvents.First();
        var inputDto = new CompetitiveEventUpdateDto()
        {
            Id = competitiveEvent.Id,
            Title = "Updated Title",
            ScheduledStartTime = DateTime.UtcNow.AddHours(1),
            ScheduledEndTime = DateTime.UtcNow.AddHours(2),
            Judges = new List<JudgeDto>
            {
                new JudgeDto { FirstName = "Judge A", LastName = "LastName A", Gender = Gender.Male },
                new JudgeDto { FirstName = "Judge B", LastName = "LastName B", Gender = Gender.Female }
            }
        };
        competitiveEventService.Setup(s => s.Update(inputDto)).ReturnsAsync(new CompetitiveEventDto
        {
            Id = inputDto.Id,
            Title = inputDto.Title,
            ScheduledStartTime = inputDto.ScheduledStartTime,
            ScheduledEndTime = inputDto.ScheduledEndTime,
            Judges = inputDto.Judges
        });

        // Act
        var result = await controller.Update(inputDto).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>(), "Expected OkObjectResult");

        var competitiveEventResult = (result as OkObjectResult).Value as CompetitiveEventDto;
        Assert.That(competitiveEventResult, Is.Not.Null);

        AssertCompetitiveEventPropertiesAreEqual(inputDto, competitiveEventResult);
        AssertJudgesAreEqual(inputDto.Judges, competitiveEventResult.Judges);
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

    [Test]
    public async Task Delete_WhenEntityExists_DeletesEntityAndReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingEvent = new CompetitiveEventDto { Id = id, Title = "Test Event" };

        competitiveEventService.Setup(s => s.GetById(id)).ReturnsAsync(existingEvent);
        competitiveEventService.Setup(s => s.Delete(id)).Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete(id);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        competitiveEventService.Verify(s => s.Delete(id), Times.Once);
    }
    
    private void AssertCompetitiveEventPropertiesAreEqual(CompetitiveEventCreateDto expected, CompetitiveEventDto actual)
    {
        Assert.That(actual, Is.Not.Null, "CompetitiveEventDto should not be null");
        Assert.AreEqual(expected.Title, actual.Title, "Title mismatch");
        Assert.AreEqual(expected.ScheduledStartTime, actual.ScheduledStartTime, "ScheduledStartTime mismatch");
        Assert.AreEqual(expected.ScheduledEndTime, actual.ScheduledEndTime, "ScheduledEndTime mismatch");
    }

    private void AssertJudgesAreEqual(List<JudgeDto> expectedJudges, List<JudgeDto> actualJudges)
    {
        Assert.That(actualJudges, Is.Not.Null, "Judges list should not be null");
        Assert.AreEqual(expectedJudges.Count, actualJudges.Count, "Judges count should match");

        for (int i = 0; i < expectedJudges.Count; i++)
        {
            Assert.AreEqual(expectedJudges[i].FirstName, actualJudges[i].FirstName, $"Judge #{i + 1} FirstName mismatch");
            Assert.AreEqual(expectedJudges[i].LastName, actualJudges[i].LastName, $"Judge #{i + 1} LastName mismatch");
            Assert.AreEqual(expectedJudges[i].Gender, actualJudges[i].Gender, $"Judge #{i + 1} Gender mismatch");
            Assert.AreEqual(expectedJudges[i].MiddleName, actualJudges[i].MiddleName, $"Judge #{i + 1} MiddleName mismatch");
            Assert.AreEqual(expectedJudges[i].Description, actualJudges[i].Description, $"Judge #{i + 1} Description mismatch");
        }
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
                Judges = new List<JudgeDto>
                {
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge A", MiddleName="A", LastName = "LastName A", Gender = Gender.Male },
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge B", MiddleName = "B", LastName = "LastName B", Gender = Gender.Female }
                }
            },
            new CompetitiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = "Test2",
                Description = "Test2",
                Judges = new List<JudgeDto>
                {
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge C", LastName = "LastName C", Gender = Gender.Male },
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge D", LastName = "LastName D", Gender = Gender.Female },
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge E", LastName = "LastName E", Gender = Gender.Male },
                }
            },
            new CompetitiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = "Test3",
                Description = "Test3",
                Judges = new List<JudgeDto>
                {
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge F", LastName = "LastName F", Description= "Description F" },
                }
            },
        };
    }
}
