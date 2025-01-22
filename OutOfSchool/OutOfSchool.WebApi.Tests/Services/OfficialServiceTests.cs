using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class OfficialServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private OfficialService service;
    private IEntityRepositorySoftDeleted<Guid, Official> repository;
    private Mock<IProviderService> providerService;
    private Mock<ILogger<OfficialService>> logger;
    private IMapper mapper;
    private Guid providerId;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
            databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        repository = new EntityRepositorySoftDeleted<Guid, Official>(context);
        providerId = Guid.NewGuid();

        providerService = new Mock<IProviderService>();
        logger = new Mock<ILogger<OfficialService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        service = new OfficialService(repository, providerService.Object, logger.Object, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetByFilter_ReturnsSearchResultWithListOfOfficials_WhenFilterIsNull()
    {
        // Arrange
        var expected = Officials();

        // Act
        var result = await service.GetByFilter(providerId, null).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Entities.First().Id, Is.EqualTo(expected.First().Id));
        Assert.That(result.TotalAmount, Is.EqualTo(expected.Count));
        Assert.IsInstanceOf<SearchResult<OfficialDto>>(result);
    }

    [Test]
    public async Task GetByFilter_ReturnsSearchResultWithFilteredListOfOfficials_WhenFilterIsSpecified()
    {
        // Arrange
        var expected = Officials().FirstOrDefault();
        var filter = new SearchStringFilter()
        {
            SearchString = "TestPosition1"
        };

        // Act
        var result = await service.GetByFilter(providerId, filter).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Entities.First().Id, Is.EqualTo(expected.Id));
        Assert.That(result.TotalAmount, Is.EqualTo(1));
        Assert.IsInstanceOf<SearchResult<OfficialDto>>(result);
    }

    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.Officials.AddRange(Officials());

            ctx.SaveChanges();
        }
    }

    private List<Official> Officials()
    {
        return new List<Official>()
        {
            new Official() {
                Id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16"),
                ExternalRegistryId = Guid.NewGuid(),
                EmploymentType = EmploymentType.Main,
                Position = new Position()
                {
                    Id = Guid.NewGuid(),
                    FullName = "TestPosition1"
                },
                Individual = new Individual()
                {
                    FirstName = "Test",
                    LastName = "Testov",
                    MiddleName = "Testovich",
                    Rnokpp = "1234567890"
                }
            },
            new Official() {
                Id = new Guid("4ca6f3af-5d02-4c16-b4b2-e202c71470f4"),
                ExternalRegistryId = Guid.NewGuid(),
                EmploymentType = EmploymentType.Main,
                Position = new Position()
                {
                    Id = Guid.NewGuid(),
                    FullName = "TestPosition2"
                },
                Individual = new Individual()
                {
                    FirstName = "Test",
                    LastName = "Testov",
                    MiddleName = "Testovich",
                    Rnokpp = "1234567890"
                }
            }
        };
    }
}
