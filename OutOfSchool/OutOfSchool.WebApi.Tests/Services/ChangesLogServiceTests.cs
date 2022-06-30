using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChangesLogServiceTests
    {
        private Mock<ILogger<ChangesLogService>> logger;
        private Mock<IMapper> mapper;
        private Mock<IChangesLogRepository> changesLogRepository;
        private Mock<IProviderRepository> providerRepository;
        private Mock<IApplicationRepository> applicationRepository;
        private Mock<IEntityRepository<ProviderAdminChangesLog>> providerAdminChangesLogRepository;
        private Mock<IValueProjector> valueProjector;

        private User user;
        private Provider provider;
        private Workshop workshop;
        private Application application;

        [SetUp]
        public void SetUp()
        {
            user = UserGenerator.Generate();
            provider = ProvidersGenerator.Generate();
            provider.Institution = InstitutionsGenerator.Generate();
            workshop = WorkshopGenerator.Generate();
            workshop.Address = AddressGenerator.Generate();
            workshop.Provider = provider;
            application = new Application()
            {
                Id = new Guid("6d4caeae-f0c3-492e-99b0-c8c105693376"),
                WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                Workshop = workshop,
                CreationTime = new DateTimeOffset(2022, 01, 12, 12, 34, 15, TimeSpan.Zero),
                Status = ApplicationStatus.Pending,
                ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
            };

            logger = new Mock<ILogger<ChangesLogService>>();
            mapper = new Mock<IMapper>(MockBehavior.Strict);
            changesLogRepository = new Mock<IChangesLogRepository>(MockBehavior.Strict);
            providerRepository = new Mock<IProviderRepository>(MockBehavior.Strict);
            applicationRepository = new Mock<IApplicationRepository>(MockBehavior.Strict);
            providerAdminChangesLogRepository = new Mock<IEntityRepository<ProviderAdminChangesLog>>(MockBehavior.Strict);
            valueProjector = new Mock<IValueProjector>();
        }

        #region AddEntityChangesToDbContext
        [Test]
        public void AddEntityChangesToDbContext_WhenTrackingIsEnabledForTheEntity_AddsToContext()
        {
            // Arrange
            var options = CreateChangesLogOptions();
            changesLogRepository.Setup(repo => repo.AddChangesLogToDbContext(
                It.IsAny<Provider>(),
                It.IsAny<string>(),
                options.Value.TrackedProperties["Provider"],
                It.IsAny<Func<Type, object, string>>()))
                .Returns(new List<ChangesLog> { new ChangesLog() });
            var changesLogService = GetChangesLogService();

            // Act
            var result = changesLogService.AddEntityChangesToDbContext(new Provider(), user.Id);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void AddEntityChangesToDbContext_WhenTrackingIsNotEnabledForTheEntity_DoesNotLogChanges()
        {
            // Arrange
            var changesLogService = GetChangesLogService();

            // Act
            var result = changesLogService.AddEntityChangesToDbContext(new Address(), user.Id);

            // Assert
            Assert.AreEqual(0, result);
        }
        #endregion

        #region GetChangesLog
        [Test]
        public async Task GetProviderChangesLog_WhenCalled_ReturnsSearchResult()
        {
            // Arange
            var changesLogService = GetChangesLogService();
            var request = new ProviderChangesLogRequest();

            var entitiesCount = 5;
            var totalAmount = 10;
            var changesMock = Enumerable.Range(1, entitiesCount)
                .Select(x => new ChangesLog { Id = x, EntityIdGuid = provider.Id, User = user })
                .AsQueryable()
                .BuildMock();
            var providersMock = new List<Provider> { provider }
                .AsQueryable()
                .BuildMock();

            mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ProviderChangesLogRequest>()))
                .Returns(new ChangesLogFilter());
            mapper.Setup(m => m.Map<ShortUserDto>(user))
                .Returns(new ShortUserDto { Id = user.Id });

            changesLogRepository
                .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
                .Returns(Task.FromResult(totalAmount));
            changesLogRepository
                .Setup(repo => repo.Get(
                    request.From,
                    request.Size,
                    string.Empty,
                    It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                    It.IsAny< Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                    It.IsAny<bool>()))
                .Returns(changesMock.Object);
            providerRepository.Setup(repo => repo.Get(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Provider, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>(),
                    It.IsAny<bool>()))
                .Returns(providersMock.Object);

            // Act
            var result = await changesLogService.GetProviderChangesLogAsync(request);

            // Assert
            Assert.AreEqual(totalAmount, result.TotalAmount);
            Assert.AreEqual(entitiesCount, result.Entities.Count);
            Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
            Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
            Assert.True(result.Entities.Any(x => x.ProviderCity == provider.LegalAddress.City));
            Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        }

        [Test]
        public async Task GetApplicationChangesLog_WhenCalled_ReturnsSearchResult()
        {
            // Arange
            var changesLogService = GetChangesLogService();
            var request = new ApplicationChangesLogRequest();

            var entitiesCount = 5;
            var totalAmount = 10;
            var changesMock = Enumerable.Range(1, entitiesCount)
                .Select(x => new ChangesLog { Id = x, EntityIdGuid = application.Id, User = user })
                .AsQueryable()
                .BuildMock();
            var applicationsMock = new List<Application> { application }
                .AsQueryable()
                .BuildMock();

            mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ApplicationChangesLogRequest>()))
                .Returns(new ChangesLogFilter());
            mapper.Setup(m => m.Map<ShortUserDto>(user))
                .Returns(new ShortUserDto { Id = user.Id });

            changesLogRepository
                .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
                .Returns(Task.FromResult(totalAmount));
            changesLogRepository
                .Setup(repo => repo.Get(
                    request.From,
                    request.Size,
                    string.Empty,
                    It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                    It.IsAny< Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                    It.IsAny<bool>()))
                .Returns(changesMock.Object);
            applicationRepository.Setup(repo => repo.Get(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                    It.IsAny<bool>()))
                .Returns(applicationsMock.Object);

            // Act
            var result = await changesLogService.GetApplicationChangesLogAsync(request);

            // Assert
            Assert.AreEqual(totalAmount, result.TotalAmount);
            Assert.AreEqual(entitiesCount, result.Entities.Count);
            Assert.True(result.Entities.All(x => x.ApplicationId == application.Id));
            Assert.True(result.Entities.All(x => x.WorkshopTitle == application.Workshop.Title));
            Assert.True(result.Entities.All(x => x.WorkshopCity == application.Workshop.Address.City));
            Assert.True(result.Entities.All(x => x.ProviderTitle == application.Workshop.ProviderTitle));
            Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        }

        [Test]
        public async Task GetProviderAdminChangesLog_WhenCalled_ReturnsSearchResult()
        {
            // Arange
            var changesLogService = GetChangesLogService();
            var request = new ProviderAdminChangesLogRequest();

            var entitiesCount = 5;
            var totalAmount = 10;
            var changesMock = Enumerable.Range(1, entitiesCount)
                .Select(x => new ProviderAdminChangesLog
                {
                    Id = x,
                    ProviderId = provider.Id,
                    ProviderAdminUser = UserGenerator.Generate(),
                    User = user,
                    Provider = provider,
                    ManagedWorkshop = workshop,
                })
                .AsQueryable()
                .BuildMock();

            mapper.Setup(m => m.Map<ShortUserDto>(user))
                .Returns(new ShortUserDto { Id = user.Id });

            providerAdminChangesLogRepository
                .Setup(repo => repo.Count(It.IsAny<Expression<Func<ProviderAdminChangesLog, bool>>>()))
                .Returns(Task.FromResult(totalAmount));
            providerAdminChangesLogRepository
                .Setup(repo => repo.Get(
                    request.From,
                    request.Size,
                    string.Empty,
                    It.IsAny<Expression<Func<ProviderAdminChangesLog, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<ProviderAdminChangesLog, object>>, SortDirection>>(),
                    It.IsAny<bool>()))
                .Returns(changesMock.Object);

            // Act
            var result = await changesLogService.GetProviderAdminChangesLogAsync(request);

            // Assert
            Assert.AreEqual(totalAmount, result.TotalAmount);
            Assert.AreEqual(entitiesCount, result.Entities.Count);
            Assert.True(result.Entities.All(x => x.ProviderTitle == provider.FullTitle));
            Assert.True(result.Entities.All(x => x.InstitutionTitle == provider.Institution.Title));
            Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        }
        #endregion

        private IOptions<ChangesLogConfig> CreateChangesLogOptions() =>
            Options.Create(new ChangesLogConfig
            {
                TrackedProperties = new Dictionary<string, string[]>
                {
                    { "Provider", new[] { "FullTitle", "EdrpouIpn", "Director", "LegalAddress" } },
                },
            });

        private IChangesLogService GetChangesLogService()
            => new ChangesLogService(
                CreateChangesLogOptions(),
                changesLogRepository.Object,
                providerRepository.Object,
                applicationRepository.Object,
                providerAdminChangesLogRepository.Object,
                logger.Object,
                mapper.Object,
                valueProjector.Object);
    }
}
