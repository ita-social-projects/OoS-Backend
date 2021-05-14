using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private IEnumerable<UserDto> users;
        private UserDto user;

        [SetUp]
        public void Setup()
        {
            serviceUser = new Mock<IUserService>();
            controller = new UserController(serviceUser.Object);

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
        public async Task GetUserById_WhenIdIsInvalid_ReturnsNull(string id)
        {
            // Arrange
            serviceUser.Setup(x => x.GetById(id)).ReturnsAsync(users.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetUserById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result.Value, Is.Null);
        }

        private UserDto FakeUser()
        {
            return new UserDto()
            {
                Id = "c4a6876a-77fb-4e9e-9c78-a0880286ae3c",
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName6",
                FirstName = "FirstName6",
                LastName = "LastName6",
                UserName = "user6@gmail.com",
                NormalizedUserName = "USER6@GMAIL.COM",
                Email = "user6@gmail.com",
                NormalizedEmail = "USER6@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                PhoneNumber = "0965679312",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            };
        }

        private IEnumerable<UserDto> FakeUsers()
        {
            return new List<UserDto>()
            {
               new UserDto()
               {
                    Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                    CreatingTime = default,
                    LastLogin = default,
                    MiddleName = "MiddleName1",
                    FirstName = "FirstName1",
                    LastName = "LastName1",
                    UserName = "user1@gmail.com",
                    NormalizedUserName = "USER1@GMAIL.COM",
                    Email = "user1@gmail.com",
                    NormalizedEmail = "USER1@GMAIL.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                    SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                    ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
                    PhoneNumber = "0965679725",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
               },
               new UserDto()
               {
                    Id = "MM4a6876a-77fb-4bvse-9c78-a0880286ae3c",
                    CreatingTime = default,
                    LastLogin = default,
                    MiddleName = "MiddleName2",
                    FirstName = "FirstName2",
                    LastName = "LastName2",
                    UserName = "user2@gmail.com",
                    NormalizedUserName = "USER2@GMAIL.COM",
                    Email = "user2@gmail.com",
                    NormalizedEmail = "USER2@GMAIL.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                    SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                    ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce822d07",
                    PhoneNumber = "0965679000",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
               },
               new UserDto()
               {
                    Id = "c4a6876a-77fb-4e9e-9c78-a0880286aeV0",
                    CreatingTime = default,
                    LastLogin = default,
                    MiddleName = "MiddleName3",
                    FirstName = "FirstName3",
                    LastName = "LastName3",
                    UserName = "user3@gmail.com",
                    NormalizedUserName = "USER3@GMAIL.COM",
                    Email = "user3@gmail.com",
                    NormalizedEmail = "USER3@GMAIL.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pMWRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                    SecurityStamp = "WGWJIYCCRG236HXFKGYS7H6QT2DE2LFF",
                    ConcurrencyStamp = "cb54f60f-70982-4416-926c-d1edce844d07",
                    PhoneNumber = "0675679312",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
               },
               new UserDto()
               {
                    Id = "CEc4a6876a-77fb-4e9e-9c78-a0880286ae3c",
                    CreatingTime = default,
                    LastLogin = default,
                    MiddleName = "MiddleName4",
                    FirstName = "FirstName4",
                    LastName = "LastName4",
                    UserName = "user4@gmail.com",
                    NormalizedUserName = "USER4@GMAIL.COM",
                    Email = "user4@gmail.com",
                    NormalizedEmail = "USER4@GMAIL.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                    SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                    ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                    PhoneNumber = "0965679312",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
               },
               new UserDto()
               {
                    Id = "CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c",
                    CreatingTime = default,
                    LastLogin = default,
                    MiddleName = "MiddleName5",
                    FirstName = "FirstName5",
                    LastName = "LastName5",
                    UserName = "user5@gmail.com",
                    NormalizedUserName = "USER5@GMAIL.COM",
                    Email = "user5@gmail.com",
                    NormalizedEmail = "USER5@GMAIL.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                    SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                    ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                    PhoneNumber = "0965889312",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
               },
            };
        }
    }
}
