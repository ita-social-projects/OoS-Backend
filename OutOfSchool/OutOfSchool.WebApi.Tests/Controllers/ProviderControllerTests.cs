using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ProviderControllerTests
    {
        private ProviderController controller;
        private Mock<IProviderService> serviceProvider;
        private Mock<IAddressService> serviceAddress;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<ProviderDto> providers;
        private ProviderDto provider;

        [SetUp]
        public void Setup()
        {
            serviceProvider = new Mock<IProviderService>();
            serviceAddress = new Mock<IAddressService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new ProviderController(serviceProvider.Object, serviceAddress.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            providers = FakeProviders();
            provider = FakeProvider();
        }

        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            serviceProvider.Setup(x => x.GetAll()).ReturnsAsync(providers);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
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
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(0)]
        public void GetProvidersById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            serviceProvider.Setup(x => x.GetById(id)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetProvidersById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            serviceProvider.Setup(x => x.GetById(id)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        //[Test]
        //public async Task CreateProvider_WhenModelIsValid_ReturnsCreatedAtActionResult()
        //{
        //    // Arrange
        //    serviceProvider.Setup(x => x.Create(provider)).ReturnsAsync(provider);

        //    // Act
        //    var result = await controller.Create(provider).ConfigureAwait(false) as CreatedAtActionResult;

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(result.StatusCode, 201);
        //}

        [Test]
        public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateProvider", "Invalid model state.");

            // Act
            var result = await controller.Create(provider).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedProvider = new ProviderDto()
            {
                Id = 1,
                FullTitle = "ChangedTitle",
            };
            serviceProvider.Setup(x => x.Update(changedProvider)).ReturnsAsync(changedProvider);

            // Act
            var result = await controller.Update(changedProvider).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
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
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
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
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 204);
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
                DirectorBirthDay = new DateTime(1975, month: 10, 5),
                EdrpouIpn = "12345656",
                PhoneNumber = "1111111111",
                Founder = "Founder",
                Ownership = OwnershipType.Private,
                Type = ProviderType.TOV,
                Status = false,
                LegalAddressId = 8,
                ActualAddressId = 34,
                UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda67a6",
            };
        }

        private IEnumerable<ProviderDto> FakeProviders()
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
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 1,
                        ActualAddressId = 2,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
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
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345645",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 3,
                        ActualAddressId = 4,
                        UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
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
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12345000",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 5,
                        ActualAddressId = 6,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
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
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "10045678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 56,
                        ActualAddressId = 23,
                        UserId = "de909f35-5eb7-4BBa-bda8-40a5bfda96a6",
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
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        EdrpouIpn = "12374678",
                        PhoneNumber = "1111111111",
                        Founder = "Founder",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Status = false,
                        LegalAddressId = 1,
                        ActualAddressId = 2,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                },
            };
        }
    }
}
