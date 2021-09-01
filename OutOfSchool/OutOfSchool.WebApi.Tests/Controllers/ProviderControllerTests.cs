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
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ProviderControllerTests
    {
        private const int OkStatusCode = 200;
        private const int CreateStatusCode = 201;
        private const int NoContentStatusCode = 204;
        private const int BadRequestStatusCode = 400;
        private const string FakeUserId = "de909f35-5eb7-4b7a-bda8-40a5bfda67a6";
        private ProviderController controller;
        private Mock<IProviderService> serviceProvider;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private List<ProviderDto> providers;
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

            providers = FakeProviders();
            provider = FakeProvider();
        }

        [Test]
        public async Task GetProfile_WhenCalledForEmptyRepository_ReturnsNoContent()
        {
            // Arrange
            serviceProvider.Setup(x => x.GetAll()).ReturnsAsync(Enumerable.Empty<ProviderDto>());

            // Act
            var result = await controller.GetProfile().ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetProfile_NoRepositoryForUser_ReturnsNoContent()
        {
            // Arrange
            serviceProvider.Setup(x => x.GetAll()).ReturnsAsync(providers);

            // Act
            var result = await controller.GetProfile().ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            serviceProvider.Setup(x => x.GetAll()).ReturnsAsync(providers);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetProvidersById_WhenIdIsValid_ReturnsOkObjectResult(long id)
        {
            // Arrange
            serviceProvider.Setup(x => x.GetById(id)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        public async Task GetProvidersById_WhenIdIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            const int invalidUserId = 0;

            // Act
            var result = await controller.GetById(invalidUserId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetProvidersById_WhenIdIsValid_ReturnsOkResult()
        {
            // Arrange
            var existingProvider = providers.RandomItem();
            serviceProvider.Setup(x => x.GetById(existingProvider.Id)).ReturnsAsync(existingProvider);

            // Act
            var result = await controller.GetById(existingProvider.Id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateProvider_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            serviceProvider.Setup(x => x.Create(provider)).ReturnsAsync(provider);

            // Act
            var result = await controller.Create(provider).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(CreateStatusCode, result.StatusCode);
        }

        [Test]
        public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateProvider", "Invalid model state.");

            // Act
            var result = await controller.Create(provider).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedProvider = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle",
                UserId = FakeUserId,
            };
            serviceProvider.Setup(x => x.Update(changedProvider, changedProvider.UserId, "provider")).ReturnsAsync(changedProvider);

            // Act
            var result = await controller.Update(changedProvider).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        public async Task UpdateProvider_WhenUserIsNotAdminAndUpdateNotOwnProvider_ReturnsBadRequestResult()
        {
            // Arrange
            var changedProvider = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle",
            };
            serviceProvider.Setup(x => x.Update(changedProvider, "CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c", "provider")).ReturnsAsync(changedProvider);

            // Act
            var result = await controller.Update(changedProvider).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateProvider", "Invalid model state.");

            // Act
            var result = await controller.Update(provider).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteProvider_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            serviceProvider.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void DeleteProvider_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
        {
            // Arrange
            serviceProvider.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteProvider_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            serviceProvider.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        private ProviderDto FakeProvider()
        {
            return new ProviderDto()
            {
                Id = 6,
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

        private List<ProviderDto> FakeProviders()
        {
            return new List<ProviderDto>()
            {
                new ProviderDto()
                {
                        Id = 1,
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
                        Id = 2,
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
                        Id = 3,
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
                        Id = 4,
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
                        Id = 5,
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
