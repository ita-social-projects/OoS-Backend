using NUnit.Framework;
using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using OutOfSchool.Common.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Models.BaseEntities;
using OutOfSchool.Tests.Common.DbContextTests;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.IntegrationTests;

[TestFixture]
public class TrackableEntityInterceptorTests
{
    [Test]
    public void SavingChanges_AddEntity_SetsCreatedAtAndCreatedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);
        var entity = new TestTrackableEntity
        {
            Name = "Test Entity",
        };
        context.TestTrackableEntities.Add(entity);

        // Act
        context.SaveChanges();

        // Assert
        Assert.IsNotNull(entity.CreatedAt);
        Assert.AreEqual(userId, entity.CreatedBy);
    }

    [Test]
    public void SavingChanges_ModifyEntity_SetsUpdatedAtAndModifiedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestTrackableEntity
        {
            Name = "Original",
        };
        context.TestTrackableEntities.Add(entity);
        context.SaveChanges();

        // Act
        entity.Name = "Modified";
        context.SaveChanges();

        // Assert
        Assert.IsNotNull(entity.ModifiedAt);
        Assert.AreEqual(userId, entity.ModifiedBy);
    }

    [Test]
    public async Task SavingChangesAsync_AddEntity_SetsCreatedAtAndCreatedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestTrackableEntity
        {
            Name = "Async Test Entity",
        };
        context.TestTrackableEntities.Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        Assert.IsNotNull(entity.CreatedAt);
        Assert.AreEqual(userId, entity.CreatedBy);
    }

    private static DbContextOptions<OutOfSchoolDbContext> GetDbContextOptions(string userId)
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(cu => cu.UserId).Returns(userId);

        var interceptor = new TrackableEntityInterceptor(currentUserMock.Object);

        var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        return options;
    }

    internal class TestTrackableEntity : TrackableBaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    internal class TestDbContext : TestOutOfSchoolDbContext
    {
        public TestDbContext(DbContextOptions<OutOfSchoolDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestTrackableEntity> TestTrackableEntities { get; set; }
    }
}
