﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ParentServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IParentRepository repo;
        private IParentService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger> logger;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTest")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            repo = new ParentRepository(context);
            logger = new Mock<ILogger>();
            service = new ParentService(repo, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Parent() { FirstName = "John", MiddleName = "Johnovich", LastName = "Johnson", UserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c" };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.FirstName, result.FirstName);
            Assert.AreEqual(expected.MiddleName, result.MiddleName);
            Assert.AreEqual(expected.LastName, result.LastName);
        }

        [Test]
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
            var changedEntity = new ParentDTO() { Id = 1, FirstName = "Changed" };

            // Act
            var result = await service.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(changedEntity.FirstName, Is.EqualTo(result.FirstName));
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new ParentDTO() { FirstName = "Changed" };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedEntity).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
        {
            // Act
            var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            context.Entry<Parent>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

            await service.Delete(id).ConfigureAwait(false);

            var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
        }

        [Test]
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

                var parents = new List<Parent>()
                {
                    new Parent() { Id = 1, FirstName = "Testone", MiddleName = "Testone", LastName = "Testone", UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6" },
                    new Parent() { Id = 2, FirstName = "Testtwo", MiddleName = "Testtwo", LastName = "Testtwo", UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6" },
                    new Parent() { Id = 3, FirstName = "Testthree", MiddleName = "Testthree", LastName = "Testthree", UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6" },
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

                context.Parents.AddRangeAsync(parents);
                context.Users.AddAsync(user);
                context.SaveChangesAsync();
            }
        }
    }
}
