using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Tests
{
    [TestFixture]
    public class ParentControllerTest
    {
        private readonly Mock<IParentService> parentService;
        private readonly ParentController parentController;
        private readonly ClaimsPrincipal user;
        public ParentControllerTest()
        {
            parentService = new Mock<IParentService>();
            parentController = new ParentController(parentService.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            parentController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        [Test]
        public async Task Task_Parents_ReturnsOkObjectResultAsync()
        {
            // Arrange

            IEnumerable<ParentDTO> parents = new List<ParentDTO>
            {
                new ParentDTO
                {
                      Id = 0b101,
                      FirstName = "Test",
                      MiddleName = "Test",
                      LastName = "Test",
                      UserId = 123,
                },
            }.ToList();

            parentService.Setup(it => it.GetAll()).Returns(Task.FromResult(parents));

            // Act

            var response = await parentController.GetParents();

            // Assert

            Assert.IsInstanceOf<OkObjectResult>(response);
        }

        [Test]
        public async Task Get_Parents_ReturnsBadRequestObjectResult()
        {
            // Arrange

            parentService.Setup(it => it.GetAll()).Throws(new ArgumentException());

            // Act

            var response = await parentController.GetParents();

            // Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task Get_ParentById_ReturnsOkObjectResult()
        {
            // Arrange

            var parent = new ParentDTO
            {
                Id = 0b101,
                FirstName = "Test",
                MiddleName = "Test",
                LastName = "Test",
                UserId = 123,
            };

            parentService.Setup(it => it.GetById(parent.Id)).Returns(Task.FromResult(parent));

            // Act

            var response = await parentController.GetParentById(parent.Id);

            // Assert

            Assert.IsInstanceOf<OkObjectResult>(response);
        }

        [Test]
        public async Task Get_ParentById_ReturnsBadRequestObjectResult()
        {
            // Arrange

            long parentId = 123;
            parentService.Setup(it => it.GetById(parentId)).Throws(new ArgumentException());

            // Act

            var response = await parentController.GetParentById(parentId);

            // Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task Update_Parent_ReturnOkObjectResult()
        {
            // Arrange

            var parent = new ParentDTO
            {
                Id = 0b101,
                FirstName = "Test",
                MiddleName = "Test",
                LastName = "Test",
                UserId = 123,
            };

            parentService.Setup(it => it.Update(parent)).Returns(Task.FromResult(parent));

            // Act

            var result = await parentController.Update(parent);

            // Assert

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_ParentNotValidModel_ReturnsBadRequestObjectResult()
        {
            // Arrange

            var parent = new ParentDTO
            {
                Id = 0b101,
                FirstName = "Test",
                MiddleName = "Test",
                LastName = "Test",
                UserId = 123,
            };

            parentController.ModelState.AddModelError("fakeError", "fakeError");

            parentService.Setup(it => it.Update(parent)).Returns(Task.FromResult(parent));

            // Act

            var response = await parentController.Update(parent);

            // Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }
        [Test]
        public async Task Update_ParentThatNull_ReturnsBadRequestObjectResult()
        {
            // Arrange

            ParentDTO parent = null;
            parentService.Setup(it => it.Update(parent)).Returns(Task.FromResult(parent));

            // Act

            var response = await parentController.Update(parent);

            // Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task Update_ParentExceptionOccurred_ReturnsBadRequestObjectResult()
        {
            // Arrange

            var parent = new ParentDTO
            {
                Id = 0b101,
                FirstName = "Test",
                MiddleName = "Test",
                LastName = "Test",
                UserId = 123,
            };

            parentService.Setup(it => it.Update(parent)).Throws(new ArgumentException());

            // Act

            var response = await parentController.Update(parent);

            // Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task Delete_Parent_ReturnsOkResult()
        {
            // Arrange

            long parentId = 123;

            parentService.Setup(it => it.Delete(parentId));

            // Act

            var response = await parentController.Delete(parentId);

            // Assert

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public async Task Delete_ParentExceptionOccurred_ReturnsBadRequestObjectResult()
        {
            // Arrange

            long parentId = 123;

            parentService.Setup(it => it.Delete(parentId)).Throws(new ArgumentException());

            // Act

            var response = await parentController.Delete(parentId);

            // Assert

            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }


        [Test]
        public async Task Create_Parent_ReturnsCreatedAtActionResult()
        {
            // Arrange

            var parent = new ParentDTO
            {
                Id = 0b101,
                FirstName = "Test",
                MiddleName = "Test",
                LastName = "Test",
                UserId = 123,
            };

            parentService.Setup(it => it.Create(parent)).Returns(Task.FromResult(parent));

            // Act

            var response = await parentController.CreateParent(parent);

            // Assert

            Assert.IsInstanceOf<CreatedAtActionResult>(response);
        }
    }
}
