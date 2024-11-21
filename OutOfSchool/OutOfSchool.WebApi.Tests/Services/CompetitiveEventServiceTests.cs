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
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.CompetitiveEvents;
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
    private IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> repo;
    private IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> accountingTypeOfEventRepository;
    private IEntityRepository<Guid, CompetitiveEventDescriptionItem> descriptionItemRepository;
    private IEntityRepository<Guid, Judge> judgeRepository;

    private Mock<ILogger<CompetitiveEventService>> logger;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private IMapper mapper;

    private CompetitiveEventService service;
    private Guid firstId;
    private Guid firstJudgeId;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        repo = new EntityRepositorySoftDeleted<Guid, CompetitiveEvent>(context);
        accountingTypeOfEventRepository = new EntityRepositorySoftDeleted<int, CompetitiveEventAccountingType>(context);
        descriptionItemRepository = new EntityRepository<Guid, CompetitiveEventDescriptionItem>(context);
        judgeRepository = new EntityRepository<Guid, Judge>(context);

        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        logger = new Mock<ILogger<CompetitiveEventService>>();

        service = new CompetitiveEventService(
            repo,
            judgeRepository,
           // accountingTypeOfEventRepository,
            descriptionItemRepository,
            logger.Object,
            localizer.Object,
            mapper);

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
        var input = new CompetitiveEventCreateDto()
        {
            Title = "Test",
            ShortTitle = "TestShort",
            Description = "Test",
            State = CompetitiveEventStates.Draft,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow,
            NumberOfSeats = 10,
            OrganizerOfTheEventId = Guid.NewGuid(),
            CompetitiveEventAccountingTypeId = 1,

            Judges = new List<JudgeDto>
            {
                new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge 1" },
                new JudgeDto { Id = Guid.NewGuid(), FirstName = "Judge 2" }
            }
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
        CompetitiveEventUpdateDto dto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Update(dto).ConfigureAwait(false));
    }

    [Test]
    [Ignore("Test is ignored because the method being tested uses a transaction, which is not supported by in-memory database.")]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedDto = new CompetitiveEventUpdateDto()
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            ShortTitle = "TestShort",
            Description = "Test",
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
        var input = new CompetitiveEventUpdateDto()
        {
            Id = firstId,
            Title = "TestNew",
            ShortTitle = "TestShort",
            Description = "Test",
            State = CompetitiveEventStates.Draft,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow,
            NumberOfSeats = 10,
            OrganizerOfTheEventId = Guid.NewGuid(),
            CompetitiveEventAccountingTypeId = 1,
            Judges = new List<JudgeDto>
            {
                new JudgeDto { Id = firstJudgeId, FirstName = "Judge C" },
                new JudgeDto { FirstName = "Judge D" }
            }
        };

        // Act
        var result = await service.Update(input).ConfigureAwait(false);

        // Assert
        Assert.That(input.Title, Is.EqualTo(result.Title), "CompetitiveEvent's title was not updated correctly.");
        Assert.That(input.Judges[0].FirstName, Is.EqualTo(result.Judges[0].FirstName), "First Judge's name was not updated correctly.");
        Assert.That(input.Judges[1].FirstName, Is.EqualTo(result.Judges[1].FirstName), "New judge (Judge D) was not added correctly.");
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

        var updateDto = new CompetitiveEventUpdateDto
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

    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            firstId = Guid.NewGuid();
            firstJudgeId = Guid.NewGuid();

            var competitiveEvents = new List<CompetitiveEvent>()
            {
                new CompetitiveEvent()
                {
                    Id = firstId,
                    Title = "Test1",
                    ShortTitle = "Test1Short",
                    Description = "Test1",
                    State = CompetitiveEventStates.Draft,
                    ScheduledStartTime = DateTime.UtcNow,
                    ScheduledEndTime = DateTime.UtcNow,
                    NumberOfSeats = 10,
                    OrganizerOfTheEventId = Guid.NewGuid(),
                    CompetitiveEventAccountingType = new CompetitiveEventAccountingType(),
                    Judges = new List<Judge>
                    {
                        new Judge { Id = firstJudgeId, FirstName = "Judge A" },
                    }
                },
                new CompetitiveEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Test2",
                    ShortTitle = "Test2Short",
                    Description = "Test2",
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
                    Description = "Test3",
                    State = CompetitiveEventStates.Draft,
                    ScheduledStartTime = DateTime.UtcNow,
                    ScheduledEndTime = DateTime.UtcNow,
                    NumberOfSeats = 10,
                    OrganizerOfTheEventId = Guid.NewGuid(),
                    CompetitiveEventAccountingType = new CompetitiveEventAccountingType(),
                },
            };

            ctx.CompetitiveEvents.AddRange(competitiveEvents);

            ctx.SaveChanges();
        }
    }
}