using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
class CompetitiveEventServiceUpdateAndCreateTests
{
    private Mock<ICompetitiveEventRepository> mockCompetitiveEventRepository;
    private Mock<IEntityRepository<Guid, CompetitiveEventDescriptionItem>> mockDescriptionItemRepository;
    private Mock<ILogger<CompetitiveEventService>> mockLogger;
    private Mock<IStringLocalizer<SharedResource>> mockLocalizer;
    private Mock<ICurrentUserService> userService;
    private Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>> contactsService;
    private Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>> competitiveImagesService;

    private CompetitiveEventService service;

    [SetUp]
    public void SetUp()
    {
        mockCompetitiveEventRepository = new Mock<ICompetitiveEventRepository>();
        mockDescriptionItemRepository = new Mock<IEntityRepository<Guid, CompetitiveEventDescriptionItem>>();
        mockLogger = new Mock<ILogger<CompetitiveEventService>>();
        mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        userService = new Mock<ICurrentUserService>();
        contactsService = new Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>>();
        competitiveImagesService = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>>();

        service = new CompetitiveEventService(
            mockCompetitiveEventRepository.Object,
            mockDescriptionItemRepository.Object,
            mockLogger.Object,
            mockLocalizer.Object,
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
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            CompetitiveEventAccountingTypeId = 1,
        };

        contactsService
            .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Verifiable();

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent);

        // Act
        var result = await service.Create(input).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(input.Title, result.Title);

        mockCompetitiveEventRepository.Verify(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()), Times.Once);
        contactsService.Verify(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()), Times.Once);
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
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        mockCompetitiveEventRepository
           .Setup(r => r.GetByIdWithDetails(existingEventId,
           String.Empty,
           It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
           .ReturnsAsync(competitiveEvent);

        mockCompetitiveEventRepository
            .Setup(r => r.Update(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync((CompetitiveEvent input) => input);

        contactsService
            .Setup(c => c.PrepareUpdatedContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Verifiable();

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(async operation => await operation());

        // Act
        var result = await service.Update(updateDto);

        // Assert
        Assert.IsNotNull(result, "Result of Update should not be null.");
        Assert.AreEqual("New Title", result.Title, "Title was not updated correctly.");

        mockCompetitiveEventRepository.Verify(r => r.GetByIdWithDetails(
           existingEventId, It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()), Times.Once);

       mockCompetitiveEventRepository.Verify(r => r.Update(It.IsAny<CompetitiveEvent>()), Times.Once);
       contactsService.Verify(c => c.PrepareUpdatedContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()), Times.Once);
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
            .ReturnsAsync((CompetitiveEvent)null); 

        // Act & Assert
        var ex = Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(updateDto));

        Assert.That(ex.Message, Does.Contain($"CompetitiveEvent with Id = {invalidEventId} doesn't exist in the system."));

        mockCompetitiveEventRepository.Verify(r => r.GetByIdWithDetails(
            invalidEventId,
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()), Times.Once);

        mockCompetitiveEventRepository.Verify(r => r.Update(It.IsAny<CompetitiveEvent>()), Times.Never);
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
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>()
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            CompetitiveEventAccountingTypeId = input.CompetitiveEventAccountingTypeId
        };

        contactsService
            .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
            .Verifiable();

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent);

        // Act
        var result = await service.Create(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(input.Title, result.Title);
        Assert.IsNull(createdEvent.CompetitiveEventDescriptionItems);
        Assert.IsNull(createdEvent.CompetitiveEventDescriptionItems, "CompetitiveEventDescriptionItems should remain null if input is null.");

        mockCompetitiveEventRepository.Verify(
            r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()),
            Times.Once);
    }

    [Test]
    public async Task Create_WhenDescriptionItemsAreNotEmpty_MapsAndAddsThem()
    {
        // Arrange
        var input = new CompetitiveEventCreateUpdateDto
        {
            Title = "Test Event",
            CompetitiveEventAccountingTypeId = 1,
            CompetitiveEventDescriptionItems = [
                new() { Description = "Desc 1", SectionName = "Section 1" },
                new() { Description = "Desc 2", SectionName = "Section 2" }
            ]
        };

        var createdEvent = input.ToModel();
        createdEvent.CompetitiveEventDescriptionItems = input.CompetitiveEventDescriptionItems.ToModel();

        contactsService
           .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventCreateUpdateDto>()))
           .Verifiable();

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .ReturnsAsync(createdEvent);

        // Act
        var result = await service.Create(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(input.Title, result.Title);
        Assert.AreEqual(input.CompetitiveEventDescriptionItems.Count, createdEvent.CompetitiveEventDescriptionItems.Count);

        var expected = input.CompetitiveEventDescriptionItems[0];
        var actual = createdEvent.CompetitiveEventDescriptionItems.First();
        Assert.AreEqual(expected.Description, actual.Description, $"First description of item is not mapped correctly.");
        Assert.AreEqual(expected.SectionName, actual.SectionName, $"First SectionName of item is not mapped correctly.");
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
            .ReturnsAsync(competitiveEvent);

        mockCompetitiveEventRepository
            .Setup(r => r.Update(It.IsAny<CompetitiveEvent>()))
            .ReturnsAsync((CompetitiveEvent input) => input);

        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(async operation => await operation());
    }

    private void AssertValidUpdateResult(CompetitiveEventDto result, Guid mustBeDeletedId)
    {
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Event", result.Title);

        mockDescriptionItemRepository.Verify(r => r.Create(It.IsAny<CompetitiveEventDescriptionItem>()), Times.Once);

        mockDescriptionItemRepository.Verify(r => r.Delete(It.Is<CompetitiveEventDescriptionItem>(item =>
            item.Id == mustBeDeletedId && item.Description == "Must deleted Description" && item.SectionName == "Must deleted Section"
        )), Times.Once);


        mockCompetitiveEventRepository.Verify(r => r.GetByIdWithDetails(
            It.IsAny<Guid>(), 
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()), Times.Once);

        mockCompetitiveEventRepository.Verify(r => r.Update(It.Is<CompetitiveEvent>(e =>
            e.CompetitiveEventDescriptionItems.Count == 2
        )), Times.Once);
    }
}
