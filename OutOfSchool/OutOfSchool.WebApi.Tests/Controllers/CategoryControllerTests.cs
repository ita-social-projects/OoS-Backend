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
    public class CategoryControllerTests
    {
        private CategoryController controller;
        private Mock<ICategoryService> service;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<CategoryDTO> categories;
        private CategoryDTO category;

        [SetUp]
        public void Setup()
        {
            service = new Mock<ICategoryService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new CategoryController(service.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            categories = FakeCategories();
            category = FakeCategory();
        }

        [Test]
        public async Task GetCategories_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(categories);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(1)]
        public async Task GetCategoriesById_WhenIdIsValid_ReturnsOkObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(-1)]
        public void GetCategoriesById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetCategoriesById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task CreateCategory_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(category)).ReturnsAsync(category);

            // Act
            var result = await controller.Create(category).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 201);
        }

        [Test]
        public async Task CreateCategory_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
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
        public async Task UpdateCategory_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedProvider = new CategoryDTO()
            {
                Id = 1,
                Title = "ChangedTitle",
            };
            service.Setup(x => x.Update(changedProvider)).ReturnsAsync(changedProvider);

            // Act
            var result = await controller.Update(changedProvider).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task UpdateCategory_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
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
        public async Task DeleteCategory_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 204);
        }

        [Test]
        [TestCase(0)]
        public void DeleteCategory_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteCategory_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        private CategoryDTO FakeCategory()
        {
            return new CategoryDTO()
            {
                Title = "Test1",
                Description = "Test1",
            };
        }

        private IEnumerable<CategoryDTO> FakeCategories()
        {
            return new List<CategoryDTO>()
            {
                   new CategoryDTO()
                   {
                       Title = "Test1",
                       Description = "Test1",
                   },
                   new CategoryDTO
                   {
                       Title = "Test2",
                       Description = "Test2",
                   },
                   new CategoryDTO
                   {
                       Title = "Test3",
                       Description = "Test3",
                   },
            };
        }
    }
}
