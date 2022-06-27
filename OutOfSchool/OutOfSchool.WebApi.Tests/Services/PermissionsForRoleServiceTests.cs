using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class PermissionsForRoleServiceTests
{
    private IPermissionsForRoleService service;
    private IEntityRepository<long, PermissionsForRole> repository;
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
        repository = new EntityRepository<long, PermissionsForRole>(context);
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
        var roleName = nameof(Role.Admin);
        var expected = repository
            .GetByFilterNoTracking(r => r.RoleName == roleName)
            .First()
            .ToModel();

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
        var lastIndex = (await repository.GetAll()).Last().Id;
        var entityToBeCreated = PermissionsForRolesGenerator.Generate();
        var expected = entityToBeCreated.ToModel();
        expected.Id = lastIndex + 1;

        // Act
        var result = await service.Create(entityToBeCreated.ToModel()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void Create_WhenPermissionsForRoleExistsInDb_ThrowsArgumentException()
    {
        // Arrange
        var newPermissionsForRole = PermissionsForRolesGenerator.Generate(nameof(Role.Admin));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Create(newPermissionsForRole.ToModel()).ConfigureAwait(false));
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var entityToChange = repository
            .GetByFilterNoTracking(x => x.RoleName == nameof(Role.Admin))
            .First();
        entityToChange.PackedPermissions = TestDataHelper.GetFakePackedPermissions();
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
        var changedEntity = PermissionsForRolesGenerator.Generate();

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedEntity.ToModel()).ConfigureAwait(false));
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

            var permissionsForRoles = PermissionsForRolesGenerator.GenerateForExistingRoles();
            context.PermissionsForRoles.AddRange(permissionsForRoles);
            context.SaveChanges();
        }
    }
}