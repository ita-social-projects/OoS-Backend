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
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CompetitiveEventServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext context;
    private ICompetitiveEventRepository repo;
    private IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeOfEventRepository;
    private IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository;
    private IEntityRepository<long, SubDirection> subDirectionRepository;

    private Mock<ILogger<CompetitiveEventService>> logger;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ICurrentUserService> userService;
    private Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>> contactsService;
    private Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>> competitiveImagesService;
    private IMapper mapper;


    private CompetitiveEventService service;
    private Guid firstId;
    private Guid firstJudgeId;
    private Guid firstProviderId;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        repo = new CompetitiveEventRepository(context);
        accountingTypeOfEventRepository = new EntityRepositorySoftDeleted<int, CompetitiveEventAccountingType>(context);
        descriptionItemRepository = new EntityRepository<Guid, CompetitiveEventDescriptionItem>(context);
        subDirectionRepository = new EntityRepository<long, SubDirection>(context);

        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        logger = new Mock<ILogger<CompetitiveEventService>>();
        userService = new Mock<ICurrentUserService>();
        contactsService = new Mock<IContactsService<CompetitiveEvent, IHasContactsDto<CompetitiveEvent>>>();
        competitiveImagesService = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEvent>>();

        service = new CompetitiveEventService(
            repo,
            descriptionItemRepository,
            subDirectionRepository,
            logger.Object,
            localizer.Object,
            mapper,
            userService.Object,
            contactsService.Object,
            competitiveImagesService.Object);

        SeedDatabase();
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ReturnsEntity()
    {
        // Arrange
        var expected = await repo.GetById(firstId);

        var expectedDto = new CompetitiveEventDto()
        {
            Id = expected.Id,
            Title = expected.Title,
        };

        // Act
        var result = await service.GetById(firstId).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Id, result.Id);
    }

    [Test]
    public async Task GetById_WhenIdIsInvalid_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.IsNull(result, "Expected null for invalid ID.");
    }

    [Test]
    [Ignore("Test is ignored because the method being tested uses a transaction, which is not supported by in-memory database.")]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var input = new CompetitiveEventCreateUpdateDto()
        {
            Title = "Test",
            ShortTitle = "TestShort",
            State = CompetitiveEventStates.Draft,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow,
            NumberOfSeats = 10,
            OrganizerOfTheEventId = Guid.NewGuid(),
            CompetitiveEventAccountingTypeId = 1,
        };
        // Act
        var countBeforeCreating = await repo.Count().ConfigureAwait(false);

        var result = await service.Create(input).ConfigureAwait(false);

        var countAfterCreating = await repo.Count().ConfigureAwait(false);

        // Assert
        Assert.AreEqual(input.Title, result.Title);
        Assert.That(countBeforeCreating, Is.EqualTo(countAfterCreating - 1));
    }

    [Test]
    public void Update_WhenDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        CompetitiveEventCreateUpdateDto dto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Update(dto).ConfigureAwait(false));
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedDto = new CompetitiveEventCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            ShortTitle = "TestShort",
            State = CompetitiveEventStates.Draft,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow,
            NumberOfSeats = 10,
            OrganizerOfTheEventId = Guid.NewGuid(),
            CompetitiveEventAccountingTypeId = 1,
        };

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedDto).ConfigureAwait(false));
    }

    [Test]
    [Ignore("Test is ignored because the method being tested uses a transaction, which is not supported by in-memory database.")]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var input = new CompetitiveEventCreateUpdateDto()
        {
            Id = firstId,
            Title = "TestNew",
            ShortTitle = "TestShort",
            State = CompetitiveEventStates.Draft,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow,
            NumberOfSeats = 10,
            OrganizerOfTheEventId = Guid.NewGuid(),
            CompetitiveEventAccountingTypeId = 1,
        };

        // Act
        var result = await service.Update(input).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result, "Update method returned null.");
        Assert.That(input.Title, Is.EqualTo(result.Title), "CompetitiveEvent's title was not updated correctly.");
    }

    [Test]
    [Ignore("Test is ignored because the method being tested uses a transaction, which is not supported by in-memory database.")]
    public async Task Update_WhenDescriptionItemsAreUpdated_UpdatesCorrectly()
    {
        // Arrange
        Guid firstDescItemId = Guid.NewGuid();
        var initialDescriptionItems = new List<CompetitiveEventDescriptionItem>
        {
            new CompetitiveEventDescriptionItem { Id = firstDescItemId, SectionName = "Old Section 1", Description = "Old Description 1" },
            new CompetitiveEventDescriptionItem { Id = Guid.NewGuid(), SectionName = "Old Section 2", Description = "Old Description 2" },
        };

        Guid eventId = Guid.NewGuid();
        var competitiveEvent = new CompetitiveEvent
        {
            Id = eventId,
            Title = "Test Event",
            ShortTitle = "Test Event Short",
            CompetitiveEventDescriptionItems = initialDescriptionItems,
        };

        context.CompetitiveEvents.Add(competitiveEvent);
        await context.SaveChangesAsync();

        var updateDto = new CompetitiveEventCreateUpdateDto
        {
            Id = eventId,
            Title = "Updated Test Event",
            ShortTitle = "Updated Test Event Short",
            CompetitiveEventDescriptionItems = new List<CompetitiveEventDescriptionItemDto>
            {
                // Update an existing item
                new CompetitiveEventDescriptionItemDto
                {
                    Id = initialDescriptionItems[1].Id,
                    SectionName = "Updated Section 2",
                    Description = "Updated Description 2"
                },
                // Add a new item
                new CompetitiveEventDescriptionItemDto
                {
                    Id = Guid.NewGuid(),
                    SectionName = "New Section 3",
                    Description = "New Description 3"
                }
            }
        };

        // Act
        var result = await service.Update(updateDto);

        // Assert
        Assert.AreEqual(updateDto.Title, result.Title);

        var updatedEvent = await context.CompetitiveEvents
            .Include(e => e.CompetitiveEventDescriptionItems)
            .FirstAsync(e => e.Id == eventId);

        Assert.AreEqual(2, updatedEvent.CompetitiveEventDescriptionItems.Count);

        // Verify updated item
        var updatedItem = updatedEvent.CompetitiveEventDescriptionItems
            .First(d => d.Id == initialDescriptionItems[0].Id);
        Assert.AreEqual("Updated Description 2", updatedItem.Description);

        // Verify new item
        var newItem = updatedEvent.CompetitiveEventDescriptionItems
            .First(d => d.Description == "New Description 3");
        Assert.IsNotNull(newItem);

        // Verify deleted item
        Assert.IsFalse(updatedEvent.CompetitiveEventDescriptionItems
            .Any(d => d.Id == firstDescItemId));
    }

    [Test]
    public async Task Delete_WhenIdIsValid_DeletesEntity()
    {
        // Act
        var countBeforeDeleting = await repo.Count().ConfigureAwait(false);

        await service.Delete(firstId);

        var countAfterDeleting = await repo.Count().ConfigureAwait(false);

        // Assert
        Assert.That(countBeforeDeleting, Is.EqualTo(countAfterDeleting + 1));
    }

    [Test]
    public void Delete_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.Delete(id).ConfigureAwait(false));
    }

    [Test]
    public async Task GetByProviderId_NotValidProviderId_ReturnsEmpty()
    {
        // Arrange
        var expected = new List<CompetitiveEvent>();
        var invalidProviderId = Guid.NewGuid();

        // Act
        var result = await service.GetByProviderId(invalidProviderId, null);

        // Assert
        Assert.That(result.Entities, Is.Empty);
        Assert.AreEqual(result.TotalAmount, expected.Count);
    }

    [Test]
    public async Task GetByProviderId_WhenValidProviderId_ReturnsSearchResult()
    {
        // Arrange
        var expected = CompetitiveEvents().Where(x => x.OrganizerOfTheEventId == firstProviderId);

        // Act
        var result = await service.GetByProviderId(firstProviderId, null);

        // Assert
        Assert.IsNotNull(result.Entities);
        Assert.AreEqual(expected.First().Id, result.Entities.First().Id);
        Assert.AreEqual(expected.First().Title, result.Entities.First().Title);
        Assert.AreEqual(expected.First().ShortTitle, result.Entities.First().ShortTitle);
        Assert.AreEqual(expected.Count(), result.TotalAmount);
        Assert.IsInstanceOf<IReadOnlyCollection<CompetitiveEventViewCardDto>>(result.Entities);
    }

    [Test]
    public async Task GetByProviderId_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var invalidProviderId = Guid.Empty;
        var filter = new ExcludeIdFilter();

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.GetByProviderId(invalidProviderId, filter).ConfigureAwait(false));

        Assert.AreEqual("ProviderId cannot be empty. (Parameter 'id')", exception.Message, "Unexpected exception message.");
    }

    private void SeedDatabase()
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        firstId = Guid.NewGuid();
        firstProviderId = Guid.NewGuid();

        List<CompetitiveEvent> competitiveEvents = CompetitiveEvents();
        context.CompetitiveEvents.AddRange(competitiveEvents);

        context.SaveChanges();
    }

    private List<CompetitiveEvent> CompetitiveEvents()
    {
        var competitiveEvents = new List<CompetitiveEvent>()
        {
            new CompetitiveEvent
            {
                Id = firstId,
                Title = "Test1",
                ShortTitle = "Test1Short",
                State = CompetitiveEventStates.Draft,
                ScheduledStartTime = DateTime.UtcNow,
                ScheduledEndTime = DateTime.UtcNow,
                NumberOfSeats = 10,
                OrganizerOfTheEventId = firstProviderId,
                CompetitiveEventAccountingTypeId = 1,
                CompetitiveEventAccountingType = new CompetitiveEventAccountingType(),
                CompetitiveEventDescriptionItems =
                [
                    new CompetitiveEventDescriptionItem
                    {
                        Id = Guid.NewGuid(),
                        Description = "Description 1",
                        SectionName = "Section 1"
                    }
                ],
                //InstitutionHierarchy = new InstitutionHierarchy { Id = new Guid(), Title = "Institution 1" },
                SubDirections =
                [
                    new SubDirection
                    {
                        Id = 1,
                        Title = "Фортепіано"
                    }
                ],
                CoverageId = 1,
            },
            new CompetitiveEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Test2",
                    ShortTitle = "Test2Short",
                    State = CompetitiveEventStates.Draft,
                    ScheduledStartTime = DateTime.UtcNow,
                    ScheduledEndTime = DateTime.UtcNow,
                    NumberOfSeats = 10,
                    OrganizerOfTheEventId = Guid.NewGuid(),
                    CompetitiveEventAccountingType = new CompetitiveEventAccountingType(),
                },
                new CompetitiveEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Test3",
                    ShortTitle = "Test3Short",
                    State = CompetitiveEventStates.Draft,
                    ScheduledStartTime = DateTime.UtcNow,
                    ScheduledEndTime = DateTime.UtcNow,
                    NumberOfSeats = 10,
                    OrganizerOfTheEventId = Guid.NewGuid(),
                    CompetitiveEventAccountingType = new CompetitiveEventAccountingType(),
                },
            };
        return competitiveEvents;
    }
}