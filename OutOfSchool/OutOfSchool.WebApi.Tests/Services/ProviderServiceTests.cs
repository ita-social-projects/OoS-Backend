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
            fakeUser = UserGenerator.Generate();

            providersRepositoryMock = CreateProvidersRepositoryMock(fakeProviders);
            usersRepositoryMock = CreateUsersRepositoryMock(fakeUser);
            var addressRepo = new Mock<IEntityRepository<Address>>();
            ratingService = new Mock<IRatingService>();
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            var logger = new Mock<ILogger<ProviderService>>();
            mapper = new Mock<IMapper>();
            var workshopServicesCombiner = new Mock<IWorkshopServicesCombiner>();

            providerService = new ProviderService(
                providersRepositoryMock.Object,
                usersRepositoryMock.Object,
                ratingService.Object,
                logger.Object,
                localizer.Object,
                mapper.Object,
                addressRepo.Object,
                workshopServicesCombiner.Object);
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var entityToBeCreated = ProvidersGenerator.Generate(); // argument for service's Create method
            var expected = entityToBeCreated.ToModel();
            providersRepositoryMock.Setup(r => r.Create(It.IsAny<Provider>())).ReturnsAsync(entityToBeCreated);

            // Act
            var result = await providerService.Create(entityToBeCreated.ToModel()).ConfigureAwait(false);
            var actualProvider = result;

            // Assert
            TestHelper.AssertDtosAreEqual(expected, actualProvider);
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
            await providerService.Create(expectedEntity.ToModel());

            // Assert
            Assert.That(receivedProvider.ActualAddress, Is.Not.Null);
            TestHelper.AssertDtosAreEqual(expectedEntity.ActualAddress, receivedProvider.ActualAddress);
        }

        [Test]
        public void Create_WhenProviderWithTheSameEdrpouOrIpnExist_ThrowsInvalidOperationException()
        {
            // Arrange
            providersRepositoryMock.Setup(r => r.SameExists(It.IsAny<Provider>())).Returns(true);
            var randomProvider = fakeProviders.RandomItem().ToModel();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await providerService.Create(randomProvider));
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllEntities()
        {
            // Arrange
            var expectedCollection = fakeProviders.Select(p => p.ToModel()).ToList(); // expected collection of dto's to return
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
        public async Task GetById_WhenIdIsValid_ReturnsEntity()
        {
            // Arrange
            var existingProvider = fakeProviders.RandomItem();

            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(existingProvider);

            // Act
            var actualProviderDto = await providerService.GetById(existingProvider.Id).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(existingProvider.ToModel(), actualProviderDto);
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
            IEnumerable<Provider> filteredCollection = new List<Provider>() { provider };

            var updatedTitle = Guid.NewGuid().ToString();
            provider.FullTitle = updatedTitle;
            var providerToUpdateDto = provider.ToModel();

            providersRepositoryMock.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), string.Empty))
                .ReturnsAsync(filteredCollection);
            mapper.Setup(mapper => mapper.Map(providerToUpdateDto, It.IsAny<Provider>()))
                .Returns(providerToUpdateDto.ToDomain());
            providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
                .ReturnsAsync(1);
            mapper.Setup(mapper => mapper.Map<ProviderDto>(It.IsAny<Provider>()))
                .Returns(provider.ToModel());

            // Act
            var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(providerToUpdateDto, result);

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
            var providerToDeleteDto = fakeProviders.RandomItem().ToModel();
            var deleteMethodArguments = new List<Provider>();
            providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(fakeProviders.Single(p => p.Id == providerToDeleteDto.Id));
            providersRepositoryMock.Setup(r => r.Delete(Capture.In(deleteMethodArguments)));

            // Act
            await providerService.Delete(providerToDeleteDto.Id).ConfigureAwait(false);
            var result = deleteMethodArguments.Single().ToModel();

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

            providersRepository.Setup(r => r.ExistsUserId(It.IsAny<string>())).Callback<string>(user => UserExist(user)).Returns(() => userExistsResult);

            return providersRepository;
        }
    }
}