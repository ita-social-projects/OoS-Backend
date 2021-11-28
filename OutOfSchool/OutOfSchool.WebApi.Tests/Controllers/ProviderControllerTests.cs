using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using Moq;
using NUnit.Framework;

using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ProviderControllerTests
    {
        private const string FakeProviderIdTestCase = "69c0a240-728f-452e-807f-70074e261a8f";
        private const string FakeUserId = "de909f35-5eb7-4b7a-bda8-40a5bfda67a6";
        private ProviderController controller;
        private Mock<IProviderService> serviceProvider;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private List<ProviderDto> providers;
        private Guid[] guids;
        private ProviderDto provider;

        [SetUp]
        public void Setup()
        {
            serviceProvider = new Mock<IProviderService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new ProviderController(serviceProvider.Object, localizer.Object, new Mock<ILogger<ProviderController>>().Object);
            user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[]
                {
                    new Claim("sub", FakeUserId),
                    new Claim("role", "provider"),
                }, "sub"));
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            guids = new Guid[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Parse(FakeProviderIdTestCase),
            };
            providers = FakeProviders(guids);
            provider = FakeProvider();
        }

        [Test]
        public async Task GetProfile_WhenNoProviderWithSuchUserId_ReturnsNoContent()
        {
            // Arrange
            ProviderDto nullProvider = null;
            serviceProvider.Setup(x => x.GetByUserId(FakeUserId)).ReturnsAsync(nullProvider);

            // Act
            var result = await controller.GetProfile().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task GetProfile_WhenProviderForUserIdExists_ReturnsOkObjectResult()
        {
            // Arrange
            var existingProvider = provider;
            serviceProvider.Setup(x => x.GetByUserId(FakeUserId)).ReturnsAsync(existingProvider);

            // Act
            var result = await controller.GetProfile().ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            serviceProvider.Setup(x => x.GetAll()).ReturnsAsync(providers);

            // Act
            var result = await controller.Get().ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<IEnumerable<ProviderDto>>();
        }

        [Test]
        public async Task GetProviders_WhenNoRecordsInDB_ReturnsNoContentResult()
        {
            // Arrange
            serviceProvider.Setup(x => x.GetAll()).ReturnsAsync(Enumerable.Empty<ProviderDto>());

            // Act
            var result = await controller.Get().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task GetProviderById_WhenIdIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var existingGuid = Guid.Parse(FakeProviderIdTestCase);
            serviceProvider.Setup(x => x.GetById(existingGuid)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == existingGuid));

            // Act
            var result = await controller.GetById(existingGuid).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task GetProviderById_WhenIdIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid();
            serviceProvider.Setup(x => x.GetById(invalidUserId)).ThrowsAsync(new ArgumentOutOfRangeException());

            // Act
            var result = await controller.GetById(invalidUserId).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task CreateProvider_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            serviceProvider.Setup(x => x.Create(provider)).ReturnsAsync(provider);

            // Act
            var result = await controller.Create(provider).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<CreatedAtActionResult>();
        }

        [Test]
        public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateProvider", "Invalid model state.");

            // Act
            var result = await controller.Create(provider).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedProvider = new ProviderDto()
            {
                Id = Guid.NewGuid(),
                FullTitle = "ChangedTitle",
                UserId = FakeUserId,
            };
            serviceProvider.Setup(x => x.Update(changedProvider, changedProvider.UserId)).ReturnsAsync(changedProvider);

            // Act
            var result = await controller.Update(changedProvider).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task UpdateProvider_WhenUserUpdateNotOwnProvider_ReturnsBadRequestResult()
        {
            // Arrange
            var providerEntityToUpdate = new ProviderDto()
            {
                Id = Guid.NewGuid(),
                FullTitle = "ChangedTitle",
            };
            serviceProvider.Setup(x => x.Update(providerEntityToUpdate, "CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c")).ReturnsAsync(providerEntityToUpdate);

            // Act
            var result = await controller.Update(providerEntityToUpdate).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateProvider", "Invalid model state.");

            // Act
            var result = await controller.Update(provider).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task DeleteProvider_WhenIdIsValid_ReturnsNoContentResult()
        {
            // Arrange
            var guid = Guid.Parse(FakeProviderIdTestCase);
            serviceProvider.Setup(x => x.Delete(guid));

            // Act
            var result = await controller.Delete(guid);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteProvider_WhenIdIsInvalid_ReturnsBadRequestObjectResultAsync()
        {
            // Arrange
            var guid = Guid.NewGuid();
            serviceProvider.Setup(x => x.Delete(guid)).ThrowsAsync(new ArgumentNullException());

            // Act
            var result = await controller.Delete(guid).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        private ProviderDto FakeProvider()
        {
            return new ProviderDto()
            {
                Id = Guid.Parse(FakeProviderIdTestCase),
                FullTitle = "Title6",
                ShortTitle = "ShortTitle6",
                Website = "Website6",
                Facebook = "Facebook6",
                Email = "user6@example.com",
                Instagram = "Instagram6",
                Description = "Description6",
                DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                EdrpouIpn = "12345656",
                PhoneNumber = "1111111111",
                Founder = "Founder",
                Ownership = OwnershipType.Private,
                Type = ProviderType.TOV,
                Status = false,
                UserId = FakeUserId,
                LegalAddress = new AddressDto
                {
                    Id = 11,
                    Region = "Region11",
                    District = "District11",
                    City = "City11",
                    Street = "Street11",
                    BuildingNumber = "BuildingNumber11",
                    Latitude = 0,
                    Longitude = 0,
                },
                ActualAddress = new AddressDto
                {
                    Id = 12,
                    Region = "Region12",
                    District = "District12",
                    City = "City12",
                    Street = "Street12",
                    BuildingNumber = "BuildingNumber12",
                    Latitude = 0,
                    Longitude = 0,
                },
            };
        }

        private List<ProviderDto> FakeProviders(params Guid[] guids)
        {
            return new List<ProviderDto>()
            {
                new ProviderDto()
                {
                        Id = guids[0],
                        FullTitle = "Title1",
                        ShortTitle = "ShortTitle1",
                        Website = "Website1",
                        Facebook = "Facebook1",
                        Email = "user1@example.com",
                        Instagram = "Instagram1",
                        Description = "Description1",
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                        LegalAddress = new AddressDto
                        {
                            Id = 1,
                            Region = "Region1",
                            District = "District1",
                            City = "City1",
                            Street = "Street1",
                            BuildingNumber = "BuildingNumber1",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new AddressDto
                        {
                            Id = 2,
                            Region = "Region2",
                            District = "District2",
                            City = "City2",
                            Street = "Street2",
                            BuildingNumber = "BuildingNumber2",
                            Latitude = 0,
                            Longitude = 0,
                        },
                },
                new ProviderDto()
                {
                        Id = guids[1],
                        FullTitle = "Title2",
                        ShortTitle = "ShortTitle2",
                        Website = "Website2",
                        Facebook = "Facebook2",
                        Email = "user2@example.com",
                        Instagram = "Instagram2",
                        Description = "Description2",
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345645",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                        LegalAddress = new AddressDto
                        {
                            Id = 3,
                            Region = "Region3",
                            District = "District3",
                            City = "City3",
                            Street = "Street3",
                            BuildingNumber = "BuildingNumber3",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new AddressDto
                        {
                            Id = 4,
                            Region = "Region4",
                            District = "District4",
                            City = "City4",
                            Street = "Street4",
                            BuildingNumber = "BuildingNumber4",
                            Latitude = 0,
                            Longitude = 0,
                        },
                },
                new ProviderDto()
                {
                        Id = guids[2],
                        FullTitle = "Title3",
                        ShortTitle = "ShortTitle3",
                        Website = "Website3",
                        Facebook = "Facebook3",
                        Email = "user3@example.com",
                        Instagram = "Instagram3",
                        Description = "Description3",
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345000",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                        LegalAddress = new AddressDto
                        {
                            Id = 5,
                            Region = "Region5",
                            District = "District5",
                            City = "City5",
                            Street = "Street5",
                            BuildingNumber = "BuildingNumber5",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new AddressDto
                        {
                            Id = 6,
                            Region = "Region6",
                            District = "District6",
                            City = "City6",
                            Street = "Street6",
                            BuildingNumber = "BuildingNumber6",
                            Latitude = 0,
                            Longitude = 0,
                        },
                },
                new ProviderDto()
                {
                        Id = guids[3],
                        FullTitle = "Title4",
                        ShortTitle = "ShortTitle4",
                        Website = "Website4",
                        Facebook = "Facebook4",
                        Email = "user4@example.com",
                        Instagram = "Instagram4",
                        Description = "Description4",
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "10045678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        UserId = "de909f35-5eb7-4BBa-bda8-40a5bfda96a6",
                        LegalAddress = new AddressDto
                        {
                            Id = 7,
                            Region = "Region7",
                            District = "District7",
                            City = "City7",
                            Street = "Street7",
                            BuildingNumber = "BuildingNumber7",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new AddressDto
                        {
                            Id = 8,
                            Region = "Region8",
                            District = "District8",
                            City = "City8",
                            Street = "Street8",
                            BuildingNumber = "BuildingNumber8",
                            Latitude = 0,
                            Longitude = 0,
                        },
                },
                new ProviderDto()
                {
                        Id = Guid.NewGuid(),
                        FullTitle = "Title5",
                        ShortTitle = "ShortTitle5",
                        Website = "Website5",
                        Facebook = "Facebook5",
                        Email = "user5@example.com",
                        Instagram = "Instagram5",
                        Description = "Description5",
                        DirectorDateOfBirth = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12374678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                        LegalAddress = new AddressDto
                        {
                            Id = 9,
                            Region = "Region9",
                            District = "District9",
                            City = "City9",
                            Street = "Street9",
                            BuildingNumber = "BuildingNumber9",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        ActualAddress = new AddressDto
                        {
                            Id = 10,
                            Region = "Region10",
                            District = "District10",
                            City = "City10",
                            Street = "Street10",
                            BuildingNumber = "BuildingNumber10",
                            Latitude = 0,
                            Longitude = 0,
                        },
                },
            };
        }
    }
}
