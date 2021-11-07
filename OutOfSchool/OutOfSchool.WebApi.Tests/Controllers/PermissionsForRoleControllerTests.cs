using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class PermissionsForRoleControllerTests
    {
        private PermissionsForRoleController controller;
        private Mock<IPermissionsForRoleService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<PermissionsForRoleDTO> permissionsForAllRoles;
        private PermissionsForRoleDTO permissionsForRoleDTO;



        [SetUp]
        public void Setup()
        {
            service = new Mock<IPermissionsForRoleService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new PermissionsForRoleController(service.Object, localizer.Object);

            permissionsForAllRoles = FakePermissionsForAllRoles();
            permissionsForRoleDTO = FakePermissionsForRole();
        }

        [Test]
        public async Task GetPermissionsForAllRoles_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(permissionsForAllRoles);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetPermissionsForRoles_WhenEmptyCollection_ReturnsNoContentResult()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(new List<PermissionsForRoleDTO>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase("admin")]
        public async Task GetByRoleName_WhenRoleNameIsValid_ReturnOkResultObject(string roleName)
        {
            // Arrange
            service.Setup(x => x.GetByRole(roleName)).ReturnsAsync(permissionsForAllRoles.SingleOrDefault(x => x.RoleName == roleName));

            // Act
            var result = await controller.GetByRoleName(roleName).ConfigureAwait(false) as OkObjectResult;
            var data = result.Value as PermissionsForRoleDTO;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(data.RoleName == "admin"
                && data.Permissions.Any(x => x == Permissions.AccessAll));
            Assert.AreEqual(200, result.StatusCode);
        }


        [Test]
        [TestCase("fakeRole")]
        public async Task GetByRoleName_NotExistingRoleName_ReturnsEmptyObject(string roleName)
        {
            // Arrange
            service.Setup(x => x.GetByRole(roleName)).ReturnsAsync(permissionsForAllRoles.SingleOrDefault(x => x.RoleName == roleName));

            // Act
            var result = await controller.GetByRoleName(roleName).ConfigureAwait(false) as OkObjectResult;
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task CreatePermissionsForRole_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(permissionsForRoleDTO)).ReturnsAsync(permissionsForRoleDTO);

            // Act
            var result = await controller.Create(permissionsForRoleDTO).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }




        [Test]
        public async Task UpdatePermissionsForRole_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            service.Setup(x => x.Update(permissionsForRoleDTO)).ReturnsAsync(permissionsForRoleDTO);

            // Act
            var result = await controller.Update(permissionsForRoleDTO).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        /// <summary>
        /// faking data for testing.
        /// </summary>
        private PermissionsForRoleDTO FakePermissionsForRole()
        {
            return new PermissionsForRoleDTO()
            {
                Id = 1,
                RoleName = "TestAdmin",
                Permissions = new List<Permissions> { Permissions.SystemManagement, Permissions.AddressAddNew, Permissions.AddressEdit, },
            };
        }

        private IEnumerable<PermissionsForRoleDTO> FakePermissionsForAllRoles()
        {
            return new List<PermissionsForRoleDTO>()
            {
                new PermissionsForRoleDTO()
                {
                Id = 1,
                RoleName = "admin",
                Permissions = new List<Permissions> { Permissions.SystemManagement, Permissions.AccessAll, },
                },
                new PermissionsForRoleDTO()
                {
                Id = 2,
                RoleName = "provider",
                Permissions = new List<Permissions> { Permissions.WorkshopAddNew, Permissions.TeacherAddNew, Permissions.ProviderAddNew, },

                },
                new PermissionsForRoleDTO()
                {
                Id = 3,
                RoleName = "perent",
                Permissions = new List<Permissions> { Permissions.FavoriteAddNew, Permissions.ApplicationAddNew, Permissions.ChildAddNew, },

                },
            };
        }

    }
}
