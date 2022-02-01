using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class DepartmentServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IDepartmentRepository repo;
        private IWorkshopRepository repositoryWorkshop;
        private IDepartmentService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<DepartmentService>> logger;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);

            repo = new DepartmentRepository(context);
            repositoryWorkshop = new WorkshopRepository(context);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger<DepartmentService>>();
            service = new DepartmentService(repo, repositoryWorkshop, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Department()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                DirectionId = 1,
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
            Assert.That(expected.Count(), Is.EqualTo(result.Count()));
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
            var changedEntity = new DepartmentDto()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                DirectionId = 1,
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
            var changedEntity = new DepartmentDto()
            {
                Title = "New",
                DirectionId = 1,
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

            context.Entry<Department>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

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
        [TestCase(2)]
        public async Task Delete_WhenThereAreRelatedWorkshops_ReturnsNotSucceeded(long id)
        {
            // Act
            var result = await service.Delete(id).ConfigureAwait(false);

            // Assert
            Assert.False(result.Succeeded);
            Assert.That(result.OperationResult.Errors, Is.Not.Empty);
        }

        [Test]
        [Order(10)]
        [TestCase(1)]
        public async Task GetByDirectionId_WhenIdIsValid_ReturnsEntities(long id)
        {
            // Arrange
            var expected = await repo.GetAll().ConfigureAwait(false);
            expected = expected.Where(x => x.DirectionId == id);

            // Act
            var entities = await service.GetByDirectionId(id);

            // Assert
            Assert.That(entities.Count(), Is.EqualTo(expected.Count()));
        }

        [Test]
        [Order(11)]
        [TestCase(10)]
        public void GetByDirectionId_WhenIdIsInvalid_ThrowsArgumentException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetByDirectionId(id).ConfigureAwait(false));
        }

        private void SeedDatabase()
        {
            using var ctx = new OutOfSchoolDbContext(options);
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();

                var directions = new List<Direction>()
                {
                   new Direction()
                   {
                       Title = "Test1",
                       Description = "Test1",
                   },
                   new Direction
                   {
                       Title = "Test2",
                       Description = "Test2",
                   },
                   new Direction
                   {
                       Title = "Test3",
                       Description = "Test3",
                   },
                };

                ctx.Directions.AddRangeAsync(directions);

                var departments = new List<Department>()
                {
                   new Department()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       DirectionId = 1,
                   },
                   new Department
                   {
                       Title = "Test2",
                       Description = "Test2",
                       DirectionId = 1,
                   },
                   new Department
                   {
                       Title = "Test3",
                       Description = "Test3",
                       DirectionId = 1,
                   },
                };

                ctx.Departments.AddRangeAsync(departments);

                var workshops = new List<Workshop>()
                {
                   new Workshop()
                   {
                        Title = "Test1",
                        DepartmentId = 2,
                   },
                };

                ctx.Workshops.AddRangeAsync(workshops);

                ctx.SaveChangesAsync();
            }
        }
    }
}
