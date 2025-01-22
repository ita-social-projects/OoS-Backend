using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
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
    private Mock<ISensitiveEntityRepositorySoftDeleted<Institution>> mockInstitutionRepository;
    private Mock<IInstitutionHierarchyRepository> mockInstitutionHierarchyRepository;
    private IMapper mockMapper;
    private Mock<ILogger<ExternalExportService>> mockLogger;

    [SetUp]
    public void Setup()
    {
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockApplicationRepository = new Mock<IApplicationRepository>();
        mockAverageRatingService = new Mock<IAverageRatingService>();
        mockDirectionRepository = new Mock<IEntityRepositorySoftDeleted<long, Direction>>();
        mockInstitutionRepository = new Mock<ISensitiveEntityRepositorySoftDeleted<Institution>>();
        mockInstitutionHierarchyRepository = new Mock<IInstitutionHierarchyRepository>();
        mockMapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportService>>();

        externalExportService = new ExternalExportService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockApplicationRepository.Object,
            mockAverageRatingService.Object,
            mockDirectionRepository.Object,
            mockInstitutionRepository.Object,
            mockInstitutionHierarchyRepository.Object,
            mockMapper,
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
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Provider,bool>>>(), null, false))
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
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Provider,bool>>>(), null, false))
            .Returns(fakeProviders.AsTestAsyncEnumerableQuery());

        mockProviderRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Provider,bool>>>())).ReturnsAsync(5);

        // Act
        var result = await externalExportService.GetProviders(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
        mockProviderRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Provider,bool>>>()), Times.Once);
    }

    [Test]
    public void GetProviders_ExceptionInGetProviders_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockProviderRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Provider,bool>>>(), null, false))
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
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Workshop,bool>>>(), null, false))
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
        
        mockApplicationRepository.Setup(x => x.CountTakenSeatsForWorkshops(It.IsAny<List<Guid>>()))
            .ReturnsAsync([]);

        mockWorkshopRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Workshop,bool>>>(), null, false))
            .Returns(fakeWorkshops.AsTestAsyncEnumerableQuery());
        
        mockWorkshopRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Workshop,bool>>>())).ReturnsAsync(3);

        // Act
        var result = await externalExportService.GetWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeWorkshops.Count, result.TotalAmount);
        Assert.AreEqual(fakeWorkshops.Count, result.Entities.Count);
        mockWorkshopRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Workshop,bool>>>()), Times.Once);
    }

    [Test]
    public void GetWorkshops_ExceptionInGetWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockWorkshopRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Workshop,bool>>>(), null, false))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetWorkshops(updatedAfter, new OffsetFilter()));
    }
    
    [Test]
    public async Task GetDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeDirections = new List<Direction>();

        mockDirectionRepository
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Direction,bool>>>(), null, false))
            .Returns(fakeDirections.AsTestAsyncEnumerableQuery());

        // Act
        var result = await externalExportService.GetDirections(offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetDirections_ReturnsSearchResultData()
    {
        // Arrange
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
            .Setup(x => x.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Direction,bool>>>(), null, false))
            .Returns(fakeDirections.AsTestAsyncEnumerableQuery());
        
        mockDirectionRepository.Setup(x => x.Count(It.IsAny<Expression<Func<Direction,bool>>>())).ReturnsAsync(fakeDirections.Count);

        // Act
        var result = await externalExportService.GetDirections(offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeDirections.Count, result.TotalAmount);
        Assert.AreEqual(fakeDirections.Count, result.Entities.Count);
        mockDirectionRepository.Verify(x => x.Count(It.IsAny<Expression<Func<Direction,bool>>>()), Times.Once);
    }

    [Test]
    public void GetDirections_ExceptionInGetDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockDirectionRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Direction,bool>>>(), null, false))
            .Throws(new Exception("Simulated exception"));

        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetDirections(new OffsetFilter()));
    }
    
    /// <summary>
    /// This is the only sub direction logic, that can be tested on mocks
    /// </summary>
    [Test]
    public void GetSubDirections_ExceptionInGetSubDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockInstitutionRepository.Setup(repo => repo.Get(offsetFilter.From, offsetFilter.Size, It.IsAny<string>(), It.IsAny<Expression<Func<Institution,bool>>>(), null, false))
            .Throws(new Exception("Simulated exception"));
    
        // Act & Assert
        Assert.CatchAsync<Exception>(() => externalExportService.GetDirections(new OffsetFilter()));
    }

    [Test]
    public void Constructor_NullProviderRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(null, Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), null, null , null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullWorkshopRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), null, Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), null, null , null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }
    
    [Test]
    public void Constructor_NullApplicationRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), null, Mock.Of<IAverageRatingService>(), null, null , null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), null, null , null, null, Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), null, null , null, Mock.Of<IMapper>(), null));
    }

    [Test]
    public void Constructor_NullAverageRatingService_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), null, null, null , null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }
}