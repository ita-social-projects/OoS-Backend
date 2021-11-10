using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class PermissionsForRoleServiceTests
    {
        private IPermissionsForRoleService service;
        private IEntityRepository<PermissionsForRole> repository;
        private DbContextOptions<OutOfSchoolDbContext> options;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            var context = new OutOfSchoolDbContext(options);
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            repository = new EntityRepository<PermissionsForRole>(context);
            var logger = new Mock<ILogger<PermissionsForRoleService>>();
            service = new PermissionsForRoleService(repository, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsGrouppedPermissionsForAllRoles()
        {
            // Arrange
            var expected = await repository.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert

            Assert.IsTrue(result.Any(r => r.RoleName == Role.Admin.ToString()));
            Assert.IsTrue(result.Any(r => r.RoleName == Role.Provider.ToString()));
            Assert.IsTrue(result.Any(r => r.RoleName == Role.Parent.ToString()));
            Assert.AreEqual(result.ToList().Count(), expected.Count());
        }

        [Test]
        [TestCase("Admin")]
        public async Task GetByRole_WhenIdIsValid_ReturnsPermissionsForRole(string roleName)
        {
            // Arrange
            var expected = (await repository.GetByFilter(r => r.RoleName == roleName)).FirstOrDefault();

            // Act
            var result = await service.GetByRole(roleName).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.PackedPermissions, result.Permissions.PackPermissionsIntoString());
        }

        [Test]
        [TestCase("User")]
        public void GetByRole_WhenNoSuchRole_ThrowsArgumentNullException(string roleName)
        {
            // Arrange
            var taskToGetPermissionsForRole = service.GetByRole(roleName);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                 async () => await taskToGetPermissionsForRole.ConfigureAwait(false));
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = new PermissionsForRoleDTO()
            {
                RoleName = "workshopAdmin",
                Permissions = new List<Permissions> { Permissions.ChildAddNew, Permissions.FavoriteEdit, },
            };

            // Act
            var result = await service.Create(expected).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.RoleName, result.RoleName);
            Assert.That(await repository.Count() == 4);
        }

        [Test]
        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
        {
            // Arrange
            var changedEntity = new PermissionsForRoleDTO()
            {
                Id = 1,
                RoleName = Role.Admin.ToString(),
                Permissions = new List<Permissions>
                {
                    Permissions.AccessAll,
                    Permissions.SystemManagement,
                },
            };

            var expectedPermissions = new List<Permissions>
            {
                Permissions.AccessAll,
                Permissions.SystemManagement,
            };

            // Act
            var result = await service.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(result.Permissions.Count() == expectedPermissions.Count());
            Assert.That(changedEntity.RoleName, Is.EqualTo(result.RoleName));
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new PermissionsForRoleDTO()
            {
                RoleName = "TestName",
            };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedEntity).ConfigureAwait(false));
        }

        /// <summary>
        /// method to seed repository with entities to test.
        /// </summary>
        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var permissionsForRoles = new List<PermissionsForRole>()
                {
                    new PermissionsForRole()
                    {
                    Id = 1,
                    RoleName = Role.Admin.ToString(),
                    PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Admin.ToString()),
                    },
                    new PermissionsForRole()
                    {
                    Id = 2,
                    RoleName = Role.Provider.ToString(),
                    PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Provider.ToString()),
                    },
                    new PermissionsForRole()
                    {
                    Id = 3,
                    RoleName = Role.Parent.ToString(),
                    PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Parent.ToString()),

                    },
                };

                context.PermissionsForRoles.AddRangeAsync(permissionsForRoles);
                context.SaveChangesAsync();
            }
        }
    }
}
