using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private ChildController controller;
        private Mock<IChildService> service;
        private Mock<IEntityRepository<Child>> repo;

        private IEnumerable<ChildDTO> children;
        private ChildDTO child;

        [SetUp]
        public void Setup()
        {
            repo = new Mock<IEntityRepository<Child>>();
            service = new Mock<IChildService>();
            controller = new ChildController(service.Object);

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
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
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
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(0)]
        public void GetChildById_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(children.SingleOrDefault(x => x.Id == id));

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
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
            Assert.AreEqual(result.Value, null);
        }

        [Test]
        public async Task CreateChild_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(child)).ReturnsAsync(child);

            // Act 
            var result = await controller.Create(child).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 201);
        }

        [Test]
        public async Task CreateChild_WhenModelIsNotValid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            var newChild = new ChildDTO()
            {
                FirstName = string.Empty,
                LastName = "ln1",
                Patronymic = "mn1",
                DateOfBirth = new DateTime(2003, 11, 9),
                Gender = Gender.Male,
                ParentId = 1,
                SocialGroupId = 2,
            };

            service.Setup(x => x.Create(newChild)).ReturnsAsync(newChild);
            controller.ModelState.AddModelError("CreateChild", "Invalid model state.");

            // Act 
            var result = await controller.Create(newChild).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateChild_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            var changedChild = new ChildDTO()
            {
                Id = 1,
                FirstName = "fn11",
                LastName = "ln11",
            };
            service.Setup(x => x.Update(changedChild)).ReturnsAsync(changedChild);

            // Act 
            var result = await controller.Update(changedChild).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task UpdateChild_WhenModelIsNotValid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            var changedChild = new ChildDTO()
            {
                Id = 1,
                FirstName = string.Empty,
                LastName = "ln11",
            };

            service.Setup(x => x.Update(changedChild)).ReturnsAsync(changedChild);
            controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

            // Act 
            var result = await controller.Update(changedChild).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
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
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 204);
        }

        [Test]
        [TestCase(0)]
        public void DeleteChild_WhenIdIsNotValid_ShouldReturnBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

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

        private ChildDTO FakeChild()
        {
            return new ChildDTO()
            {
                Id = 1,
                FirstName = "fn1",
                LastName = "ln1",
                Patronymic = "mn1",
                DateOfBirth = new DateTime(2003, 11, 9),
                Gender = Gender.Male,
                ParentId = 1,
                SocialGroupId = 2
            };
        }

        private IEnumerable<ChildDTO> FakeChildren()
        {
            return new List<ChildDTO>()
            {
                new ChildDTO()
                {
                    Id = 1,
                    FirstName = "fn1",
                    LastName = "ln1",
                    Patronymic = "mn1",
                    DateOfBirth = new DateTime(2003, 11, 9),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 2,
                },
                new ChildDTO()
                {
                    Id = 2,
                    FirstName = "fn2",
                    LastName = "ln2",
                    Patronymic = "mn2",
                    DateOfBirth = new DateTime(2004, 11, 8),
                    Gender = Gender.Female,
                    ParentId = 2,
                    SocialGroupId = 1,
                },
                new ChildDTO()
                {
                    Id = 3,
                    FirstName = "fn3",
                    LastName = "ln3",
                    Patronymic = "mn3",
                    DateOfBirth = new DateTime(2006, 11, 2),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 1,
                },
            };
        }
    }
}
