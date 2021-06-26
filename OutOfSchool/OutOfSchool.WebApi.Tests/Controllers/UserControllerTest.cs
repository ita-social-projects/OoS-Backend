using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTest
    {
        private UserController controller;
        private Mock<IUserService> serviceUser;
        private ClaimsPrincipal claimsPrincipal;
        private IEnumerable<ShortUserDto> users;
        private ShortUserDto user;

        [SetUp]
        public void Setup()
        {
            serviceUser = new Mock<IUserService>();
            controller = new UserController(serviceUser.Object);

            var claims = new List<Claim>()
            {
                new Claim("sub", "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c"),
            };

            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            users = FakeUsers();
            user = FakeUser();
        }

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
            Assert.AreEqual(403, result.StatusCode);
        }

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
