using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    [Obsolete("Full refactor of directions, classes won't be needed in current implementation")]
    [Ignore("Full refactor of directions, classes won't be needed in current implementation")]
    public class ClassServiceTests
    {
        private Mock<IClassRepository> repo;
        private Mock<IWorkshopRepository> repositoryWorkshop;
        private IClassService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<ClassService>> logger;
        private Mock<IMapper> mapper;

        [SetUp]
        public void SetUp()
        {
            repo = new Mock<IClassRepository>();
            repositoryWorkshop = new Mock<IWorkshopRepository>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger<ClassService>>();
            mapper = new Mock<IMapper>();
            service = new ClassService(
                repo.Object,
                repositoryWorkshop.Object,
                logger.Object,
                localizer.Object,
                mapper.Object);
        }

        [Test]
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
            mapper.Setup(m => m.Map<ClassDto>(It.IsAny<Class>())).Returns(input);
            repo.Setup(r => r.Create(expected)).ReturnsAsync(expected);

            // Act
            var result = await service.Create(input).ConfigureAwait(false);

            // Assert
            repo.Verify(r => r.Create(expected), Times.Once);
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.Description, result.Description);
        }

        [Test]
        public async Task Create_NotUniqueEntity_ReturnsArgumentException()
        {
            // Arrange
            var input = new ClassDto()
            {
                Title = "Test1",
                Description = "Test1",
                DepartmentId = 1,
            };
            var mockDbResponse = new List<Class>()
            {
                new Class()
                {
                    Title = "Test1",
                    Description = "Test1",
                    DepartmentId = 1,
                },
            }.AsTestAsyncEnumerableQuery();

            repo.Setup(r => r.Get<int>(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Class, bool>>>(),
                null,
                true,
                false))
                .Returns(mockDbResponse);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(input).ConfigureAwait(false));
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllEntities()
        {
            // Arrange
            var expectedEntity = new List<Class>()
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
            var expectedDto = new List<ClassDto>()
            {
                new ClassDto()
                {
                    Title = "Test1",
                    Description = "Test1",
                    DepartmentId = 1,
                },
                new ClassDto
                {
                    Title = "Test2",
                    Description = "Test2",
                    DepartmentId = 1,
                },
                new ClassDto
                {
                    Title = "Test3",
                    Description = "Test3",
                    DepartmentId = 1,
                },
            };

            repo.Setup(r => r.GetAll()).ReturnsAsync(expectedEntity);
            mapper.Setup(m => m.Map<List<ClassDto>>(expectedEntity)).Returns(expectedDto);

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            repo.Verify(r => r.GetAll(), Times.Once);
            Assert.That(expectedEntity.Count(), Is.EqualTo(result.Count()));
        }

        [Test]
        [TestCase(1)]
        public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
        {
            // Arrange
            var expected = new ClassDto()
            {
                Title = "Test1",
                Description = "Test1",
                DepartmentId = id,
            };
            var mockDbEntry = new Class()
            {
                Title = "Test1",
                Description = "Test1",
                DepartmentId = id,
            };

            repo.Setup(r => r.GetById(id)).ReturnsAsync(mockDbEntry);
            mapper.Setup(m => m.Map<ClassDto>(mockDbEntry)).Returns(expected);

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(100)]
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
            var changedEntity = new Class()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                DepartmentId = 1,
            };
            var changedDto = new ClassDto()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                DepartmentId = 1,
            };

            mapper.Setup(m => m.Map<ClassDto>(changedEntity)).Returns(changedDto);
            mapper.Setup(m => m.Map<Class>(changedDto)).Returns(changedEntity);
            repo.Setup(r => r.Update(changedEntity)).ReturnsAsync(changedEntity);

            // Act
            var result = await service.Update(changedDto).ConfigureAwait(false);

            // Assert
            repo.Verify(r => r.Update(changedEntity), Times.Once);
            Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new Class()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                DepartmentId = 1,
            };
            var changedDto = new ClassDto()
            {
                Id = 1,
                Title = "ChangedTitle1",
                Description = "Bla-bla",
                DepartmentId = 1,
            };
            mapper.Setup(m => m.Map<Class>(changedDto)).Returns(changedEntity);
            repo.Setup(r => r.Update(changedEntity)).Throws<DbUpdateConcurrencyException>();

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedDto).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
        {
            // Arrange
            var deleted = new ClassDto()
            {
                Id = id,
            };
            repositoryWorkshop.Setup(w => w.GetByFilter(workshop => workshop.ClassId == id, It.IsAny<string>()))
                .ReturnsAsync(new List<Workshop>());
            mapper.Setup(m => m.Map<ClassDto>(It.IsAny<Class>())).Returns(deleted);

            // Act
            var result = await service.Delete(id).ConfigureAwait(false);

            // Assert
            repo.Verify(r => r.Delete(It.IsAny<Class>()), Times.Once);
            Assert.True(result.Succeeded);
        }

        [Test]
        [TestCase(10)]
        public void Delete_WhenIdIsInvalid_ThrowsDbUpdateConcurrencyException(long id)
        {
            // Arrange
            repositoryWorkshop.Setup(w => w.GetByFilter(workshop => workshop.ClassId == id, It.IsAny<string>()))
                .ReturnsAsync(new List<Workshop>());
            repo.Setup(r => r.Delete(It.IsAny<Class>())).Throws<DbUpdateConcurrencyException>();

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(2)]
        public async Task Delete_WhenThereAreRelatedWorkshops_ReturnsNotSucceeded(long id)
        {
            // Arrange
            repositoryWorkshop.Setup(w => w.GetByFilter(workshop => workshop.ClassId == id, It.IsAny<string>()))
                .ReturnsAsync(new List<Workshop>()
                {
                    new Workshop()
                    {
                        Title = "Test1",
                        ClassId = 1,
                    },
                });

            // Act
            var result = await service.Delete(id).ConfigureAwait(false);

            // Assert
            repo.Verify(r => r.Delete(It.IsAny<Class>()), Times.Never);
            Assert.False(result.Succeeded);
            Assert.That(result.OperationResult.Errors, Is.Not.Empty);
        }

        [Test]
        [TestCase(1)]
        public async Task GetByDepartmentId_WhenIdIsValid_ReturnsEntities(long id)
        {
            // Arrange
            var expectedEntity = new List<Class>()
            {
                new Class()
                {
                    Title = "Test1",
                    Description = "Test1",
                    DepartmentId = id,
                },
                new Class
                {
                    Title = "Test2",
                    Description = "Test2",
                    DepartmentId = id,
                },
                new Class
                {
                    Title = "Test3",
                    Description = "Test3",
                    DepartmentId = id,
                },
            };
            var expectedDto = new List<ClassDto>()
            {
                new ClassDto()
                {
                    Title = "Test1",
                    Description = "Test1",
                    DepartmentId = id,
                },
                new ClassDto
                {
                    Title = "Test2",
                    Description = "Test2",
                    DepartmentId = id,
                },
                new ClassDto
                {
                    Title = "Test3",
                    Description = "Test3",
                    DepartmentId = id,
                },
            };

            repo.Setup(r => r.Get<int>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Class, bool>>>(),
                    null,
                    true,
                    false))
                .Returns(expectedEntity.AsTestAsyncEnumerableQuery());
            repo.Setup(r => r.DepartmentExists(id)).Returns(true);
            mapper.Setup(m => m.Map<List<ClassDto>>(It.IsAny<List<Class>>())).Returns(expectedDto);

            // Act
            var entities = await service.GetByDepartmentId(id).ConfigureAwait(false);

            // Assert
            repo.Verify(
                r => r.Get<int>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Class, bool>>>(),
                    null,
                    true,
                    false), Times.Once);
            Assert.That(entities.Count(), Is.EqualTo(expectedEntity.Count()));
        }

        [Test]
        [TestCase(10)]
        public void GetByDepartmentId_WhenIdIsInvalid_ThrowsArgumentException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.GetByDepartmentId(id).ConfigureAwait(false));
        }
    }
}