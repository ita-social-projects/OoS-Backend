using System;
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
public class ProviderRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    private List<Provider> providers;

    [SetUp]
    public async Task SetUp()
    {
        providers = ProvidersGenerator.Generate(3)
            .WithWorkshops().WithUser();

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
        var provider = context.Providers.IncludeProperties("LegalAddress,ActualAddress").First();
        var expectedProvidersCount = initialProvidersCount - 1;
        var expectedWorkshopsCount = context.Workshops.Count(x => !x.IsDeleted) - provider.Workshops.Count;
        var expectedAddressesCount = context.Addresses.Count() - 2; // 2 = Legal + Actual
        var expectedProviderAdminsCount = context.Employees.Count() - provider.Employees.Count;

        // Act
        await providerRepository.Delete(provider);
        var workshops = context.Workshops
            .IgnoreQueryFilters()
            .Where(x => x.ProviderId == provider.Id)
            .Select(x => context.Entry(x))
            .ToList();
        var providerAdmins = context.Employees
            .IgnoreQueryFilters()
            .Where(x => x.ProviderId == provider.Id)
            .Select(x => context.Entry(x))
            .ToList();

        // Assert
        Assert.AreEqual(initialProvidersCount, context.Providers.IgnoreQueryFilters().Count());
        Assert.AreEqual(expectedProvidersCount, context.Providers.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedAddressesCount, context.Addresses.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedWorkshopsCount, context.Workshops.Count(x => !x.IsDeleted));
        Assert.AreEqual(expectedProviderAdminsCount, context.Employees.Count(x => !x.IsDeleted));
        Assert.False(context.Workshops.Any(x => !x.IsDeleted && x.ProviderId == provider.Id));
        Assert.True(context.Workshops.IgnoreQueryFilters().Any(x => x.ProviderId == provider.Id));
        Assert.False(context.Employees.Any(x => !x.IsDeleted && x.ProviderId == provider.Id));
        Assert.True(context.Employees.IgnoreQueryFilters().Any(x => x.ProviderId == provider.Id));
        Assert.AreEqual(EntityState.Unchanged, context.Entry(provider).State);
        Assert.AreEqual(true, context.Entry(provider).CurrentValues["IsDeleted"]);
        Assert.True(workshops.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
        Assert.True(providerAdmins.All(x => (bool)x.CurrentValues["IsDeleted"] == true));
    }

    [Test]
    public async Task CheckExistsByEdrpous_ReturnValues_ExcludeExistsValues()
    {
        // Arrange
        using var context = GetContext();
        var providerRepository = GetProviderRepository(context);

        var firstEdrpou = context.Providers.First().EdrpouIpn;

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

    [Test]
    public async Task CheckExistsByEmails_ReturnValues_ExcludeExistsValues()
    {
        // Arrange
        using var context = GetContext();
        var providerRepository = GetProviderRepository(context);

        var firstEmail = context.Providers.First().User.Email;

        var data = new Dictionary<int, string>()
        {
            { 1, firstEmail },
            { 2, "q" + firstEmail },
            { 3, "qq" + firstEmail },
        };

        var expectedResult = new List<int>()
        {
            1,
        };

        // Act
        var result = await providerRepository.CheckExistsByEmails(data).ConfigureAwait(false);

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

    private void AddProviderAdmins(List<Provider> providers)
    {
        providers.ForEach(p =>
        {
            p.Employees = new List<Employee>
            {
                { new Employee { Id = (Guid.NewGuid().ToString(), p.Id)} },
                {
                    new Employee
                    {
                        Id = (Guid.NewGuid().ToString(), p.Id),
                        ManagedWorkshops = p.Workshops.ToList(),
                    }
                },
            };
        });
    }

    #endregion
}
