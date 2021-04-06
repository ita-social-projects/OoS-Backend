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
        private Mock<IProviderService> service;
        private ClaimsPrincipal user;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<ProviderDto> providers;
        private ProviderDto provider;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IProviderService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new ProviderController(service.Object, localizer.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            providers = FakeProviders();
            provider = FakeProvider();
        }

        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(providers);

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
            service.Setup(x => x.GetById(id)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == id));

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
            service.Setup(x => x.GetById(id)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task GetProvidersById_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(providers.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task CreateProvider_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(provider)).ReturnsAsync(provider);

            // Act
            var result = await controller.Create(provider).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 201);
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
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var changedProvider = new ProviderDto()
            {
                Id = 1,
                Title = "ChangedTitle",
            };
            service.Setup(x => x.Update(changedProvider)).ReturnsAsync(changedProvider);

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
            service.Setup(x => x.Delete(id));

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
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteProvider_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

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
                Title = "Title6",
                ShortTitle = "ShortTitle6",
                Website = "Website6",
                Facebook = "Facebook6",
                Instagram = "Instagram6",
                Description = "Description6",
                MFO = "193496",
                EDRPOU = "19945998",
                KOATUU = "0600000000",
                INPP = "1234446690",
                Director = "Director6",
                DirectorPosition = "Position6",
                AuthorityHolder = "Holder6",
                DirectorBirthDay = new DateTime(1985, month: 10, 5),
                DirectorPhonenumber = "1111111111",
                ManagerialBody = "ManagerialBody6",
                Ownership = OwnershipType.State,
                Type = ProviderType.Social,
                Form = "Form6",
                Profile = ProviderProfile.Athletic,
                Index = "Index6",
                IsSubmitPZ1 = true,
                AttachedDocuments = "Dcument6",
                AddressId = 24,
            };
        }

        private IEnumerable<ProviderDto> FakeProviders()
        {
            return new List<ProviderDto>()
            {
                new ProviderDto()
                {
                        Id = 1,
                        Title = "Title1",
                        ShortTitle = "ShortTitle1",
                        Website = "Website1",
                        Facebook = "Facebook1",
                        Instagram = "Instagram1",
                        Description = "Description1",
                        MFO = "123456",
                        EDRPOU = "12345678",
                        KOATUU = "0100000000",
                        INPP = "1234567890",
                        Director = "Director1",
                        DirectorPosition = "Position1",
                        AuthorityHolder = "Holder1",
                        DirectorBirthDay = new DateTime(1975, month: 10, 5),
                        DirectorPhonenumber = "1111111111",
                        ManagerialBody = "ManagerialBody1",
                        Ownership = OwnershipType.Common,
                        Type = ProviderType.FOP,
                        Form = "Form1",
                        Profile = ProviderProfile.Athletic,
                        Index = "Index1",
                        IsSubmitPZ1 = true,
                        AttachedDocuments = "Dcument1",
                        AddressId = 5,
                        UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6",
                },
                new ProviderDto()
                {
                        Id = 2,
                        Title = "Title2",
                        ShortTitle = "ShortTitle2",
                        Website = "Website2",
                        Facebook = "Facebook2",
                        Instagram = "Instagram2",
                        Description = "Description2",
                        MFO = "654321",
                        EDRPOU = "87654321",
                        KOATUU = "0200000000",
                        INPP = "0987654321",
                        Director = "Director2",
                        DirectorPosition = "Position2",
                        AuthorityHolder = "Holder2",
                        DirectorBirthDay = new DateTime(1982, month: 11, 23),
                        DirectorPhonenumber = "1111111111",
                        ManagerialBody = "ManagerialBody2",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.TOV,
                        Form = "Form2",
                        Profile = ProviderProfile.ArtisticallyAesthetic,
                        Index = "Index2",
                        IsSubmitPZ1 = true,
                        AttachedDocuments = "Dcument2",
                        AddressId = 6,
                        UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6",
                },
                new ProviderDto()
                {
                        Id = 3,
                        Title = "Title3",
                        ShortTitle = "ShortTitle3",
                        Website = "Website3",
                        Facebook = "Facebook3",
                        Instagram = "Instagram3",
                        Description = "Description3",
                        MFO = "321654",
                        EDRPOU = "43218765",
                        KOATUU = "0300000000",
                        INPP = "5432109876",
                        Director = "Director3",
                        DirectorPosition = "Position3",
                        AuthorityHolder = "Holder3",
                        DirectorBirthDay = new DateTime(1978, month: 10, 13),
                        DirectorPhonenumber = "1111111111",
                        ManagerialBody = "ManagerialBody3",
                        Ownership = OwnershipType.State,
                        Type = ProviderType.EducationalInstitution,
                        Form = "Form3",
                        Profile = ProviderProfile.Curative,
                        Index = "Index3",
                        IsSubmitPZ1 = true,
                        AttachedDocuments = "Dcument3",
                        AddressId = 7,
                        UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6",
                },
                new ProviderDto()
                {
                        Id = 4,
                        Title = "Title4",
                        ShortTitle = "ShortTitle4",
                        Website = "Website4",
                        Facebook = "Facebook4",
                        Instagram = "Instagram4",
                        Description = "Description4",
                        MFO = "165432",
                        EDRPOU = "21874365",
                        KOATUU = "0400000000",
                        INPP = "5438762109",
                        Director = "Director4",
                        DirectorPosition = "Position4",
                        AuthorityHolder = "Holder4",
                        DirectorBirthDay = new DateTime(1979, month: 10, 27),
                        DirectorPhonenumber = "1111111111",
                        ManagerialBody = "ManagerialBody4",
                        Ownership = OwnershipType.State,
                        Type = ProviderType.Social,
                        Form = "Form4",
                        Profile = ProviderProfile.Scout,
                        Index = "Index4",
                        IsSubmitPZ1 = false,
                        AttachedDocuments = "Dcument4",
                        AddressId = 8,
                        UserId = "de804f35-bda9-4b9n-8eb1-54a5okfg90a6",
                },
                new ProviderDto()
                {
                        Id = 5,
                        Title = "Title5",
                        ShortTitle = "ShortTitle5",
                        Website = "Website5",
                        Facebook = "Facebook5",
                        Instagram = "Instagram5",
                        Description = "Description5",
                        MFO = "105402",
                        EDRPOU = "20804065",
                        KOATUU = "0500000000",
                        INPP = "5400700109",
                        Director = "Director5",
                        DirectorPosition = "Position5",
                        AuthorityHolder = "Holder5",
                        DirectorBirthDay = new DateTime(1985, month: 10, 21),
                        DirectorPhonenumber = "1111111111",
                        ManagerialBody = "ManagerialBody5",
                        Ownership = OwnershipType.Private,
                        Type = ProviderType.FOP,
                        Form = "Form5",
                        Profile = ProviderProfile.MilitaryPatriotic,
                        Index = "Index5",
                        IsSubmitPZ1 = false,
                        AttachedDocuments = "Dcument5",
                        AddressId = 9,
                        UserId = "de804f35-tga0-4g9n-8db1-54a5okfg80a4",
                },
            };
        }
    }
}