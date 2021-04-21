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
    public class SubsubcategoryControllerTests
    {
            private SubsubcategoryController controller;
            private Mock<ISubsubcategoryService> service;
            private ClaimsPrincipal user;
            private Mock<IStringLocalizer<SharedResource>> localizer;

            private IEnumerable<SubsubcategoryDTO> categories;
            private SubsubcategoryDTO category;

            [SetUp]
            public void Setup()
            {
                service = new Mock<ISubsubcategoryService>();
                localizer = new Mock<IStringLocalizer<SharedResource>>();

                controller = new SubsubcategoryController(service.Object, localizer.Object);
                user = new ClaimsPrincipal(new ClaimsIdentity());
                controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

                categories = FakeSubsubcategories();
                category = FakeSubsubcategory();
            }

            [Test]
            public async Task GetSubsubcategories_WhenCalled_ReturnsOkResultObject()
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
            public async Task GetSubsubcategoriesById_WhenIdIsValid_ReturnsOkObjectResult(long id)
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
            public void GetSubsubcategoriesById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
            {
                // Arrange
                service.Setup(x => x.GetById(id)).ReturnsAsync(categories.SingleOrDefault(x => x.Id == id));

                // Act and Assert
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    async () => await controller.GetById(id).ConfigureAwait(false));
            }

            [Test]
            [TestCase(10)]
            public async Task GetSubsubcategoriesById_WhenIdIsInvalid_ReturnsNull(long id)
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
            public async Task CreateSubsubcategory_WhenModelIsValid_ReturnsCreatedAtActionResult()
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
            public async Task CreateSubsubcategory_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
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
            public async Task UpdateSubsubcategory_WhenModelIsValid_ReturnsOkObjectResult()
            {
                // Arrange
                var changedSubsubcategory = new SubsubcategoryDTO()
                {
                    Id = 1,
                    Title = "ChangedTitle",
                    SubcategoryId = 1,
                };
                service.Setup(x => x.Update(changedSubsubcategory)).ReturnsAsync(changedSubsubcategory);

                // Act
                var result = await controller.Update(changedSubsubcategory).ConfigureAwait(false) as OkObjectResult;

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.AreEqual(result.StatusCode, 200);
            }

            [Test]
            public async Task UpdateSubsubcategory_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
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
            public async Task DeleteSubsubcategory_WhenIdIsValid_ReturnsNoContentResult(long id)
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
            public void DeleteSubsubcategory_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
            {
                // Arrange
                service.Setup(x => x.Delete(id));

                // Act and Assert
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    async () => await controller.Delete(id).ConfigureAwait(false));
            }

            [Test]
            [TestCase(10)]
            public async Task DeleteSubsubcategory_WhenIdIsInvalid_ReturnsNull(long id)
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
                service.Setup(x => x.GetBySubcategoryId(id)).ReturnsAsync(categories.Where(x => x.SubcategoryId == id));

                // Act
                var result = await controller.GetBySubcategoryId(id).ConfigureAwait(false) as OkObjectResult;

                // Assert
                Assert.That(result, Is.Null);
            }

            [Test]
            [TestCase(1)]
            public async Task GetByCategoryId_WhenIdIsValid_ReturnsOkObject(long id)
            {
                // Arrange
                service.Setup(x => x.GetBySubcategoryId(id)).ReturnsAsync(categories.Where(x => x.SubcategoryId == id));

                // Act
                var result = await controller.GetBySubcategoryId(id).ConfigureAwait(false) as OkObjectResult;

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.AreEqual(result.StatusCode, 200);
            }

            [Test]
            [TestCase(10)]
            public async Task GetByCategoryId_WhenIdIsInvalid_ReturnsBadRequest(long id)
            {
                // Arrange
                service.Setup(x => x.GetBySubcategoryId(id)).ThrowsAsync(new ArgumentException("message"));

                // Act
                var result = await controller.GetBySubcategoryId(id).ConfigureAwait(false);

                // Assert
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
            }

            private SubsubcategoryDTO FakeSubsubcategory()
            {
                return new SubsubcategoryDTO()
                {
                    Title = "Test1",
                    Description = "Test1",
                    SubcategoryId = 1,
                };
            }

            private IEnumerable<SubsubcategoryDTO> FakeSubsubcategories()
            {
                return new List<SubsubcategoryDTO>()
            {
                   new SubsubcategoryDTO()
                   {
                       Title = "Test1",
                       Description = "Test1",
                       SubcategoryId = 1,
                   },
                   new SubsubcategoryDTO
                   {
                       Title = "Test2",
                       Description = "Test2",
                       SubcategoryId = 1,
                   },
                   new SubsubcategoryDTO
                   {
                       Title = "Test3",
                       Description = "Test3",
                       SubcategoryId = 1,
                   },
            };
        }
    }
}
