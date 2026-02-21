using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class ApplicationRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB_Application")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    [Test]
    public async Task DeleteChildApplications_ShouldDeleteApplicationsSuccessfully()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetApplicationRepository(context);

        var testChild = new Child
        {
            Id = Guid.NewGuid(),
            DateOfBirth = DateTime.Now.AddYears(-5),
            Gender = Gender.Male,
        };

        var testApplications = ApplicationGenerator.Generate(3)
            .WithChild(testChild)
            .ToList();

        context.Children.Add(testChild);
        context.Applications.AddRange(testApplications);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteChildApplications(testChild.Id);

        // Assert
        var remainingApplications = await context.Applications
        .Where(a => a.ChildId == testChild.Id && !a.IsDeleted).ToListAsync();

        Assert.IsEmpty(remainingApplications, "Applications related to the specified childId should be deleted");
    }

    [Test]
    public async Task Update_Application_ShouldUpdateSuccessfully()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetApplicationRepository(context);
        var fakeApplication = ApplicationGenerator.Generate();

        context.Applications.Add(fakeApplication);
        await context.SaveChangesAsync();

        var updatedApplication = new Application
        {
            Id = fakeApplication.Id,
        };

        // Act
        var result = await repository.Update(updatedApplication);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(updatedApplication.Id, result.Id);
        var retrievedApplication = await context.Applications.FindAsync(updatedApplication.Id);
        Assert.NotNull(retrievedApplication);
    }

    [Test]
    public async Task GetCountByWorkshop_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetApplicationRepository(context);
        var testWorkshop = new Workshop
        {
            Id = Guid.NewGuid(),
        };

        var testApplications = ApplicationGenerator.Generate(3)
            .WithWorkshop(testWorkshop)
            .ToList();

        context.Workshops.Add(testWorkshop);
        context.Applications.AddRange(testApplications);
        await context.SaveChangesAsync();

        // Act
        var count = await repository.GetCountByWorkshop(testWorkshop.Id);

        // Assert
        Assert.AreEqual(3, count);
    }

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IApplicationRepository GetApplicationRepository(TestOutOfSchoolDbContext dbContext)
        => new ApplicationRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        await context.SaveChangesAsync();
    }
}
