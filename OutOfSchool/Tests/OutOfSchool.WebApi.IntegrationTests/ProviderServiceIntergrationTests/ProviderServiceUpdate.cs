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
        private const string NOT_ADMIN_USER_ROLE = "Provider";

        private IProviderService providerService;

        private User fakeUser;

        private Mapper mapper;
        private DbContextOptions<OutOfSchoolDbContext> unitTestDbOptions;

        private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(this.unitTestDbOptions);

        [SetUp]
        public async Task SetUp()
        {
            this.unitTestDbOptions = UnitTestHelper.GetUnitTestDbOptions();
            await using var context = this.GetContext();
            var fakeProvider = ProvidersGenerator.Generate(1).First();
            context.Add(fakeProvider);
            await context.SaveChangesAsync();
            this.fakeUser = CreateFakeUser();
            this.mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(typeof(MappingProfile))));

            var usersRepositoryMock = CreateUsersRepositoryMock(this.fakeUser);

            var ratingService = new Mock<IRatingService>();
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            var logger = new Mock<ILogger<ProviderService>>();
            var addressRepository = new Mock<IEntityRepository<Address>>();
            var providerRepository = new ProviderRepository(this.GetContext());
            this.providerService = new ProviderService(providerRepository, usersRepositoryMock.Object,
                ratingService.Object, logger.Object, localizer.Object, this.mapper, addressRepository.Object);
        }

        [Test]
        public async Task UpdateWhenProviderHasSameAdresses_WithSameAddresses_UpdatesOneAddress()
        {
            // Arrange
            await using var context = this.GetContext();
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            provider.ActualAddress = provider.LegalAddress;

            // Act
            var result = await this.providerService
                .Update(this.mapper.Map<ProviderDto>(provider), provider.UserId, NOT_ADMIN_USER_ROLE)
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
            await using var context = this.GetContext();
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            var providerDto = this.mapper.Map<ProviderDto>(provider);
            providerDto.ActualAddress = null;

            // Act
            var result = await this.providerService.Update(providerDto, provider.UserId, NOT_ADMIN_USER_ROLE)
                .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result.ActualAddress.IsSameOrEqualTo(result.LegalAddress));
            Assert.IsNotNull(result.LegalAddress);
            Assert.IsNull(result.ActualAddress);
        }

        [Test]
        public async Task
            UpdateWhenProviderHasSameAdresses_WithNewLegalAddressAndActualIsPreviousLegal_UpdatesOneAddress()
        {
            // Arrange
            await using var context = this.GetContext();
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            await context.Entry(provider).ReloadAsync().ConfigureAwait(false);
            var randomAddressToAdd = AddressGenerator.Generate();
            randomAddressToAdd.Id = 0;

            // provider.ActualAddress = provider.LegalAddress;
            provider.LegalAddress = randomAddressToAdd;
            var providerDto = this.mapper.Map<ProviderDto>(provider);

            // Act
            var result = await this.providerService.Update(providerDto, provider.UserId, NOT_ADMIN_USER_ROLE)
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
            await using var context = this.GetContext();
            var provider = context.Providers.First();
            provider.ActualAddressId = null;
            await context.SaveChangesAsync().ConfigureAwait(false);
            await context.Entry(provider).ReloadAsync().ConfigureAwait(false);
            var randomAddress = AddressGenerator.Generate();
            randomAddress.Id = 0;

            provider.ActualAddress = randomAddress;
            var providerDto = this.mapper.Map<ProviderDto>(provider);

            // Act
            var result = await this.providerService.Update(providerDto, provider.UserId, NOT_ADMIN_USER_ROLE)
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
    }
}