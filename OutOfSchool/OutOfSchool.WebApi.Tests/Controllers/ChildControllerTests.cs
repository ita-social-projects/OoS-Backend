using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;
using NUnit.Framework;

using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
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

        private ParentDtoWithContactInfo existingParent;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IChildService>();
            controller = new ChildController(service.Object);

            // TODO: find out why it is a string but not a GUID
            currentUserId = Guid.NewGuid().ToString();

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[] { new Claim("sub", currentUserId) }, "sub"));

            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            existingParent = ParentDtoWithContactInfoGenerator.Generate();
            existingParent.UserId = currentUserId;

            var parent2 = ParentDtoWithContactInfoGenerator.Generate();

            children =
                ChildDtoGenerator.Generate(2)
                .WithParent(existingParent)
                .WithSocial(new SocialGroupDto { Id = 1 })
                .Concat(ChildDtoGenerator.Generate(2)
                .WithParent(parent2)
                .WithSocial(new SocialGroupDto { Id = 2 }))
                .ToList();
            child = ChildDtoGenerator.Generate();
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

        public async Task GetUsersChilById_WhenChildWasFound_ShouldReturnOkResultObject()
        {
            // Arrange
            var existingChildId = children.RandomItem().Id;
            service.Setup(x => x.GetByIdAndUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                   .ReturnsAsync(children.First(x => x.Id.Equals(existingChildId)));

            // Act
            var result = await controller.GetUsersChildById(existingChildId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetUsersChildById_WhenChildWasNotFound_ShouldReturnOkObjectResult()
        {
            // Arrange
            var noneExistingChildId = Guid.NewGuid();
            service.Setup(x => x.GetByIdAndUserId(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(default, new TimeSpan(1));

            // Act
            var result = await controller.GetUsersChildById(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        public async Task GetByParentId_WhenThereAreChildren_ShouldReturnOkResultObject()
        {
            // Arrange
            var filter = new OffsetFilter();
            service.Setup(x => x.GetByParentIdOrderedByFirstName(existingParent.Id, filter))
                .ReturnsAsync(
                new SearchResult<ChildDto>()
                {
                    TotalAmount = children.Where(p => p.ParentId == existingParent.Id).Count(),
                    Entities = children.Where(p => p.ParentId == existingParent.Id).ToList(),
                });

            // Act
            var result = await controller.GetByParentIdForAdmin(existingParent.Id, filter).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [TestCase(10)]
        public async Task GetByParentId_WhenThereIsNoChild_ShouldReturnOkObjectResult(long id)
        {
            // Arrange
            var noneExistingParentId = Guid.NewGuid();
            var filter = new OffsetFilter();
            service.Setup(x => x.GetByParentIdOrderedByFirstName(noneExistingParentId, filter)).ReturnsAsync(new SearchResult<ChildDto>() { Entities = children.Where(p => p.ParentId == noneExistingParentId).ToList() });

            // Act
            var result = await controller.GetByParentIdForAdmin(noneExistingParentId, filter).ConfigureAwait(false);

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
            var childToUpdate = ChildDtoGenerator.Generate();
            childToUpdate.Id = children.RandomItem().Id;

            service.Setup(x => x.UpdateChildCheckingItsUserIdProperty(childToUpdate, It.IsAny<string>())).ReturnsAsync(childToUpdate);

            // Act
            var result = await controller.Update(childToUpdate).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task DeleteChild_WhenChildWithIdExists_ShouldReturnNoContentResult()
        {
            // Arrange
            var childToDelete = children.RandomItem();

            // Act
            var result = await controller.Delete(childToDelete.Id);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteChild_WhenIdIsNotValid_ShouldReturnNull()
        {
            // Act
            var result = await controller.Delete(Guid.NewGuid());

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }
    }
}
