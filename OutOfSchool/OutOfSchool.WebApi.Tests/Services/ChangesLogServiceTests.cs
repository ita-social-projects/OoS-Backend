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
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChangesLogServiceTests
    {
        private Mock<ILogger<ChangesLogService>> logger;
        private Mock<IMapper> mapper;
        private Mock<IChangesLogRepository> changesLogRepository;

        private User user;

        [SetUp]
        public void SetUp()
        {
            user = UserGenerator.Generate();

            logger = new Mock<ILogger<ChangesLogService>>();
            mapper = new Mock<IMapper>();
            changesLogRepository = new Mock<IChangesLogRepository>(MockBehavior.Strict);
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
            changesLogRepository.Setup(repo => repo.AddEntityAddressChangesLogToDbContext(
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
            var addressPropertyName = "LegalAddress";
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
        public async Task GetChangesLog_WhenCalled_ReturnsSearchResult()
        {
            // Arange
            var changesLogService = GetChangesLogService();
            var filter = new ChangesLogFilter();

            var entitiesCount = 5;
            var totalAmount = 10;
            var changesMock = Enumerable.Range(1, entitiesCount)
                .Select(x => new ChangesLog { Id = x })
                .AsQueryable()
                .BuildMock();

            mapper.Setup(m => m.Map<IReadOnlyCollection<ChangesLogDto>>(It.IsAny<IReadOnlyCollection<ChangesLog>>()))
                .Returns(changesMock.Object.Select(x => new ChangesLogDto()).ToList());

            changesLogRepository
                .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
                .Returns(Task.FromResult(totalAmount));
            changesLogRepository
                .Setup(repo => repo.Get(
                    filter.From,
                    filter.Size,
                    "User",
                    It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                    It.IsAny<Expression<Func<ChangesLog, dynamic>>>(),
                    It.IsAny<bool>()))
                .Returns(changesMock.Object);

            // Act
            var result = await changesLogService.GetChangesLog(filter);

            // Assert
            Assert.AreEqual(totalAmount, result.TotalAmount);
            Assert.AreEqual(entitiesCount, result.Entities.Count);
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
            => new ChangesLogService(CreateChangesLogOptions(), changesLogRepository.Object, logger.Object, mapper.Object);
    }
}
