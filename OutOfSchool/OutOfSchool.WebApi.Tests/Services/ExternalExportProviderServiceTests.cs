using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class ExternalExportProviderServiceTests
{
    private ExternalExportProviderService externalExportProviderService;
    private Mock<IProviderRepository> mockProviderRepository;
    private Mock<IWorkshopRepository> mockWorkshopRepository;
    private IMapper mockMapper;
    private Mock<ILogger<ExternalExportProviderService>> mockLogger;

    [SetUp]
    public void Setup()
    {
        mockProviderRepository = new Mock<IProviderRepository>();
        mockWorkshopRepository = new Mock<IWorkshopRepository>();
        mockMapper = OutOfSchool.Tests.Common.TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportProviderService>>();

        externalExportProviderService = new ExternalExportProviderService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockMapper,
            mockLogger.Object);
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ReturnsEmptySearchResult()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var sizeFilter = new SizeFilter { Size = 10 };
        var fakeProviders = new List<Provider>();

        mockProviderRepository
            .Setup(x => x.GetAllWithDeleted())
            .ReturnsAsync(fakeProviders);

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(updatedAfter, sizeFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetProvidersWithWorkshops_ReturnsSearchResultWithNullWorkshops()
    {
        // Arrange
        var updatedAfter = DateTime.UtcNow;
        var sizeFilter = new SizeFilter { Size = 10 };
        var fakeProviders = ProvidersGenerator.Generate(5).WithWorkshops();

        mockProviderRepository
            .Setup(x => x.GetAllWithDeleted())
            .ReturnsAsync(fakeProviders);

        mockWorkshopRepository
            .Setup(x => x.GetAllWithDeleted(It.IsAny<Expression<Func<Workshop, bool>>>()))
            .ReturnsAsync(WorkshopGenerator.Generate(3).WithProvider());

        // Act
        var result = await externalExportProviderService.GetProvidersWithWorkshops(updatedAfter, sizeFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
        Assert.AreEqual(fakeProviders.Count, result.Entities.Count);
        Assert.IsTrue(result.Entities.All(provider => provider.Workshops != null && provider.Workshops.Any()));
    }
}