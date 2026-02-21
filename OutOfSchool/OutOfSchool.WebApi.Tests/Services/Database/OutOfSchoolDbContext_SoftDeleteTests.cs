using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class OutOfSchoolDbContext_SoftDeleteTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    [Test]
    public async Task NewEntities_IsDeletedPropertyValue_IsFalse()
    {
        // Arrange
        using var dbContext = GetContext();
        var workshopEntities = await dbContext.Workshops
            .Select(x => dbContext.Entry(x))
            .ToListAsync();

        // Act
        var workshop = WorkshopGenerator.Generate();
        dbContext.Add(workshop);
        await dbContext.SaveChangesAsync();

        // Assert
        Assert.True(workshopEntities.All(x => (bool)x.CurrentValues["IsDeleted"] == false));
    }

    [Test]
    public void DeleteWorkshop_SaveChanges_PerformsSoftDelete()
    {
        // Arrange
        using var dbContext = GetContext();
        var workshop = dbContext.Workshops.First();
        var initialWorkshopsCount = dbContext.Workshops.Count();
        var expectedWorkshopsCount = initialWorkshopsCount - 1;

        // Act
        dbContext.Entry(workshop).State = EntityState.Deleted;
        dbContext.SaveChanges();
        var deletedEntry = dbContext.Entry(workshop);
        var unmodifiedEntries = dbContext.Workshops.Where(x => x.Id != workshop.Id).Select(x => dbContext.Entry(x)).ToList();

        // Assert
        Assert.AreEqual(initialWorkshopsCount, dbContext.Workshops.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedWorkshopsCount, dbContext.Workshops.Count(x => !x.IsDeleted));
        Assert.False(dbContext.Workshops.Any(x => !x.IsDeleted && x.Id == workshop.Id));
        Assert.True(dbContext.Workshops.IgnoreQueryFilters().Any(x => x.Id == workshop.Id));
        Assert.AreEqual(EntityState.Unchanged, deletedEntry.State);
        Assert.AreEqual(true, deletedEntry.CurrentValues["IsDeleted"]);
        Assert.True(unmodifiedEntries.All(x => (bool)x.CurrentValues["IsDeleted"] == false));
    }

    [Test]
    public async Task DeleteWorkshop_SaveChangesAsync_PerformsSoftDelete()
    {
        // Arrange
        using var dbContext = GetContext();
        var workshop = dbContext.Workshops.First();
        var initialWorkshopsCount = dbContext.Workshops.Count();
        var expectedWorkshopsCount = initialWorkshopsCount - 1;

        // Act
        dbContext.Entry(workshop).State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
        var deletedEntry = dbContext.Entry(workshop);
        var unmodifiedEntries = await dbContext.Workshops.Where(x => x.Id != workshop.Id).Select(x => dbContext.Entry(x)).ToListAsync();

        // Assert
        Assert.AreEqual(initialWorkshopsCount, dbContext.Workshops.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedWorkshopsCount, dbContext.Workshops.Count(x => !x.IsDeleted));
        Assert.False(dbContext.Workshops.Any(x => !x.IsDeleted && x.Id == workshop.Id));
        Assert.True(dbContext.Workshops.IgnoreQueryFilters().Any(x => x.Id == workshop.Id));
        Assert.AreEqual(EntityState.Unchanged, deletedEntry.State);
        Assert.AreEqual(true, deletedEntry.CurrentValues["IsDeleted"]);
        Assert.True(unmodifiedEntries.All(x => (bool)x.CurrentValues["IsDeleted"] == false));
    }

    [Test]
    public async Task DeleteWorkshop_HardDeleteSaveChangesAsync_PerformsHardDelete()
    {
        // Arrange
        using var dbContext = GetContext();
        var workshop = dbContext.Workshops.First();
        var initialWorkshopsCount = dbContext.Workshops.Count();
        var expectedWorkshopsCount = initialWorkshopsCount - 1;

        // Act
        dbContext.Entry(workshop).State = EntityState.Deleted;
        await dbContext.HardDeleteSaveChangesAsync();
        var entry = dbContext.Entry(workshop);

        // Assert
        Assert.AreEqual(expectedWorkshopsCount, dbContext.Workshops.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedWorkshopsCount, dbContext.Workshops.Count());
        Assert.False(dbContext.Workshops.Any(x => x.Id == workshop.Id));
        Assert.False(dbContext.Workshops.IgnoreQueryFilters().Any(x => x.Id == workshop.Id));
        Assert.AreEqual(EntityState.Detached, entry.State);
    }

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.AddRange(ProvidersGenerator.Generate(3));
        context.AddRange(UserGenerator.Generate(3));
        context.AddRange(WorkshopGenerator.Generate(3));
        await context.SaveChangesAsync();
    }
}
