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
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ClassServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IClassRepository repo;
        private IWorkshopRepository repositoryWorkshop;
        private IClassService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<ClassService>> logger;
        private Mock<IMapper> mapper;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);

            repo = new ClassRepository(context);
            repositoryWorkshop = new WorkshopRepository(context);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger<ClassService>>();
            mapper = new Mock<IMapper>();
            service = new ClassService(repo, repositoryWorkshop, logger.Object, localizer.Object, mapper.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new Class()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                DepartmentId = 1,
            };
            var input = new ClassDto()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                DepartmentId = 1,
            };

            mapper.Setup(m => m.Map<Class>(It.IsAny<ClassDto>())).Returns(expected);
            // mapper.Setup(m => m.Map<ClassDto>(It.IsAny<Class>())).Returns(input);

            // Act
            var result = await service.Create(input).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.Description, result.Description);
        }

        [Test]
        [Order(2)]
        public async Task Create_NotUniqueEntity_ReturnsArgumentException()
        {
            // Arrange
            // TODO: Make independent test
            var expected = (await repo.GetAll()).FirstOrDefault();
            var input = new ClassDto()
            {
                Title = expected.Title,
                Description = expected.Description,
                DepartmentId = expected.DepartmentId,
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(input).ConfigureAwait(false));
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
            var changedEntity = new ClassDto()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                DepartmentId = 1,
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
            var changedEntity = new ClassDto()
            {
                Title = "NewTitle1",
                DepartmentId = 1,
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

            context.Entry<Class>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

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
        public async Task GetByDepartmentId_WhenIdIsValid_ReturnsEntities(long id)
        {
            // Arrange
            var expected = await repo.GetAll().ConfigureAwait(false);
            expected = expected.Where(x => x.DepartmentId == id);

            // Act
            var entities = await service.GetByDepartmentId(id);

            // Assert
            Assert.That(entities.Count(), Is.EqualTo(expected.Count()));
        }

        [Test]
        [Order(11)]
        [TestCase(10)]
        public void GetByDepartmentId_WhenIdIsInvalid_ThrowsArgumentException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetByDepartmentId(id).ConfigureAwait(false));
        }

        private void SeedDatabase()
        {
            using var ctx = new OutOfSchoolDbContext(options);
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();

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

                var classes = new List<Class>()
                {
                   new Class()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       DepartmentId = 1,
                   },
                   new Class
                   {
                       Title = "Test2",
                       Description = "Test2",
                       DepartmentId = 1,
                   },
                   new Class
                   {
                       Title = "Test3",
                       Description = "Test3",
                       DepartmentId = 1,
                   },
                };

                ctx.Classes.AddRangeAsync(classes);

                var workshops = new List<Workshop>()
                {
                   new Workshop()
                   {
                        Title = "Test1",
                        ClassId = 2,
                   },
                };

                ctx.Workshops.AddRangeAsync(workshops);

                ctx.SaveChangesAsync();
            }
        }
    }
}
