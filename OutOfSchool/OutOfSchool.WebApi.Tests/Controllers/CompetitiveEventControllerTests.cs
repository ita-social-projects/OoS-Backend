using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
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
    IMapper mapper;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        this.mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>(); // is it enough?
    }

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
                    FirstName ="Judge A"
                },
                new JudgeDto()
                {
                    FirstName ="Judge B"
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

        Assert.AreEqual(inputDto.Title, competitiveEventResult.Title);
        Assert.AreEqual(inputDto.ScheduledStartTime, competitiveEventResult.ScheduledStartTime);
        Assert.AreEqual(inputDto.ScheduledEndTime, competitiveEventResult.ScheduledEndTime);

        // var expectedDto = mapper.Map<CompetitiveEventDto>(inputDto); // does not work mapping
        // AssertCompetitiveEventPropertiesAreEqual(expectedDto, competitiveEventResult);

        AssertJudgesAreEqual(inputDto.Judges, competitiveEventResult.Judges);

        Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
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
                new JudgeDto { FirstName = "Judge A", LastName = "LastName A" },
                new JudgeDto { FirstName = "Judge B", LastName = "LastName B" }
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

        Assert.AreEqual(inputDto.Title, competitiveEventResult.Title);
        Assert.AreEqual(inputDto.ScheduledStartTime, competitiveEventResult.ScheduledStartTime);
        Assert.AreEqual(inputDto.ScheduledEndTime, competitiveEventResult.ScheduledEndTime);

        // var expectedDto = mapper.Map<CompetitiveEventDto>(inputDto); // does not work mapping : could not find needed map

        //AssertCompetitiveEventPropertiesAreEqual(expectedDto, competitiveEventResult);
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

    private void AssertCompetitiveEventPropertiesAreEqual(CompetitiveEventDto expected, CompetitiveEventDto actual)
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
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge A", LastName = "LastName A" },
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge B", LastName = "LastName B" }
                }
            },
            new CompetitiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = "Test2",
                Description = "Test2",
                Judges = new List<JudgeDto>
                {
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge C", LastName = "LastName C" },
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge D", LastName = "LastName D" },
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge E", LastName = "LastName E" },
                }
            },
            new CompetitiveEventDto
            {
                Id = Guid.NewGuid(),
                Title = "Test3",
                Description = "Test3",
                Judges = new List<JudgeDto>
                {
                    new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge F", LastName = "LastName F" },
                }
            },
        };
    }
}
