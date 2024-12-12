using System;
using System.Collections.Generic;
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

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CompetitiveEventServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
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
        context = new OutOfSchoolDbContext(options);

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
            accountingTypeOfEventRepository,
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
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity() // need more checks (Judge)???/
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
    public void Update_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        CompetitiveEventUpdateDto dto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Update(dto).ConfigureAwait(false));
    }

    [Test]
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
