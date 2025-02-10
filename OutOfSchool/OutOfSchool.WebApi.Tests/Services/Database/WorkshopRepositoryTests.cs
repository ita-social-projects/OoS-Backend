using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class WorkshopRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    private IReadOnlyCollection<Workshop> workshops;

    [SetUp]
    public async Task SetUp()
    {
        workshops = WorkshopGenerator.Generate(3)
            .WithAddress()
            .WithApplications()
            .WithTeachers()
            .WithProvider()
            .WithImages();

        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    #region Delete

    [Test]
    public async Task Delete_SoftDeletes_Workshop_DeletesRelatedEntities()
    {
        // Arrange
        using var context = GetContext();
        var workshopRepository = GetWorkshopRepository(context);
        var initialWorkshopsCount = context.Workshops.Count(x => !x.IsDeleted);
        var workshop = context.Workshops.IncludeProperties("Address").First();
        var expectedWorkshopsCount = initialWorkshopsCount - 1;
        var expectedApplicationsCount = context.Applications.Count(x => !x.IsDeleted) - workshop.Applications.Count;
        var expectedTeachersCount = context.Teachers.Count(x => !x.IsDeleted) - workshop.Teachers.Count;
        var expectedImagesCount = context.WorkshopImages.Count() - workshop.Images.Count;
        var expectedAddressesCount = context.Addresses.Count() - 1;

        // Act
        await workshopRepository.Delete(workshop);
        var applications = context.Applications
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted && x.WorkshopId == workshop.Id)
            .Select(x => context.Entry(x))
            .ToList();
        var teachers = context.Teachers
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted && x.WorkshopId == workshop.Id)
            .Select(x => context.Entry(x))
            .ToList();
        var images = context.WorkshopImages
            .IgnoreQueryFilters()
            .Where(x => x.EntityId == workshop.Id)
            .ToList();

        // Assert
        Assert.AreEqual(initialWorkshopsCount, context.Workshops.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedWorkshopsCount, context.Workshops.Count(x => !x.IsDeleted));
        Assert.NotZero(expectedApplicationsCount);
        Assert.NotZero(expectedTeachersCount);
        Assert.NotZero(expectedImagesCount);
        Assert.NotZero(expectedAddressesCount);
        Assert.AreEqual(expectedApplicationsCount, context.Applications.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedTeachersCount, context.Teachers.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedImagesCount, context.WorkshopImages.Count());
        Assert.AreEqual(expectedAddressesCount, context.Addresses.Count(x => !x.IsDeleted));
        Assert.False(context.Workshops.Any(x => !x.IsDeleted && x.Id == workshop.Id));
        Assert.True(context.Workshops.IgnoreQueryFilters().Any(x => x.Id == workshop.Id));
        Assert.AreEqual(EntityState.Unchanged, context.Entry(workshop).State);
        Assert.AreEqual(true, context.Entry(workshop).CurrentValues["IsDeleted"]);
        Assert.True(applications.All(x => x.State == EntityState.Unchanged));
        Assert.True(applications.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
        Assert.True(teachers.All(x => x.State == EntityState.Unchanged));
        Assert.True(teachers.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
        Assert.IsEmpty(images);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task GetWithNavigations_WhenGetValidEntity_ReturnValue(bool asNoTracking)
    {
        // Arrange
        using var context = GetContext();
        var workshopRepository = GetWorkshopRepository(context);
        var workshop = context.Workshops.First();

        // Act
        var expectedWorkshop = await workshopRepository.GetWithNavigations(workshop.Id, asNoTracking);

        // Assert
        Assert.IsNotNull(expectedWorkshop);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task GetWithNavigations_WhenGetValidEntity_ReturnEmpty(bool asNoTracking)
    {
        // Arrange
        using var context = GetContext();
        var workshopRepository = GetWorkshopRepository(context);
        var workshop = context.Workshops.First();

        // Act
        await workshopRepository.Delete(workshop);
        var expectedWorkshop = await workshopRepository.GetWithNavigations(workshop.Id, asNoTracking);

        // Assert
        Assert.IsNull(expectedWorkshop);
    }

    #endregion

    #region private

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IWorkshopRepository GetWorkshopRepository(TestOutOfSchoolDbContext dbContext)
        => new WorkshopRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.AddRange(workshops);
        await context.SaveChangesAsync();
    }

    #endregion
}
