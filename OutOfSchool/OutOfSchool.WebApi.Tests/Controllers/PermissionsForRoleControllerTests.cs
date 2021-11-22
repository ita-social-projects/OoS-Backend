using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common;
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

        private IEnumerable<PermissionsForRoleDTO> permissionsForAllRoles;
        private PermissionsForRoleDTO permissionsForRoleDTO;



        [SetUp]
        public void Setup()
        {
            service = new Mock<IPermissionsForRoleService>();

            controller = new PermissionsForRoleController(service.Object);

            permissionsForAllRoles = FakePermissionsForAllRoles();
            permissionsForRoleDTO = FakePermissionsForRole();
        }

        [Test]
        public async Task GetsAllPermissionsForRoles_ReturnsOkAllEnititiesInValue()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(permissionsForAllRoles);

            // Act
            var response = await controller.Get().ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseOkAndValidValue<IEnumerable<PermissionsForRoleDTO>>();
        }

        [Test]
        public async Task GetPermissionsForRoles_WhenEmptyCollection_ReturnsNoContentResult()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(new List<PermissionsForRoleDTO>());

            // Act
            var response = await controller.Get().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(response);
        }

        [Test]
        public void GetAllPermissions_WhenCalled_ReturnsAllSystemPermissions()
        {
            // Act
            var response = controller.GetAllPermissions();

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<OkObjectResult>();
        }

        [Test]
        [TestCase("Admin")]
        public async Task GetByRoleName_WhenRoleNameIsValid_ReturnOkResultObject(string roleName)
        {
            // Arrange
            service.Setup(x => x.GetByRole(roleName)).ReturnsAsync(permissionsForAllRoles.SingleOrDefault(x => x.RoleName == roleName));

            // Act
            var response = await controller.GetByRoleName(roleName).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseOkAndValidValue<PermissionsForRoleDTO>();
        }

        [Test]
        [TestCase("fakeRole")]
        public async Task GetByRoleName_NotExistingRoleName_ReturnsBadRequest(string roleName)
        {
            // Arrange
            service.Setup(x => x.GetByRole(roleName)).Throws<ArgumentNullException>();

            // Act
            var response = await controller.GetByRoleName(roleName).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task CreatePermissionsForRole_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(permissionsForRoleDTO)).ReturnsAsync(permissionsForRoleDTO);

            // Act
            var response = await controller.Create(permissionsForRoleDTO).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<CreatedAtActionResult>();
        }

        [Test]
        public async Task UpdatePermissionsForRole_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            service.Setup(x => x.Update(permissionsForRoleDTO)).ReturnsAsync(permissionsForRoleDTO);

            // Act
            var response = await controller.Update(permissionsForRoleDTO).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseOkAndValidValue<PermissionsForRoleDTO>();
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
                RoleName = Role.Admin.ToString(),
                Permissions = new List<Permissions> { Permissions.SystemManagement, Permissions.AccessAll, },
                },
                new PermissionsForRoleDTO()
                {
                Id = 2,
                RoleName = Role.Provider.ToString(),
                Permissions = new List<Permissions> { Permissions.WorkshopAddNew, Permissions.TeacherAddNew, Permissions.ProviderAddNew, },

                },
                new PermissionsForRoleDTO()
                {
                Id = 3,
                RoleName = Role.Parent.ToString(),
                Permissions = new List<Permissions> { Permissions.FavoriteAddNew, Permissions.ApplicationAddNew, Permissions.ChildAddNew, },

                },
            };
        }

    }
}
