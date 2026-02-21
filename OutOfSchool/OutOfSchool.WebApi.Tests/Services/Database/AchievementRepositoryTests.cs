using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class AchievementRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private Achievement achievement;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    #region Update

    [Test]
    public async Task Update_WhenExistsDeletedTeachers_RestoreIntersected()
    {
        // Arrange
        achievement = AchievementGenerator.Generate()
            .WithAchievementTeachers();

        var teachers = AchievementTeacherGenerator.Generate(3);
        teachers[0].IsDeleted = true;
        teachers[1].IsDeleted = true;
        achievement.Teachers = teachers;

        var children = ChildGenerator.Generate(2);
        achievement.Children = children;

        using var context = GetContext();
        context.Add(achievement);
        await context.SaveChangesAsync();

        var achievementRepository = GetAchievementRepository(context);
        var childrenIds = children.Select(x => x.Id).ToList();

        var teacherTitles = new List<string>() { teachers[1].Title, teachers[2].Title, "New Test Title" };

        // Act
        var expectedAchievement = await achievementRepository.Update(achievement, childrenIds, teacherTitles);

        // Assert
        Assert.AreEqual(3, expectedAchievement.Teachers.Count);

        var teachersFromDB = context.AchievementTeachers.Where(x => x.AchievementId == expectedAchievement.Id).ToList();
        Assert.AreEqual(1, teachersFromDB.Count(x => x.IsDeleted));
        Assert.AreEqual(3, teachersFromDB.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedAchievement.Teachers.Select(x => x.Title), teacherTitles);
    }

    #endregion

    #region private

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IAchievementRepository GetAchievementRepository(TestOutOfSchoolDbContext dbContext)
        => new AchievementRepository(dbContext);

    #endregion
}
