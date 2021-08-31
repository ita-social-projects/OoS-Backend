using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTest
    {
        private UserController controller;
        private Mock<IUserService> serviceUser;
        private IEnumerable<ShortUserDto> users;
        private ShortUserDto user;
        private Mock<HttpContext> httpContextMoq;

        [SetUp]
        public void Setup()
        {
            serviceUser = new Mock<IUserService>();

            httpContextMoq = new Mock<HttpContext>();
            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c"));

            controller = new UserController(serviceUser.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
            };

            users = FakeUsers();
            user = FakeUser();
        }

        #region GetUsers
        [Test]
        public async Task GetUsers_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            serviceUser.Setup(x => x.GetAll()).ReturnsAsync(users);

            // Act
            var result = await controller.GetUsers().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetUsers_WhenThereIsNoUsers_ShouldReturnNoConterntResult()
        {
            // Arrange
            var emptyList = new List<ShortUserDto>();
            serviceUser.Setup(x => x.GetAll()).ReturnsAsync(emptyList);

            // Act
            var result = await controller.GetUsers().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        #endregion

        #region GetUserById
        [Test]
        [TestCase("cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c")]
        public async Task GetUserById_WhenIdIsValid_ReturnsOkObjectResult(string id)
        {
            // Arrange
            serviceUser.Setup(x => x.GetById(id)).ReturnsAsync(users.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetUserById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase("Invalid Id")]
        public async Task GetUserById_WhenIdIsInvalid_ReturnsForbidden(string id)
        {
            // Arrange
            serviceUser.Setup(x => x.GetById(id)).ReturnsAsync(users.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetUserById(id).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase("cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c")]
        public async Task GetUserById_WhenThereIsNoUser_ReturnsNoContent(string id)
        {
            // Arrange
            ShortUserDto user = null;
            serviceUser.Setup(x => x.GetById(id)).ReturnsAsync(user);

            // Act
            var result = await controller.GetUserById(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }
        #endregion

        #region UpdateUser
        public async Task UpdateUser_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedUser = new ShortUserDto()
            {
                Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                PhoneNumber = "1160327456",
                LastName = "LastName",
                MiddleName = "MiddleName",
                FirstName = "FirstName",
            };

            serviceUser.Setup(x => x.Update(changedUser)).ReturnsAsync(changedUser);

            // Act
            var result = await controller.Update(changedUser).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task UpdateUser_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateUser", "Invalid model state.");

            // Act
            var result = await controller.Update(new ShortUserDto()).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateUser_WhenIdUserHasNoRights_ShouldReturn403ObjectResult()
        {
            // Arrange
            var changedUser = new ShortUserDto()
            {
                Id = "Forbidden Id",
                FirstName = "FirstName",
                LastName = "LastName",
                PhoneNumber = "0960456732",
            };

            // Act
            var result = await controller.Update(user).ConfigureAwait(false) as ObjectResult;

            // Assert
            serviceUser.Verify(x => x.Update(It.IsAny<ShortUserDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }
        #endregion

        private ShortUserDto FakeUser()
        {
            return new ShortUserDto()
            {
                Id = "c4a6876a-77fb-4e9e-9c78-a0880286ae3c",
                MiddleName = "MiddleName6",
                FirstName = "FirstName6",
                LastName = "LastName6",
                UserName = "user6@gmail.com",
                Email = "user6@gmail.com",
                PhoneNumber = "0965679312",
                Role = "provider",
            };
        }

        private IEnumerable<ShortUserDto> FakeUsers()
        {
            return new List<ShortUserDto>()
            {
               new ShortUserDto()
               {
                    Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                    MiddleName = "MiddleName1",
                    FirstName = "FirstName1",
                    LastName = "LastName1",
                    UserName = "user1@gmail.com",
                    Email = "user1@gmail.com",
                    PhoneNumber = "0965679725",
                    Role = "provider",
               },
               new ShortUserDto()
               {
                    Id = "MM4a6876a-77fb-4bvse-9c78-a0880286ae3c",
                    MiddleName = "MiddleName2",
                    FirstName = "FirstName2",
                    LastName = "LastName2",
                    UserName = "user2@gmail.com",
                    Email = "user2@gmail.com",
                    PhoneNumber = "0965679000",
                    Role = "parent",
               },
               new ShortUserDto()
               {
                    Id = "c4a6876a-77fb-4e9e-9c78-a0880286aeV0",
                    MiddleName = "MiddleName3",
                    FirstName = "FirstName3",
                    LastName = "LastName3",
                    UserName = "user3@gmail.com",
                    Email = "user3@gmail.com",
                    PhoneNumber = "0675679312",
                    Role = "parent",
               },
               new ShortUserDto()
               {
                    Id = "CEc4a6876a-77fb-4e9e-9c78-a0880286ae3c",
                    MiddleName = "MiddleName4",
                    FirstName = "FirstName4",
                    LastName = "LastName4",
                    UserName = "user4@gmail.com",
                    Email = "user4@gmail.com",
                    PhoneNumber = "0965679312",
                    Role = "parent",
               },
               new ShortUserDto()
               {
                    Id = "CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c",
                    MiddleName = "MiddleName5",
                    FirstName = "FirstName5",
                    LastName = "LastName5",
                    UserName = "user5@gmail.com",
                    Email = "user5@gmail.com",
                    PhoneNumber = "0965889312",
                    Role = "parent",
               },
            };
        }
    }
}
