using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class ChangesLogRepositoryTests
{
    private const long InstitutionStatusId1 = 1;
    private const long InstitutionStatusId2 = 2;

    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private ValueProjector valueProjector = new ValueProjector();

    private Provider provider;
    private User user;

    private Institution institution1;
    private Institution institution2;

    [SetUp]
    public async Task SetUp()
    {
        institution1 = InstitutionsGenerator.Generate();
        institution2 = InstitutionsGenerator.Generate();

        provider = ProvidersGenerator.Generate();
        var contacts = new Contacts
        {
            Title = "Test",
            IsDefault = true,
            Address = ContactsAddressGenerator.Generate(),
        };
        contacts.Address.CATOTTGId = 4970;
        provider.Contacts = [contacts];
        provider.InstitutionId = institution1.Id;
        provider.InstitutionStatusId = InstitutionStatusId1; // Safe to use, because institution statuses would be seeded on context building phase
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
        var trackedProperties = new[] { "FullTitle", "ShortTitle", "Contacts.Address", };
        var provider = await context.Providers.FirstAsync();

        var oldFullTitle = provider.FullTitle;

        // Act
        provider.FullTitle += "new";

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
        var trackedProperties = new[] { "FullTitle", "ShortTitle", "Contacts.Address",};
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
        var trackedProperties = new[] { "FullTitle", "ShortTitle", "Contacts.Address", };
        var provider = await context.Providers.FirstAsync();

        var oldFullTitle = provider.FullTitle;

        // Act
        provider.FullTitle += "new";

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
        var trackedProperties = new[] { "Contacts.Address" };
        var provider = await context.Providers.Include(p => p.Contacts).ThenInclude(c => c.Address).ThenInclude(p => p.CATOTTG).FirstAsync();

        var oldContacts = this.ProjectAddress(provider.Contacts.Single(c => c.IsDefault).Address);

        // Act
        provider.Contacts.Single(c => c.IsDefault).Address.BuildingNumber += "X";

        var newLegalAddress = this.ProjectAddress(provider.Contacts.Single(c => c.IsDefault).Address);

        var added = changesLogRepository.AddChangesLogToDbContext(
            provider,
            user.Id,
            trackedProperties,
            valueProjector.ProjectValue);

        // Assert
        var contactsChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName.Contains(nameof(Provider.Contacts)));

        Assert.AreEqual(added.ToList(), context.ChangesLog.Local.ToList());
        Assert.AreEqual(1, context.ChangesLog.Local.Count);
        Assert.AreEqual(oldContacts, contactsChanges.OldValue);
        Assert.AreEqual(newLegalAddress, contactsChanges.NewValue);
        Assert.AreEqual(user.Id, contactsChanges.UserId);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenInstitutionIdIsModified_AddsLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "InstitutionId" };
        var provider = await context.Providers.Include(p => p.Institution).FirstAsync();

        var oldInstitution = institution1.Title;
        var newInstitution = institution2.Title;

        // Act
        provider.InstitutionId = institution2.Id;

        var added = changesLogRepository.AddChangesLogToDbContext(
            provider,
            user.Id,
            trackedProperties,
            valueProjector.ProjectValue);

        // Assert
        var legalChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.Institution));

        Assert.AreEqual(added.ToList(), context.ChangesLog.Local.ToList());
        Assert.AreEqual(1, context.ChangesLog.Local.Count);
        Assert.AreEqual(oldInstitution, legalChanges.OldValue);
        Assert.AreEqual(newInstitution, legalChanges.NewValue);
        Assert.AreEqual(user.Id, legalChanges.UserId);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenInstitutionStatusIdIsModified_AddsLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "InstitutionStatusId" };
        var provider = await context.Providers.Include(p => p.InstitutionStatus).FirstAsync();

        var oldInstitutionStatusName = (await context.InstitutionStatuses.SingleAsync(i => i.Id == InstitutionStatusId1)).Name;
        var newInstitutionStatusName = (await context.InstitutionStatuses.SingleAsync(i => i.Id == InstitutionStatusId2)).Name;

        // Act
        provider.InstitutionStatusId = InstitutionStatusId2; // Safe to use, because institution statuses would be seeded on context building phase

        var added = changesLogRepository.AddChangesLogToDbContext(
            provider,
            user.Id,
            trackedProperties,
            valueProjector.ProjectValue);

        // Assert
        var legalChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == nameof(Provider.InstitutionStatus));

        Assert.AreEqual(added.ToList(), context.ChangesLog.Local.ToList());
        Assert.AreEqual(1, context.ChangesLog.Local.Count);
        Assert.AreEqual(oldInstitutionStatusName, legalChanges.OldValue);
        Assert.AreEqual(newInstitutionStatusName, legalChanges.NewValue);
        Assert.AreEqual(user.Id, legalChanges.UserId);
    }

    [Test]
    public async Task AddChangesLogToDbContext_WhenAddressIsNotModified_DoesNotAddLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var trackedProperties = new[] { "Contacts.Address" };
        var provider = await context.Providers.Include(p => p.Contacts).FirstAsync();

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

    [Test]
    public async Task AddCreatingOfEntityToChangesLog_WhenEntityIsProvider_AddsLogToDbContext()
    {
        // Arrange
        using var context = GetContext();
        var changesLogRepository = GetChangesLogRepository(context);
        var provider = await context.Providers.FirstAsync();

        // Act
        var added = await changesLogRepository.AddCreatingOfEntityToChangesLog(provider, user.Id);

        // Assert
        var legalChanges = context.ChangesLog.Local
            .Single(x => x.EntityType == nameof(Provider) && x.PropertyName == "Creating");

        Assert.AreEqual(1, context.ChangesLog.Local.Count);
        Assert.AreEqual(added.Id, legalChanges.Id);
    }
    #endregion

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IChangesLogRepository GetChangesLogRepository(TestOutOfSchoolDbContext dbContext)
        => new ChangesLogRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Add(institution1);
        context.Add(institution2);
        context.Add(provider);
        context.Add(user);
        await context.SaveChangesAsync();
    }

    private string ProjectAddress(ContactsAddress address)
    {
        if (address == null)
        {
            return null;
        }
        
        return $"{address.CATOTTGId}, {address.Street}, {address.BuildingNumber}";
    }

    public class ProviderTest : IKeyedEntity
    {
        public string FullTitle { get; set; }

        public List<Contacts> Contacts { get; set; }
    }
}