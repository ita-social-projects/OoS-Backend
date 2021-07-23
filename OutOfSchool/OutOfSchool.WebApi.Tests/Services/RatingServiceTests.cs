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
using OutOfSchool.WebApi.Services;
using ILogger = Serilog.ILogger;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class RatingServiceTests
    {
        private const int RoundToDigits = 2;
        private IRatingService service;
        private OutOfSchoolDbContext context;
        private IRatingRepository ratingRepository;
        private IWorkshopRepository workshopRepository;
        private IProviderRepository providerRepository;
        private IParentRepository parentRepository;
        private IEntityRepository<User> userRepository;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger> logger;
        private DbContextOptions<OutOfSchoolDbContext> options;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            ratingRepository = new RatingRepository(context);
            workshopRepository = new WorkshopRepository(context);
            providerRepository = new ProviderRepository(context);
            parentRepository = new ParentRepository(context);
            userRepository = new EntityRepository<User>(context);
            logger = new Mock<ILogger>();
            service = new RatingService(ratingRepository, workshopRepository, providerRepository, parentRepository, userRepository, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllRatings()
        {
            // Arrange
            var expected = await ratingRepository.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.AreEqual(result.ToList().Count(), expected.Count());
        }

        [Test]
        [TestCase(1)]
        [TestCase(4)]
        public async Task GetById_WhenIdIsValid_ReturnsRating(long id)
        {
            // Arrange
            var expected = await ratingRepository.GetById(id);

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
        [TestCase(1, RatingType.Provider)]
        [TestCase(1, RatingType.Workshop)]
        public async Task GetAllByEntityId_WhenRatingExist_ReturnsCorrectRating(long entityId, RatingType type)
        {
            // Arrange
            var expected = await ratingRepository.GetByFilter(r => r.EntityId == entityId && r.Type == type).ConfigureAwait(false);

            // Act
            var result = await service.GetAllByEntityId(entityId, type).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(result.Count(), expected.Count());
        }

        [Test]
        [TestCase(1, 1, RatingType.Provider)]
        [TestCase(3, 1, RatingType.Workshop)]
        public async Task GetParentRating_WhenRatingExist_ReturnsCorrectRating(long parentId, long entityId, RatingType type)
        {
            // Arrange
            var ratings = await ratingRepository
                                    .GetByFilter(r => r.ParentId == parentId
                                                   && r.EntityId == entityId
                                                   && r.Type == type)
                                    .ConfigureAwait(false);
            var expected = ratings.FirstOrDefault().Rate;

            // Act
            var result = await service.GetParentRating(parentId, entityId, type).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(expected, result.Rate);
        }

        [Test]
        [TestCase(10, 10, RatingType.Provider)]
        public async Task GetParentRating_WhenRatingNotExist_ReturnsNull(long parentId, long entityId, RatingType type)
        {
            // Act
            var result = await service.GetParentRating(parentId, entityId, type).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [TestCase(1, RatingType.Provider)]
        public void GetAverageRating_WhenExistRatingForEntityId_ReturnsAverageRating(long entityId, RatingType type)
        {
            // Arrange
            var ratingTuple = ratingRepository.GetAverageRating(entityId, type);
            var expectedRating = (float)Math.Round(ratingTuple.Item1, RoundToDigits);
            var expectedNumbers = ratingTuple.Item2;

            // Act
            var result = service.GetAverageRating(entityId, type);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(expectedRating, result.Item1);
            Assert.AreEqual(expectedNumbers, result.Item2);
        }

        [Test]
        [TestCase(100, RatingType.Provider)]
        public void GetAverageRating_WhenEntityNotExist_ReturnsZero(long entityId, RatingType type)
        {
            // Arrange
            var expectedRating = default(float);
            var expectedNumber = default(int);

            // Act
            var result = service.GetAverageRating(entityId, type);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(expectedRating, result?.Item1);
            Assert.AreEqual(expectedNumber, result?.Item1);
        }

        [Test]
        [TestCase(3, RatingType.Provider)]
        public void GetAverageRating_WhenNotExistRatingForEntityId_ReturnsZero(long entityId, RatingType type)
        {
            // Arrange
            var expectedRating = default(float);
            var expectedNumber = default(int);

            // Act
            var result = service.GetAverageRating(entityId, type);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(expectedRating, result?.Item1);
            Assert.AreEqual(expectedNumber, result?.Item1);
        }

        [Test]
        [TestCase(new[] { 1L, 2L, 3L }, RatingType.Provider)]
        public void GetAverageRatingForRange_WhenCalled_ReturnsTheSameNumberOfElements(IEnumerable<long> entities, RatingType type)
        {
            // Arrange
            var expected = ratingRepository.GetAverageRatingForEntities(entities, type);

            // Act
            var result = service.GetAverageRatingForRange(entities, type);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(expected.Count, result.Count);
        }

        [Test]
        [TestCase(new[] { 1L, 2L, 3L }, RatingType.Provider)]
        public void GetAverageRatingForRange_WhenCalled_ReturnsAverageRatingIfExistElseZero(IEnumerable<long> entities, RatingType type)
        {
            // Arrange
            var averageEntities = ratingRepository.GetAverageRatingForEntities(entities, type);
            var expected = new Dictionary<long, Tuple<double, int>>(averageEntities.Count);

            foreach (var entity in averageEntities)
            {
                expected.Add(entity.Key, new Tuple<double, int>((float)Math.Round(entity.Value.Item1, RoundToDigits), entity.Value.Item2));
            }

            // Act
            var result = service.GetAverageRatingForRange(entities, type);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public async Task Create_WhenRatingIsValid_ReturnsCreatedRating()
        {
            // Arrange
            var expected = new Rating()
            {
                Rate = 5,
                ParentId = 5,
                EntityId = 1,
                Type = RatingType.Provider,
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Rate, result.Rate);
            Assert.AreEqual(expected.ParentId, result.ParentId);
            Assert.AreEqual(expected.EntityId, result.EntityId);
            Assert.AreEqual(expected.Type, result.Type);
        }

        [Test]
        public void Create_WhenArgumentIsNull_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await service.Create(null).ConfigureAwait(false));
        }

        [Test]
        public async Task Create_WhenEntityIdNotExist_ReturnsNull()
        {
            // Arrange
            var rating = new Rating()
            {
                Rate = 5,
                ParentId = 50,
                EntityId = 100,
                Type = RatingType.Provider,
            };

            // Act
            var result = await service.Create(rating.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task Create_WhenRatingAlreadyExist_ReturnsNull()
        {
            // Arrange
            var rating = new Rating()
            {
                Rate = 5,
                ParentId = 1,
                EntityId = 1,
                Type = RatingType.Provider,
            };

            // Act
            var result = await service.Create(rating.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task Update_WhenRatingIsValid_ReturnsUpdatedRating()
        {
            // Arrange
            var expected = new Rating()
            {
                Id = 1,
                Rate = 3,
                ParentId = 1,
                EntityId = 1,
                Type = RatingType.Provider,
            };

            // Act
            var result = await service.Update(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
            Assert.AreEqual(expected.Rate, result.Rate);
            Assert.AreEqual(expected.ParentId, result.ParentId);
            Assert.AreEqual(expected.EntityId, result.EntityId);
            Assert.AreEqual(expected.Type, result.Type);
        }

        [Test]
        public void Update_WhenArgumentIsNull_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await service.Update(null).ConfigureAwait(false));
        }

        [Test]
        public async Task Update_WhenEntityIdNotExist_ReturnsNull()
        {
            // Arrange
            var rating = new Rating()
            {
                Id = 1,
                Rate = 5,
                ParentId = 1,
                EntityId = 100,
                Type = RatingType.Provider,
            };

            // Act
            var result = await service.Update(rating.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task Update_WhenRatingNotExist_ReturnsNull()
        {
            // Arrange
            var rating = new Rating()
            {
                Id = 100,
                Rate = 5,
                ParentId = 2,
                EntityId = 2,
                Type = RatingType.Provider,
            };

            // Act
            var result = await service.Update(rating.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
        {
            // Arrange
            var expected = await context.Ratings.CountAsync();

            // Act
            await service.Delete(id).ConfigureAwait(false);

            var result = (await service.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.AreEqual(expected - 1, result);
        }

        [Test]
        [TestCase(10)]
        [TestCase(100)]
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
                    },
                };

                context.Providers.AddRangeAsync(providers);
                context.SaveChangesAsync();

                var ratings = new List<Rating>()
                {
                    new Rating()
                    {
                        Id = 1,
                        Rate = 5,
                        ParentId = 1,
                        EntityId = 1,
                        Type = RatingType.Provider,
                    },
                    new Rating()
                    {
                        Id = 2,
                        Rate = 4,
                        ParentId = 2,
                        EntityId = 2,
                        Type = RatingType.Provider,
                    },
                    new Rating()
                    {
                        Id = 3,
                        Rate = 3,
                        ParentId = 3,
                        EntityId = 1,
                        Type = RatingType.Provider,
                    },
                    new Rating()
                    {
                        Id = 4,
                        Rate = 2,
                        ParentId = 3,
                        EntityId = 1,
                        Type = RatingType.Workshop,
                    },
                };

                context.Ratings.AddRangeAsync(ratings);
                context.SaveChangesAsync();
            }
        }
    }
}
