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
        private OutOfSchoolDbContext context;
        private IEntityRepository<PermissionsForRole> repository;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<PermissionsForRoleService>> logger;
        private DbContextOptions<OutOfSchoolDbContext> options;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            repository = new EntityRepository<PermissionsForRole>(context);
            logger = new Mock<ILogger<PermissionsForRoleService>>();
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

            Assert.IsTrue(result.Any(r => r.RoleName == "admin"));
            Assert.IsTrue(result.Any(r => r.RoleName == "provider"));
            Assert.IsTrue(result.Any(r => r.RoleName == "parent"));
            Assert.AreEqual(result.ToList().Count(), expected.Count());
        }

        [Test]
        [TestCase("admin")]
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
        [TestCase("user")]
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
                RoleName = "admin",
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
                    RoleName = "admin",
                    PackedPermissions = PermissionsSeeder.SeedPermissions("admin"),
                    },
                    new PermissionsForRole()
                    {
                    Id = 2,
                    RoleName = "provider",
                    PackedPermissions = PermissionsSeeder.SeedPermissions("provider"),
                    },
                    new PermissionsForRole()
                    {
                    Id = 3,
                    PackedPermissions = PermissionsSeeder.SeedPermissions("parent"),

                    },
                };

                context.PermissionsForRoles.AddRangeAsync(permissionsForRoles);
                context.SaveChangesAsync();
            }
        }
    }
}
