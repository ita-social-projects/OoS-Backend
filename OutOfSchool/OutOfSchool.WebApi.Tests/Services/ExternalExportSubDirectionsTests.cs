using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

/// <summary>
/// Joins can't be tested by mock implementation, so this class focuses on in memory db tests.
/// </summary>
[TestFixture]
public class ExternalExportSubDirectionsTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private OutOfSchoolDbContext dbContext;
    private ExternalExportService externalExportService;
    private Mock<IProviderRepository> mockProviderRepository;
    private Mock<IWorkshopRepository> mockWorkshopRepository;
    private Mock<IApplicationRepository> mockApplicationRepository;
    private Mock<IAverageRatingService> mockAverageRatingService;
    private IMapper mockMapper;
    private Mock<ILogger<ExternalExportService>> mockLogger;
    
    [SetUp]
    public void Setup()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new TestOutOfSchoolDbContext(dbContextOptions);
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockApplicationRepository = new Mock<IApplicationRepository>();
        mockAverageRatingService = new Mock<IAverageRatingService>();
        mockMapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, ExternalExportMappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportService>>();

        externalExportService = new ExternalExportService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockApplicationRepository.Object,
            mockAverageRatingService.Object,
            new EntityRepositorySoftDeleted<long, Direction>(dbContext),
            new SensitiveEntityRepositorySoftDeleted<CompetitiveEvent>(dbContext),
            new EntityRepositorySoftDeleted<long, SubDirection>(dbContext),
            mockMapper,
            mockLogger.Object);
        
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
    
    [Test]
    public async Task GetSubDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

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
    
        // Act
        var result = await externalExportService.GetSubDirections(default, offsetFilter);
    
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.TotalAmount);
        Assert.AreEqual(4, result.Entities.Count);
    }
}