using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class PermissionsForRoleControllerTests
    {
        private PermissionsForRoleController controller;
        private Mock<IPermissionsForRoleService> service;

        private IEnumerable<PermissionsForRole> permissionsForAllRoles;
        private PermissionsForRole permissionsForRoleEntity;



        [SetUp]
        public void Setup()
        {
            service = new Mock<IPermissionsForRoleService>();

            controller = new PermissionsForRoleController(service.Object);

            permissionsForAllRoles = FakePermissionsForAllRoles();
            permissionsForRoleEntity = FakePermissionsForRole();
        }

        [Test]
        public async Task GetsAllPermissionsForRoles_ReturnsOkAllEnititiesInValue()
        {
            // Arrange
            var expected = permissionsForAllRoles.Select(s => s.ToModel());
            service.Setup(x => x.GetAll()).ReturnsAsync(permissionsForAllRoles.Select(s => s.ToModel()));

            // Act
            var response = await controller.Get().ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
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
            //Arrange
            var expectedValue = Enum.GetValues(typeof(Permissions))
                .Cast<Permissions>()
                .Select(p => (p, p.ToString()));

            // Act
            var response = controller.GetAllPermissions();

            // Assert
            response.AssertResponseOkResultAndValidateValue(expectedValue);
        }


        [Test]
        [TestCase("Admin")]
        public async Task GetByRoleName_WhenRoleNameIsValid_ReturnOkResultObject(string roleName)
        {
            // Arrange
            var expected = permissionsForAllRoles.Where(s => s.RoleName == roleName).Select(p => p.ToModel()).FirstOrDefault();
            service.Setup(x => x.GetByRole(roleName)).ReturnsAsync(permissionsForAllRoles.SingleOrDefault(x => x.RoleName == roleName).ToModel());

            // Act
            var response = await controller.GetByRoleName(roleName).ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
        }

        [Test]
        [TestCase("fakeRole")]
        public async Task GetByRoleName_NotExistingRoleName_ReturnsBadRequest(string roleName)
        {
            // Arrange
            var expectedResponse = new BadRequestObjectResult("roleName");
            service.Setup(x => x.GetByRole(roleName)).ThrowsAsync(new ArgumentNullException());

            // Act
            var response = await controller.GetByRoleName(roleName).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expectedResponse);
        }

        [Test]
        public async Task CreatePermissionsForRole_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var expected = permissionsForRoleEntity.ToModel();
            var expectedResponse = new CreatedAtActionResult(
                nameof(controller.GetByRoleName),
                nameof(ProviderController),
                new { id = expected.Id, roleName = expected.RoleName },
                expected);
            service.Setup(x => x.Create(expected)).ReturnsAsync(permissionsForRoleEntity.ToModel());

            // Act
            var response = await controller.Create(expected).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<CreatedAtActionResult>(expectedResponse);
        }

        [Test]
        public async Task UpdatePermissionsForRole_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            permissionsForRoleEntity.Description = "new";
            var expected = permissionsForRoleEntity.ToModel();
            service.Setup(x => x.Update(expected)).ReturnsAsync(permissionsForRoleEntity.ToModel());

            // Act
            var response = await controller.Update(expected).ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
        }

        [Test]
        public async Task UpdatePermissionsForRole_WhenProblemsWithDb_ReturnsBadRequestWithError()
        {
            // Arrange
            var expected = permissionsForRoleEntity.ToModel();
            var expectedResponse = new BadRequestObjectResult("message error");
            service.Setup(x => x.Update(expected)).ThrowsAsync(new DbUpdateConcurrencyException("message error"));

            // Act
            var response = await controller.Update(expected).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expectedResponse);
        }

        /// <summary>
        /// faking data for testing.
        /// </summary>
        private PermissionsForRole FakePermissionsForRole()
        {
            var permissions = new List<Permissions> { Permissions.SystemManagement, Permissions.AddressAddNew, Permissions.AddressEdit, };
            return new PermissionsForRole()
            {
                Id = 1,
                RoleName = "TestAdmin",
                PackedPermissions = permissions.PackPermissionsIntoString(),
            };
        }



        private IEnumerable<PermissionsForRole> FakePermissionsForAllRoles()
        {
            var permissions1 = new List<Permissions> { Permissions.SystemManagement, Permissions.AccessAll, };
            var permissions2 = new List<Permissions> { Permissions.WorkshopAddNew, Permissions.TeacherAddNew, Permissions.ProviderAddNew, };
            var permissions3 = new List<Permissions> { Permissions.FavoriteAddNew, Permissions.ApplicationAddNew, Permissions.ChildAddNew, };

            return new List<PermissionsForRole>()
            {
                new PermissionsForRole()
                {
                Id = 1,
                RoleName = Role.Admin.ToString(),
                PackedPermissions = permissions1.PackPermissionsIntoString(),  },
                new PermissionsForRole()
                {
                Id = 2,
                RoleName = Role.Provider.ToString(),
                PackedPermissions = permissions2.PackPermissionsIntoString(),
                },
                new PermissionsForRole()
                {
                Id = 3,
                RoleName = Role.Parent.ToString(),
                PackedPermissions = permissions3.PackPermissionsIntoString(),

                },
            };
        }

    }
}
