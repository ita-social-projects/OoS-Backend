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

namespace OutOfSchool.WebApi.Tests.Services.Database
{
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
            var provider = await context.Providers.FirstOrDefaultAsync();

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
                .Single(x => x.EntityType == "Provider" && x.PropertyName == "FullTitle");
            var directorChanges = context.ChangesLog.Local
                .Single(x => x.EntityType == "Provider" && x.PropertyName == "Director");

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
            var provider = await context.Providers.FirstOrDefaultAsync();

            var oldFullTitle = provider.FullTitle;

            // Act
            provider.FullTitle += new string('-', maxLength * 2);

            var newFullTitle = provider.FullTitle;

            var added = changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue);

            // Assert
            var fullTitleChanges = context.ChangesLog.Local
                .Single(x => x.EntityType == "Provider" && x.PropertyName == "FullTitle");

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
            var provider = await context.Providers.FirstOrDefaultAsync();

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
            var provider = await context.Providers.FirstOrDefaultAsync();

            var oldFullTitle = provider.FullTitle;

            // Act
            provider.FullTitle += "new";
            provider.Director += "new";

            var newFullTitle = provider.FullTitle;

            var added = changesLogRepository.AddChangesLogToDbContext(provider, user.Id, trackedProperties, valueProjector.ProjectValue);

            // Assert
            var fullTitleChanges = context.ChangesLog.Local
                .Single(x => x.EntityType == "Provider" && x.PropertyName == "FullTitle");

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
        public async Task AddChangesLogToDbContext_WhenAddressIsModified_AddsLogToDbContext()
        {
            // Arrange
            using var context = GetContext();
            var changesLogRepository = GetChangesLogRepository(context);
            var trackedProperties = new[] { "LegalAddress" };
            var provider = await context.Providers.Include(p => p.LegalAddress).FirstOrDefaultAsync();

            var oldLegalAddress = ProjectAddress(provider.LegalAddress);

            // Act
            provider.LegalAddress.City += "new";
            provider.LegalAddress.BuildingNumber += "X";

            var newLegalAddress = ProjectAddress(provider.LegalAddress);

            var added = changesLogRepository.AddChangesLogToDbContext(
                provider,
                user.Id,
                trackedProperties,
                valueProjector.ProjectValue);

            // Assert
            var legalAddressChanges = context.ChangesLog.Local
                .Single(x => x.EntityType == "Provider" && x.PropertyName == "LegalAddress");

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
            var provider = await context.Providers.Include(p => p.LegalAddress).FirstOrDefaultAsync();

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
            : $"{address.District}, {address.City}, {address.Region}, {address.Street}, {address.BuildingNumber}";

        public class ProviderTest : IKeyedEntity
        {
            public string FullTitle { get; set; }

            public Address LegalAddress { get; set; }
        }
    }
}
