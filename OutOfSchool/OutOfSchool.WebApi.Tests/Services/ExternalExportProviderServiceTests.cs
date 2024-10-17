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
using OutOfSchool.BusinessLogic.Models.ProvidersInfo;
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
public class ExternalExportProviderServiceTests
{
    private ExternalExportProviderService externalExportProviderService;
    private Mock<IProviderRepository> mockProviderRepository;
    private Mock<IWorkshopRepository> mockWorkshopRepository;
    private Mock<IAverageRatingService> mockAverageRatingService;
    private IMapper mockMapper;
    private Mock<ILogger<ExternalExportProviderService>> mockLogger;

    [SetUp]
    public void Setup()
    {
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockAverageRatingService = new Mock<IAverageRatingService>();
        mockMapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportProviderService>>();

        externalExportProviderService = new ExternalExportProviderService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockAverageRatingService.Object,
            mockMapper,
            mockLogger.Object);
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        var fakeProviders = ProvidersGenerator.Generate(0);
        var fakeWorkshops = WorkshopGenerator.Generate(0);

        mockProviderRepository
            .Setup(x => x.GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size))
            .ReturnsAsync(fakeProviders);

        mockWorkshopRepository
            .Setup(x => x.GetAllWithDeleted(It.IsAny<Expression<Func<Workshop, bool>>>()))
            .ReturnsAsync(fakeWorkshops);

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ReturnsSearchResultWithWorkshops()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };

        var fakeProviders = ProvidersGenerator.Generate(5);
        var fakeWorkshops = WorkshopGenerator.Generate(3);

        mockProviderRepository
            .Setup(x => x.GetAllWithDeleted(It.IsAny<DateTime>(), offsetFilter.From, offsetFilter.Size))
            .ReturnsAsync(fakeProviders);

        mockProviderRepository.Setup(x => x.CountWithDeleted(It.IsAny<DateTime>())).ReturnsAsync(5);

        mockWorkshopRepository
            .Setup(x => x.GetAllWithDeleted(It.IsAny<Expression<Func<Workshop, bool>>>()))
            .ReturnsAsync(fakeWorkshops);

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
        mockProviderRepository.Verify(x => x.CountWithDeleted(updatedAfter), Times.Once);
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ExceptionInGetAllUpdatedProviders_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockProviderRepository.Setup(repo => repo.GetAllWithDeleted(updatedAfter, offsetFilter.From,  offsetFilter.Size))
            .ThrowsAsync(new Exception("Simulated exception"));

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(DateTime.Now, new OffsetFilter());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0); 
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<ProviderInfoBaseDto>());
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ExceptionInGetWorkshopListByProviderId_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockWorkshopRepository.Setup(repo => repo.GetAllWithDeleted(It.IsAny<Expression<Func<Workshop, bool>>>()))
           .ThrowsAsync(new Exception("Simulated exception"));

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(updatedAfter, offsetFilter);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0); 
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<ProviderInfoBaseDto>());
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ProvidersIsNull_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var offsetFilter = new OffsetFilter { Size = 10 };
        mockProviderRepository.Setup(repo => repo.GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size))
        .ReturnsAsync((List<Provider>)null);

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(DateTime.Now, new OffsetFilter());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(0, result?.TotalAmount ?? 0);
        Assert.IsEmpty(result?.Entities ?? Enumerable.Empty<ProviderInfoBaseDto>());
    }

    [Test]
    public void Constructor_NullProviderRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportProviderService(null, Mock.Of<IWorkshopRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportProviderService>>()));
    }

    [Test]
    public void Constructor_NullWorkshopRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportProviderService(Mock.Of<IProviderRepository>(), null, Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportProviderService>>()));
    }

    [Test]
    public void Constructor_NullMapper_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportProviderService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IAverageRatingService>(), null, Mock.Of<ILogger<ExternalExportProviderService>>()));
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportProviderService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), Mock.Of<IAverageRatingService>(), Mock.Of<IMapper>(), null));
    }

    [Test]
    public void Constructor_NullAverageRatingService_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ExternalExportProviderService(Mock.Of<IProviderRepository>(), Mock.Of<IWorkshopRepository>(), null, Mock.Of<IMapper>(), Mock.Of<ILogger<ExternalExportProviderService>>()));
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
            var fakeWorkshops = WorkshopGenerator.Generate(3);

            fakeProviders[0].IsDeleted = true;
            fakeProviders[2].IsDeleted = true;

            dbContext.Providers.AddRange(fakeProviders);
            dbContext.Workshops.AddRange(fakeWorkshops);
            dbContext.SaveChanges();

            var providerRepository = new ProviderRepository(dbContext);
            var workshopRepository = new WorkshopRepository(dbContext);

            var externalExportProviderService = new ExternalExportProviderService(
                providerRepository,
                workshopRepository,
                new Mock<IAverageRatingService>().Object,
                mockMapper,
                new Mock<ILogger<ExternalExportProviderService>>().Object
            );

            // Act
            var result = await externalExportProviderService.GetProvidersWithWorkshops(updatedAfter, offsetFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fakeProviders.Count - 2, result.TotalAmount);
            Assert.AreEqual(fakeProviders.Count - 2, result.Entities.Count);
            Assert.IsTrue(result.Entities.All(provider => !provider.IsDeleted));
        }
    }
}