using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        userService = new Mock<ICurrentUserService>();
        contactsService = new Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>>();
        competitiveImagesService = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>>();

        service = new CompetitiveEventService(
            mockCompetitiveEventRepository.Object,
            mockDescriptionItemRepository.Object,
            mockSubDirectionRepository.Object,
            mockLogger.Object,
            mockLocalizer.Object,
            userService.Object,
            contactsService.Object,
            competitiveImagesService.Object);
    }

    #region Create
    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var judge1Id = Guid.NewGuid();
        var judge2Id = Guid.NewGuid();
        var input = new CompetitiveEventBaseDto
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

        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        contactsService
            .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventBaseDto>()))
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
    public async Task Create_WhenDescriptionItemsAreNullOrEmpty_DoesNotMapDescriptionItems()
    {
        // Arrange
        var input = new CompetitiveEventBaseDto
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

        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
        contactsService
            .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventBaseDto>()))
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
        var input = new CompetitiveEventBaseDto
        {
            Title = "Test Event",
            CompetitiveEventAccountingTypeId = 1,
            SubDirectionIds = [1],
            CompetitiveEventDescriptionItems =
            [
                new CompetitiveEventDescriptionItemDto { Description = "Desc 1", SectionName = "Section 1" },
                new CompetitiveEventDescriptionItemDto { Description = "Desc 2", SectionName = "Section 2" }
            ]
        };

        var createdEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            CompetitiveEventAccountingTypeId = input.CompetitiveEventAccountingTypeId,
            CompetitiveEventDescriptionItems =
            [
                new CompetitiveEventDescriptionItem { Description = "Desc 1", SectionName = "Section 1" },
                new CompetitiveEventDescriptionItem { Description = "Desc 2", SectionName = "Section 2" }
            ]
        };

        contactsService
           .Setup(c => c.PrepareNewContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventBaseDto>()))
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
        // Verify second item
        var expected2 = input.CompetitiveEventDescriptionItems[1];
        var actual2 = createdEvent.CompetitiveEventDescriptionItems.ElementAt(1);
        Assert.AreEqual(expected2.Description, actual2.Description, $"Second description of item is not mapped correctly.");
        Assert.AreEqual(expected2.SectionName, actual2.SectionName, $"Second SectionName of item is not mapped correctly.");
        Mock.VerifyAll();
    }

    [Test]
    public void Create_WhenEntityIsInvalidAndHasNoSubDirections_ThrowsInvalidOperationException()
    {
        // Arrange
        var input = new CompetitiveEventBaseDto
        {
            Title = "Title",
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>(),
            SubDirectionIds = []
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.Create(input));
        Assert.That(ex.Message, Does.Contain("Failed to create CompetitiveEvent. The created CompetitiveEvent does not contain any existing SubDirection."));
        Mock.VerifyAll();
    }
    #endregion

    #region Update
    [Test]
    public async Task Update_WhenCompetitiveEventExists_UpdatesEntity()
    {
        // Arrange
        var existingEventId = Guid.NewGuid();
        var newCompetitiveEvent = new CompetitiveEvent
        {
            Id = existingEventId,
            Title = "New Title",
            CompetitiveEventDescriptionItems =
            [
                new CompetitiveEventDescriptionItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Description 1",
                    SectionName = "Section 1"
                }
            ]
        };
        var updateDto = new CompetitiveEventBaseDto
        {
            Id = existingEventId,
            Title = "New Title",
            SubDirectionIds = [1],
            CompetitiveEventDescriptionItems = []
        };

        mockCompetitiveEventRepository
           .Setup(r => r.GetByIdWithDetails(
               existingEventId,
               string.Empty,
               It.IsAny<Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>>>()))
           .ReturnsAsync(newCompetitiveEvent)
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
            .ReturnsAsync([new() { Id = 1, DirectionId = 1, IsDeleted = false, Direction = new() { Id = 1, IsDeleted = false } }])
            .Verifiable(Times.Once);
        contactsService
            .Setup(c => c.PrepareUpdatedContacts(It.IsAny<CompetitiveEvent>(), It.IsAny<CompetitiveEventBaseDto>()))
            .Verifiable(Times.Once);
        mockCompetitiveEventRepository
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task<CompetitiveEvent>>>()))
            .Returns<Func<Task<CompetitiveEvent>>>(f => f())
            .Verifiable(Times.Once);

        // Act
        var result = await service.Update(updateDto);

        // Assert
        Assert.IsNotNull(result, "Result of Update should not be null.");
        Assert.AreEqual("New Title", result.Title, "Title was not updated correctly.");
        Mock.VerifyAll();
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var invalidEventId = Guid.NewGuid();
        var updateDto = new CompetitiveEventBaseDto
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

        Assert.That(ex.Message, Does.Contain($"Updating failed. CompetitiveEvent with Id = {invalidEventId} doesn't exist in the DB."));
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
        var updatedCompetitiveEvent = new CompetitiveEvent
        {
            Id = eventId,
            Title = "Updated Event",
            SubDirections = new List<SubDirection>()
            {
                new() { Id = 1, DirectionId = 1, IsDeleted = false, Direction = new() { Id = 1, IsDeleted = false } }
            },
            CompetitiveEventDescriptionItems =
            [
                new CompetitiveEventDescriptionItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Updated Description",
                    SectionName = "Updated Section"
                }
            ]
        };

        var updateDto = CreateUpdateDto(eventId, existingDescriptionItemId, newDescriptionItemId);

        SetupMocksForUpdateTest(competitiveEvent, updatedCompetitiveEvent, eventId);

        // Act
        var result = await service.Update(updateDto);

        // Assert
        AssertValidUpdateResult(result, mustBeDeletedDescItemId);
    }

    [Test]
    public void Update_WhenEntityIsInvalidAndHasNoSubDirections_ThrowsInvalidOperationException()
    {
        // Arrange
        var competitiveEventId = Guid.NewGuid();
        var updateDto = new CompetitiveEventBaseDto
        {
            Id = competitiveEventId,
            Title = "New Title",
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>(),
            SubDirectionIds = []
        };
        var competitiveEvent = new CompetitiveEvent
        {
            Id = competitiveEventId,
            Title = "Old Title",
            SubDirections = new List<SubDirection>(),
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

        mockCompetitiveEventRepository
            .Setup(r => r.GetByIdWithDetails(
                competitiveEventId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<CompetitiveEvent>,
                IQueryable<CompetitiveEvent>>>()))
            .ReturnsAsync(competitiveEvent)
            .Verifiable(Times.Once);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.Update(updateDto));
        Assert.That(ex.Message, Does.Contain("Failed to update CompetitiveEvent. The passed CompetitiveEvent dto does not contain any existing SubDirection."));
        Mock.VerifyAll();
    }
    #endregion

    #region Ctor
    [Test]
    public void Ctor_WhenCompetitiveEventRepositoryNotSet_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new CompetitiveEventService(
            null,
            mockDescriptionItemRepository.Object,
            mockSubDirectionRepository.Object,
            mockLogger.Object,
            mockLocalizer.Object,
            userService.Object,
            contactsService.Object,
            competitiveImagesService.Object));

        Assert.That(ex.Message, Does.Contain("Value cannot be null. (Parameter 'competitiveEventRepository')"));
    }

    [Test]
    public void Ctor_WhenDescriptionItemRepositoryNotSet_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new CompetitiveEventService(
            mockCompetitiveEventRepository.Object,
            null,
            mockSubDirectionRepository.Object,
            mockLogger.Object,
            mockLocalizer.Object,
            userService.Object,
            contactsService.Object,
            competitiveImagesService.Object));

        Assert.That(ex.Message, Does.Contain("Value cannot be null. (Parameter 'descriptionItemRepository')"));
    }

    [Test]
    public void Ctor_WhenSubDirectionRepositoryNotSet_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new CompetitiveEventService(
            mockCompetitiveEventRepository.Object,
            mockDescriptionItemRepository.Object,
            null,
            mockLogger.Object,
            mockLocalizer.Object,
            userService.Object,
            contactsService.Object,
            competitiveImagesService.Object));

        Assert.That(ex.Message, Does.Contain("Value cannot be null. (Parameter 'subDirectionRepository')"));
    }
    #endregion

    private CompetitiveEvent CreateCompetitiveEvent(Guid eventId, Guid existingDescriptionItemId, Guid mustBeDeletedDescItemId)
    {
        return new CompetitiveEvent
        {
            Id = eventId,
            Title = "Existing Event",
            SubDirections = new List<SubDirection>(),
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

    private CompetitiveEventBaseDto CreateUpdateDto(Guid eventId, Guid existingDescriptionItemId, Guid newDescriptionItemId)
    {
        return new CompetitiveEventBaseDto
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

    private void SetupMocksForUpdateTest(CompetitiveEvent competitiveEvent, CompetitiveEvent updatedCompetitiveEvent, Guid eventId)
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
            .ReturnsAsync(updatedCompetitiveEvent)
            .Verifiable(Times.Once);

        mockSubDirectionRepository
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<SubDirection, bool>>>(),
                string.Empty,
                It.IsAny<Func<IQueryable<SubDirection>, IQueryable<SubDirection>>>()))
            .ReturnsAsync(new List<SubDirection>() { new SubDirection { Id = 1 } })
            .Verifiable(Times.Once);
    }

    private void AssertValidUpdateResult(CompetitiveEventDto result, Guid mustBeDeletedId)
    {
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Event", result.Title);

        // Validate SubDirections were properly updated
        Assert.IsTrue(result.SubDirectionIds.Contains(1), "SubDirection was not properly updated");

        // Validate DescriptionItems were properly updated
        Assert.IsTrue(result.CompetitiveEventDescriptionItems.Count > 0, "Description items should not be empty");
        Assert.IsFalse(result.CompetitiveEventDescriptionItems.Any(d => d.Id == mustBeDeletedId),
        "Description item that should have been deleted still exists");

        // Validate description item content
        var descItem = result.CompetitiveEventDescriptionItems.First();
        Assert.AreEqual("Updated Description", descItem.Description, "Description content was not updated correctly");
        Assert.AreEqual("Updated Section", descItem.SectionName, "Section name was not updated correctly");

        Mock.VerifyAll();
    }
}