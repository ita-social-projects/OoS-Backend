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
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ChildControllerTests
    {
        private ChildController controller;
        private Mock<IChildService> service;
        private List<ChildDto> children;
        private ChildDto child;
        private string currentUserId;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IChildService>();
            controller = new ChildController(service.Object);

            currentUserId = "3341c870-5ef4-462b-8c86-b4e8bd4e6d41";
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[] { new Claim("sub", currentUserId) }, "sub"));

            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            children = FakeChildren();
            child = FakeChild();
        }

        [Test]
        public async Task GetChildren_WhenThereAreChildren_ShouldReturnOkResultObject()
        {
            // Arrange
            var filter = new OffsetFilter();
            service.Setup(x => x.GetAllWithOffsetFilterOrderedById(filter)).ReturnsAsync(new SearchResult<ChildDto>() { TotalAmount = children.Count(), Entities = children });

            // Act
            var result = await controller.GetAllForAdmin(filter).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetChildren_WhenThereIsNoChild_ShouldReturnOkObjectResult()
        {
            // Arrange
            var filter = new OffsetFilter();
            service.Setup(x => x.GetAllWithOffsetFilterOrderedById(filter)).ReturnsAsync(new SearchResult<ChildDto>() { Entities = new List<ChildDto>() });

            // Act
            var result = await controller.GetAllForAdmin(filter).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(1)]
        public async Task GetUsersChilById_WhenChildWasFound_ShouldReturnOkResultObject(long id)
        {
            // Arrange
            var filter = new OffsetFilter();
            service.Setup(x => x.GetByIdAndUserId(id, It.IsAny<string>())).ReturnsAsync(children.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetUsersChildById(id).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(10)]
        public async Task GetUsersChilById_WhenChildWasNotFound_ShouldReturnOkObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.GetByIdAndUserId(id, It.IsAny<string>())).ReturnsAsync(children.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetUsersChildById(id).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(1)]
        public async Task GetByParentId_WhenThereAreChildren_ShouldReturnOkResultObject(long id)
        {
            // Arrange
            var filter = new OffsetFilter();
            service.Setup(x => x.GetByParentIdOrderedByFirstName(id, filter))
                .ReturnsAsync(new SearchResult<ChildDto>() { TotalAmount = children.Where(p => p.ParentId == id).Count(), Entities = children.Where(p => p.ParentId == id).ToList() });

            // Act
            var result = await controller.GetByParentIdForAdmin(id, filter).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(10)]
        public async Task GetByParentId_WhenThereIsNoChild_ShouldReturnOkObjectResult(long id)
        {
            // Arrange
            var filter = new OffsetFilter();
            service.Setup(x => x.GetByParentIdOrderedByFirstName(id, filter)).ReturnsAsync(new SearchResult<ChildDto>() { Entities = children.Where(p => p.ParentId == id).ToList() });

            // Act
            var result = await controller.GetByParentIdForAdmin(id, filter).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CreateChild_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.CreateChildForUser(child, currentUserId)).ReturnsAsync(child);

            // Act
            var result = await controller.Create(child).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
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
            service.Setup(x => x.UpdateChildCheckingItsUserIdProperty(changedChild, It.IsAny<string>())).ReturnsAsync(changedChild);

            // Act
            var result = await controller.Update(changedChild).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(1)]
        public async Task DeleteChild_WhenChildWithIdExists_ShouldReturnNoContentResult(long id)
        {
            // Act
            var result = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [TestCase(10)]
        public async Task DeleteChild_WhenIdIsNotValid_ShouldReturnNull(long id)
        {
            // Act
            var result = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
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

        private List<ChildDto> FakeChildren()
        {
            var parent1 = new ParentDTO() { Id = 1, UserId = currentUserId };
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
