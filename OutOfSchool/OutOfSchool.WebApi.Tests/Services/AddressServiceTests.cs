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
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class AddressServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private IEntityRepository<long, Address> repo;
    private IAddressService service;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ILogger<AddressService>> logger;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        repo = new EntityRepository<long, Address>(context);
        logger = new Mock<ILogger<AddressService>>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Util.MappingProfile>());
        mapper = config.CreateMapper();

        service = new AddressService(repo, logger.Object, localizer.Object, mapper);

        SeedDatabase();
    }

    [Test]
    [Order(1)]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var expected = new Address()
        {
            Street = "NewStreet",
            BuildingNumber = "NewBuildingNumber",
            Latitude = 60.45383,
            Longitude = 70.56765,
            CATOTTGId = 1,
        };

        // Act
        var result = await service.Create(mapper.Map<AddressDto>(expected)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.CATOTTGId, result.CATOTTGId);
        Assert.AreEqual(expected.Street, result.Street);
        Assert.AreEqual(expected.BuildingNumber, result.BuildingNumber);
        Assert.AreEqual(expected.Latitude, result.Latitude);
        Assert.AreEqual(expected.Longitude, result.Longitude);
    }

    [Test]
    [Order(2)]
    public async Task GetAll_WhenCalled_ReturnsAllEntities()
    {
        // Arrange
        var expected = await repo.GetAll();

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        Assert.That(expected.ToList().Count(), Is.EqualTo(result.Count()));
    }

    [Test]
    [Order(3)]
    [TestCase(1)]
    public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
    {
        // Arrange
        var expected = await repo.GetById(id);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Id, result.Id);
    }

    [Test]
    [Order(4)]
    [TestCase(10)]
    public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.GetById(id).ConfigureAwait(false));
    }

    [Test]
    [Order(5)]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var changedEntity = new AddressDto()
        {
            Id = 1,
            CATOTTGId = 4970,
        };

        // Act
        var result = await service.Update(changedEntity).ConfigureAwait(false);

        // Assert
        Assert.That(changedEntity.CATOTTGId, Is.EqualTo(result.CATOTTGId));
    }

    [Test]
    [Order(6)]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedEntity = new AddressDto()
        {
            CATOTTGId = 4970,
        };

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedEntity).ConfigureAwait(false));
    }

    [Test]
    [Order(7)]
    [TestCase(1)]
    public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
    {
        // Act
        var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

        context.Entry<Address>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

        await service.Delete(id).ConfigureAwait(false);

        var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

        // Assert
        Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
    }

    [Test]
    [Order(8)]
    [TestCase(10)]
    public void Delete_WhenIdIsInvalid_ThrowsDbUpdateConcurrencyException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Delete(id).ConfigureAwait(false));
    }

    private void SeedDatabase()
    {
        using var context = new OutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var addresses = new List<Address>()
            {
                new Address()
                {
                    Id = 1,
                    Street = "Street1",
                    BuildingNumber = "BuildingNumber1",
                    Latitude = 41.45383,
                    Longitude = 51.56765,
                    CATOTTGId = 1,
                },
                new Address()
                {
                    Id = 2,
                    Street = "Street2",
                    BuildingNumber = "BuildingNumber2",
                    Latitude = 42.45383,
                    Longitude = 52.56765,
                    CATOTTGId = 2,
                },
                new Address()
                {
                    Id = 3,
                    Street = "Street3",
                    BuildingNumber = "BuildingNumber3",
                    Latitude = 43.45383,
                    Longitude = 53.56765,
                    CATOTTGId = 2,
                },
                new Address()
                {
                    Id = 4,
                    Street = "Street4",
                    BuildingNumber = "BuildingNumber4",
                    Latitude = 44.45383,
                    Longitude = 54.56765,
                    CATOTTGId = 2,
                },
                new Address()
                {
                    Id = 5,
                    Street = "Street5",
                    BuildingNumber = "BuildingNumber5",
                    Latitude = 45.45383,
                    Longitude = 55.56765,
                    CATOTTGId = 2,
                },
            };

            context.Addresses.AddRange(addresses);

            var codeficators = new List<CATOTTG>()
            {
                new CATOTTG()
                {
                    Id = 1,
                    Name = "Test",
                    Category = "О",
                    Latitude = 41.45383,
                    Longitude = 51.56765,
                },
                new CATOTTG()
                {
                    Id = 2,
                    Name = "Test2",
                    Category = "О",
                    Latitude = 41.45383,
                    Longitude = 51.56765,
                },
            };

            context.CATOTTGs.AddRange(codeficators);

            context.SaveChanges();
        }
    }
}