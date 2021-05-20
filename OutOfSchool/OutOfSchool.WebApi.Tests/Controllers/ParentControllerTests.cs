using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ParentControllerTests
    {
        private ParentController controller;
        private Mock<IParentService> service;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<ParentDTO> parents;
        private ParentDTO parent;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IParentService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new ParentController(service.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            parents = FakeParents();
            parent = FakeParent();
        }

        public IEnumerable<ParentDTO> FakeParents()
        {
            return new List<ParentDTO>
            {
                    new ParentDTO() { Id = 0, FirstName = "Testone", MiddleName = "Testone", LastName = "Testone", UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6" },
                    new ParentDTO() { Id = 1, FirstName = "Testtwo", MiddleName = "Testtwo", LastName = "Testtwo", UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6" },
                    new ParentDTO() { Id = 2, FirstName = "Testthree", MiddleName = "Testthree", LastName = "Testthree", UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6" },
            };
        }

        public ParentDTO FakeParent()
        {
            return new ParentDTO() { Id = 0, FirstName = "Testone", MiddleName = "Testone", LastName = "Testone", UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6" };
        }

        [Test]
        public async Task GetParents_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(parents);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(1)]
        public async Task GetParentsById_WhenIdIsValid_ReturnsOkObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(parents.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(0)]
        public void GetParentById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(parents.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetParentsById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(parents.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task CreateParent_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(parent)).ReturnsAsync(parent);

            // Act
            var result = await controller.Create(parent).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 201);
        }

        [Test]
        public async Task CreatePrarent_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateParent", "Invalid model state.");

            // Act
            var result = await controller.Create(parent).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateParent_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedParent = new ParentDTO()
            {
                Id = 1,
                FirstName = "ChangedName",
            };
            service.Setup(x => x.Update(changedParent)).ReturnsAsync(changedParent);

            // Act
            var result = await controller.Update(changedParent).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task UpdateParent_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateParent", "Invalid model state.");

            // Act
            var result = await controller.Update(parent).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteParent_WhenIdIsValid_ReturnsNoContentResult(long id)
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
        public void DeleteParent_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteParent_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}
