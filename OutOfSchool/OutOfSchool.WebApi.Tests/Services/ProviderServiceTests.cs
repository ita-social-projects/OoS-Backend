using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        private IProviderService service;
        private Mock<IRatingService> ratingService;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger> logger;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB").ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);

            repoProvider = new ProviderRepository(context);
            ratingService = new Mock<IRatingService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger>();
            service = new ProviderService(repoProvider, ratingService.Object, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Provider()
            {
                FullTitle = "Title",
                ShortTitle = "ShortTitle",
                Website = "Website",
                Facebook = "Facebook",
                Email = "user68@example.com",
                Instagram = "Instagram1",
                Description = "Description1",
                DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                EdrpouIpn = "55545128",
                PhoneNumber = "1111111111",
                Founder = "Founder",
                Ownership = OwnershipType.Private,
                Type = ProviderType.TOV,
                Status = false,
                LegalAddressId = 11,
                ActualAddressId = 12,
                UserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                LegalAddress = new Address
                {
                    Id = 11,
                    Region = "Region11",
                    District = "District11",
                    City = "City11",
                    Street = "Street11",
                    BuildingNumber = "BuildingNumber11",
                    Latitude = 0,
                    Longitude = 0,
                },
                ActualAddress = new Address
                {
                    Id = 12,
                    Region = "Region12",
                    District = "District12",
                    City = "City12",
                    Street = "Street12",
                    BuildingNumber = "BuildingNumber12",
                    Latitude = 0,
                    Longitude = 0,
                },
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.FullTitle, result.FullTitle);
            Assert.AreEqual(expected.ShortTitle, result.ShortTitle);
            Assert.AreEqual(expected.Website, result.Website);
            Assert.AreEqual(expected.Facebook, result.Facebook);
            Assert.AreEqual(expected.Email, result.Email);
            Assert.AreEqual(expected.Instagram, result.Instagram);
            Assert.AreEqual(expected.Description, result.Description);
            Assert.AreEqual(expected.DirectorDateOfBirth, result.DirectorDateOfBirth);
            Assert.AreEqual(expected.EdrpouIpn, result.EdrpouIpn);
            Assert.AreEqual(expected.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(expected.Founder, result.Founder);
            Assert.AreEqual(expected.Ownership, result.Ownership);
            Assert.AreEqual(expected.Type, result.Type);
            Assert.AreEqual(expected.Status, result.Status);
            Assert.AreEqual(expected.LegalAddressId, result.LegalAddressId);
            Assert.AreEqual(expected.ActualAddressId, result.ActualAddressId);
            Assert.AreEqual(expected.UserId, result.UserId);
            Assert.AreEqual(expected.LegalAddress.City, result.LegalAddress.City);
            Assert.AreEqual(expected.LegalAddress.BuildingNumber, result.LegalAddress.BuildingNumber);
            Assert.AreEqual(expected.LegalAddress.Street, result.LegalAddress.Street);
            Assert.AreEqual(expected.ActualAddress.City, result.ActualAddress.City);
            Assert.AreEqual(expected.ActualAddress.BuildingNumber, result.ActualAddress.BuildingNumber);
            Assert.AreEqual(expected.ActualAddress.Street, result.ActualAddress.Street);
        }

        [Test]
        public void Create_WhenUserIdExists_ReturnsArgumentException()
        {
            // Arrange
            var expected = new Provider()
            {
                FullTitle = "Title",
                ShortTitle = "ShortTitle",
                Website = "Website",
                Facebook = "Facebook",
                Email = "user167@example.com",
                Instagram = "Instagram1",
                Description = "Description1",
                DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                EdrpouIpn = "55545000",
                PhoneNumber = "1111111111",
                Founder = "Founder",
                Ownership = OwnershipType.Private,
                Type = ProviderType.TOV,
                Status = false,
                LegalAddressId = 166,
                ActualAddressId = 125,
                UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                LegalAddress = new Address
                {
                    Id = 166,
                    Region = "Region166",
                    District = "District166",
                    City = "City166",
                    Street = "Street166",
                    BuildingNumber = "BuildingNumber166",
                    Latitude = 0,
                    Longitude = 0,
                },
                ActualAddress = new Address
                {
                    Id = 125,
                    Region = "Region125",
                    District = "District125",
                    City = "City125",
                    Street = "Street125",
                    BuildingNumber = "BuildingNumber125",
                    Latitude = 0,
                    Longitude = 0,
                },
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(expected.ToModel()).ConfigureAwait(false));
        }

        [Test]
        public void Create_NotUniqueEntity_ReturnsArgumentException()
        {
            // Arrange
            var expected = new Provider()
            {
                FullTitle = "Title1",
                ShortTitle = "ShortTitle1",
                Website = "Website1",
                Facebook = "Facebook1",
                Email = "user1@example.com",
                Instagram = "Instagram1",
                Description = "Description1",
                DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                EdrpouIpn = "12345678",
                PhoneNumber = "1111111111",
                Founder = "Founder",
                Ownership = OwnershipType.Private,
                Type = ProviderType.TOV,
                Status = false,
                UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                LegalAddress = new Address
                {
                    Region = "Region",
                    District = "District",
                    City = "City",
                    Street = "Street",
                    BuildingNumber = "BuildingNumber",
                    Latitude = 0,
                    Longitude = 0,
                },
                ActualAddress = new Address
                {
                    Region = "Region",
                    District = "District",
                    City = "City",
                    Street = "Street",
                    BuildingNumber = "BuildingNumber",
                    Latitude = 0,
                    Longitude = 0,
                },
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(expected.ToModel()).ConfigureAwait(false));
        }

        [Test]
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
        [TestCase(10)]
        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await service.GetById(id).ConfigureAwait(false));
        }

        [Test]
        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle1",
            };

            // Act
            var result = await service.Update(changedEntity, "de909f35-5eb7-4b7a-bda8-40a5bfda96a6", "provider").ConfigureAwait(false);

            // Assert
            Assert.AreEqual(changedEntity.FullTitle, result.FullTitle);
        }

        [Test]
        public async Task Update_WhenUserIsAdmin_UpdatesExistedEntity()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle1",
            };

            // Act
            var result = await service.Update(changedEntity, "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c", "admin").ConfigureAwait(false);

            // Assert
            Assert.AreEqual(changedEntity.FullTitle, result.FullTitle);
        }

        [Test]
        public async Task Update_WhenUserIdIsNotValid_ReturnNull()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle1",
            };

            // Act
            var result = await service.Update(changedEntity, "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c", "provider").ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                FullTitle = "NewTitle1",
            };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedEntity, "de909f35-5eb7-4b7a-bda8-40a5bfda96a6", "admin").ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
        {
            // Act
            var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            await service.Delete(id).ConfigureAwait(false);

            var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
        }

        [Test]
        [TestCase(10)]
        public void Delete_WhenIdIsInvalid_ThrowsArgumentNullException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await service.Delete(id).ConfigureAwait(false));
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
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 1,
                        ActualAddressId = 2,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                        LegalAddress = new Address
                        {
                            Id = 1,
                            Region = "Region1",
                            District = "District1",
                            City = "City1",
                            Street = "Street1",
                            BuildingNumber = "BuildingNumber1",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new Address
                        {
                            Id = 2,
                            Region = "Region2",
                            District = "District2",
                            City = "City2",
                            Street = "Street2",
                            BuildingNumber = "BuildingNumber2",
                            Latitude = 0,
                            Longitude = 0,
                        },
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
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345645",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 3,
                        ActualAddressId = 4,
                        UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                        LegalAddress = new Address
                        {
                            Id = 3,
                            Region = "Region3",
                            District = "District3",
                            City = "City3",
                            Street = "Street3",
                            BuildingNumber = "BuildingNumber3",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new Address
                        {
                            Id = 4,
                            Region = "Region4",
                            District = "District4",
                            City = "City4",
                            Street = "Street4",
                            BuildingNumber = "BuildingNumber4",
                            Latitude = 0,
                            Longitude = 0,
                        },
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
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345000",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 5,
                        ActualAddressId = 6,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                        LegalAddress = new Address
                        {
                            Id = 5,
                            Region = "Region5",
                            District = "District5",
                            City = "City5",
                            Street = "Street5",
                            BuildingNumber = "BuildingNumber5",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new Address
                        {
                            Id = 6,
                            Region = "Region6",
                            District = "District6",
                            City = "City6",
                            Street = "Street6",
                            BuildingNumber = "BuildingNumber6",
                            Latitude = 0,
                            Longitude = 0,
                        },
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
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "10045678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 7,
                        ActualAddressId = 8,
                        UserId = "de909f35-5eb7-4BBa-bda8-40a5bfda96a6",
                        LegalAddress = new Address
                        {
                            Id = 7,
                            Region = "Region7",
                            District = "District7",
                            City = "City7",
                            Street = "Street7",
                            BuildingNumber = "BuildingNumber7",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new Address
                        {
                            Id = 8,
                            Region = "Region8",
                            District = "District8",
                            City = "City8",
                            Street = "Street8",
                            BuildingNumber = "BuildingNumber8",
                            Latitude = 0,
                            Longitude = 0,
                        },
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
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12374678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 9,
                        ActualAddressId = 10,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                        LegalAddress = new Address
                        {
                            Id = 9,
                            Region = "Region9",
                            District = "District9",
                            City = "City9",
                            Street = "Street9",
                            BuildingNumber = "BuildingNumber9",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new Address
                        {
                            Id = 10,
                            Region = "Region10",
                            District = "District10",
                            City = "City10",
                            Street = "Street10",
                            BuildingNumber = "BuildingNumber10",
                            Latitude = 0,
                            Longitude = 0,
                        },
                    },
                };

                var user = new User()
                {
                    Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                    CreatingTime = default,
                    LastLogin = default,
                    MiddleName = "MiddleName",
                    FirstName = "FirstName",
                    LastName = "LastName",
                    UserName = "user@gmail.com",
                    NormalizedUserName = "USER@GMAIL.COM",
                    Email = "user@gmail.com",
                    NormalizedEmail = "USER@GMAIL.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                    SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                    ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
                    PhoneNumber = "0965679725",
                    Role = "provider",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    IsRegistered = false,
                };

                context.Providers.AddRangeAsync(providers);
                context.Users.AddAsync(user);
                context.SaveChangesAsync();
            }
        }
    }
}
