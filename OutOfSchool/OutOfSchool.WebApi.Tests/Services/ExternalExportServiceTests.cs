using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class ExternalExportServiceTests
{
    private ExternalExportService externalExportService;
    private Mock<IProviderRepository> mockProviderRepository;
    private Mock<IWorkshopRepository> mockWorkshopRepository;
    private Mock<IApplicationRepository> mockApplicationRepository;
    private Mock<IAverageRatingService> mockAverageRatingService;
    private Mock<IEntityRepositorySoftDeleted<long, Direction>> mockDirectionRepository;
    private Mock<ISensitiveEntityRepositorySoftDeleted<CompetitiveEvent>> mockCompetitiveEventRepository;
    private Mock<IEntityRepositorySoftDeleted<long, SubDirection>> mockSubDirectionRepository;
    private Mock<ILogger<ExternalExportService>> mockLogger;

    [SetUp]
    public void Setup()
    {
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockApplicationRepository = new Mock<IApplicationRepository>();
        mockAverageRatingService = new Mock<IAverageRatingService>();
        mockDirectionRepository = new Mock<IEntityRepositorySoftDeleted<long, Direction>>();
        mockCompetitiveEventRepository = new Mock<ISensitiveEntityRepositorySoftDeleted<CompetitiveEvent>>();
        mockSubDirectionRepository = new Mock<IEntityRepositorySoftDeleted<long, SubDirection>>();
        mockLogger = new Mock<ILogger<ExternalExportService>>();

        externalExportService = new ExternalExportService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockApplicationRepository.Object,
            mockAverageRatingService.Object,
            mockDirectionRepository.Object,
            mockCompetitiveEventRepository.Object,
            mockSubDirectionRepository.Object,
            mockLogger.Object);
    }

    [Test]
    public async Task GetProviders_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeProviders = ProvidersGenerator.Generate(0);

        mockProviderRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Provider, bool>>>(), null))
            .Returns(fakeProviders.AsTestAsyncEnumerableQuery());

        // Act
        var result = await externalExportService.GetProviders(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetProviders_ReturnsSearchResultData()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

        var fakeProviders = ProvidersGenerator.Generate(5);

        mockProviderRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Provider, bool>>>(), null))
            .Returns(fakeProviders.AsTestAsyncEnumerableQuery());

        mockProviderRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(5);

        // Act
        var result = await externalExportService.GetProviders(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
        mockProviderRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Provider, bool>>>()), Times.Once);
    }

    [Test]
    public void GetProviders_ExceptionInGetProviders_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockProviderRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Provider, bool>>>(), null))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetProviders(updatedAfter, new OffsetFilter()));
    }

    [Test]
    public async Task GetWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeWorkshops = WorkshopGenerator.Generate(0);

        mockApplicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>()))
            .ReturnsAsync([]);

        mockWorkshopRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Workshop, bool>>>(), null))
            .Returns(fakeWorkshops.AsTestAsyncEnumerableQuery());

        // Act
        var result = await externalExportService.GetWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetWorkshops_ReturnsSearchResultData()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

        var fakeWorkshops = WorkshopGenerator.Generate(3);
        fakeWorkshops.ForEach(w => w.LanguageOfEducation = new ()
        {
            Name = "test"
        });

        mockApplicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>()))
            .ReturnsAsync([]);

        mockWorkshopRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Workshop, bool>>>(), null))
            .Returns(fakeWorkshops.AsTestAsyncEnumerableQuery());

        mockWorkshopRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Workshop, bool>>>())).ReturnsAsync(3);

        // Act
        var result = await externalExportService.GetWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeWorkshops.Count, result.TotalAmount);
        Assert.AreEqual(fakeWorkshops.Count, result.Entities.Count);
        mockWorkshopRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Workshop, bool>>>()), Times.Once);
    }

    [Test]
    public void GetWorkshops_ExceptionInGetWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockWorkshopRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Workshop, bool>>>(), null))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetWorkshops(updatedAfter, new OffsetFilter()));
    }

    [Test]
    public async Task GetCompetitiveEvents_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        List<CompetitiveEvent> fakeEvents = [];

        mockCompetitiveEventRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<CompetitiveEvent, bool>>>(), null))
            .Returns(fakeEvents.AsTestAsyncEnumerableQuery());

        // Act
        var result = await externalExportService.GetCompetitiveEvents(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetCompetitiveEvents_ReturnsSearchResultData()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

        var fakeEvents = CompetitiveEvents();

        mockCompetitiveEventRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<CompetitiveEvent, bool>>>(), null))
            .Returns(fakeEvents.AsTestAsyncEnumerableQuery());

        mockCompetitiveEventRepository.Setup(x => x.Count(It.IsAny<Expression<Func<CompetitiveEvent, bool>>>())).ReturnsAsync(3);

        // Act
        var result = await externalExportService.GetCompetitiveEvents(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeEvents.Count, result.TotalAmount);
        Assert.AreEqual(fakeEvents.Count, result.Entities.Count);
        mockCompetitiveEventRepository.Verify(x => x.Count(It.IsAny<Expression<Func<CompetitiveEvent, bool>>>()), Times.Once);
    }

    [Test]
    public void GetCompetitiveEvents_ExceptionInGetCompetitiveEvents_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockCompetitiveEventRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<CompetitiveEvent, bool>>>(), null))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetCompetitiveEvents(updatedAfter, new OffsetFilter()));
    }

    [Test]
    public async Task GetDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeDirections = new List<Direction>();

        mockDirectionRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Direction, bool>>>(), null))
            .Returns(fakeDirections.AsTestAsyncEnumerableQuery());

        // Act
        var result = await externalExportService.GetDirections(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetDirections_ReturnsSearchResultData()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

        List<Direction> fakeDirections = [
            new()
            {
                Id = 1,
                Title = "A"
            },
            new()
            {
                Id = 2,
                Title = "B"
            },
            new()
            {
                Id = 3,
                Title = "C"
            },
        ];

        mockDirectionRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Direction, bool>>>(), null))
            .Returns(fakeDirections.AsTestAsyncEnumerableQuery());

        mockDirectionRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Direction, bool>>>())).ReturnsAsync(fakeDirections.Count);

        // Act
        var result = await externalExportService.GetDirections(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeDirections.Count, result.TotalAmount);
        Assert.AreEqual(fakeDirections.Count, result.Entities.Count);
        mockDirectionRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Direction, bool>>>()), Times.Once);
    }

    [Test]
    public void GetDirections_ExceptionInGetDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockDirectionRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<Direction, bool>>>(), null))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetDirections(updatedAfter, new OffsetFilter()));
    }
    
    [Test]
    public async Task GetSubDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeSubDirections = new List<SubDirection>();

        mockSubDirectionRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<SubDirection, bool>>>(), null))
            .Returns(fakeSubDirections.AsTestAsyncEnumerableQuery());

        // Act
        var result = await externalExportService.GetSubDirections(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetSubDirections_ReturnsSearchResultData()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

        var fakeSubDirections = new List<SubDirection>
        {
            new() { Id = 1, Title = "SubDirection 1" },
            new() { Id = 2, Title = "SubDirection 2" },
            new() { Id = 3, Title = "SubDirection 3" },
            new() { Id = 4, Title = "SubDirection 4" }
        };

        mockSubDirectionRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<SubDirection, bool>>>(), null))
            .Returns(fakeSubDirections.AsTestAsyncEnumerableQuery());

        mockSubDirectionRepository.Setup(x => x.Count(It.IsAny<Expression<Func<SubDirection, bool>>>())).ReturnsAsync(fakeSubDirections.Count);

        // Act
        var result = await externalExportService.GetSubDirections(default, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeSubDirections.Count, result.TotalAmount);
        Assert.AreEqual(fakeSubDirections.Count, result.Entities.Count);
        mockSubDirectionRepository.Verify(x => x.Count(It.IsAny<Expression<Func<SubDirection, bool>>>()), Times.Once);
    }

    [Test]
    public void GetSubDirections_ExceptionInGetSubDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockSubDirectionRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<Expression<Func<SubDirection, bool>>>(), null))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetSubDirections(updatedAfter, new OffsetFilter()));
    }

    private List<CompetitiveEvent> CompetitiveEvents()
    {
        var competitiveEvents = new List<CompetitiveEvent>()
            {
                new CompetitiveEvent()
                {
                    Id = Guid.NewGuid(),
                    Title = "Test1",
                    ShortTitle = "Test1Short",
                    State = CompetitiveEventStates.Published,
                    ScheduledStartTime = DateTime.UtcNow,
                    ScheduledEndTime = DateTime.UtcNow,
                    NumberOfSeats = 10,
                    OrganizerOfTheEventId = Guid.NewGuid(),
                    CompetitiveEventAccountingType = new CompetitiveEventAccountingType(),
                    Judges = new List<Judge>
                    {
                        new Judge { Id =  Guid.NewGuid(), FirstName = "Judge A" },
                    }
                },
                new CompetitiveEvent
                {
                    Id = Guid.NewGuid(),
                    Title = "Test2",
                    ShortTitle = "Test2Short",
                    State = CompetitiveEventStates.Published,
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
                    State = CompetitiveEventStates.Published,
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