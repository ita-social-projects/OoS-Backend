//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.Extensions.Localization;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using OutOfSchool.Services;
//using OutOfSchool.Services.Models;
//using OutOfSchool.Services.Repository;
//using OutOfSchool.WebApi.Extensions;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Services
//{
//    [TestFixture]
//    public class ParentServiceTests
//    {
//        private DbContextOptions<OutOfSchoolDbContext> options;
//        private OutOfSchoolDbContext context;
//        private IParentRepository repoParent;
//        private IEntityRepository<User> repoUser;
//        private IParentService service;
//        private Mock<IStringLocalizer<SharedResource>> localizer;
//        private Mock<ILogger<ParentService>> logger;

//        [SetUp]
//        public void SetUp()
//        {
//            var builder =
//                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
//                    databaseName: "OutOfSchoolTest")
//                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

//            options = builder.Options;
//            context = new OutOfSchoolDbContext(options);
//            localizer = new Mock<IStringLocalizer<SharedResource>>();
//            repoParent = new ParentRepository(context);
//            repoUser = new EntityRepository<User>(context);
//            logger = new Mock<ILogger<ParentService>>();
//            service = new ParentService(repoParent, repoUser, logger.Object, localizer.Object);

//            SeedDatabase();
//        }

//        [Test]
//        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
//        {
//            // Arrange
//            var expected = new Parent() { UserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c" };

//            // Act
//            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.UserId, result.UserId);
//        }

//        [Test]
//        public async Task GetAll_WhenCalled_ReturnsAllEntities()
//        {
//            // Arrange
//            var expected = await repoParent.GetAll();

//            // Act
//            var result = await service.GetAll().ConfigureAwait(false);

//            // Assert
//            Assert.That(expected.ToList().Count(), Is.EqualTo(result.Count()));
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
//        {
//            // Arrange
//            var expected = await repoParent.GetById(id);

//            // Act
//            var result = await service.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.Id, result.Id);
//        }

//        [Test]
//        [TestCase(10)]
//        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
//        {
//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
//                async () => await service.GetById(id).ConfigureAwait(false));
//        }

//        [Test]
//        [TestCase("de909f35-5eb7-4b7a-bda8-40a5bfda96a6")]
//        public async Task GetByUserId_WhenIdIsValid_ReturnsEntities(string id)
//        {
//            // Arrange
//            var expected = await repoParent.GetByFilter(p => p.UserId == id);

//            // Act
//            var result = await service.GetByUserId(id).ConfigureAwait(false);

//            // Assert
//            result.Should().BeEquivalentTo(expected.FirstOrDefault().ToModel());
//        }

//        [Test]
//        [TestCase("fakeString")]
//        public void GetByUserId_WhenIdIsNotValid_TrowsArgumentException(string id)
//        {
//            // Act and Assert
//            service.Invoking(s => s.GetByUserId(id)).Should().ThrowAsync<ArgumentException>();
//        }

//        [Test]
//        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
//        {
//            // Arrange
//            var changedEntity = new ShortUserDto()
//            {
//                Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
//                PhoneNumber = "1160327456",
//                LastName = "LastName",
//                MiddleName = "MiddleName",
//                FirstName = "FirstName",
//            };
//            Expression<Func<User, bool>> filter = p => p.Id == changedEntity.Id;

//            var users = repoUser.GetByFilterNoTracking(filter);

//            // Act
//            var result = await repoUser.Update(changedEntity.ToDomain(users.FirstOrDefault())).ConfigureAwait(false);

//            // Assert
//            Assert.That(changedEntity.FirstName, Is.EqualTo(result.FirstName));
//            Assert.That(changedEntity.LastName, Is.EqualTo(result.LastName));
//            Assert.That(changedEntity.MiddleName, Is.EqualTo(result.MiddleName));
//            Assert.That(changedEntity.PhoneNumber, Is.EqualTo(result.PhoneNumber));
//        }

//        [Test]
//        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
//        {
//            // Arrange
//            var changedEntity = new ShortUserDto()
//            {
//               Id = "Invalid Id",
//               PhoneNumber = "1160327456",
//               LastName = "LastName",
//               MiddleName = "MiddleName",
//               FirstName = "FirstName",
//            };

//            // Act and Assert
//            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
//                async () => await service.Update(changedEntity).ConfigureAwait(false));
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
//        {
//            // Act
//            var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

//            context.Entry<Parent>(await repoParent.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

//            await service.Delete(id).ConfigureAwait(false);

//            var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

//            // Assert
//            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
//        }

//        [Test]
//        [TestCase(10)]
//        public void Delete_WhenIdIsInvalid_ThrowsDbUpdateConcurrencyException(long id)
//        {
//            // Act and Assert
//            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
//                async () => await service.Delete(id).ConfigureAwait(false));
//        }

//        private void SeedDatabase()
//        {
//            using var context = new OutOfSchoolDbContext(options);
//            {
//                context.Database.EnsureDeleted();
//                context.Database.EnsureCreated();

//                var parents = new List<Parent>()
//                {
//                    new Parent() { Id = 1,  UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6" },
//                    new Parent() { Id = 2,  UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6" },
//                    new Parent() { Id = 3,  UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6" },
//                };

//                var user = new User()
//                {
//                    Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
//                    CreatingTime = default,
//                    LastLogin = default,
//                    MiddleName = "MiddleName",
//                    FirstName = "FirstName",
//                    LastName = "LastName",
//                    UserName = "user@gmail.com",
//                    NormalizedUserName = "USER@GMAIL.COM",
//                    Email = "user@gmail.com",
//                    NormalizedEmail = "USER@GMAIL.COM",
//                    EmailConfirmed = false,
//                    PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
//                    SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
//                    ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
//                    PhoneNumber = "0965679725",
//                    Role = "provider",
//                    PhoneNumberConfirmed = false,
//                    TwoFactorEnabled = false,
//                    LockoutEnabled = true,
//                    AccessFailedCount = 0,
//                    IsRegistered = false,
//                };

//                context.Parents.AddRangeAsync(parents);
//                context.Users.AddAsync(user);
//                context.SaveChangesAsync();
//            }
//        }
//    }
//}
