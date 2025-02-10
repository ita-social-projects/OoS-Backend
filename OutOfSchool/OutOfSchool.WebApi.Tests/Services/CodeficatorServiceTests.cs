using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CodeficatorServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext context;
    private ICodeficatorRepository repository;
    private ICodeficatorService service;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        repository = new CodeficatorRepository(context);
        service = new CodeficatorService(repository, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetChildrenByParentId_WhenIdIsNull_ReturnsEntity()
    {
        // Arrange
        Expression<Func<CATOTTG, bool>> filter = p => !p.ParentId.HasValue;
        var expected = await repository.GetByFilter(filter).ConfigureAwait(false);

        // Act
        var result = await service.GetChildrenByParentId(null).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Count(), result.Count());
        Assert.AreEqual(expected.Count(x => x.Category == CodeficatorCategory.Region.Name), result.Count(x => x.Category == CodeficatorCategory.Region.Name));
    }

    [Test]
    [TestCase(1)]
    public async Task GetChildrenByParentId_WhenIdIsExists_ReturnsEntity(long id)
    {
        // Arrange
        Expression<Func<CATOTTG, bool>> filter = p => p.ParentId == id;
        var expected = await repository.GetByFilter(filter).ConfigureAwait(false);

        // Act
        var result = await service.GetChildrenByParentId(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(expected.FirstOrDefault().Id, result.FirstOrDefault().Id);
        Assert.AreEqual(expected.FirstOrDefault().Name, result.FirstOrDefault().Name);
    }

    [Test]
    public async Task GetChildrenNamesByParentId_WhenIdIsNull_ReturnsEntity()
    {
        // Arrange
        Expression<Func<CATOTTG, bool>> filter = p => !p.ParentId.HasValue;
        var expected = await repository.GetByFilter(filter).ConfigureAwait(false);

        // Act
        var result = await service.GetChildrenNamesByParentId(null).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Count(), result.Count());
    }

    [Test]
    [TestCase(1)]
    public async Task GetChildrenNamesByParentId_WhenIdIsExists_ReturnsEntity(long id)
    {
        // Arrange
        Expression<Func<CATOTTG, bool>> filter = p => p.ParentId == id;
        var expected = await repository.GetByFilter(filter).ConfigureAwait(false);

        // Act
        var result = await service.GetChildrenNamesByParentId(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(expected.FirstOrDefault().Id, result.FirstOrDefault().Key);
        Assert.AreEqual(expected.FirstOrDefault().Name, result.FirstOrDefault().Value);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public async Task GetAllAddressPartsById_WhenIdIsRegionDistrictTerritorialUnitOrSettlementId_ReturnsEntity(long id)
    {
        // Act
        var result = await service.GetAllAddressPartsById(id).ConfigureAwait(false);

        // Assert
        AssertValue(id, result);
    }

    private void AssertValue(long id, AllAddressPartsDto result)
    {
        switch (id)
        {
            case 1: Assert.AreEqual("Автономна Республіка Крим", result.FullAddress); break;
            case 2: Assert.AreEqual("Бахчисарайський, Автономна Республіка Крим", result.FullAddress); break;
            case 3: Assert.AreEqual("Андріївська, Бахчисарайський, Автономна Республіка Крим", result.FullAddress); break;
            case 4: Assert.AreEqual("Андріївка, Андріївська, Бахчисарайський, Автономна Республіка Крим", result.FullAddress); break;
        }
    }

    private void SeedDatabase()
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var region = new CATOTTG()
        {
            Id = 1,
            Name = "Автономна Республіка Крим",
            Code = "UA01000000000013043",
            ParentId = null,
            Category = "O",
            GeoHash = 599773520179757055,
            Latitude = 44.95829,
            Longitude = 34.11014,
            NeedCheck = false,
        };

        var district = new CATOTTG()
        {
            Id = 2,
            Name = "Бахчисарайський",
            Code = "UA01020000000022387",
            ParentId = 1,
            Category = "P",
            GeoHash = 599773520179757055,
            Latitude = 44.95829,
            Longitude = 34.11014,
            NeedCheck = false,
            Parent = region,
        };

        var territorialUnit = new CATOTTG()
        {
            Id = 3,
            Name = "Андріївська",
            Code = "UA01020010000048857",
            ParentId = 2,
            Category = "H",
            GeoHash = 599773520179757055,
            Latitude = 44.95829,
            Longitude = 34.11014,
            NeedCheck = false,
            Parent = district,
        };

        var codeficators = new List<CATOTTG>()
        {
            region,
            district,
            territorialUnit,
            new CATOTTG()
            {
                Id = 4,
                Name = "Андріївка",
                Code = "UA01020010010075540",
                ParentId = 3,
                Category = "C",
                GeoHash = 599773520179757055,
                Latitude = 44.95829,
                Longitude = 34.11014,
                NeedCheck = false,
                Parent = territorialUnit,
            },
            new CATOTTG()
            {
                Id = 4087,
                Name = "Дніпропетровська",
                Code = "UA12000000000090473",
                ParentId = null,
                Category = "O",
                GeoHash = 599773520179757055,
                Latitude = 44.95829,
                Longitude = 34.11014,
                NeedCheck = false,
            },
        };
        context.CATOTTGs.AddRange(codeficators);
        context.SaveChanges();
    }
}