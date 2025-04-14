using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
class CompetitiveEventServiceUpdateAndCreateTests
{
    private Mock<ICompetitiveEventRepository> mockCompetitiveEventRepository;
    private Mock<IEntityRepository<Guid, CompetitiveEventDescriptionItem>> mockDescriptionItemRepository;
    private Mock<IEntityRepository<long, SubDirection>> mockSubDirectionRepository;
    private Mock<ILogger<CompetitiveEventService>> mockLogger;
    private Mock<IStringLocalizer<SharedResource>> mockLocalizer;
    private Mock<IMapper> mockMapper;
    private Mock<ICurrentUserService> userService;
    private Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>> contactsService;
    private Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>> competitiveImagesService;

    private CompetitiveEventService service;

    [SetUp]
    public void SetUp()
    {
        mockCompetitiveEventRepository = new Mock<ICompetitiveEventRepository>();
        mockDescriptionItemRepository = new Mock<IEntityRepository<Guid, CompetitiveEventDescriptionItem>>();
        mockSubDirectionRepository = new Mock<IEntityRepository<long, SubDirection>>();
        mockLogger = new Mock<ILogger<CompetitiveEventService>>();
        mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        mockMapper = new Mock<IMapper>();
        userService = new Mock<ICurrentUserService>();
        contactsService = new Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>>();
        competitiveImagesService = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>>();

        service = new CompetitiveEventService(
            mockCompetitiveEventRepository.Object,
            mockDescriptionItemRepository.Object,
            mockSubDirectionRepository.Object,
            mockLogger.Object,
            mockLocalizer.Object,
            mockMapper.Object,
            userService.Object,
            contactsService.Object,
            competitiveImagesService.Object);
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var judge1Id = Guid.NewGuid();
        var judge2Id = Guid.NewGuid();
        var input = new CompetitiveEventCreateUpdateDto
        {
            Title = "Test",
            CompetitiveEventAccountingTypeId = 1,
            SubDirectionIds = [1]
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            CompetitiveEventAccountingTypeId = 1,
        };

        mockMapper
            .Setup(m => m.Map<CompetitiveEvent>(It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Returns(createdEvent)
            .Verifiable(Times.Once);
        mockMapper
            .Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns(new CompetitiveEventDto
            {
                Id = createdEvent.Id,
                Title = createdEvent.Title,
            })
            .Verifiable(Times.Once);
        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        contactsService
            .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Verifiable(Times.Once);
        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent)
            .Verifiable(Times.Once);

        // Act
        var result = await service.Create(input).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(input.Title, result.Title);
        Mock.VerifyAll();
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
            SubDirections = new List<SubDirection>() { new SubDirection { Id = 1 } },
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItem>()
            {
                new CompetitiveEventDescriptionItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Description 1",
                    SectionName = "Section 1"
                }
            }
        };

        var updateDto = new CompetitiveEventCreateUpdateDto
        {
            Id = existingEventId,
            Title = "New Title",
            SubDirectionIds = [1],
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        mockCompetitiveEventRepository
           .Setup(r => r.GetByIdWithDetails(
               existingEventId,
               string.Empty,
               It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
           .ReturnsAsync(competitiveEvent)
           .Verifiable(Times.Once);
        mockCompetitiveEventRepository
            .Setup(r => r.Update(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync((CompetitiveEvent input) => input)
            .Verifiable(Times.Once);
        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        contactsService
            .Setup(c => c.PrepareUpdatedContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Verifiable(Times.Once);
        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(async operation => await operation())
            .Verifiable(Times.Once);
        mockMapper.Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns(new CompetitiveEventDto { Id = existingEventId, Title = "New Title" })
            .Verifiable(Times.Once);

        // Act
        var result = await service.Update(updateDto);

        // Assert
        Assert.IsNotNull(result, "Result of Update should not be null.");
        Assert.AreEqual("New Title", result.Title, "Title was not updated correctly.");
        Mock.VerifyAll();
    }

    [Test]
    public async Task Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var invalidEventId = Guid.NewGuid();
        var updateDto = new CompetitiveEventCreateUpdateDto
        {
            Id = invalidEventId,
            Title = "Invalid Event",
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        mockCompetitiveEventRepository
            .Setup(r => r.GetByIdWithDetails(
                invalidEventId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<CompetitiveEvent>,
                IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync((CompetitiveEvent)null)
            .Verifiable(Times.Once);

        // Act & Assert
        var ex = Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(updateDto));

        Assert.That(ex.Message, Does.Contain("Updating failed. CompetitiveEvent with Id = {dtoId} doesn't exist in the DB."));
        mockCompetitiveEventRepository.Verify(r => r.Update(It.IsAny<CompetitiveEvent>()), Times.Never);
        Mock.VerifyAll();
    }

    [Test]
    public async Task Update_WhenDescriptionItemsAreUpdated_UpdatesCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingDescriptionItemId = Guid.NewGuid();
        var newDescriptionItemId = Guid.NewGuid();
        var mustBeDeletedDescItemId = Guid.NewGuid();

        var competitiveEvent = CreateCompetitiveEvent(eventId, existingDescriptionItemId, mustBeDeletedDescItemId);
        var updateDto = CreateUpdateDto(eventId, existingDescriptionItemId, newDescriptionItemId);

        SetupMocksForUpdateTest(competitiveEvent, eventId);

        // Act
        var result = await service.Update(updateDto);

        // Assert
        AssertValidUpdateResult(result, mustBeDeletedDescItemId);
    }

    [Test]
    public async Task Create_WhenDescriptionItemsAreNullOrEmpty_DoesNotMapDescriptionItems()
    {
        // Arrange
        var input = new CompetitiveEventCreateUpdateDto
        {
            Title = "Test Event",
            CompetitiveEventAccountingTypeId = 1,
            SubDirectionIds = [1],
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            CompetitiveEventAccountingTypeId = input.CompetitiveEventAccountingTypeId
        };

        mockMapper
            .Setup(m => m.Map<CompetitiveEvent>(input))
            .Returns(createdEvent)
            .Verifiable(Times.Once);
        mockMapper
            .Setup(m => m.Map<CompetitiveEventDto>(createdEvent))
            .Returns(new CompetitiveEventDto
            {
                Id = createdEvent.Id,
                Title = createdEvent.Title,
            })
            .Verifiable(Times.Once);
        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        contactsService
            .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Verifiable(Times.Once);
        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent)
            .Verifiable(Times.Once);

        // Act
        var result = await service.Create(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(input.Title, result.Title);
        Assert.IsNull(createdEvent.CompetitiveEventDescriptionItems);
        Mock.VerifyAll();
    }

    [Test]
    public async Task Create_WhenDescriptionItemsAreNotEmpty_MapsAndAddsThem()
    {
        // Arrange
        var input = new CompetitiveEventCreateUpdateDto
        {
            Title = "Test Event",
            CompetitiveEventAccountingTypeId = 1,
            SubDirectionIds = [1],
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>
            {
                new CompetitiveEventDescriptionItemDto { Description = "Desc 1", SectionName = "Section 1" },
                new CompetitiveEventDescriptionItemDto { Description = "Desc 2", SectionName = "Section 2" }
            }
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            CompetitiveEventAccountingTypeId = input.CompetitiveEventAccountingTypeId,
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItem>()
        };

        mockMapper
            .Setup(m => m.Map<CompetitiveEvent>(input))
            .Returns(createdEvent)
            .Verifiable(Times.Once);
        mockMapper
            .Setup(m => m.Map<CompetitiveEventDescriptionItem>(It.IsAny<CompetitiveEventDescriptionItemDto>()))
            .Returns<CompetitiveEventDescriptionItemDto>(dto => new CompetitiveEventDescriptionItem
            {
                Description = dto.Description,
                SectionName = dto.SectionName
            })
            .Verifiable(Times.Exactly(input.CompetitiveEventDescriptionItems.Count));
        mockMapper
            .Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns((CompetitiveEvent ce) => new CompetitiveEventDto
            {
                Id = ce.Id,
                Title = ce.Title,
                CompetitiveEventAccountingTypeId = ce.CompetitiveEventAccountingTypeId
            })
            .Verifiable(Times.Once);
        contactsService
           .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
           .Verifiable(Times.Once);
        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent)
            .Verifiable(Times.Once);

        // Act
        var result = await service.Create(input);

        // Assert
        var expected = input.CompetitiveEventDescriptionItems[0];
        var actual = createdEvent.CompetitiveEventDescriptionItems.First();
        Assert.IsNotNull(result);
        Assert.AreEqual(input.Title, result.Title);
        Assert.AreEqual(input.CompetitiveEventDescriptionItems.Count, createdEvent.CompetitiveEventDescriptionItems.Count);
        Assert.AreEqual(expected.Description, actual.Description, $"First description of item is not mapped correctly.");
        Assert.AreEqual(expected.SectionName, actual.SectionName, $"First SectionName of item is not mapped correctly.");
        Mock.VerifyAll();
    }

    private CompetitiveEvent CreateCompetitiveEvent(Guid eventId, Guid existingDescriptionItemId, Guid mustBeDeletedDescItemId)
    {
        return new CompetitiveEvent
        {
            Id = eventId,
            Title = "Existing Event",
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

    private CompetitiveEventCreateUpdateDto CreateUpdateDto(Guid eventId, Guid existingDescriptionItemId, Guid newDescriptionItemId)
    {
        return new CompetitiveEventCreateUpdateDto
        {
            Id = eventId,
            Title = "Updated Event",
            SubDirectionIds = [1],
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

            .Setup(r => r.GetByIdWithDetails(
                eventId,
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync(competitiveEvent)
            .Verifiable(Times.Once);

        mockCompetitiveEventRepository
            .Setup(r => r.Update(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync((CompetitiveEvent input) => input)
            .Verifiable(Times.Once);

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(async operation => await operation())
            .Verifiable(Times.Once);

        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);

        mockMapper
            .Setup(m => m.Map<CompetitiveEventDto>(It.IsAny<CompetitiveEvent>()))
            .Returns(new CompetitiveEventDto
            {
                Id = eventId,
                Title = "Updated Event"
            })
            .Verifiable(Times.Once);

        mockMapper
            .Setup(m => m.Map<CompetitiveEventDescriptionItem>(It.IsAny<CompetitiveEventDescriptionItemDto>()))
            .Returns<CompetitiveEventDescriptionItemDto>(dto => new CompetitiveEventDescriptionItem
            {
                Id = dto.Id,
                Description = dto.Description,
                SectionName = dto.SectionName,
                CompetitiveEventId = Guid.Empty
            })
            .Verifiable(Times.Once);
    }

    private void AssertValidUpdateResult(CompetitiveEventDto result, Guid mustBeDeletedId)
    {
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Event", result.Title);
        Mock.VerifyAll();
    }
}


