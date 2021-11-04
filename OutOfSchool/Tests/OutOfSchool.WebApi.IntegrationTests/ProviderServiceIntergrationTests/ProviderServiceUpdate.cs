namespace OutOfSchool.WebApi.IntegrationTests.ProviderServiceIntergrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentAssertions.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using OutOfSchool.Services;
    using OutOfSchool.Services.Models;
    using OutOfSchool.Services.Repository;
    using OutOfSchool.Tests;
    using OutOfSchool.Tests.Common.TestDataGenerators;
    using OutOfSchool.WebApi.Models;
    using OutOfSchool.WebApi.Services;
    using OutOfSchool.WebApi.Util;

    [TestFixture]
    public class ProviderServiceUpdate
    {
        private const string FakeUserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c";

        private IProviderService providerService;

        // private Mock<IProviderRepository> providersRepositoryMock;
        private Mock<IEntityRepository<User>> usersRepositoryMock;
        private Mock<IRatingService> ratingService;
        private Mock<IEntityRepository<Address>> adkressRepository;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<ProviderService>> logger;

        private List<Provider> fakeProviders;
        private User fakeUser;

        private IProviderRepository providerRepository;
        private Mapper mapper;
        private DbContextOptions<OutOfSchoolDbContext> unitTestDbOptions;

        private OutOfSchoolDbContext GetContext => new OutOfSchoolDbContext(unitTestDbOptions);

        [SetUp]
        public async Task SetUp()
        {
            unitTestDbOptions = UnitTestHelper.GetUnitTestDbOptions();
            await using var context = GetContext;
            fakeProviders = ProvidersGenerator.Generate(10);
            await context.AddRangeAsync(fakeProviders);
            await context.SaveChangesAsync();
            fakeUser = CreateFakeUser();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(typeof(MappingProfile))));
            // providersRepositoryMock = CreateProvidersRepositoryMock(fakeProviders);
            usersRepositoryMock = CreateUsersRepositoryMock(fakeUser);

            ratingService = new Mock<IRatingService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger<ProviderService>>();
            adkressRepository = new Mock<IEntityRepository<Address>>();
            providerRepository = new ProviderRepository(GetContext);
            providerService = new ProviderService(providerRepository, usersRepositoryMock.Object, ratingService.Object,
                logger.Object, localizer.Object, mapper, adkressRepository.Object);
        }

        [Test]
        public async Task UpdateWhenProviderHasSameAdresses_WithSameAddresses_UpdatesOneAddress()
        {
            // Arrange
            await using var context = GetContext;
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            provider.ActualAddress = provider.LegalAddress;

            // Act
            var result = await providerService.Update(mapper.Map<ProviderDto>(provider), provider.UserId, "Provider")
                .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result.ActualAddress.IsSameOrEqualTo(result.LegalAddress));
            Assert.IsNotNull(result.LegalAddress);
            Assert.IsNull(result.ActualAddress);
        }

        [Test]
        public async Task UpdateWhenProviderHasSameAdresses_WithActualAddressNull_UpdatesOneAddress()
        {
            // Arrange
            await using var context = GetContext;
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            var providerDto = mapper.Map<ProviderDto>(provider);
            providerDto.ActualAddress = null;

            // Act
            var result = await providerService.Update(providerDto, provider.UserId, "Provider")
                .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result.ActualAddress.IsSameOrEqualTo(result.LegalAddress));
            Assert.IsNotNull(result.LegalAddress);
            Assert.IsNull(result.ActualAddress);
        }

        [Test]
        public async Task UpdateWhenProviderHasSameAdresses_WithNewLegalAddressAndActualIsPreviousLegal_UpdatesOneAddress()
        {
            // Arrange
            await using var context = GetContext;
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            await context.Entry(provider).ReloadAsync().ConfigureAwait(false);
            var randomAddress = AddressGenerator.Generate();
            randomAddress.Id = 0;
            // provider.ActualAddress = provider.LegalAddress;
            provider.LegalAddress = randomAddress;
            var providerDto = mapper.Map<ProviderDto>(provider);

            // Act
            var result = await providerService.Update(providerDto, provider.UserId, "Provider")
                .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result.ActualAddress.IsSameOrEqualTo(result.LegalAddress));
            Assert.IsNotNull(result.LegalAddress);
            Assert.IsNull(result.ActualAddress);
        }

        [Test]
        public async Task UpdateWhenProviderHasSameAdresses_WithSameLegalAddressAndNewActual_UpdatesOneAddress()
        {
            // Arrange
            await using var context = GetContext;
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            await context.Entry(provider).ReloadAsync().ConfigureAwait(false);
            var randomAddress = AddressGenerator.Generate();
            randomAddress.Id = 0;
            // provider.ActualAddress = provider.LegalAddress;
            provider.ActualAddress = randomAddress;
            var providerDto = mapper.Map<ProviderDto>(provider);

            // Act
            var result = await providerService.Update(providerDto, provider.UserId, "Provider")
                .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result.ActualAddress.IsSameOrEqualTo(result.LegalAddress));
            Assert.IsNotNull(result.LegalAddress);
            Assert.IsNull(result.ActualAddress);
        }

        private static User CreateFakeUser()
        {
            return new User
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
            usersRepository.Setup(r => r.GetAll())
                .Returns(Task.FromResult<IEnumerable<User>>(new List<User> { fakeUser }));
            usersRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), string.Empty))
                .Returns(Task.FromResult<IEnumerable<User>>(new List<User> { fakeUser }));

            return usersRepository;
        }

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
    }
}