using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
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
    private Mock<IAverageRatingService> mockAverageRatingService;
    private IMapper mockMapper;
    private Mock<ILogger<ExternalExportService>> mockLogger;

    [SetUp]
    public void Setup()
    {
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockAverageRatingService = new Mock<IAverageRatingService>();
        mockMapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportService>>();

        externalExportService = new ExternalExportService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
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
            .Setup(x => x.GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size))
            .ReturnsAsync(fakeProviders);

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
            .Setup(x => x.GetAllWithDeleted(It.IsAny<DateTime>(), offsetFilter.From, offsetFilter.Size))
            .ReturnsAsync(fakeProviders);

        mockProviderRepository.Setup(x => x.CountWithDeleted(It.IsAny<DateTime>())).ReturnsAsync(5);

        // Act
        var result = await externalExportService.GetProviders(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
        mockProviderRepository.Verify(x => x.CountWithDeleted(updatedAfter), Times.Once);
    }

    [Test]
    public async Task GetProviders_ExceptionInGetProviders_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockProviderRepository.Setup(repo => repo.GetAllWithDeleted(updatedAfter, offsetFilter.From,  offsetFilter.Size))
            .ThrowsAsync(new Exception("Simulated exception"));

        // Act
        var result = await externalExportService.GetProviders(DateTime.Now, new OffsetFilter());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0); 
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<ProviderInfoBaseDto>());
    }

    [Test]
    public async Task GetProviders_ProvidersIsNull_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockProviderRepository.Setup(repo => repo.GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size))
        .ReturnsAsync((List<Provider>)null);

        // Act
        var result = await externalExportService.GetProviders(DateTime.Now, new OffsetFilter());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0);
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<ProviderInfoBaseDto>());
    }
    
    [Test]
    public async Task GetWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeWorkshops = WorkshopGenerator.Generate(0);

        mockWorkshopRepository
            .Setup(x => x.GetAllWithDeleted(updatedAfter, 0, 10))
            .ReturnsAsync(fakeWorkshops);

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

        mockWorkshopRepository
            .Setup(x => x.GetAllWithDeleted(updatedAfter, 0, 10))
            .ReturnsAsync(fakeWorkshops);
        
        mockWorkshopRepository.Setup(x => x.CountWithDeleted(It.IsAny<DateTime>())).ReturnsAsync(3);

        // Act
        var result = await externalExportService.GetWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeWorkshops.Count, result.TotalAmount);
        Assert.AreEqual(fakeWorkshops.Count, result.Entities.Count);
        mockWorkshopRepository.Verify(x => x.CountWithDeleted(updatedAfter), Times.Once);
    }

    [Test]
    public async Task GetWorkshops_ExceptionInGetWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockWorkshopRepository.Setup(repo => repo.GetAllWithDeleted(updatedAfter, offsetFilter.From,  offsetFilter.Size))
            .ThrowsAsync(new Exception("Simulated exception"));

        // Act
        var result = await externalExportService.GetWorkshops(DateTime.Now, new OffsetFilter());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0); 
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<WorkshopInfoBaseDto>());
    }

    [Test]
    public async Task GetWorkshops_ProvidersIsNull_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockWorkshopRepository.Setup(repo => repo.GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size))
        .ReturnsAsync((List<Workshop>)null);

        // Act
        var result = await externalExportService.GetWorkshops(DateTime.Now, new OffsetFilter());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0);
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<WorkshopInfoBaseDto>());
    }

    [Test]
    public void Constructor_NullProviderRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(null, Mock.Of<IWorkshopRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullWorkshopRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), null, Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IAverageRatingService>(), null, Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), null));
    }

    [Test]
    public void Constructor_NullAverageRatingService_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportService>>()));
    }

    [Test]
    public async Task GetAllUpdatedProviders_DefaultUpdatedAfter_ReturnsNonDeletedProviders()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryTestDatabase")
            .Options;

        using (var dbContext = new OutOfSchoolDbContext(options))
        {
            var updatedAfter = default(DateTime);
            var offsetFilter = new OffsetFilter { Size = 10 }; 
            var fakeProviders = ProvidersGenerator.Generate(5);

            fakeProviders[0].IsDeleted = true;
            fakeProviders[2].IsDeleted = true;

            dbContext.Providers.AddRange(fakeProviders);
            dbContext.SaveChanges();

            var providerRepository = new ProviderRepository(dbContext);

            var externalExportProviderService = new ExternalExportService(
                providerRepository,
                new Mock<IWorkshopRepository>().Object,
                new Mock<IAverageRatingService>().Object,
                mockMapper,
                new Mock<ILogger<ExternalExportService>>().Object
            );

            // Act
            var result = await externalExportProviderService.GetProviders(updatedAfter, offsetFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fakeProviders.Count - 2, result.TotalAmount);
            Assert.AreEqual(fakeProviders.Count - 2, result.Entities.Count);
            Assert.IsTrue(result.Entities.All(provider => !provider.IsDeleted));
        }
    }
    
    [Test]
    public async Task GetAllUpdatedWorkshops_DefaultUpdatedAfter_ReturnsNonDeletedWorkshops()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryTestDatabase")
            .Options;

        using (var dbContext = new OutOfSchoolDbContext(options))
        {
            var updatedAfter = default(DateTime);
            var offsetFilter = new OffsetFilter { Size = 10 }; 
            var fakeWorkshops = WorkshopGenerator.Generate(3);

            fakeWorkshops[0].IsDeleted = true;
            fakeWorkshops[2].IsDeleted = true;

            dbContext.Workshops.AddRange(fakeWorkshops);
            dbContext.SaveChanges();

            var workshopRepository = new WorkshopRepository(dbContext);

            var externalExportProviderService = new ExternalExportService(
                new Mock<IProviderRepository>().Object,
                workshopRepository,
                new Mock<IAverageRatingService>().Object,
                mockMapper,
                new Mock<ILogger<ExternalExportService>>().Object
            );

            // Act
            var result = await externalExportProviderService.GetWorkshops(updatedAfter, offsetFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fakeWorkshops.Count - 2, result.TotalAmount);
            Assert.AreEqual(fakeWorkshops.Count - 2, result.Entities.Count);
            Assert.IsTrue(result.Entities.All(provider => !provider.IsDeleted));
        }
    }
}