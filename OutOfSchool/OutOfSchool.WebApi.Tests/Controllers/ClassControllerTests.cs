using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ClassControllerTests
    {
        private ClassController controller;
        private Mock<IClassService> service;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<ClassDto> classes;
        private ClassDto classEntity;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IClassService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new ClassController(service.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            classes = FakeClasses();
            classEntity = FakeClass();
        }

        [Test]
        public async Task Get_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(classes);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetById_WhenIdIsValid_ReturnsOkObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(classes.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(-1)]
        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(classes.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(classes.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Create_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(classEntity)).ReturnsAsync(classEntity);

            // Act
            var result = await controller.Create(classEntity).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task Create_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateClass", "Invalid model state.");

            // Act
            var result = await controller.Create(classEntity).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Update_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedClass = new ClassDto()
            {
                Id = 1,
                Title = "ChangedTitle",
                DepartmentId = 1,
            };
            service.Setup(x => x.Update(changedClass)).ReturnsAsync(changedClass);

            // Act
            var result = await controller.Update(changedClass).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Update_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateClass", "Invalid model state.");

            // Act
            var result = await controller.Update(classEntity).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void Delete_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task Delete_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(3)]
        public async Task GetByDepartmentId_WhenIdIsInvalid_ReturnsNoContent(long id)
        {
            // Arrange
            service.Setup(x => x.GetByDepartmentId(id)).ReturnsAsync(classes.Where(x => x.DepartmentId == id));

            // Act
            var result = await controller.GetByDepartmentId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(1)]
        public async Task GetByDepartmentId_WhenIdIsValid_ReturnsOkObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetByDepartmentId(id)).ReturnsAsync(classes.Where(x => x.DepartmentId == id));

            // Act
            var result = await controller.GetByDepartmentId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(10)]
        public async Task GetByDepartmentId_WhenIdIsInvalid_ReturnsBadRequest(long id)
        {
            // Arrange
            service.Setup(x => x.GetByDepartmentId(id)).ThrowsAsync(new ArgumentException("message"));

            // Act
            var result = await controller.GetByDepartmentId(id).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        private ClassDto FakeClass()
        {
            return new ClassDto()
            {
                Title = "Test1",
                Description = "Test1",
                DepartmentId = 1,
            };
        }

        private IEnumerable<ClassDto> FakeClasses()
        {
            return new List<ClassDto>()
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
        }
    }
}
