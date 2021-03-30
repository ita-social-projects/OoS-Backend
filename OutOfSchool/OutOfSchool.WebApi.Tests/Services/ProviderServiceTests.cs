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
        private IProviderRepository repo;
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

            repo = new ProviderRepository(context);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger>();
            service = new ProviderService(repo, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Provider()
            {
                Title = "NewTitle",
                ShortTitle = "NewShortTitle",
                Description = "NewDescription",
                MFO = "874356",
                EDRPOU = "16745678",
                INPP = "1230167890",
                Ownership = OwnershipType.State,
                Type = ProviderType.FOP,
                Profile = ProviderProfile.Artistical,
                AddressId = 10,
                UserId = "de909f35-5e56-4g7r-bda8-40a5bfda96a6",
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.ShortTitle, result.ShortTitle);
            Assert.AreEqual(expected.Description, result.Description);
            Assert.AreEqual(expected.MFO, result.MFO);
            Assert.AreEqual(expected.EDRPOU, result.EDRPOU);
            Assert.AreEqual(expected.INPP, result.INPP);
            Assert.AreEqual(expected.Ownership, result.Ownership);
            Assert.AreEqual(expected.Type, result.Type);
            Assert.AreEqual(expected.Profile, result.Profile);
            Assert.AreEqual(expected.AddressId, result.AddressId);
            Assert.AreEqual(expected.UserId, result.UserId);
        }

        [Test]
        [Order(2)]
        public void Create_NotUniqueEntity_ReturnsArgumentException()
        {
            // Arrange
            var expected = new Provider()
            {
                Title = "NewTitle",
                ShortTitle = "NewShortTitle",
                Description = "NewDescription",
                MFO = "874356",
                EDRPOU = "12345678",
                INPP = "1234567890",
                Ownership = OwnershipType.Private,
                Type = ProviderType.FOP,
                Profile = ProviderProfile.ResearchecallyExperimental,
                AddressId = 5,
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
            var expected = await repo.GetAll();

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
            var expected = await repo.GetById(id);

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
                Title = "ChangedTitle1",
            };

            // Act
            var result = await service.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
        }

        [Test]
        [Order(7)]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new ProviderDto()
            {
                Title = "NewTitle1",
            };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedEntity).ConfigureAwait(false));
        }

        [Test]
        [Order(8)]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
        {
            // Act
            var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            context.Entry<Provider>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

            await service.Delete(id).ConfigureAwait(false);

            var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
        }

        [Test]
        [Order(9)]
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

                var providers = new List<Provider>()
                {
                    new Provider()
                    {
                        Id = 1,
                        Title = "Title1",
                        ShortTitle = "ShortTitle1",
                        Website = "Website1",
                        Facebook = "Facebook1",
                        Instagram = "Instagram1",
                        Description = "Description1",
                        MFO = "123456",
                        EDRPOU = "12345678",
                        KOATUU = "0100000000",
                        INPP = "1234567890",
                        Director = "Director1",
                        DirectorPosition = "Position1",
                        AuthorityHolder = "Holder1",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        DirectorPhone = "1111111111",
                        ManagerialBody = "ManagerialBody1",
                        Ownership = OwnershipType.Common,
                        Type = ProviderType.FOP,
                        Form = "Form1",
                        Profile = ProviderProfile.Athletic,
                        Index = "Index1",
                        IsSubmitPZ1 = true,
                        AttachedDocuments = "Dcument1",
                        AddressId = 5,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                    },
                    new Provider()
                    {
                        Id = 2,
                        Title = "Title2",
                        ShortTitle = "ShortTitle2",
                        Website = "Website2",
                        Facebook = "Facebook2",
                        Instagram = "Instagram2",
                        Description = "Description2",
                        MFO = "654321",
                        EDRPOU = "87654321",
                        KOATUU = "0200000000",
                        INPP = "0987654321",
                        Director = "Director2",
                        DirectorPosition = "Position2",
                        AuthorityHolder = "Holder2",
                        DirectorBirthDay = new DateTime(1982, month: 11, 23),
                        DirectorPhone = "1111111111",
                        ManagerialBody = "ManagerialBody2",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Form = "Form2",
                        Profile = ProviderProfile.ArtisticallyAesthetic,
                        Index = "Index2",
                        IsSubmitPZ1 = true,
                        AttachedDocuments = "Dcument2",
                        AddressId = 6,
                        UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6",
                    },
                    new Provider()
                    {
                        Id = 3,
                        Title = "Title3",
                        ShortTitle = "ShortTitle3",
                        Website = "Website3",
                        Facebook = "Facebook3",
                        Instagram = "Instagram3",
                        Description = "Description3",
                        MFO = "321654",
                        EDRPOU = "43218765",
                        KOATUU = "0300000000",
                        INPP = "5432109876",
                        Director = "Director3",
                        DirectorPosition = "Position3",
                        AuthorityHolder = "Holder3",
                        DirectorBirthDay = new DateTime(1978, month: 10, 13),
                        DirectorPhone = "1111111111",
                        ManagerialBody = "ManagerialBody3",
                        Ownership = OwnershipType.State,
                        Type = ProviderType.EducationalInstitution,
                        Form = "Form3",
                        Profile = ProviderProfile.Curative,
                        Index = "Index3",
                        IsSubmitPZ1 = true,
                        AttachedDocuments = "Dcument3",
                        AddressId = 7,
                        UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6",
                    },
                    new Provider()
                    {
                        Id = 4,
                        Title = "Title4",
                        ShortTitle = "ShortTitle4",
                        Website = "Website4",
                        Facebook = "Facebook4",
                        Instagram = "Instagram4",
                        Description = "Description4",
                        MFO = "165432",
                        EDRPOU = "21874365",
                        KOATUU = "0400000000",
                        INPP = "5438762109",
                        Director = "Director4",
                        DirectorPosition = "Position4",
                        AuthorityHolder = "Holder4",
                        DirectorBirthDay = new DateTime(1979, month: 10, 27),
                        DirectorPhone = "1111111111",
                        ManagerialBody = "ManagerialBody4",
                        Ownership = OwnershipType.State,
                        Type = ProviderType.Social,
                        Form = "Form4",
                        Profile = ProviderProfile.Scout,
                        Index = "Index4",
                        IsSubmitPZ1 = false,
                        AttachedDocuments = "Dcument4",
                        AddressId = 8,
                        UserId = "de804f35-bda9-4b9n-8eb1-54a5okfg90a6",
                    },
                    new Provider()
                    {
                        Id = 5,
                        Title = "Title5",
                        ShortTitle = "ShortTitle5",
                        Website = "Website5",
                        Facebook = "Facebook5",
                        Instagram = "Instagram5",
                        Description = "Description5",
                        MFO = "105402",
                        EDRPOU = "20804065",
                        KOATUU = "0500000000",
                        INPP = "5400700109",
                        Director = "Director5",
                        DirectorPosition = "Position5",
                        AuthorityHolder = "Holder5",
                        DirectorBirthDay = new DateTime(1985, month: 10, 21),
                        DirectorPhone = "1111111111",
                        ManagerialBody = "ManagerialBody5",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.FOP,
                        Form = "Form5",
                        Profile = ProviderProfile.MilitaryPatriotic,
                        Index = "Index5",
                        IsSubmitPZ1 = false,
                        AttachedDocuments = "Dcument5",
                        AddressId = 9,
                        UserId = "de804f35-tga0-4g9n-8db1-54a5okfg80a4",
                    },
                };

                context.Providers.AddRangeAsync(providers);
                context.SaveChangesAsync();
            }
        }
    }
}
