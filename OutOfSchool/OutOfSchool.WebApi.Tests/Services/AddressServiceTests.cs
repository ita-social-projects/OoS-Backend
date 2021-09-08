using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class AddressServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IEntityRepository<Address> repo;
        private IAddressService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<AddressService>> logger;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            repo = new EntityRepository<Address>(context);
            logger = new Mock<ILogger<AddressService>>();
            service = new AddressService(repo, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Address()
            {
                Region = "NewRegion",
                District = "NewDistrict",
                City = "NewCity",
                Street = "NewStreet",
                BuildingNumber = "NewBuildingNumber",
                Latitude = 60.45383,
                Longitude = 70.56765,
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Region, result.Region);
            Assert.AreEqual(expected.District, result.District);
            Assert.AreEqual(expected.City, result.City);
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
                City = "ChangedCity",
            };

            // Act
            var result = await service.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(changedEntity.City, Is.EqualTo(result.City));
        }

        [Test]
        [Order(6)]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new AddressDto()
            {
                City = "ChangedCity",
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
                        Region = "Region1",
                        District = "District1",
                        City = "City1",
                        Street = "Street1",
                        BuildingNumber = "BuildingNumber1",
                        Latitude = 41.45383,
                        Longitude = 51.56765,
                    },
                    new Address()
                    {
                        Id = 2,
                        Region = "Region2",
                        District = "District2",
                        City = "City2",
                        Street = "Street2",
                        BuildingNumber = "BuildingNumber2",
                        Latitude = 42.45383,
                        Longitude = 52.56765,
                    },
                    new Address()
                    {
                        Id = 3,
                        Region = "Region3",
                        District = "District3",
                        City = "City3",
                        Street = "Street3",
                        BuildingNumber = "BuildingNumber3",
                        Latitude = 43.45383,
                        Longitude = 53.56765,
                    },
                    new Address()
                    {
                        Id = 4,
                        Region = "Region4",
                        District = "District4",
                        City = "City4",
                        Street = "Street4",
                        BuildingNumber = "BuildingNumber4",
                        Latitude = 44.45383,
                        Longitude = 54.56765,
                    },
                    new Address()
                    {
                        Id = 5,
                        Region = "Region5",
                        District = "District5",
                        City = "City5",
                        Street = "Street5",
                        BuildingNumber = "BuildingNumber5",
                        Latitude = 45.45383,
                        Longitude = 55.56765,
                    },
                };

                context.Addresses.AddRangeAsync(addresses);
                context.SaveChangesAsync();
            }
        }
    }
}
