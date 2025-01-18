using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

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
    private IInstitutionHierarchyRepository institutionHierarchyRepository;
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
        mockMapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        mockLogger = new Mock<ILogger<ExternalExportService>>();
        institutionHierarchyRepository = new InstitutionHierarchyRepository(dbContext);

        externalExportService = new ExternalExportService(
            mockProviderRepository.Object,
            mockWorkshopRepository.Object,
            mockApplicationRepository.Object,
            mockAverageRatingService.Object,
            new EntityRepositorySoftDeleted<long, Direction>(dbContext),
            new SensitiveEntityRepositorySoftDeleted<Institution>(dbContext),
            institutionHierarchyRepository,
            mockMapper,
            mockLogger.Object);
        
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
    
    [Test]
    public async Task GetSubDirections_ReturnsEmptySearchResult()
    {
        // Arrange
        var offsetFilter = new OffsetFilter { Size = 10 };

        // Act
        var result = await externalExportService.GetSubDirections(offsetFilter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(0, result.Entities.Count);
    }

    [Test]
    public async Task GetSubDirections_ReturnsSearchResultData()
    {
        // Arrange
        var offsetFilter = new OffsetFilter { Size = 10 };
        SeedSubDirections(institutionHierarchyRepository);
    
        // Act
        var result = await externalExportService.GetSubDirections(offsetFilter);
    
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.TotalAmount);
        Assert.AreEqual(2, result.Entities.Count);
        Assert.AreEqual(1, result.Entities.First(s => s.Id == Guid.Parse("b7e1322e-7575-48c1-a444-4effb8f4d083")).DirectionIds.Count);
        Assert.AreEqual(2, result.Entities.First(s => s.Id == Guid.Parse("a042661d-9be8-4bfb-adcd-06cbe91388a0")).DirectionIds.Count);
    }

    private void SeedSubDirections(IInstitutionHierarchyRepository repository)
    {
        var twoLevelsId = Guid.Parse("a11164b7-35c8-4ecb-8500-b6c4cac722bd");
        var fourLevelsId = Guid.Parse("a63588e4-f57f-4075-8927-525113be55d5");
        
        var fakeInstitutions = InstitutionsGenerator.Generate(2);
        fakeInstitutions[0].WithLevels(2).WithId(twoLevelsId);
        fakeInstitutions[1].WithLevels(4).WithId(fourLevelsId);

        List<Guid> hierarchyIds = [
            Guid.Parse("b7e1322e-7575-48c1-a444-4effb8f4d083"),
            Guid.Parse("a042661d-9be8-4bfb-adcd-06cbe91388a0"),
            Guid.Parse("dd116229-e0a1-4c9a-aae5-1f6d5878d37f")
        ];
        var fakeInstitutionHierarchies = InstitutionHierarchyGenerator.Generate(3);
        fakeInstitutionHierarchies[0]
            .WithId(hierarchyIds[0])
            .WithParentId(hierarchyIds[2])
            .WithInstitutionId(fakeInstitutions[0].Id)
            .WithLevel(2);
        fakeInstitutionHierarchies[1]
            .WithId(hierarchyIds[1])
            .WithInstitutionId(fakeInstitutions[1].Id)
            .WithLevel(4);
        fakeInstitutionHierarchies[2]
            .WithId(hierarchyIds[2])
            .WithInstitutionId(fakeInstitutions[0].Id)
            .WithLevel(1);

        var fakeDirections = DirectionsGenerator.Generate(4);

        dbContext.Institutions.AddRange(fakeInstitutions);
        dbContext.InstitutionHierarchies.AddRange(fakeInstitutionHierarchies);
        dbContext.Directions.AddRange(fakeDirections);
        dbContext.SaveChanges();
        repository.Update(fakeInstitutionHierarchies[0], [fakeDirections[0].Id]).Wait();
        repository.Update(fakeInstitutionHierarchies[1], [fakeDirections[1].Id, fakeDirections[2].Id]).Wait();
        repository.Update(fakeInstitutionHierarchies[2], [fakeDirections[3].Id]).Wait();
    }
}