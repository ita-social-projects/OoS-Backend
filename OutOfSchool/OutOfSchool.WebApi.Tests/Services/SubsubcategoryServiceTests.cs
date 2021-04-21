﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    public class SubsubcategoryServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private ISubsubcategoryRepository repo;
        private ISubsubcategoryService service;
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

            repo = new SubsubcategoryRepository(context);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger>();
            service = new SubsubcategoryService(repo, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Subsubcategory()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                SubcategoryId = 1,
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.Description, result.Description);
        }

        [Test]
        [Order(2)]
        public async Task Create_NotUniqueEntity_ReturnsArgumentException()
        {
            // Arrange
            var expected = (await repo.GetAll()).FirstOrDefault();

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
        [TestCase(100)]
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
            var changedEntity = new SubsubcategoryDTO()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                SubcategoryId = 1,
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
            var changedEntity = new SubsubcategoryDTO()
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

            context.Entry<Subsubcategory>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

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

        [Test]
        [Order(10)]
        [TestCase(1)]
        public async Task GetBySubcategoryId_WhenIdIsValid_ReturnsEntities(long id)
        {
            // Arrange
            var expected = await repo.GetAll().ConfigureAwait(false);
            expected = expected.Where(x => x.SubcategoryId == id);

            // Act
            var entities = await service.GetBySubcategoryId(id);

            // Assert
            Assert.That(entities.ToList().Count(), Is.EqualTo(expected.Count()));
        }

        [Test]
        [Order(11)]
        [TestCase(10)]
        public void GetBySubcategoryId_WhenIdIsInvalid_ThrowsArgumentException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetBySubcategoryId(id).ConfigureAwait(false));
        }

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var subcategories = new List<Subcategory>()
                {
                   new Subcategory()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       CategoryId = 1,
                   },
                   new Subcategory
                   {
                       Title = "Test2",
                       Description = "Test2",
                       CategoryId = 1,
                   },
                   new Subcategory
                   {
                       Title = "Test3",
                       Description = "Test3",
                       CategoryId = 1,
                   },
                };

                context.Subcategories.AddRangeAsync(subcategories);

                var subsubcategories = new List<Subsubcategory>()
                {
                   new Subsubcategory()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       SubcategoryId = 1,
                   },
                   new Subsubcategory
                   {
                       Title = "Test2",
                       Description = "Test2",
                       SubcategoryId = 1,
                   },
                   new Subsubcategory
                   {
                       Title = "Test3",
                       Description = "Test3",
                       SubcategoryId = 1,
                   },
                };

                context.Subsubcategories.AddRangeAsync(subsubcategories);

                context.SaveChangesAsync();
            }
        }
    }
}
