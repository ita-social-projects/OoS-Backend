using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
class CompetitiveEventServiceUpdateAndCreateTests
{
    private Mock<IEntityRepositorySoftDeleted<Guid, CompetitiveEvent>> mockCompetitiveEventRepository;
    private Mock<IEntityRepository<Guid, Judge>> mockJudgeRepository;
    private Mock<IEntityRepository<Guid, CompetitiveEventDescriptionItem>> mockDescriptionItemRepository;
    private Mock<ILogger<CompetitiveEventService>> mockLogger;
    private Mock<IStringLocalizer<SharedResource>> mockLocalizer;
    private Mock<IMapper> mockMapper;

    private CompetitiveEventService service;

    [SetUp]
    public void SetUp()
    {
        mockCompetitiveEventRepository = new Mock<IEntityRepositorySoftDeleted<Guid, CompetitiveEvent>>();
        mockJudgeRepository = new Mock<IEntityRepository<Guid, Judge>>();
        mockDescriptionItemRepository = new Mock<IEntityRepository<Guid, CompetitiveEventDescriptionItem>>();
        mockLogger = new Mock<ILogger<CompetitiveEventService>>();
        mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        mockMapper = new Mock<IMapper>();

        service = new CompetitiveEventService(
            mockCompetitiveEventRepository.Object,
            mockJudgeRepository.Object,
            mockDescriptionItemRepository.Object,
            mockLogger.Object,
            mockLocalizer.Object,
            mockMapper.Object);
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var judge1Id = Guid.NewGuid();
        var judge2Id = Guid.NewGuid();
        var input = new CompetitiveEventCreateDto
        {
            Title = "Test",
            CompetitiveEventAccountingTypeId = 1,
            Judges = new List<JudgeDto>
            {
                new JudgeDto { Id = judge1Id, FirstName = "Judge 1" },
                new JudgeDto { Id = judge2Id, FirstName = "Judge 2" }
            }
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            CompetitiveEventAccountingTypeId = 1,
            Judges = input.Judges.Select(dto => new Judge
            {
                Id = dto.Id,
                FirstName = dto.FirstName
            }).ToList()
        };

        mockMapper
            .Setup(m => m.Map<CompetitiveEvent>(It.IsAny<CompetitiveEventCreateDto>()))
            .Returns(createdEvent);

        mockMapper
            .Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns(new CompetitiveEventDto
            {
                Id = createdEvent.Id,
                Title = createdEvent.Title,
                Judges = input.Judges,
            });

        
        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent);// ???

        // Act
        var result = await service.Create(input).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(input.Title, result.Title);

        Assert.IsNotNull(result.Judges, "Judges should not be null.");
        Assert.AreEqual(2, result.Judges.Count, "There should be 2 judges.");
        Assert.AreEqual("Judge 1", result.Judges[0].FirstName, "First judge's name is incorrect.");
        Assert.AreEqual("Judge 2", result.Judges[1].FirstName, "Second judge's name is incorrect.");

        mockMapper.Verify(m => m.Map<CompetitiveEvent>(It.IsAny<CompetitiveEventCreateDto>()), Times.Once);
        mockMapper.Verify(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()), Times.Once);
        mockCompetitiveEventRepository.Verify(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()), Times.Once);
    }

    [Test]
    public async Task Update_WhenCompetitiveEventExists_UpdatesEntity()
    {
        // Arrange
        var existingEventId = Guid.NewGuid();
        var competitiveEvent = new CompetitiveEvent
        {
            Id = existingEventId,
            Title = "Old Title",
            Judges = new List<Judge>(),
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItem>()
        };

        var updateDto = new CompetitiveEventUpdateDto
        {
            Id = existingEventId,
            Title = "New Title",
            Judges = new List<JudgeDto>(),
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        mockCompetitiveEventRepository
            .Setup(r => r.GetByIdWithDetails(existingEventId, "Judges,CompetitiveEventDescriptionItems"))
            .ReturnsAsync(competitiveEvent);

        mockCompetitiveEventRepository
            .Setup(r => r.Update(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync((CompetitiveEvent input) => input);

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(async operation => await operation());

        mockMapper.Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns(new CompetitiveEventDto { Id = existingEventId, Title = "New Title" });

        // Act
        var result = await service.Update(updateDto);

        // Assert
        Assert.IsNotNull(result, "Result of Update should not be null.");
        Assert.AreEqual("New Title", result.Title, "Title was not updated correctly.");
        mockCompetitiveEventRepository.Verify(r => r.GetByIdWithDetails(existingEventId, "Judges,CompetitiveEventDescriptionItems"), Times.Once);
        mockCompetitiveEventRepository.Verify(r => r.Update(It.IsAny<CompetitiveEvent>()), Times.Once);
    }

    [Test]
    public async Task Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var invalidEventId = Guid.NewGuid();
        var updateDto = new CompetitiveEventUpdateDto
        {
            Id = invalidEventId,
            Title = "Invalid Event",
            Judges = new List<JudgeDto>(),
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        mockCompetitiveEventRepository
            .Setup(r => r.GetByIdWithDetails(invalidEventId, "Judges,CompetitiveEventDescriptionItems"))
            .ReturnsAsync((CompetitiveEvent)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(updateDto));

        Assert.That(ex.Message, Does.Contain($"CompetitiveEvent with Id = {invalidEventId} doesn't exist in the system."));
        mockCompetitiveEventRepository.Verify(r => r.GetByIdWithDetails(invalidEventId, "Judges,CompetitiveEventDescriptionItems"), Times.Once);
        mockCompetitiveEventRepository.Verify(r => r.Update(It.IsAny<CompetitiveEvent>()), Times.Never);
    }

    [Test]
    public async Task Update_WhenDescriptionItemsAndJudgesAreUpdated_UpdatesCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingDescriptionItemId = Guid.NewGuid();
        var newDescriptionItemId = Guid.NewGuid();
        var mustBeDeletedDescItemId = Guid.NewGuid();

        var existingJudgeId = Guid.NewGuid();
        var newJudgeId = Guid.NewGuid();
        var mustBeDeletedJudgeId = Guid.NewGuid();

        var competitiveEvent = CreateCompetitiveEvent(eventId, existingDescriptionItemId, mustBeDeletedDescItemId, existingJudgeId, mustBeDeletedJudgeId);
        var updateDto = CreateUpdateDto(eventId, existingDescriptionItemId, newDescriptionItemId, existingJudgeId, newJudgeId);

        SetupMocksForUpdateTest(competitiveEvent, eventId);

        // Act
        var result = await service.Update(updateDto);

        // Assert
        AssertValidUpdateResult(result, mustBeDeletedDescItemId);
    }
    private CompetitiveEvent CreateCompetitiveEvent(Guid eventId, Guid existingDescriptionItemId, Guid mustBeDeletedDescItemId, Guid existingJudgeId, Guid mustBeDeletedJudgeId)
    {
        return new CompetitiveEvent
        {
            Id = eventId,
            Title = "Existing Event",
            Judges = new List<Judge>()
            {
                new Judge
                {
                    Id = existingJudgeId,
                    FirstName = "Old Judge's FirstName",
                },
                new Judge
                {
                    Id = mustBeDeletedJudgeId,
                    FirstName = "Must delted Judge's FirstName",
                },
            },
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItem>
            {
                new CompetitiveEventDescriptionItem
                {
                    Id = existingDescriptionItemId,
                    Description = "Old Description",
                    SectionName = "Old Section"
                },
                new CompetitiveEventDescriptionItem
                {
                    Id = mustBeDeletedDescItemId,
                    Description = "Must deleted Description",
                    SectionName = "Must deleted Section"
                }
            }
        };
    }

    private CompetitiveEventUpdateDto CreateUpdateDto(Guid eventId, Guid existingDescriptionItemId, Guid newDescriptionItemId, Guid existingJudgeId, Guid newJudgeId)
    {
        return new CompetitiveEventUpdateDto
        {
            Id = eventId,
            Title = "Updated Event",
            Judges = new List<JudgeDto>()
            {
                new JudgeDto
                {
                    Id = existingJudgeId,
                    FirstName = "Updated Judge's FirstName",
                },
                new JudgeDto
                {
                    Id = newJudgeId,
                    FirstName = "New Judge's FirstName",
                },
            },
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>
            {
                new CompetitiveEventDescriptionItemDto
                {
                    Id = existingDescriptionItemId,
                    Description = "Updated Description",
                    SectionName = "Updated Section"
                },
                new CompetitiveEventDescriptionItemDto
                {
                    Id = newDescriptionItemId,
                    Description = "New Description",
                    SectionName = "New Section"
                }
            }
        };
    }

    private void SetupMocksForUpdateTest(CompetitiveEvent competitiveEvent, Guid eventId)
    {
        mockCompetitiveEventRepository
            .Setup(r => r.GetByIdWithDetails(eventId, "Judges,CompetitiveEventDescriptionItems"))
            .ReturnsAsync(competitiveEvent);

        mockCompetitiveEventRepository
            .Setup(r => r.Update(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync((CompetitiveEvent input) => input);

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(async operation => await operation());

        mockMapper
            .Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns(new CompetitiveEventDto
            {
                Id = eventId,
                Title = "Updated Event"
            });

        mockMapper
            .Setup(m => m.Map<CompetitiveEventDescriptionItem>(It.IsAny<CompetitiveEventDescriptionItemDto>()))
            .Returns<CompetitiveEventDescriptionItemDto>(dto => new CompetitiveEventDescriptionItem
            {
                Id = dto.Id,
                Description = dto.Description,
                SectionName = dto.SectionName,
                CompetitiveEventId = Guid.Empty
            });

        mockMapper
          .Setup(m => m.Map<Judge>(It.IsAny<JudgeDto>()))
          .Returns<JudgeDto>(dto => new Judge
          {
              Id = dto.Id,
              FirstName = dto.FirstName,
              CompetitiveEventId = Guid.Empty
          });
    }

    private void AssertValidUpdateResult(CompetitiveEventDto result, Guid mustBeDeletedId)
    {
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Event", result.Title);

        mockDescriptionItemRepository.Verify(r => r.Create(It.IsAny<CompetitiveEventDescriptionItem>()), Times.Once);

        mockDescriptionItemRepository.Verify(r => r.Delete(It.Is<CompetitiveEventDescriptionItem>(item =>
            item.Id == mustBeDeletedId && item.Description == "Must deleted Description" && item.SectionName == "Must deleted Section"
        )), Times.Once);

        mockCompetitiveEventRepository.Verify(r => r.GetByIdWithDetails(It.IsAny<Guid>(), "Judges,CompetitiveEventDescriptionItems"), Times.Once);

        mockCompetitiveEventRepository.Verify(r => r.Update(It.Is<CompetitiveEvent>(e =>
            e.CompetitiveEventDescriptionItems.Count == 2
        )), Times.Once);
    }
}


