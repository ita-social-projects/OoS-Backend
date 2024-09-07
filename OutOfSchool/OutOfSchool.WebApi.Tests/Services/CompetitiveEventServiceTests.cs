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
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CompetitiveEventServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<Guid, CompetitiveEvent> repo;
    private Mock<ILogger<CompetitiveEventService>> logger;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private IMapper mapper;

    private CompetitiveEventService service;
    private Guid firstId;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);

        repo = new EntityRepositorySoftDeleted<Guid, CompetitiveEvent>(context);

        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        logger = new Mock<ILogger<CompetitiveEventService>>();

        service = new CompetitiveEventService(
            repo,
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
    public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.GetById(id).ConfigureAwait(false));
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var input = new CompetitiveEventDto()
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
            AccountingTypeOfEvent = new List<CompetitiveEventAccountingTypeDto>(),
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
    public void Create_NotUniqueEntity_ReturnsArgumentException()
    {
        // Arrange
        var input = new CompetitiveEventDto()
        {
            Id = firstId,
            Title = "Test",
            ShortTitle = "TestShort",
            Description = "Test",
            State = CompetitiveEventStates.Draft,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow,
            NumberOfSeats = 10,
            OrganizerOfTheEventId = Guid.NewGuid(),
            AccountingTypeOfEvent = new List<CompetitiveEventAccountingTypeDto>(),
        };

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Create(input).ConfigureAwait(false));
    }

    [Test]
    public void Update_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        CompetitiveEventDto dto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Update(dto).ConfigureAwait(false));
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedDto = new CompetitiveEventDto()
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
            AccountingTypeOfEvent = new List<CompetitiveEventAccountingTypeDto>(),
        };

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedDto).ConfigureAwait(false));
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var input = new CompetitiveEventDto()
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
            AccountingTypeOfEvent = new List<CompetitiveEventAccountingTypeDto>(),
        };

        // Act
        var result = await service.Update(input).ConfigureAwait(false);

        // Assert
        Assert.That(input.Title, Is.EqualTo(result.Title));
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
        using var ctx = new OutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            firstId = Guid.NewGuid();

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
                    AccountingTypeOfEvent = new List<CompetitiveEventAccountingType>(),
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
                    AccountingTypeOfEvent = new List<CompetitiveEventAccountingType>(),
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
                    AccountingTypeOfEvent = new List<CompetitiveEventAccountingType>(),
                },
            };

            ctx.CompetitiveEvents.AddRange(competitiveEvents);

            ctx.SaveChanges();
        }
    }
}
