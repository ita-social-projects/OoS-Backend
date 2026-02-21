using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class ChildRepositoryTests
{
    private static User user = UserGenerator.Generate();
    private static Parent parent = ParentGenerator.Generate().WithUserId(user.Id);
    private static SocialGroup socialGroup = new();

    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private List<Child> children;


    [SetUp]
    public async Task SetUp()
    {
        parent.User = user;
        children = ChildGenerator.Generate(3).WithParent(parent)
                                             .WithSocial(socialGroup);

        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    #region Delete

    [Test]
    public async Task Delete_SoftDeletes_Child_DeletesRelatedEntities()
    {
        // Arrange
        using var context = GetContext();
        var childRepository = GetChildRepository(context);
        var initialChildrenCount = context.Children.Count(x => !x.IsDeleted);
        IQueryable<Child> includeFunc(IQueryable<Child> p) =>
            p.Include(p => p.SocialGroups)
             .Include(p => p.Parent).ThenInclude(p => p.User);
        var child = context.Children.IncludeProperties(includeFunc, "SocialGroups, Parent, Parent.User").First();
        var expectedChildrenCount = initialChildrenCount - 1;

        // Act
        await childRepository.Delete(child);

        // Assert
        Assert.AreEqual(initialChildrenCount, context.Children.IgnoreQueryFilters().Count());
        Assert.AreEqual(EntityState.Unchanged, context.Entry(child).State);
        Assert.IsTrue((bool)context.Entry(child).CurrentValues["IsDeleted"]);
    }
    #endregion

    #region private

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IEntityRepository<Guid, Child> GetChildRepository(TestOutOfSchoolDbContext dbContext)
        => new ChildRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.AddRange(children);

        await context.SaveChangesAsync();
    }
    #endregion
}