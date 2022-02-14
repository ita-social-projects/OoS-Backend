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
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class DepartmentControllerTests
    {
        private DepartmentController controller;
        private Mock<IDepartmentService> service;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<DepartmentDto> departments;
        private DepartmentDto department;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IDepartmentService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new DepartmentController(service.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            departments = FakeDepartments();
            department = FakeDepartment();
        }

        [Test]
        public async Task Get_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(departments);

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
            service.Setup(x => x.GetById(id)).ReturnsAsync(departments.SingleOrDefault(x => x.Id == id));

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
            service.Setup(x => x.GetById(id)).ReturnsAsync(departments.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(departments.SingleOrDefault(x => x.Id == id));

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
            service.Setup(x => x.Create(department)).ReturnsAsync(department);

            // Act
            var result = await controller.Create(department).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task Create_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateDepartment", "Invalid model state.");

            // Act
            var result = await controller.Create(department).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Update_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedDepartment = new DepartmentDto()
            {
                Id = 1,
                Title = "ChangedTitle",
                DirectionId = 1,
            };
            service.Setup(x => x.Update(changedDepartment)).ReturnsAsync(changedDepartment);

            // Act
            var result = await controller.Update(changedDepartment).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Update_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateDepartment", "Invalid model state.");

            // Act
            var result = await controller.Update(department).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id)).ReturnsAsync(Result<DepartmentDto>.Success(department));

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
            service.Setup(x => x.Delete(id)).ReturnsAsync(Result<DepartmentDto>.Success(department));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(10)]
        public async Task Delete_WhenThereAreRelatedWorkshops_ReturnsBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id)).ReturnsAsync(Result<DepartmentDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "Some workshops assosiated with this department. Deletion prohibited.",
            }));

            // Act
            var result = await controller.Delete(id);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(3)]
        public async Task GetByDirectionId_WhenIdIsInvalid_ReturnsNoContent(long id)
        {
            // Arrange
            service.Setup(x => x.GetByDirectionId(id)).ReturnsAsync(departments.Where(x => x.DirectionId == id));

            // Act
            var result = await controller.GetByDirectionId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(1)]
        public async Task GetByDirectionId_WhenIdIsValid_ReturnsOkObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetByDirectionId(id)).ReturnsAsync(departments.Where(x => x.DirectionId == id));

            // Act
            var result = await controller.GetByDirectionId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(10)]
        public async Task GetByDirectionId_WhenIdIsInvalid_ReturnsBadRequest(long id)
        {
            // Arrange
            service.Setup(x => x.GetByDirectionId(id)).ThrowsAsync(new ArgumentException("message"));

            // Act
            var result = await controller.GetByDirectionId(id).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        private DepartmentDto FakeDepartment()
        {
            return new DepartmentDto()
            {
                Title = "Test1",
                Description = "Test1",
                DirectionId = 1,
            };
        }

        private IEnumerable<DepartmentDto> FakeDepartments()
        {
            return new List<DepartmentDto>()
            {
                   new DepartmentDto()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       DirectionId = 1,
                   },
                   new DepartmentDto
                   {
                       Title = "Test2",
                       Description = "Test2",
                       DirectionId = 1,
                   },
                   new DepartmentDto
                   {
                       Title = "Test3",
                       Description = "Test3",
                       DirectionId = 1,
                   },
            };
        }
    }
}
