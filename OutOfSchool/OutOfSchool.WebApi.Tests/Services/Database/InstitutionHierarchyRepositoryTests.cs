using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class InstitutionHierarchyRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB_InstitutionHierarchy")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    [Test]
    public async Task Create_InstitutionHierarchy_ShouldUpdateSuccessfully()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetInstitutionHierarchyRepository(context);
        var fakeInstitutionHierarchy = InstitutionHierarchyGenerator.Generate();

        // Act
        var result = await repository.Create(fakeInstitutionHierarchy, new List<long>());

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(fakeInstitutionHierarchy.Id, result.Id);
        var retrievedEntity = await context.InstitutionHierarchies.FindAsync(fakeInstitutionHierarchy.Id);
        Assert.NotNull(retrievedEntity);
    }

    [Test]
    public async Task Update_InstitutionHierarchy_ShouldUpdateSuccessfully()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetInstitutionHierarchyRepository(context);
        var fakeInstitutionHierarchy = InstitutionHierarchyGenerator.Generate();
        var createdEntity = await repository.Create(fakeInstitutionHierarchy, new List<long>()).ConfigureAwait(false);
        createdEntity.Title += "+";

        // Act
        var result = await repository.Update(createdEntity, createdEntity.Directions.Select(x => x.Id).ToList()).ConfigureAwait(false);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(createdEntity.Title, result.Title);
        var retrievedEntity = await context.InstitutionHierarchies.FindAsync(createdEntity.Id);
        Assert.NotNull(retrievedEntity);
        Assert.AreEqual(createdEntity.Title, retrievedEntity.Title);
    }

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IInstitutionHierarchyRepository GetInstitutionHierarchyRepository(TestOutOfSchoolDbContext dbContext)
        => new InstitutionHierarchyRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        await context.SaveChangesAsync();
    }
}
