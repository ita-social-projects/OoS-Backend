using System;
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
using OutOfSchool.Tests.Common.TestDataGenerators;
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

            permissionsForAllRoles = PermissionsForRolesGenerator.GenerateForExistingRoles();
            permissionsForRoleEntity = PermissionsForRolesGenerator.Generate();
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
            // Arrange
            var expectedValue = Enum.GetValues(typeof(Permissions))
                .Cast<Permissions>()
                .Select(p => (p, p.ToString()));

            // Act
            var response = controller.GetAllPermissions();

            // Assert
            response.AssertResponseOkResultAndValidateValue(expectedValue);
        }


        [Test]
        public async Task GetByRoleName_WhenRoleNameIsValid_ReturnOkResultObject()
        {
            // Arrange
            var roleName = nameof(Role.Admin);
            var expected = permissionsForAllRoles.Where(s => s.RoleName == roleName).Select(p => p.ToModel()).First();
            service.Setup(x => x.GetByRole(roleName)).ReturnsAsync(permissionsForAllRoles.SingleOrDefault(x => x.RoleName == roleName).ToModel());

            // Act
            var response = await controller.GetByRoleName(roleName).ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
        }

        [Test]
        public async Task GetByRoleName_NotExistingRoleName_ReturnsBadRequest()
        {
            // Arrange
            var roleName = TestDataHelper.GetRandomRole();
            var expectedResponse = new BadRequestObjectResult(nameof(roleName));
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
            permissionsForRoleEntity.Description = TestDataHelper.GetRandomWords();
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
            var errorMessage = TestDataHelper.GetRandomWords();
            var expectedResponse = new BadRequestObjectResult(errorMessage);
            service.Setup(x => x.Update(expected)).ThrowsAsync(new DbUpdateConcurrencyException(errorMessage));

            // Act
            var response = await controller.Update(expected).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expectedResponse);
        }
    }
}
