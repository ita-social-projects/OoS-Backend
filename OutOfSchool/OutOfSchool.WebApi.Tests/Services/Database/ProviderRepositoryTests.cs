using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
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

        AddProviderAdmins(providers);

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
        var expectedAddressesCount = context.Addresses.Count() - 2; // 2 = Legal + Actual
        var expectedProviderAdminsCount = context.ProviderAdmins.Count() - provider.ProviderAdmins.Count;

        // Act
        await providerRepository.Delete(provider);
        var workshops = context.Workshops
            .IgnoreQueryFilters()
            .Where(x => x.ProviderId == provider.Id)
            .Select(x => context.Entry(x))
            .ToList();
        var providerAdmins = context.ProviderAdmins
            .IgnoreQueryFilters()
            .Where(x => x.ProviderId == provider.Id)
            .Select(x => context.Entry(x))
            .ToList();

        // Assert
        Assert.AreEqual(initialProvidersCount, context.Providers.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedProvidersCount, context.Providers.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedAddressesCount, context.Addresses.Count());
        Assert.AreEqual(expectedWorkshopsCount, context.Workshops.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedProviderAdminsCount, context.ProviderAdmins.Count());
        Assert.False(context.Workshops.Any(x => !x.IsDeleted && x.ProviderId == provider.Id));
        Assert.True(context.Workshops.IgnoreQueryFilters().Any(x => x.ProviderId == provider.Id));
        Assert.False(context.ProviderAdmins.Any(x => x.ProviderId == provider.Id));
        Assert.True(context.ProviderAdmins.IgnoreQueryFilters().Any(x => x.ProviderId == provider.Id));
        Assert.AreEqual(EntityState.Unchanged, context.Entry(provider).State);
        Assert.AreEqual(true, context.Entry(provider).CurrentValues["IsDeleted"]);
        Assert.True(workshops.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
        Assert.True(providerAdmins.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
    }

    #endregion

    #region private

    private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(dbContextOptions);

    private IProviderRepository GetProviderRepository(OutOfSchoolDbContext dbContext)
        => new ProviderRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.AddRange(providers);
        await context.SaveChangesAsync();
    }

    private void AddProviderAdmins(List<Provider> providers)
    {
        providers.ForEach(p =>
        {
            p.ProviderAdmins = new List<ProviderAdmin>
            {
                { new ProviderAdmin { Id = (Guid.NewGuid().ToString(), p.Id), IsDeputy = true, } },
                {
                    new ProviderAdmin
                    {
                        Id = (Guid.NewGuid().ToString(), p.Id),
                        IsDeputy = false,
                        ManagedWorkshops = p.Workshops.ToList(),
                    }
                },
            };
        });
    }

    #endregion
}
