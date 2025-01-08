using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
public class CompetitiveEventControllerTests
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
        CompetitiveEventCreateDto inputDto = FakeCompetitiveEventCreateDto();
        competitiveEventService.Setup(x => x.Create(It.IsAny<CompetitiveEventCreateDto>()))
            .ReturnsAsync(
            new CompetitiveEventDto()
            {
                Id = Guid.NewGuid(),
                Title = inputDto.Title,
                ScheduledStartTime = inputDto.ScheduledStartTime,
                ScheduledEndTime = inputDto.ScheduledEndTime,
                RegistrationStartTime = inputDto.RegistrationStartTime,
                RegistrationEndTime = inputDto.RegistrationEndTime,
                Description = inputDto.Description,
                DescriptionOfTheEnrollmentProcedure = inputDto.DescriptionOfTheEnrollmentProcedure,
                AreThereBenefits = inputDto.AreThereBenefits,
                Benefits = inputDto.Benefits,
                NumberOfSeats = inputDto.NumberOfSeats,
                NumberOfOccupiedSeats = inputDto.NumberOfOccupiedSeats,
                OptionsForPeopleWithDisabilities = inputDto.OptionsForPeopleWithDisabilities,
                DescriptionOfOptionsForPeopleWithDisabilities = inputDto.DescriptionOfOptionsForPeopleWithDisabilities,
                MinimumAge = inputDto.MinimumAge,
                MaximumAge = inputDto.MaximumAge,
                VenueName = inputDto.VenueName,
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
        CompetitiveEventCreateDto invalidDto = FakeInvalidCreateDto();

        // Act
        var result = await controller.Create(invalidDto);

        // Assert
        AssertBadRequestWithMessage(result, "A competitive event can have no more than one chief judge.");
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
    public async Task Update_WhenDtoIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await controller.Update(null);

        // Assert
        AssertBadRequestWithMessage(result, "The request body is empty.");

    }

    [Test]
    public async Task Update_WhenJudgesAreInvalid_ReturnBadRequest()
    {
        // Arrange
        var invalidUpdateDto = FakeInvalidCreateDto();

        // Act
        var result = await controller.Update(invalidUpdateDto);

        // Assert
        AssertBadRequestWithMessage(result, "A competitive event can have no more than one chief judge.");
    }

    [Test]
    public async Task Update_WhenIdNotFound_ThrowsDbUpdateConcurrencyException_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var inputDto = new CompetitiveEventUpdateDto
        {
            Id = nonExistentId,
            Title = "Non-existent Event",
            ScheduledStartTime = DateTime.UtcNow.AddHours(1),
            ScheduledEndTime = DateTime.UtcNow.AddHours(2),
            Judges = new List<JudgeDto> { new JudgeDto { FirstName = "Judge A", IsChiefJudge = true } }
        };

        competitiveEventService.Setup(s => s.Update(inputDto))
            .ThrowsAsync(new DbUpdateConcurrencyException($"Event with ID {nonExistentId} not found."));

        // Act
        var result = await controller.Update(inputDto);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual($"Event with ID {nonExistentId} not found.", notFoundResult.Value);
    }

    [Test]
    public async Task Update_WhenUnexpectedErrorOccurs_ReturnsInternalServerError()
    {
        // Arrange
        var inputDto = new CompetitiveEventUpdateDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            ScheduledStartTime = DateTime.UtcNow.AddHours(1),
            ScheduledEndTime = DateTime.UtcNow.AddHours(2),
            Judges = new List<JudgeDto> { new JudgeDto { FirstName = "Judge A", IsChiefJudge = true } }
        };

        competitiveEventService.Setup(s => s.Update(inputDto))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await controller.Update(inputDto);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.AreEqual("An unexpected error occurred. Unexpected error", objectResult.Value);
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

    private void AssertBadRequestWithMessage(IActionResult result, string message)
    {
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(message, badRequestResult.Value);
    }
    private void AssertCompetitiveEventPropertiesAreEqual(CompetitiveEventCreateDto expected, CompetitiveEventDto actual)
    {
        Assert.That(actual, Is.Not.Null, "CompetitiveEventDto should not be null");

        // Get properties that are primitive, string, DateTimeOffset, or decimal types (simple types)
        var simpleProperties = typeof(CompetitiveEventCreateDto)
            .GetProperties()
            .Where(p => (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(decimal))
            && !p.Name.Contains("Id", StringComparison.OrdinalIgnoreCase)); 

        foreach (var property in simpleProperties)
        {
            var expectedValue = property.GetValue(expected);
            var actualProperty = typeof(CompetitiveEventDto).GetProperty(property.Name);

            if (actualProperty != null)
            {
                var actualValue = actualProperty.GetValue(actual);
                Assert.AreEqual(expectedValue, actualValue, $"Property '{property.Name}' mismatch");
            }
            else
            {
                Assert.Fail($"Property '{property.Name}' is missing in the target object");
            }
        }
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

    private static CompetitiveEventUpdateDto FakeInvalidCreateDto()
    {
        return new CompetitiveEventUpdateDto
        {
            Title = "Title",
            ShortTitle = "Short Title",
            AdditionalDescription = "Additional Description",
            Judges = new List<JudgeDto>
            {
                new JudgeDto { IsChiefJudge = true },
                new JudgeDto { IsChiefJudge = true },
            },
        };
    }
    private static CompetitiveEventCreateDto FakeCompetitiveEventCreateDto()
    {
        return new CompetitiveEventCreateDto()
        {
            Title = "New Event",
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow.AddHours(1),
            RegistrationStartTime = DateTime.UtcNow,
            RegistrationEndTime = DateTime.UtcNow.AddHours(1),
            Description = "Event Description",
            DescriptionOfTheEnrollmentProcedure = "Event Description Of The Enrollment Procedure",
            AreThereBenefits = true,
            Benefits = "Event Benefits",
            NumberOfSeats = 100,
            NumberOfOccupiedSeats = 99,
            OptionsForPeopleWithDisabilities = true,
            DescriptionOfOptionsForPeopleWithDisabilities = "Description Of Options For People With Disabilities",
            MinimumAge = 0,
            MaximumAge = 70,
            VenueName = "Venue Name",
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
                RegistrationStartTime = DateTime.UtcNow,
                RegistrationEndTime = DateTime.UtcNow.AddHours(1),
                MaximumAge = 20,
                MinimumAge = 10,
                AreThereBenefits = true,
                Benefits = "Some benefits",
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
