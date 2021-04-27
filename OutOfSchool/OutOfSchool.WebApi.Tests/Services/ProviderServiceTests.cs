using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ProviderServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IProviderRepository repoProvider;
        private IEntityRepository<Address> repoAddress;
        private IProviderService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger> logger;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);

            repoProvider = new ProviderRepository(context);
            repoAddress = new EntityRepository<Address>(context);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger>();
            service = new ProviderService(repoProvider, repoAddress, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Provider()
            {
                FullTitle = "NewTitle",
                ShortTitle = "NewShortTitle",
                Description = "NewDescription",
                EdrpouIpn = "16745678",
                Founder = "Founder",
                Email = "user@example.com",
                Ownership = OwnershipType.State,
                Type = ProviderType.FOP,
                LegalAddressId = 67,
                ActualAddressId = 87,
                UserId = "de909f35-5e56-4g7r-bda8-40a5bfda96a6",
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.FullTitle, result.FullTitle);
            Assert.AreEqual(expected.ShortTitle, result.ShortTitle);
            Assert.AreEqual(expected.Description, result.Description);
            Assert.AreEqual(expected.EdrpouIpn, result.EdrpouIpn);
            Assert.AreEqual(expected.Ownership, result.Ownership);
            Assert.AreEqual(expected.Type, result.Type);
            Assert.AreEqual(expected.LegalAddressId, result.LegalAddressId);
            Assert.AreEqual(expected.ActualAddressId, result.ActualAddressId);
            Assert.AreEqual(expected.UserId, result.UserId);
        }

        [Test]
        [Order(2)]
        public void Create_NotUniqueEntity_ReturnsArgumentException()
        {
            // Arrange
            var expected = new Provider()
            {
                FullTitle = "NewTitle",
                ShortTitle = "NewShortTitle",
                Description = "NewDescription",
                EdrpouIpn = "12345678",
                Ownership = OwnershipType.Private,
                Type = ProviderType.FOP,
                LegalAddressId = 5,
                ActualAddressId = 6,
                UserId = "de989f35-5e56-8k7r-bva8-40a5bfdcd6a6",
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(expected.ToModel()).ConfigureAwait(false));
        }

        [Test]
        [Order(3)]
        public async Task GetAll_WhenCalled_ReturnsAllEntities()
        {
            // Arrange
            var expected = await repoProvider.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.That(expected.ToList().Count(), Is.EqualTo(result.Count()));
        }

        [Test]
        [Order(4)]
        [TestCase(1)]
        public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
        {
            // Arrange
            var expected = await repoProvider.GetById(id);

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
        }

        [Test]
        [Order(5)]
        [TestCase(10)]
        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await service.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [Order(6)]
        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle1",
            };

            // Act
            var result = await service.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(changedEntity.FullTitle, Is.EqualTo(result.FullTitle));
        }

        [Test]
        [Order(7)]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                FullTitle = "NewTitle1",
            };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedEntity).ConfigureAwait(false));
        }

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var providers = new List<Provider>()
                {
                    new Provider()
                    {
                        Id = 1,
                        FullTitle = "Title1",
                        ShortTitle = "ShortTitle1",
                        Website = "Website1",
                        Facebook = "Facebook1",
                        Email = "user1@example.com",
                        Instagram = "Instagram1",
                        Description = "Description1",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 1,
                        ActualAddressId = 2,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                    },
                    new Provider()
                    {
                        Id = 2,
                        FullTitle = "Title2",
                        ShortTitle = "ShortTitle2",
                        Website = "Website2",
                        Facebook = "Facebook2",
                        Email = "user2@example.com",
                        Instagram = "Instagram2",
                        Description = "Description2",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345645",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 3,
                        ActualAddressId = 4,
                        UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                    },
                    new Provider()
                    {
                        Id = 3,
                        FullTitle = "Title3",
                        ShortTitle = "ShortTitle3",
                        Website = "Website3",
                        Facebook = "Facebook3",
                        Email = "user3@example.com",
                        Instagram = "Instagram3",
                        Description = "Description3",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345000",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 5,
                        ActualAddressId = 6,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                    },
                    new Provider()
                    {
                        Id = 4,
                        FullTitle = "Title4",
                        ShortTitle = "ShortTitle4",
                        Website = "Website4",
                        Facebook = "Facebook4",
                        Email = "user4@example.com",
                        Instagram = "Instagram4",
                        Description = "Description4",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "10045678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 56,
                        ActualAddressId = 23,
                        UserId = "de909f35-5eb7-4BBa-bda8-40a5bfda96a6",
                    },
                    new Provider()
                    {
                        Id = 5,
                        FullTitle = "Title5",
                        ShortTitle = "ShortTitle5",
                        Website = "Website5",
                        Facebook = "Facebook5",
                        Email = "user5@example.com",
                        Instagram = "Instagram5",
                        Description = "Description5",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12374678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 1,
                        ActualAddressId = 2,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                    },
                };

                context.Providers.AddRangeAsync(providers);
                context.SaveChangesAsync();
            }
        }
    }
}
