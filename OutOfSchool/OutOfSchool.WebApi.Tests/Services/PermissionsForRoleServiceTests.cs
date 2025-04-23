using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

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
        var context = new TestOutOfSchoolDbContext(options);
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
        var expected = (await repository.GetAll()).ToDto();


        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result);
    }

    [Test]
    public async Task GetByRole_WhenIdIsValid_ReturnsPermissionsForRole()
    {
        // Arrange
        var roleName = nameof(Role.TechAdmin);
        var expected = repository
            .GetByFilterNoTracking(r => r.RoleName == roleName)
            .First().ToDto();

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
        var expected = entityToBeCreated.ToDto();
        expected.Id = lastIndex + 1;

        // Act
        var result = await service.Create(entityToBeCreated.ToDto()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void Create_WhenPermissionsForRoleExistsInDb_ThrowsArgumentException()
    {
        // Arrange
        var newPermissionsForRole = PermissionsForRolesGenerator.Generate(nameof(Role.TechAdmin));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Create(newPermissionsForRole.ToDto()).ConfigureAwait(false));
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var entityToChange = repository
            .GetByFilterNoTracking(x => x.RoleName == nameof(Role.TechAdmin))
            .First();
        entityToChange.PackedPermissions = TestDataHelper.GetFakePackedPermissions();
        var expected = entityToChange.ToDto();

        // Act
        var result = await service.Update(entityToChange.ToDto()).ConfigureAwait(false);

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
            async () => await service.Update(changedEntity.ToDto()).ConfigureAwait(false));
    }

    /// <summary>
    /// method to seed repository with entities to test.
    /// </summary>
    private void SeedDatabase()
    {
        using var context = new TestOutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var permissionsForRoles = PermissionsForRolesGenerator.GenerateForExistingRoles();
            context.PermissionsForRoles.AddRange(permissionsForRoles);
            context.SaveChanges();
        }
    }
}