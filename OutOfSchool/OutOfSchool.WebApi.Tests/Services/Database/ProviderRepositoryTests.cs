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
public class ProviderRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    private List<Provider> providers;

    [SetUp]
    public async Task SetUp()
    {
        providers = ProvidersGenerator.Generate(3)
            .WithWorkshops();

        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    #region Delete

    [Test]
    public async Task Delete_SoftDeletes_Provider_DeletesRelatedEntities()
    {
        // Arrange
        using var context = GetContext();
        var providerRepository = GetProviderRepository(context);
        var initialProvidersCount = context.Providers.Count(x => !x.IsDeleted);
        var provider = context.Providers.First();
        var expectedProvidersCount = initialProvidersCount - 1;
        var expectedWorkshopsCount = context.Workshops.Count(x => !x.IsDeleted) - provider.Workshops.Count;
        
        // Act
        await providerRepository.Delete(provider);
        var workshops = context.Workshops
            .IgnoreQueryFilters()
            .Where(x => x.ProviderId == provider.Id)
            .Select(x => context.Entry(x))
            .ToList();

        // Assert
        Assert.AreEqual(initialProvidersCount, context.Providers.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedProvidersCount, context.Providers.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedWorkshopsCount, context.Workshops.Count(x => !x.IsDeleted));
        Assert.False(context.Workshops.Any(x => !x.IsDeleted && x.ProviderId == provider.Id));
        Assert.True(context.Workshops.IgnoreQueryFilters().Any(x => x.ProviderId == provider.Id));
        Assert.AreEqual(EntityState.Unchanged, context.Entry(provider).State);
        Assert.AreEqual(true, context.Entry(provider).CurrentValues["IsDeleted"]);
        Assert.True(workshops.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
    }

    [Test]
    public async Task CheckExistsByEdrpous_ReturnValues_ExcludeExistsValues()
    {
        // Arrange
        using var context = GetContext();
        var providerRepository = GetProviderRepository(context);

        var firstEdrpou = context.Providers.First().Edrpou;

        var data = new Dictionary<int, string>()
        {
            { 1, firstEdrpou },
            { 2, firstEdrpou + "1" },
            { 3, firstEdrpou + "2" },
        };

        var expectedResult = new List<int>()
        {
            1,
        };

        // Act
        var result = await providerRepository.CheckExistsByEdrpous(data).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }
    #endregion

    #region private

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IProviderRepository GetProviderRepository(TestOutOfSchoolDbContext dbContext)
        => new ProviderRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.AddRange(providers);
        await context.SaveChangesAsync();
    }

    #endregion
}
