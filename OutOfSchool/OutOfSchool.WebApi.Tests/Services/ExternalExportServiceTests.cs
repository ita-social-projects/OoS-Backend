using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
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
    private IMapper mockMapper;
    private Mock<ILogger<ExternalExportService>> mockLogger;

    [SetUp]
    public void Setup()
    {
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockApplicationRepository = new Mock<IApplicationRepository>();
        mockAverageRatingService = new Mock<IAverageRatingService>();
        mockMapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportService>>();

        externalExportService = new ExternalExportService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockApplicationRepository.Object,
            mockAverageRatingService.Object,
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
    public void Constructor_NullProviderRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(null, Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullWorkshopRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), null, Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }
    
    [Test]
    public void Constructor_NullApplicationRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), null, Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), null, Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), null));
    }

    [Test]
    public void Constructor_NullAverageRatingService_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IApplicationRepository>(), null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }
}