using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.IntegrationTests;

[TestFixture]
public class BusinessEntityInterceptorTests
{
    [Test]
    public void SavingChanges_AddEntity_SetsCreatedAtAndCreatedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);
        var entity = new TestEntity
        {
            Name = "Test Entity",
        };
        context.TestEntities.Add(entity);

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

        var entity = new TestEntity
        {
            Name = "Original",
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Act
        entity.Name = "Modified";
        context.SaveChanges();

        // Assert
        Assert.IsNotNull(entity.UpdatedAt);
        Assert.AreEqual(userId, entity.ModifiedBy);
    }

    [Test]
    public void SavingChanges_DeleteEntity_WhenNotSystemProtected_SetsDeleteDateAndDeletedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestEntity
        {
            Name = "Test Entity",
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Act
        context.TestEntities.Remove(entity);
        context.SaveChanges();

        // Assert
        Assert.IsNotNull(entity.DeleteDate);
        Assert.AreEqual(userId, entity.DeletedBy);
    }

    [Test]
    public void SavingChanges_DeleteEntity_WhenSystemProtected_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestEntity
        {
            Name = "Protected Entity",
        };

        SetPrivateField(entity, "_isSystemProtected", true);

        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Delete the entity
        context.TestEntities.Remove(entity);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
        Assert.AreEqual("Cannot delete a protected object", ex?.Message);
    }

    [Test]
    public void SavingChanges_ModifyEntityWithIsDeletedTrue_WhenNotSystemProtected_SetsDeleteDateAndDeletedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestEntity
        {
            Name = "Test Entity",
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Act
        entity.IsDeleted = true;
        context.SaveChanges();

        // Assert
        Assert.IsNotNull(entity.DeleteDate);
        Assert.AreEqual(userId, entity.DeletedBy);
    }

    /// <summary>
    /// This test is here for the future if we move soft delete logic to business entity entirely
    /// </summary>
    [Test]
    public void SavingChanges_ModifyEntityWithIsDeletedTrue_WhenSystemProtected_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestEntity
        {
            Name = "Protected Entity",
        };

        SetPrivateField(entity, "_isSystemProtected", true);

        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.IsDeleted = true;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
        Assert.AreEqual("Cannot delete a protected object", ex?.Message);
    }

    [Test]
    public async Task SavingChangesAsync_AddEntity_SetsCreatedAtAndCreatedBy()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var options = GetDbContextOptions(userId);

        using var context = new TestDbContext(options);

        var entity = new TestEntity
        {
            Name = "Async Test Entity",
        };
        context.TestEntities.Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        Assert.IsNotNull(entity.CreatedAt);
        Assert.AreEqual(userId, entity.CreatedBy);
    }

    [Test]
    public void SavingChanges_WithNullCurrentUser_SetsUserIdToEmptyString()
    {
        // Arrange
        var interceptor = new BusinessEntityInterceptor(null);
        var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        var expectedUserId = string.Empty;

        using var context = new TestDbContext(options);

        var entity = new TestEntity
        {
            Name = "Test Entity",
        };
        context.TestEntities.Add(entity);

        // Act
        context.SaveChanges();

        // Assert
        Assert.AreEqual(expectedUserId, entity.CreatedBy);
    }

    private static DbContextOptions<OutOfSchoolDbContext> GetDbContextOptions(string userId)
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(cu => cu.UserId).Returns(userId);

        var interceptor = new BusinessEntityInterceptor(currentUserMock.Object);

        var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        return options;
    }

    private static void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        fieldInfo.SetValue(obj, value);
    }

    internal class TestEntity : BusinessEntity
    {
        public string Name { get; set; }
    }

    internal class TestDbContext : TestOutOfSchoolDbContext
    {
        public TestDbContext(DbContextOptions<OutOfSchoolDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
}