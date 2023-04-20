using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class ChangesLogRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private ValueProjector valueProjector = new ValueProjector();

    private Provider provider;
    private User user;

    [SetUp]
    public async Task SetUp()
    {
        provider = ProvidersGenerator.Generate();
        provider.LegalAddress.Id = 6;
        provider.LegalAddressId = 6;
        provider.LegalAddress.CATOTTGId = 4970;
        provider.LegalAddress.CATOTTG.Id = 4970;
        user = UserGenerator.Generate();

        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    #region AddChangesLogToDbContext
    [Test]
    public async Task AddChangesLogToDbContext_WhenEntityIsModified_AddsLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "FullTitle", "EdrpouIpn", "Director", "LegalAddress" };
        var provider = await context.Providers.FirstAsync();

        var oldFullTitle = provider.FullTitle;
        var oldDirector = provider.Director;

        // Act
        provider.FullTitle += "new";
        provider.Director += "new";

        var newFullTitle = provider.FullTitle;
        var newDirector = provider.Director;

        var added = changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue);

        // Assert
        var fullTitleChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.FullTitle));
        var directorChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.Director));

        Assert.AreEqual(added.ToList(), context.ChangesLog.Local.ToList());
        Assert.AreEqual(2, context.ChangesLog.Local.Count);
        Assert.AreEqual(oldFullTitle, fullTitleChanges.OldValue);
        Assert.AreEqual(newFullTitle, fullTitleChanges.NewValue);
        Assert.AreEqual(user.Id, fullTitleChanges.UserId);
        Assert.AreEqual(oldDirector, directorChanges.OldValue);
        Assert.AreEqual(newDirector, directorChanges.NewValue);
        Assert.AreEqual(user.Id, directorChanges.UserId);
    }

    [Test]
    public async Task AddChangesLogToDbContext_LongValues_AreLimited()
    {
        // Arrange
        const int maxLength = 500;
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "FullTitle" };
        var provider = await context.Providers.FirstAsync();

        var oldFullTitle = provider.FullTitle;

        // Act
        provider.FullTitle += new string('-', maxLength * 2);

        var newFullTitle = provider.FullTitle;

        var added = changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue);

        // Assert
        var fullTitleChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.FullTitle));

        Assert.True(oldFullTitle.StartsWith(fullTitleChanges.OldValue));
        Assert.True(newFullTitle.StartsWith(fullTitleChanges.NewValue));
        Assert.AreEqual(Math.Min(maxLength, oldFullTitle.Length), fullTitleChanges.OldValue.Length);
        Assert.AreEqual(Math.Min(maxLength, newFullTitle.Length), fullTitleChanges.NewValue.Length);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenEntityIsNotModified_DoesNotAddLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "FullTitle", "EdrpouIpn", "Director", "LegalAddress" };
        var provider = await context.Providers.FirstAsync();

        // Act
        var added = changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue);

        // Assert
        Assert.IsEmpty(added);
        Assert.IsEmpty(context.ChangesLog.Local);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenEntityIsModified_LogsOnlyTrackedProperties()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "FullTitle", "EdrpouIpn", "LegalAddress" };
        var provider = await context.Providers.FirstAsync();

        var oldFullTitle = provider.FullTitle;

        // Act
        provider.FullTitle += "new";
        provider.Director += "new";

        var newFullTitle = provider.FullTitle;

        var added = changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue);

        // Assert
        var fullTitleChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.FullTitle));

        Assert.AreEqual(added.ToList(), context.ChangesLog.Local.ToList());
        Assert.AreEqual(1, context.ChangesLog.Local.Count);
        Assert.AreEqual(oldFullTitle, fullTitleChanges.OldValue);
        Assert.AreEqual(newFullTitle, fullTitleChanges.NewValue);
        Assert.AreEqual(user.Id, fullTitleChanges.UserId);
    }

    [Test]
    public void AddChangesLogToDbContext_InvalidEntityType_ThrowsException()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "FullTitle" };

        // Act
        var provider = new ProviderTest { FullTitle = "Full Title" };

        // Assert
        Assert.Throws(
            typeof(InvalidOperationException),
            () => changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue));
    }

    [Test]
    public void AddChangesLogToDbContext_TrackedPropertiesIsNull_ThrowsException()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        string[] trackedProperties = null;

        // Act
        var provider = new Provider();

        // Assert
        var ex = Assert.Throws(
            typeof(ArgumentNullException),
            () => changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue));
        Assert.AreEqual("Value cannot be null. (Parameter 'trackedProperties')", ex.Message);
    }

    [Test]
    public void AddChangesLogToDbContext_ValueProjectorIsNull_ThrowsException()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "FullTitle" };

        // Act
        var provider = new Provider();

        // Assert
        var ex = Assert.Throws(
            typeof(ArgumentNullException),
            () => changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, null));
        Assert.AreEqual("Value cannot be null. (Parameter 'valueProjector')", ex.Message);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenAddressIsModified_AddsLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "LegalAddress" };
        var provider = await context.Providers.Include(p => p.LegalAddress).ThenInclude(p => p.CATOTTG).FirstAsync();

        var oldLegalAddress = ProjectAddress(provider.LegalAddress);

        // Act
        //provider.LegalAddress.CATOTTGId += 1;
        provider.LegalAddress.BuildingNumber += "X";

        var newLegalAddress = ProjectAddress(provider.LegalAddress);

        var added = changesLogRepository.AddChangesLogToDbContext(
            provider,
            user.Id,
            trackedProperties,
            valueProjector.ProjectValue);

        // Assert
        var legalAddressChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.LegalAddress));

        Assert.AreEqual(added.ToList(), context.ChangesLog.Local.ToList());
        Assert.AreEqual(1, context.ChangesLog.Local.Count);
        Assert.AreEqual(oldLegalAddress, legalAddressChanges.OldValue);
        Assert.AreEqual(newLegalAddress, legalAddressChanges.NewValue);
        Assert.AreEqual(user.Id, legalAddressChanges.UserId);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenAddressIsNotModified_DoesNotAddLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "LegalAddress" };
        var provider = await context.Providers.Include(p => p.LegalAddress).FirstAsync();

        // Act
        var added = changesLogRepository.AddChangesLogToDbContext(
            provider,
            user.Id,
            trackedProperties,
            valueProjector.ProjectValue);

        // Assert
        Assert.IsEmpty(added);
        Assert.IsEmpty(context.ChangesLog.Local);
    }
    #endregion

    private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(dbContextOptions);

    private IChangesLogRepository GetChangesLogRepository(OutOfSchoolDbContext dbContext)
        => new ChangesLogRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Add(provider);
        context.Add(user);
        await context.SaveChangesAsync();
    }

    private string ProjectAddress(Address address) =>
        address == null
            ? null
            : $"{address.CATOTTGId}, {address.Street}, {address.BuildingNumber}";

    public class ProviderTest : IKeyedEntity
    {
        public string FullTitle { get; set; }

        public Address LegalAddress { get; set; }
    }
}