﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ProviderServiceTests
    {
        private ProviderService providerService;

        private Mock<IProviderRepository> providersRepositoryMock;
        private Mock<IProviderAdminRepository> providerAdminRepositoryMock;
        private Mock<IEntityRepository<User>> usersRepositoryMock;
        private Mock<IRatingService> ratingService;
        private IMapper mapper;
        private Mock<INotificationService> notificationService;
        private Mock<IProviderAdminService> providerAdminService;

        private List<Provider> fakeProviders;
        private User fakeUser;

        [SetUp]
        public void SetUp()
        {
            fakeProviders = ProvidersGenerator.Generate(10);
            fakeUser = UserGenerator.Generate();

            providersRepositoryMock = CreateProvidersRepositoryMock(fakeProviders);

            // TODO: configure mock and writer tests for provider admins
            providerAdminRepositoryMock = new Mock<IProviderAdminRepository>();
            usersRepositoryMock = CreateUsersRepositoryMock(fakeUser);
            var addressRepo = new Mock<IEntityRepository<Address>>();
            ratingService = new Mock<IRatingService>();
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            var logger = new Mock<ILogger<ProviderService>>();
            var workshopServicesCombiner = new Mock<IWorkshopServicesCombiner>();
            var providerImagesService = new Mock<IImageDependentEntityImagesInteractionService<Provider>>();
            var changesLogService = new Mock<IChangesLogService>();
            notificationService = new Mock<INotificationService>(MockBehavior.Strict);
            providerAdminService = new Mock<IProviderAdminService>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<Util.MappingProfile>());
            mapper = config.CreateMapper();

            providerService = new ProviderService(
                providersRepositoryMock.Object,
                usersRepositoryMock.Object,
                ratingService.Object,
                logger.Object,
                localizer.Object,
                mapper,
                addressRepo.Object,
                workshopServicesCombiner.Object,
                providerAdminRepositoryMock.Object,
                providerImagesService.Object,
                changesLogService.Object,
                notificationService.Object,
                providerAdminService.Object);
        }

        [TestCase(null, ProviderLicenseStatus.NotProvided)]
        [TestCase("1234567890", ProviderLicenseStatus.Pending)]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity(string license, ProviderLicenseStatus expectedLicenseStatus)
        {
            // Arrange
            var dto = ProviderDtoGenerator.Generate();
            dto.License = license;
            dto.Status = ProviderStatus.Approved;

            var expected = mapper.Map<ProviderDto>(dto);
            expected.Status = ProviderStatus.Pending;
            expected.License = license;
            expected.LicenseStatus = expectedLicenseStatus;
            expected.CoverImageId = null;
            expected.ImageIds = new List<string>();
            expected.ProviderSectionItems = Enumerable.Empty<ProviderSectionItemDto>();

            providersRepositoryMock.Setup(r => r.Create(It.IsAny<Provider>()))
                .ReturnsAsync((Provider p) => p);
            providersRepositoryMock.Setup(r => r.GetById(expected.Id))
                .ReturnsAsync(mapper.Map<Provider>(expected));
            notificationService
                .Setup(s => s.Create(
                    NotificationType.Provider,
                    NotificationAction.Create,
                    expected.Id,
                    providerService,
                    It.IsAny<Dictionary<string, string>>(),
                    null))
                .Callback((
                    NotificationType type,
                    NotificationAction action,
                    Guid objectId,
                    INotificationReciever service,
                    Dictionary<string, string> additionalData,
                    string groupedData) => service.GetNotificationsRecipientIds(action, additionalData, objectId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.Create(dto).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public async Task Create_ValidEntity_UpdatesOwnerIsRegisteredField()
        {
            // Arrange
            var provider = ProviderDtoGenerator.Generate();
            provider.UserId = fakeUser.Id;
            fakeUser.IsRegistered = false;

            // Act
            await providerService.Create(provider).ConfigureAwait(false);

            // Assert
            Assert.That(fakeUser.IsRegistered, Is.True);
        }

        [Test]
        public void Create_WhenInputProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await providerService.Create(default).ConfigureAwait(false));
        }

        [Test]
        public void Create_WhenUserIdExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var providerToBeCreated = ProviderDtoGenerator.Generate();
            fakeProviders.RandomItem().UserId = providerToBeCreated.UserId;

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await providerService.Create(providerToBeCreated).ConfigureAwait(false));
        }

        [Test]
        public async Task Create_WhenActualAddressIsTheSameAsLegal_ActualAddressIsCleared()
        {
            // Arrange
            var expectedEntity = ProviderDtoGenerator.Generate();
            expectedEntity.ActualAddress = expectedEntity.LegalAddress;
            Provider receivedProvider = default;
            providersRepositoryMock.Setup(x => x.Create(It.IsAny<Provider>())).
                Callback<Provider>(p => receivedProvider = p);

            // Act
            await providerService.Create(expectedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(receivedProvider.ActualAddress, Is.Null);
        }

        [Test]
        public async Task Create_WhenActualAddressDiffersFromTheLegal_ActualAddressIsSaved()
        {
            // Arrange
            var expectedEntity = ProvidersGenerator.Generate();

            Provider receivedProvider = default;
            providersRepositoryMock.Setup(x => x.Create(It.IsAny<Provider>())).Callback<Provider>(p => receivedProvider = p);

            // Act
            await providerService.Create(mapper.Map<ProviderDto>(expectedEntity));// expectedEntity.ToModel());

            // Assert
            Assert.That(receivedProvider.ActualAddress, Is.Not.Null);
            TestHelper.AssertDtosAreEqual(expectedEntity.ActualAddress, receivedProvider.ActualAddress);
        }

        [Test]
        public void Create_WhenProviderWithTheSameEdrpouOrIpnExist_ThrowsInvalidOperationException()
        {
            // Arrange
            providersRepositoryMock.Setup(r => r.SameExists(It.IsAny<Provider>())).Returns(true);
            var randomProvider = mapper.Map<ProviderDto>(fakeProviders.RandomItem());// fakeProviders.RandomItem().ToModel();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await providerService.Create(randomProvider));
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllEntities()
        {
            // Arrange
            var expectedCollection = fakeProviders.Select(p => mapper.Map<ProviderDto>(p)).ToList(); // expected collection of dto's to return
            var fakeRatings = RatingsGenerator.GetAverageRatingForRange(fakeProviders.Select(p => p.Id)); // expected ratings
            expectedCollection.ForEach(p => p.Rating = fakeRatings.Where(r => r.Key == p.Id)
                .Select(p => p.Value.Item1).FirstOrDefault()); // seed rating to use in assertion
            expectedCollection.ForEach(p => p.NumberOfRatings = fakeRatings.Where(r => r.Key == p.Id)
                .Select(p => p.Value.Item2).FirstOrDefault()); // seed number of ratings to use in assertion

            providersRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(fakeProviders);
            ratingService
                .Setup(r => r.GetAverageRatingForRange(It.IsAny<IEnumerable<Guid>>(), RatingType.Provider))
                .Returns(fakeRatings);

            // Act
            var actualProviders = await providerService.GetAll().ConfigureAwait(false);

            // Assert
            TestHelper.AssertTwoCollectionsEqualByValues(expectedCollection, actualProviders);
        }

        [Test]
        public async Task GetByFilter_WhenCalled_ReturnsEntities()
        {
            // Arrange
            var filter = new ProviderFilter();
            var providersMock = fakeProviders.AsQueryable().BuildMock();

            var fakeRatings = RatingsGenerator.GetAverageRatingForRange(fakeProviders.Select(p => p.Id)); // expected ratings
            var expected = fakeProviders
                .Select(p => mapper.Map<ProviderDto>(p))
                .ToList();
            expected
                .ForEach(p => p.Rating = fakeRatings
                    .Where(r => r.Key == p.Id)
                    .Select(p => p.Value.Item1)
                    .FirstOrDefault());
            expected
                .ForEach(p => p.NumberOfRatings = fakeRatings
                    .Where(r => r.Key == p.Id)
                    .Select(p => p.Value.Item2)
                    .FirstOrDefault());
            providersRepositoryMock
                .Setup(repo => repo.Count(It.IsAny<Expression<Func<Provider, bool>>>()))
                .ReturnsAsync(fakeRatings.Count);
            providersRepositoryMock
                .Setup(repo => repo.Get(
                    filter.From,
                    filter.Size,
                    string.Empty,
                    It.IsAny<Expression<Func<Provider, bool>>>(),
                    It.IsAny<Expression<Func<Provider, dynamic>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(providersMock.Object);
            ratingService.Setup(r => r.GetAverageRatingForRange(It.IsAny<IEnumerable<Guid>>(), RatingType.Provider))
                .Returns(fakeRatings);

            // Act
            var result = await providerService.GetByFilter(filter).ConfigureAwait(false);

            // Assert
            TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
            Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
        }

        [Test]
        public async Task GetById_WhenIdIsValid_ReturnsEntity()
        {
            // Arrange
            var existingProvider = fakeProviders.RandomItem();

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(existingProvider);

            // Act
            var actualProviderDto = await providerService.GetById(existingProvider.Id).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(mapper.Map<ProviderDto>(existingProvider), actualProviderDto);
        }

        [Test]
        public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
        {
            // Arrange
            var noneExistingId = Guid.NewGuid();

            // Act
            var result = await providerService.GetById(noneExistingId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Update_UserCanUpdateExistingEntityOfRelatedProvider_UpdatesExistedEntity()
        {
            // Arrange
            var provider = fakeProviders.RandomItem();
            provider.Status = ProviderStatus.Pending;

            var updatedTitle = Guid.NewGuid().ToString();
            var providerToUpdateDto = mapper.Map<ProviderDto>(provider);
            providerToUpdateDto.FullTitle = updatedTitle;

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Provider>>>()))
                .Returns((Func<Task<Provider>> f) => f.Invoke());
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);
            notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                providerService,
                It.IsAny<Dictionary<string, string>>(),
                null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(providerToUpdateDto, result);
        }

        [TestCase(ProviderStatus.Pending)]
        [TestCase(ProviderStatus.Editing)]
        [TestCase(ProviderStatus.Approved)]
        public async Task Update_UserTriesToChangeStatus_StatusIsNotChanged(ProviderStatus initialStatus)
        {
            // Arrange
            var provider = fakeProviders.RandomItem();
            var updatedTitle = Guid.NewGuid().ToString();
            provider.Status = initialStatus;

            var providerToUpdateDto = mapper.Map<ProviderDto>(provider);
            providerToUpdateDto.Status = ProviderStatus.Approved;
            providerToUpdateDto.ShortTitle = updatedTitle;

            var expected = mapper.Map<ProviderDto>(provider);
            expected.Status = initialStatus;
            expected.ShortTitle = updatedTitle;

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [TestCase(ProviderStatus.Pending)]
        [TestCase(ProviderStatus.Editing)]
        [TestCase(ProviderStatus.Approved)]
        public async Task Update_UserChangesFullTitle_StatusIsChangedToPending(ProviderStatus initialStatus)
        {
            // Arrange
            var provider = fakeProviders.RandomItem();
            var updatedTitle = Guid.NewGuid().ToString();
            provider.Status = initialStatus;

            var providerToUpdateDto = mapper.Map<ProviderDto>(provider);
            providerToUpdateDto.FullTitle = updatedTitle;

            var expected = mapper.Map<ProviderDto>(provider);
            expected.Status = ProviderStatus.Pending;
            expected.FullTitle = updatedTitle;

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Provider>>>()))
                .Returns((Func<Task<Provider>> f) => f.Invoke());
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);
            notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                providerService,
                It.IsAny<Dictionary<string, string>>(),
                null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [TestCase(ProviderStatus.Pending)]
        [TestCase(ProviderStatus.Editing)]
        [TestCase(ProviderStatus.Approved)]
        public async Task Update_UserChangesEdrpouIpn_StatusIsChangedToPending(ProviderStatus initialStatus)
        {
            // Arrange
            var provider = fakeProviders.RandomItem();
            provider.EdrpouIpn = 1234512345;
            var updatedEdrpouIpn = "1234567890";
            provider.Status = initialStatus;

            var providerToUpdateDto = mapper.Map<ProviderDto>(provider);
            providerToUpdateDto.EdrpouIpn = updatedEdrpouIpn;

            var expected = mapper.Map<ProviderDto>(provider);
            expected.Status = ProviderStatus.Pending;
            expected.EdrpouIpn = updatedEdrpouIpn;

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);
            notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                providerService,
                It.IsAny<Dictionary<string, string>>(),
                null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [TestCase(ProviderLicenseStatus.NotProvided)]
        [TestCase(ProviderLicenseStatus.Pending)]
        [TestCase(ProviderLicenseStatus.Approved)]
        public async Task Update_UserChangesLicense_LicenseStatusIsChangedToPending(ProviderLicenseStatus initialStatus)
        {
            // Arrange
            var provider = fakeProviders.RandomItem();
            provider.License = "1234512345";
            var updatedLicense = "1234567890";
            provider.LicenseStatus = initialStatus;

            var providerToUpdateDto = mapper.Map<ProviderDto>(provider);
            providerToUpdateDto.License = updatedLicense;

            var expected = mapper.Map<ProviderDto>(provider);
            expected.LicenseStatus = ProviderLicenseStatus.Pending;
            expected.License = updatedLicense;

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);
            notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                providerService,
                It.IsAny<Dictionary<string, string>>(),
                null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [TestCase(ProviderLicenseStatus.NotProvided)]
        [TestCase(ProviderLicenseStatus.Pending)]
        [TestCase(ProviderLicenseStatus.Approved)]
        public async Task Update_UserSetsLicenseToNull_LicenseStatusIsChangedToNotProvided(ProviderLicenseStatus initialStatus)
        {
            // Arrange
            var provider = fakeProviders.RandomItem();
            provider.License = "1234512345";
            string updatedLicense = null;
            provider.LicenseStatus = initialStatus;

            var providerToUpdateDto = mapper.Map<ProviderDto>(provider);
            providerToUpdateDto.License = updatedLicense;

            var expected = mapper.Map<ProviderDto>(provider);
            expected.LicenseStatus = ProviderLicenseStatus.NotProvided;
            expected.License = updatedLicense;

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public async Task Update_WhenUsersIdAndProvidersIdDoesntMatch_ReturnsNull()
        {
            // Arrange
            var changedEntity = ProviderDtoGenerator.Generate();
            var noneExistingUserId = Guid.NewGuid().ToString();

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(fakeProviders.RandomItem());

            // Act
            var result = await providerService.Update(changedEntity, noneExistingUserId).ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task Delete_WhenIdIsValid_CalledProvidersRepositoryDeleteMethod()
        {
            // Arrange
            var providerToDeleteDto = mapper.Map<ProviderDto>(fakeProviders.RandomItem());//fakeProviders.RandomItem().ToModel();
            var deleteMethodArguments = new List<Provider>();
            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(fakeProviders.Single(p => p.Id == providerToDeleteDto.Id));
            providersRepositoryMock.Setup(r => r.Delete(Capture.In(deleteMethodArguments)));

            // Act
            await providerService.Delete(providerToDeleteDto.Id).ConfigureAwait(false);
            var result = mapper.Map<ProviderDto>(deleteMethodArguments.Single());//deleteMethodArguments.Single().ToModel();

            // Assert
            TestHelper.AssertDtosAreEqual(providerToDeleteDto, result);
        }

        // TODO: providerService.Delete method should be fixed before

        [Test]
        public void Delete_WhenIdIsInvalid_ThrowsArgumentNullException()
        {
            // Arrange
            var fakeProviderInvalidId = Guid.NewGuid();
            var provider = new Provider()
            {
                Id = fakeProviderInvalidId,
            };
            providersRepositoryMock.Setup(p => p.Delete(provider)).Returns(Task.CompletedTask);
            providersRepositoryMock.Setup(p => p.GetById(provider.Id)).ThrowsAsync(new ArgumentNullException());

            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await providerService.Delete(fakeProviderInvalidId).ConfigureAwait(false));
        }

        [Test]
        public async Task UpdateStatus_WhenDtoIsValid_UpdatesProviderEntity()
        {
            // Arrange
            var provider = ProvidersGenerator.Generate();
            provider.Status = ProviderStatus.Pending;
            var dto = new ProviderStatusDto
            {
                ProviderId = provider.Id,
                Status = ProviderStatus.Approved,
            };

            providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(It.IsAny<int>());
            notificationService
                .Setup(s => s.Create(
                    NotificationType.Provider,
                    NotificationAction.Update,
                    dto.ProviderId,
                    providerService,
                    It.IsAny<Dictionary<string, string>>(),
                    null))
                .Callback((
                    NotificationType type,
                    NotificationAction action,
                    Guid objectId,
                    INotificationReciever service,
                    Dictionary<string, string> additionalData,
                    string groupedData) => service.GetNotificationsRecipientIds(action, additionalData, objectId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.UpdateStatus(dto, fakeUser.Id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(provider.Status, dto.Status);
        }

        [Test]
        public async Task UpdateStatus_WhenProviderIdIsNonExistent_ReturnsNull()
        {
            // Arrange
            var dto = new ProviderStatusDto
            {
                ProviderId = Guid.NewGuid(),
                Status = ProviderStatus.Approved,
            };

            providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
                .ReturnsAsync(null as Provider);

            // Act
            var result = await providerService.UpdateStatus(dto, fakeUser.Id).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateLicenseStatus_WhenDtoIsValid_UpdatesProviderEntity()
        {
            // Arrange
            var provider = ProvidersGenerator.Generate();
            provider.License = TestDataHelper.GetPositiveInt(10, 10).ToString();
            provider.LicenseStatus = ProviderLicenseStatus.Pending;
            var dto = new ProviderLicenseStatusDto
            {
                ProviderId = provider.Id,
                LicenseStatus = ProviderLicenseStatus.Approved,
            };

            providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
                .ReturnsAsync(provider);
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(It.IsAny<int>());
            notificationService
                .Setup(s => s.Create(
                    NotificationType.Provider,
                    NotificationAction.Update,
                    dto.ProviderId,
                    providerService,
                    It.IsAny<Dictionary<string, string>>(),
                    null))
                .Callback((
                    NotificationType type,
                    NotificationAction action,
                    Guid objectId,
                    INotificationReciever service,
                    Dictionary<string, string> additionalData,
                    string groupedData) => service.GetNotificationsRecipientIds(action, additionalData, objectId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await providerService.UpdateLicenseStatus(dto, fakeUser.Id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(provider.LicenseStatus, dto.LicenseStatus);
        }

        [Test]
        public async Task UpdateLicenseStatus_WhenProviderIdIsNonExistent_ReturnsNull()
        {
            // Arrange
            var dto = new ProviderLicenseStatusDto
            {
                ProviderId = Guid.NewGuid(),
                LicenseStatus = ProviderLicenseStatus.Approved,
            };

            providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
                .ReturnsAsync(null as Provider);

            // Act
            var result = await providerService.UpdateLicenseStatus(dto, fakeUser.Id).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void UpdateLicenseStatus_WhenProviderLicenseIsNull_ThrowsException()
        {
            // Arrange
            var provider = ProvidersGenerator.Generate();
            provider.License = null;
            provider.LicenseStatus = ProviderLicenseStatus.NotProvided;
            // provider.License = TestDataHelper.GetPositiveInt(10, 10).ToString();
            var dto = new ProviderLicenseStatusDto
            {
                ProviderId = provider.Id,
                LicenseStatus = ProviderLicenseStatus.Approved,
            };

            providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
                .ReturnsAsync(provider);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await providerService.UpdateLicenseStatus(dto, fakeUser.Id));
        }

        [Test]
        public void UpdateLicenseStatus_SetNotProvidedLicenseStatus_WhenProviderHasLicense_ThrowsException()
        {
            // Arrange
            var provider = ProvidersGenerator.Generate();
            provider.License = TestDataHelper.GetPositiveInt(10, 10).ToString();
            provider.LicenseStatus = ProviderLicenseStatus.Pending;
            var dto = new ProviderLicenseStatusDto
            {
                ProviderId = provider.Id,
                LicenseStatus = ProviderLicenseStatus.NotProvided,
            };

            providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
                .ReturnsAsync(provider);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await providerService.UpdateLicenseStatus(dto, fakeUser.Id));
        }

        private static Mock<IEntityRepository<User>> CreateUsersRepositoryMock(User fakeUser)
        {
            var usersRepository = new Mock<IEntityRepository<User>>();
            usersRepository.Setup(r => r.GetAll()).Returns(Task.FromResult<IEnumerable<User>>(new List<User> { fakeUser }));
            usersRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), string.Empty)).Returns(Task.FromResult<IEnumerable<User>>(new List<User> { fakeUser }));

            return usersRepository;
        }

        private static Mock<IProviderRepository> CreateProvidersRepositoryMock(IEnumerable<Provider> providersCollection)
        {
            var providersRepository = new Mock<IProviderRepository>();
            var userExistsResult = false;

            bool UserExist(string userId)
            {
                userExistsResult = providersCollection.Any(p => p.UserId.Equals(userId));
                return userExistsResult;
            }

            providersRepository.Setup(r => r.ExistsUserId(It.IsAny<string>()))
                .Callback<string>(user => UserExist(user))
                .Returns(() => userExistsResult);

            return providersRepository;
        }
    }
}