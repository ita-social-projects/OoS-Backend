using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CityServiceTests
{
    private ICityService service;
    private OutOfSchoolDbContext context;
    private IEntityRepository<long, City> repository;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ILogger<CityService>> logger;
    private DbContextOptions<OutOfSchoolDbContext> options;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        repository = new EntityRepository<long, City>(context);
        logger = new Mock<ILogger<CityService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<Util.MappingProfile>();
        service = new CityService(repository, logger.Object, localizer.Object, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllCities()
    {
        // Arrange
        var expected = await repository.GetAll();

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        Assert.AreEqual(result.ToList().Count(), expected.Count());
    }

    [Test]
    [TestCase(1)]
    public async Task GetById_WhenIdIsValid_ReturnsCity(long id)
    {
        // Arrange
        var expected = await repository.GetById(id);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Id, result.Id);
    }

    [Test]
    [TestCase(10)]
    public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.GetById(id).ConfigureAwait(false));
    }

    [Test]
    [TestCase("NoName")]
    public async Task GetByCityName_WhenNameIsValid_ReturnsCities(string name)
    {
        // Arrange
        var expected = await repository.GetByFilter(c => c.Name.Contains(name));

        // Act
        var result = await service.GetByCityName(name).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(result.Count(), expected.Count());
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedCityEntity()
    {
        // Arrange
        var expected = new CityDto()
        {
            Name = "TestName",
            Region = "Test",
            District = "Test",
            Latitude = 45.45,
            Longitude = 55.55,
        };

        // Act
        var result = await service.Create(expected).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Name, result.Name);
        Assert.AreEqual(expected.Region, result.Region);
        Assert.AreEqual(expected.District, result.District);
        Assert.AreEqual(expected.Latitude, result.Latitude);
        Assert.AreEqual(expected.Longitude, result.Longitude);
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedCityEntity()
    {
        // Arrange
        var changedEntity = new CityDto()
        {
            Id = 1,
            Name = "TestName",
            Region = "Test",
            District = "Test",
            Latitude = 45.45,
            Longitude = 55.55,
        };

        // Act
        var result = await service.Update(changedEntity).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(changedEntity.Name, result.Name);
        Assert.AreEqual(changedEntity.Region, result.Region);
        Assert.AreEqual(changedEntity.District, result.District);
        Assert.AreEqual(changedEntity.Latitude, result.Latitude);
        Assert.AreEqual(changedEntity.Longitude, result.Longitude);
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedEntity = new CityDto()
        {
            Name = "TestName",
        };

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedEntity).ConfigureAwait(false));
    }

    [Test]
    [TestCase(1)]
    public async Task Delete_WhenIdIsValid_DeletesCityEntity(long id)
    {
        // Arrange
        var expected = await context.Cities.CountAsync();

        // Act
        await service.Delete(id).ConfigureAwait(false);

        var result = (await service.GetAll().ConfigureAwait(false)).Count();

        // Assert
        Assert.AreEqual(expected - 1, result);
    }

    [Test]
    [TestCase(10)]
    public void Delete_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.Delete(id).ConfigureAwait(false));
    }

    private void SeedDatabase()
    {
        using var context = new OutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var cities = new List<City>()
        {
            new City()
            {
                Id = 1,
                Name = "NoName",
                Region = "Test",
                District = "Test",
            },
            new City()
            {
                Id = 2,
                Name = "HaveName",
                Region = "Test",
                District = "Test",
            },
            new City()
            {
                Id = 3,
                Name = "MissName",
                Region = "Test",
                District = "Test",
            },
        };

            context.Cities.AddRange(cities);
            context.SaveChanges();
        }
    }
}