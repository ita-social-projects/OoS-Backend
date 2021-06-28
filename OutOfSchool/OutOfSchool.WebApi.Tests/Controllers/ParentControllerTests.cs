﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
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
        private Mock<HttpContext> httpContextMoq;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<ParentDTO> parents;
        private ParentDTO parent;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IParentService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            httpContextMoq = new Mock<HttpContext>();
            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, "38776161-734b-4aec-96eb-4a1f87a2e5f3"));
            httpContextMoq.Setup(x => x.User.IsInRole("parent"))
                .Returns(true);

            controller = new ParentController(service.Object, localizer.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
            };

            parents = FakeParents();
            parent = FakeParent();
        }

        public IEnumerable<ParentDTO> FakeParents()
        {
            return new List<ParentDTO>
            {
                    new ParentDTO() { Id = 0, UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6" },
                    new ParentDTO() { Id = 1, UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6" },
                    new ParentDTO() { Id = 2, UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6" },
            };
        }

        public ParentDTO FakeParent()
        {
            return new ParentDTO() { Id = 4, UserId = "de909f35-5eb7-4b7a-bda8-ccc0a5bfda96a6" };
        }

        #region GetParents
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
        public async Task GetParents_WhenThereIsNoTAnyParents_ShouldReturnNoConterntResult()
        {
            // Arrange
            var emptyList = new List<ParentDTO>();
            service.Setup(x => x.GetAll()).ReturnsAsync(emptyList);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }
        #endregion

        #region GetParentById
        [Test]
        [TestCase(1)]
        public async Task GetParentById_WhenIdIsValid_ReturnsOkObjectResult(long id)
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
        public async Task GetParentById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(parents.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }
        #endregion

        #region UpdateParent
        [Test]
        public async Task UpdateParent_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedParent = new ShortUserDto()
            {
                Id = "38776161-734b-4aec-96eb-4a1f87a2e5f3",
                PhoneNumber = "1160327456",
                LastName = "LastName",
                MiddleName = "MiddleName",
                FirstName = "FirstName",
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
            var result = await controller.Update(new ShortUserDto()).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateParent_WhenIdUserHasNoRights_ShouldReturn403ObjectResult()
        {
            // Arrange
            var notAuthorParent = new ParentDTO() { Id = 7, UserId = "Forbidden Id" };
            service.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorParent);

            // Act
            var result = await controller.Update(new ShortUserDto()).ConfigureAwait(false) as ObjectResult;

            // Assert
            service.Verify(x => x.Update(It.IsAny<ShortUserDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }
        #endregion

        #region DeleteParent

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

        [Test]
        [TestCase(2)]
        public async Task DeleteParent_WhenParentHasNoRights_ShouldReturn403ObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(parent);
            var notAuthorParent = new ParentDTO() { Id = 10, UserId = "Forbidden Id" };
            service.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorParent);

            // Act
            var result = await controller.Delete(id) as ObjectResult;

            // Assert
            service.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }
        #endregion
    }
}
