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
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Extensions;
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
            var expected = (await repository.GetAll()).Select(p => p.ToModel());


            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            TestHelper.AssertTwoCollectionsEqualByValues(expected, result);
        }

        [Test]
        public async Task GetByRole_WhenIdIsValid_ReturnsPermissionsForRole()
        {
            // Arrange
            var roleName = Role.Admin.ToString();
            var expected = (await repository.GetByFilter(r => r.RoleName == roleName))
                .FirstOrDefault().ToModel();

            // Act
            var result = await service.GetByRole(roleName).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public void GetByRole_WhenNoSuchRole_ThrowsArgumentNullException()
        {
            // Arrange
            var roleName = TestDataHelper.GetRandomRole();
            var taskToGetPermissionsForRole = service.GetByRole(roleName);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                 async () => await taskToGetPermissionsForRole.ConfigureAwait(false));
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var permissions = new List<Permissions> 
            { 
                Permissions.ChildAddNew,
                Permissions.FavoriteEdit,
            };
            var entityToBeCreated = new PermissionsForRole()
            {
                Id = 4,
                RoleName = TestDataHelper.GetRandomRole(),
                PackedPermissions = permissions.PackPermissionsIntoString(),
            };

            var expected = entityToBeCreated.ToModel();

            // Act
            var result = await service.Create(entityToBeCreated.ToModel()).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public void Create_WhenPermissionsForRoleExistsInDb_ThrowsArgumentException()
        {
            // Arrange
            var newPermissionsForRole = new PermissionsForRoleDTO()
            {
                RoleName = Role.Admin.ToString(),
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(newPermissionsForRole).ConfigureAwait(false));
        }

        [Test]
        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
        {
            // Arrange
            var expectedPermissions = new List<Permissions> 
            { Permissions.AccessAll,
                Permissions.SystemManagement,
            };
            var entityToChange = new PermissionsForRole()
            {
                Id = 1,
                RoleName = Role.Admin.ToString(),
                PackedPermissions = expectedPermissions.PackPermissionsIntoString(),
            };
            var expected = entityToChange.ToModel();

            // Act
            var result = await service.Update(entityToChange.ToModel()).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new PermissionsForRoleDTO()
            {
                RoleName = TestDataHelper.GetRandomRole(),
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

                var permissionsForRoles = FakeRolesPermissions();
                context.PermissionsForRoles.AddRangeAsync(permissionsForRoles);
                context.SaveChangesAsync();
            }
        }

        private List<PermissionsForRole> FakeRolesPermissions()
        {
            return new List<PermissionsForRole>()
                {
                    new PermissionsForRole()
                    {
                    Id = 1,
                    RoleName = Role.Admin.ToString(),
                    PackedPermissions = TestDataHelper.GetFakePackedPermissions(),
                    },
                    new PermissionsForRole()
                    {
                    Id = 2,
                    RoleName = Role.Provider.ToString(),
                    PackedPermissions = TestDataHelper.GetFakePackedPermissions(),
                    },
                    new PermissionsForRole()
                    {
                    Id = 3,
                    RoleName = Role.Parent.ToString(),
                    PackedPermissions = TestDataHelper.GetFakePackedPermissions(),

                    },
                };
        }
    }
}
