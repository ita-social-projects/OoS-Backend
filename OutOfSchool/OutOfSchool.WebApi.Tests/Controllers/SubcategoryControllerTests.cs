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
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class SubcategoryControllerTests
    {
        private SubcategoryController controller;
        private Mock<ISubcategoryService> service;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<SubcategoryDTO> categories;
        private SubcategoryDTO category;

        [SetUp]
        public void Setup()
        {
            service = new Mock<ISubcategoryService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new SubcategoryController(service.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            categories = FakeSubcategories();
            category = FakeSubcategory();
        }

        [Test]
        public async Task GetSubcategories_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(categories);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetSubcategoriesById_WhenIdIsValid_ReturnsOkObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(-1)]
        public void GetSubcategoriesById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetSubcategoriesById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task CreateSubcategory_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(category)).ReturnsAsync(category);

            // Act
            var result = await controller.Create(category).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task CreateSubcategory_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateCategory", "Invalid model state.");

            // Act
            var result = await controller.Create(category).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateSubcategory_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedSubcategory = new SubcategoryDTO()
            {
                Id = 1,
                Title = "ChangedTitle",
                CategoryId = 1,
            };
            service.Setup(x => x.Update(changedSubcategory)).ReturnsAsync(changedSubcategory);

            // Act
            var result = await controller.Update(changedSubcategory).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task UpdateSubcategory_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateCategory", "Invalid model state.");

            // Act
            var result = await controller.Update(category).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteSubcategory_WhenIdIsValid_ReturnsNoContentResult(long id)
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
        public void DeleteSubcategory_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteSubcategory_WhenIdIsInvalid_ReturnsNull(long id)
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
        public async Task GetByCategoryId_WhenIdIsInvalid_ReturnsNoContent(long id)
        {
            // Arrange
            service.Setup(x => x.GetByCategoryId(id)).ReturnsAsync(categories.Where(x => x.CategoryId == id));

            // Act
            var result = await controller.GetByCategoryId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(1)]
        public async Task GetByCategoryId_WhenIdIsValid_ReturnsOkObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetByCategoryId(id)).ReturnsAsync(categories.Where(x => x.CategoryId == id));

            // Act
            var result = await controller.GetByCategoryId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(10)]
        public async Task GetByCategoryId_WhenIdIsInvalid_ReturnsBadRequest(long id)
        {
            // Arrange
            service.Setup(x => x.GetByCategoryId(id)).ThrowsAsync(new ArgumentException("message"));

            // Act
            var result = await controller.GetByCategoryId(id).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        private SubcategoryDTO FakeSubcategory()
        {
            return new SubcategoryDTO()
            {
                Title = "Test1",
                Description = "Test1",
                CategoryId = 1,
            };
        }

        private IEnumerable<SubcategoryDTO> FakeSubcategories()
        {
            return new List<SubcategoryDTO>()
            {
                   new SubcategoryDTO()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       CategoryId = 1,
                   },
                   new SubcategoryDTO
                   {
                       Title = "Test2",
                       Description = "Test2",
                       CategoryId = 1,
                   },
                   new SubcategoryDTO
                   {
                       Title = "Test3",
                       Description = "Test3",
                       CategoryId = 1,
                   },
            };
        }
    }
}
