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
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.IntegrationTests.ProviderServiceIntergrationTests
{
    [TestFixture]
    public class ProviderServiceUpdate
    {
        private const string FakeUserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c";

        private IProviderService providerService;

        // private Mock<IProviderRepository> providersRepositoryMock;
        private Mock<IEntityRepository<User>> usersRepositoryMock;
        private Mock<IRatingService> ratingService;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<ProviderService>> logger;

        private List<Provider> fakeProviders;
        private User fakeUser;
        private OutOfSchoolDbContext getContext => new OutOfSchoolDbContext(unitTestDbOptions);
        private IProviderRepository providerRepository;
        private Mapper mapper;
        private DbContextOptions<OutOfSchoolDbContext> unitTestDbOptions;

        [SetUp]
        public async Task SetUp()
        {
            unitTestDbOptions = UnitTestHelper.GetUnitTestDbOptions();
            await using var context = getContext;
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
            providerRepository = new ProviderRepository(getContext);
            providerService = new ProviderService(providerRepository, usersRepositoryMock.Object, ratingService.Object,
                logger.Object, localizer.Object, mapper);
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

        [Test]
        public async Task Update_WithSameAddresses_UpdatesOneAddress()
        {
            // Arrange
            await using var context = getContext;
            var provider = context.Providers.First();
            provider.ActualAddress = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            provider.ActualAddress = provider.LegalAddress;

            // Act
            var result = await providerService.Update(mapper.Map<ProviderDto>(provider), provider.UserId, "Provider")
                .ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result.ActualAddress.IsSameOrEqualTo(result.LegalAddress));
        }

        // TODO: move random test data creation to Tests.Common
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