using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ProviderServiceTests
    {
        private const string FakeUserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c";

        private IProviderService providerService;

        private Mock<IProviderRepository> providersRepositoryMock;
        private Mock<IEntityRepository<User>> usersRepositoryMock;
        private Mock<IRatingService> ratingService;
        private Mock<IMapper> mapper;

        private List<Provider> fakeProviders;
        private User fakeUser;

        [SetUp]
        public void SetUp()
        {
            fakeProviders = ProvidersGenerator.Generate(10);
            fakeUser = CreateFakeUser();

            providersRepositoryMock = CreateProvidersRepositoryMock(fakeProviders);
            usersRepositoryMock = CreateUsersRepositoryMock(fakeUser);
            var addressRepo = new Mock<IEntityRepository<Address>>();
            ratingService = new Mock<IRatingService>();
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            var logger = new Mock<ILogger<ProviderService>>();
            mapper = new Mock<IMapper>();


            providerService = new ProviderService(providersRepositoryMock.Object, usersRepositoryMock.Object, ratingService.Object, logger.Object, localizer.Object, mapper.Object, addressRepo.Object);
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var expected = ProvidersGenerator.Generate();
            providersRepositoryMock.Setup(r => r.Create(It.IsAny<Provider>())).Returns(Task.FromResult(expected));

            // Act
            var result = await providerService.Create(expected.ToModel()).ConfigureAwait(false);
            var actualProvider = result.ToDomain();

            // Assert
            AssertProvidersAreEqual(expected, actualProvider);
        }

        [Test]
        public async Task Create_ValidEntity_UpdatesOwnerIsRegisteredField()
        {
            // Arrange
            var provider = ProvidersGenerator.Generate();
            provider.UserId = fakeUser.Id;
            fakeUser.IsRegistered = false;

            // Act
            await providerService.Create(provider.ToModel()).ConfigureAwait(false);

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
            var expected = ProvidersGenerator.Generate();
            fakeProviders.RandomItem().UserId = expected.UserId;

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await providerService.Create(expected.ToModel()).ConfigureAwait(false));
        }

        [Test]
        public async Task Create_WhenActualAddressIsTheSameAsLegal_ActualAddressIsCleared()
        {
            // Arrange
            var expected = ProvidersGenerator.Generate();
            expected.ActualAddress = expected.LegalAddress;
            Provider receivedProvider = default;
            providersRepositoryMock.Setup(x => x.Create(It.IsAny<Provider>())).Callback<Provider>(p => receivedProvider = p);

            // Act
            await providerService.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.That(receivedProvider.ActualAddress, Is.Null);
        }

        [Test]
        public async Task Create_WhenActualAddressDiffersFromTheLegal_ActualAddressIsSaved()
        {
            // Arrange
            var expected = ProvidersGenerator.Generate();

            Provider receivedProvider = default;
            providersRepositoryMock.Setup(x => x.Create(It.IsAny<Provider>())).Callback<Provider>(p => receivedProvider = p);

            // Act
            await providerService.Create(expected.ToModel());

            // Act & Assert
            AssertAdressesAreEqual(expected.ActualAddress, receivedProvider.ActualAddress);
        }

        [Test]
        public void Create_WhenProviderWithTheSameEdrpouOrIpnExist_ThrowsInvalidOperationException()
        {
            // Arrange
            providersRepositoryMock.Setup(r => r.SameExists(It.IsAny<Provider>())).Returns(true);

            var randomProvider = fakeProviders.RandomItem();
            var existingProvider = new ProviderDto
            {
                LegalAddress = new AddressDto(),
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await providerService.Create(existingProvider));
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllEntities()
        {
            // Arrange
            providersRepositoryMock.Setup(r => r.GetAll()).Returns(Task.FromResult(fakeProviders.AsEnumerable()));
            ratingService
                .Setup(r => r.GetAverageRatingForRange(It.IsAny<IEnumerable<Guid>>(), RatingType.Provider))
                .Returns(RatingsGenerator.GetAverageRatingForRange(fakeProviders.Select(p => p.Id)));

            // Act
            var providers = await providerService.GetAll().ConfigureAwait(false);
            var actualProviders = providers.Select(p => p.ToDomain()).ToList();
            var providersToCompare = fakeProviders.Zip(actualProviders, (f, a) => new { Fake = f, Actual = a }).ToList();

            // Assert
            providersToCompare.ForEach(pair => AssertProvidersAreEqual(pair.Fake, pair.Actual));
        }

        [Test]
        public async Task GetById_WhenIdIsValid_ReturnsEntity()
        {
            // Arrange
            var existingProvider = fakeProviders.RandomItem();
            var existingProviderId = existingProvider.Id;
            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns(Task.FromResult(existingProvider));

            // Act
            var actualProviderDto = await providerService.GetById(existingProviderId).ConfigureAwait(false);
            var actualProvider = actualProviderDto.ToDomain();

            // Assert
            AssertProvidersAreEqual(existingProvider, actualProvider);
        }

        [Test]
        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException()
        {
            var noneExistingId = Guid.NewGuid();

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await providerService.GetById(noneExistingId).ConfigureAwait(false));
        }

        // TODO: providerService.Update method should be fixed before
        [Test]
        public async Task Update_UserCanUpdateExistingEntityOfRelatedProvider_UpdatesExistedEntity()
        {
            // Arrange
            var providerToUpdate = fakeProviders.RandomItem();
            IEnumerable<Provider> filteredCollection = new List<Provider>() { providerToUpdate };
            var updatedTitle = Guid.NewGuid().ToString();
            var providerToUpdateDto = providerToUpdate.ToModel();
            providerToUpdateDto.FullTitle = updatedTitle;
            providersRepositoryMock.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), string.Empty)).Returns(Task.FromResult(filteredCollection));
            mapper.Setup(mapper => mapper.Map(providerToUpdateDto, It.IsAny<Provider>())).Returns(providerToUpdateDto.ToDomain());
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync()).ReturnsAsync(1);
            mapper.Setup(mapper => mapper.Map<ProviderDto>(It.IsAny<Provider>())).Returns(providerToUpdateDto);


            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(result.FullTitle, updatedTitle);
        }

        [Test]
        public async Task Update_WhenUsersIdAndProvidersIdDoesntMatch_ReturnsNull()
        {
            // Arrange
            var changedEntity = new ProviderDto();
            var noneExistingUserId = Guid.NewGuid().ToString();

            // Act
            var result = await providerService.Update(changedEntity, noneExistingUserId).ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task Delete_WhenIdIsValid_CalledProvidersRepositoryDeleteMethod()
        {
            // Arrange
            var providerToDelete = fakeProviders.RandomItem();
            var deleteMethodArguments = new List<Provider>();
            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns(Task.FromResult(providerToDelete));
            providersRepositoryMock.Setup(r => r.Delete(Capture.In(deleteMethodArguments)));

            // Act
            await providerService.Delete(providerToDelete.Id).ConfigureAwait(false);

            // Assert
            AssertProvidersAreEqual(deleteMethodArguments.Single(), providerToDelete);
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

        // TODO: move random test data creation to Tests.Common

        private static void AssertProvidersAreEqual(Provider expected, Provider result)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected.FullTitle, result.FullTitle);
                Assert.AreEqual(expected.ShortTitle, result.ShortTitle);
                Assert.AreEqual(expected.Website, result.Website);
                Assert.AreEqual(expected.Facebook, result.Facebook);
                Assert.AreEqual(expected.Email, result.Email);
                Assert.AreEqual(expected.Instagram, result.Instagram);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.DirectorDateOfBirth, result.DirectorDateOfBirth);
                Assert.AreEqual(expected.EdrpouIpn, result.EdrpouIpn);
                Assert.AreEqual(expected.PhoneNumber, result.PhoneNumber);
                Assert.AreEqual(expected.Founder, result.Founder);
                Assert.AreEqual(expected.Ownership, result.Ownership);
                Assert.AreEqual(expected.Type, result.Type);
                Assert.AreEqual(expected.Status, result.Status);
                Assert.AreEqual(expected.UserId, result.UserId);
                Assert.AreEqual(expected.LegalAddress.City, result.LegalAddress.City);
                Assert.AreEqual(expected.LegalAddress.BuildingNumber, result.LegalAddress.BuildingNumber);
                Assert.AreEqual(expected.LegalAddress.Street, result.LegalAddress.Street);
                Assert.AreEqual(expected.ActualAddress.City, result.ActualAddress.City);
                Assert.AreEqual(expected.ActualAddress.BuildingNumber, result.ActualAddress.BuildingNumber);
                Assert.AreEqual(expected.ActualAddress.Street, result.ActualAddress.Street);
            });
        }

        private static void AssertAdressesAreEqual(Address expected, Address actual)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected.Region, actual.Region);
                Assert.AreEqual(expected.District, actual.District);
                Assert.AreEqual(expected.City, actual.City);
                Assert.AreEqual(expected.Street, actual.Street);
                Assert.AreEqual(expected.BuildingNumber, actual.BuildingNumber);
                Assert.AreEqual(expected.Latitude, actual.Latitude);
                Assert.AreEqual(expected.Longitude, actual.Longitude);
            });
        }

        private static User CreateFakeUser()
        {
            return new User()
            {
                Id = FakeUserId,
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName",
                FirstName = "FirstName",
                LastName = "LastName",
                UserName = "user@gmail.com",
                NormalizedUserName = "USER@GMAIL.COM",
                Email = "user@gmail.com",
                NormalizedEmail = "USER@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
                PhoneNumber = "0965679725",
                Role = "provider",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                IsRegistered = false,
            };
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

            providersRepository.Setup(r => r.GetAll()).Returns(Task.FromResult(providersCollection));
            providersRepository.Setup(r => r.ExistsUserId(It.IsAny<string>())).Callback<string>(user => UserExist(user)).Returns(() => userExistsResult);

            return providersRepository;
        }


    }
}