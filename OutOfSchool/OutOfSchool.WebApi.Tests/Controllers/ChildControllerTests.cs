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
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ChildControllerTests
    {
        private const int OkStatusCode = 200;
        private const int NoContentStatusCode = 204;
        private const int CreateStatusCode = 201;
        private const int BadRequestStatusCode = 400;
        private ChildController controller;
        private Mock<IChildService> service;
        private Mock<IEntityRepository<Child>> repo;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private IEnumerable<ChildDto> children;
        private ChildDto child;

        [SetUp]
        public void Setup()
        {
            repo = new Mock<IEntityRepository<Child>>();
            service = new Mock<IChildService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            controller = new ChildController(service.Object, localizer.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[] { new Claim("sub", "3341c870-5ef4-462b-8c86-b4e8bd4e6d41") }, "sub"));
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            children = FakeChildren();
            child = FakeChild();
        }

        [Test]
        public async Task GetChildren_WhenCalled_ShouldReturnOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(children);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetChildById_WhenIdIsValid_ShouldReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(children.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void GetChildById_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.GetById(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task GetChildById_WhenIdIsNotValid_ShouldReturnNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(children.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
            Assert.AreEqual(result.Value, null);
        }

        [Test]
        [TestCase(1, "3341c870-5ef4-462b-8c86-b4e8bd4e6d41")]
        public async Task GetByParentId_WhenIdIsValid_ShouldReturnOkResultObject(long id, string userId)
        {
            // Arrange
            service.Setup(x => x.GetAllByParent(id, userId)).ReturnsAsync(children.Where(p => p.ParentId == id && p.Parent.UserId == userId));

            // Act
            var result = await controller.GetByParentId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void GetByParentId_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.GetByParentId(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10, "aab42f43-f5d6-48ed-95aa-0b4f7b77e541")]
        public async Task GetByParentId_WhenIdIsNotValid_ShouldReturnNull(long id, string userId)
        {
            // Arrange
            service.Setup(x => x.GetAllByParent(id, userId)).ReturnsAsync(children.Where(p => p.ParentId == id && p.Parent.UserId == userId));

            // Act
            var result = await controller.GetByParentId(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
        }

        [Test]
        public async Task CreateChild_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(child)).ReturnsAsync(child);

            // Act
            var result = await controller.Create(child).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CreateStatusCode, result.StatusCode);
        }

        [Test]
        public async Task CreateChild_WhenModelIsNotValid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateChild", "Invalid model state.");

            // Act
            var result = await controller.Create(child).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
        }

        [Test]
        public async Task UpdateChild_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            var changedChild = new ChildDto()
            {
                Id = 1,
                FirstName = "fn11",
                LastName = "ln11",
            };
            service.Setup(x => x.Update(changedChild)).ReturnsAsync(changedChild);

            // Act
            var result = await controller.Update(changedChild).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        public async Task UpdateChild_WhenModelIsNotValid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateChild", "Invalid model state.");

            // Act
            var result = await controller.Update(child).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteChild_WhenIdIsValid_ShouldReturnNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void DeleteChild_WhenIdIsNotValid_ShouldReturnBadRequestObjectResult(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.Delete(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteChild_WhenIdIsNotValid_ShouldReturnNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        private ChildDto FakeChild()
        {
            return new ChildDto()
            {
                Id = 1,
                FirstName = "fn1",
                LastName = "ln1",
                MiddleName = "mn1",
                DateOfBirth = new DateTime(2003, 11, 9),
                Gender = Gender.Male,
                ParentId = 1,
                SocialGroupId = 2,
            };
        }

        private IEnumerable<ChildDto> FakeChildren()
        {
            var parent1 = new ParentDTO() { Id = 1, UserId = "3341c870-5ef4-462b-8c86-b4e8bd4e6d41" };
            var parent2 = new ParentDTO() { Id = 2, UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6" };

            return new List<ChildDto>()
            {
                new ChildDto()
                {
                    Id = 1,
                    FirstName = "fn1",
                    LastName = "ln1",
                    MiddleName = "mn1",
                    DateOfBirth = new DateTime(2003, 11, 9),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 2,
                    Parent = parent1,
                },
                new ChildDto()
                {
                    Id = 2,
                    FirstName = "fn2",
                    LastName = "ln2",
                    MiddleName = "mn2",
                    DateOfBirth = new DateTime(2004, 11, 8),
                    Gender = Gender.Female,
                    ParentId = 2,
                    SocialGroupId = 1,
                    Parent = parent2,
                },
                new ChildDto()
                {
                    Id = 3,
                    FirstName = "fn3",
                    LastName = "ln3",
                    MiddleName = "mn3",
                    DateOfBirth = new DateTime(2006, 11, 2),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 1,
                    Parent = parent1,
                },
            };
        }
    }
}
