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

        private User user;
        private Provider provider;

        [SetUp]
        public void SetUp()
        {
            user = UserGenerator.Generate();
            provider = ProvidersGenerator.Generate();

            logger = new Mock<ILogger<ChangesLogService>>();
            mapper = new Mock<IMapper>(MockBehavior.Strict);
            changesLogRepository = new Mock<IChangesLogRepository>(MockBehavior.Strict);
            providerRepository = new Mock<IProviderRepository>(MockBehavior.Strict);
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
                options.Value.TrackedFields["Provider"]))
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

        #region AddEntityAddressChangesLogToDbContext
        [Test]
        public void AddEntityAddressChangesLogToDbContext_WhenTrackingIsEnabledForTheEntity_AddsToContext()
        {
            // Arrange
            var addressPropertyName = "LegalAddress";
            changesLogRepository.Setup(repo => repo.AddPropertyChangesLogToDbContext(
                It.IsAny<Provider>(),
                addressPropertyName,
                It.IsAny<Func<Address, string>>(),
                It.IsAny<string>()))
                .Returns(new ChangesLog());
            var changesLogService = GetChangesLogService();

            // Act
            var result = changesLogService.AddEntityAddressChangesLogToDbContext(
                new Provider(),
                addressPropertyName,
                user.Id);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void AddEntityAddressChangesLogToDbContext_WhenTrackingIsNotEnabledForTheEntity_DoesNotLogChanges()
        {
            // Arrange
            var changesLogService = GetChangesLogService();

            // Act
            var result = changesLogService.AddEntityAddressChangesLogToDbContext(
                new Address(),
                "LegalAddress",
                user.Id);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void AddEntityAddressChangesLogToDbContext_WhenTrackingIsNotEnabledForTheField_DoesNotLogChanges()
        {
            // Arrange
            var addressPropertyName = "ActualAddress";
            var changesLogService = GetChangesLogService();

            // Act
            var result = changesLogService.AddEntityAddressChangesLogToDbContext(
                new Provider(),
                addressPropertyName,
                user.Id);

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

            mapper.Setup(m => m.Map<IReadOnlyCollection<ProviderChangesLogDto>>(It.IsAny<IReadOnlyCollection<ChangesLog>>()))
                .Returns(changesMock.Object.Select(x => new ProviderChangesLogDto()).ToList());
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
                    "User",
                    It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                    It.IsAny<Expression<Func<ChangesLog, dynamic>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(changesMock.Object);
            providerRepository.Setup(repo => repo.Get(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Provider, bool>>>(),
                    It.IsAny<Expression<Func<Provider, It.IsAnyType>>>(),
                    It.IsAny<bool>(),
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
        #endregion

        private IOptions<ChangesLogConfig> CreateChangesLogOptions() =>
            Options.Create(new ChangesLogConfig
            {
                TrackedFields = new Dictionary<string, string[]>
                {
                    { "Provider", new[] { "FullTitle", "EdrpouIpn", "Director", "LegalAddress" } },
                },
            });

        private IChangesLogService GetChangesLogService()
            => new ChangesLogService(CreateChangesLogOptions(), changesLogRepository.Object, providerRepository.Object, logger.Object, mapper.Object);
    }
}
