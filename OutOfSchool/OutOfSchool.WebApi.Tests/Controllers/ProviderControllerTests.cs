using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ProviderControllerTests
    {

        private ProviderController providerController;
        private Mock<IProviderService> providerService;
        private List<Provider> providers;
        private Provider provider;

        [SetUp]
        public void Setup()
        {

            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            var user = new ClaimsPrincipal
                (new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(IdentityResourceClaimsTypes.Sub, Guid.NewGuid().ToString()),
                        new Claim(IdentityResourceClaimsTypes.Role, Role.Provider.ToString()),
                    },
                    IdentityResourceClaimsTypes.Sub));

            providerService = new Mock<IProviderService>();
            providerController = new ProviderController(providerService.Object, localizer.Object, new Mock<ILogger<ProviderController>>().Object);



            providerController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            providers = ProvidersGenerator.Generate(10);
            provider = ProvidersGenerator.Generate();
        }

        [Test]
        public async Task GetProfile_WhenNoProviderWithSuchUserId_ReturnsNoContent()
        {
            // Arrange
            providerService.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(null as ProviderDto);

            // Act
            var result = await providerController.GetProfile().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task GetProfile_WhenProviderForUserIdExists_ReturnsOkObjectResult()
        {
            // Arrange
            providerService.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider.ToModel());

            // Act
            var result = await providerController.GetProfile().ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task GetProfile_WhenProviderForUserExists_ReturnsValidProviderDto()
        {
            // Arrange
            providerService.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider.ToModel());

            // Act
            var resultValue = (await providerController.GetProfile().ConfigureAwait(false) as ObjectResult).Value as ProviderDto;

            // Assert
            AssertProviderDtosAreEqual(provider.ToModel(), resultValue);
        }

        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject_WithCollectionDtos()
        {
            // Arrange
            providerService.Setup(x => x.GetAll()).ReturnsAsync(providers.Select(p => p.ToModel()));

            // Act
            var result = await providerController.Get().ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<IEnumerable<ProviderDto>>();
        }


        [Test]
        public async Task GetProviders_ReturnsExpectedCollectionOfDtos()
        {
            // Arrange
            var expected = providers.Select(p => p.ToModel()).ToList();
            providerService.Setup(x => x.GetAll()).ReturnsAsync(providers.Select(p => p.ToModel()));


            // Act
            var result = (await providerController.Get().ConfigureAwait(false) as ObjectResult).Value as IEnumerable<ProviderDto>;

            // Assert
            AssertTwoCollectionsEqualByValues(expected,result);
        }

        [Test]
        public async Task GetProviders_WhenNoRecordsInDB_ReturnsNoContentResult()
        {
            // Arrange
            providerService.Setup(x => x.GetAll()).ReturnsAsync(Enumerable.Empty<ProviderDto>());

            // Act
            var result = await providerController.Get().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task GetProviderById_WhenProviderWithIdExistsInDb_ReturnsOkObjectResultWithDtoAsValue()
        {
            // Arrange
            var existingId = providers.Select(x => x.Id).FirstOrDefault();
            providerService.Setup(x => x.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(providers.SingleOrDefault(x => x.Id == existingId).ToModel());

            // Act
            var result = await providerController.GetById(existingId).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task GetProviderById_WhenProviderWithIdExistsInDb_ReturnsExpectedProviderDto()
        {
            // Arrange
            var existingId = providers.Select(x => x.Id).FirstOrDefault();
            var expectedProvider = providers.SingleOrDefault(x => x.Id == existingId).ToModel();
            providerService.Setup(x => x.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(providers.SingleOrDefault(x => x.Id == existingId).ToModel());

            // Act
            var resultValue = (await providerController.GetById(existingId).ConfigureAwait(false) as ObjectResult).Value as ProviderDto;

            // Assert
            AssertProviderDtosAreEqual(expectedProvider, resultValue);
        }

        [Test]
        public async Task GetProviderById_WhenIdDoesntExistsInDb_ReturnsNotFoundResult()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid();
            providerService.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(null as ProviderDto);

            // Act
            var result = await providerController.GetById(invalidUserId).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<NotFoundObjectResult>();
        }

        [Test]
        public async Task CreateProvider_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            providerService.Setup(x => x.Create(It.IsAny<ProviderDto>())).ReturnsAsync(provider.ToModel());

            // Act
            var result = await providerController.Create(provider.ToModel()).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<CreatedAtActionResult>();
        }

        [Test]
        public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            providerController.ModelState.AddModelError("CreateProvider", "Invalid model state.");

            // Act
            var result = await providerController.Create(provider.ToModel()).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsValidAndProviderExists_ReturnsOkObjectResult()
        {
            // Arrange
            var providerToUpdate = providers.FirstOrDefault();
            providerToUpdate.FullTitle = "New Title for changed provider";
            var providerDto = providerToUpdate.ToModel();   
            providerService.Setup(x => x.Update(providerDto, It.IsAny<string>()))
                .ReturnsAsync(providerToUpdate.ToModel());

            // Act
            var result = await providerController.Update(providerDto).ConfigureAwait(false);
            var value = (result as ObjectResult).Value as ProviderDto;

            // Assert
            AssertProviderDtosAreEqual(providerDto, value);
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task UpdateProvider_WhenModelWithErrorsReceived_BadRequest_And_ModelsIsValid_False()
        {
            // Arrange
            var providerToUpdateDto = provider.ToModel();
            providerController.ModelState.AddModelError("UpdateError", "bad model state");

            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>()))
                .ReturnsAsync(provider.ToModel());

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
            Assert.That(!providerController.ModelState.IsValid);
        }

        [Test]
        public async Task UpdateProvider_WhenCorrectData_AND_WrongUserId_ModelIsValid_But_BadRequest()
        {
            // Arrange
            var providerToUpdateDto = providers.FirstOrDefault().ToModel();
            providerToUpdateDto.FullTitle = "New Title for changed provider";
            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>())).ReturnsAsync(null as ProviderDto);

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            Assert.That(providerController.ModelState.IsValid);
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateProvider_ServiceCantGetRequestedProvider_BadRequest_WithExceptionAsValue()
        {
            // Arrange
            var providerToUpdateDto = ProviderDtoGenerator.Generate();
            providerToUpdateDto.FullTitle = "New Title for changed provider";
            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>())).ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
            Assert.IsInstanceOf<DbUpdateConcurrencyException>((result as ObjectResult).Value);
        }

        [Test]
        public async Task DeleteProvider_WhenIdIsValid_ReturnsNoContentResult()
        {
            // Arrange
            var existingProviderGuid = providers.Select(p => p.Id).FirstOrDefault();
            providerService.Setup(x => x.Delete(existingProviderGuid));

            // Act
            var result = await providerController.Delete(existingProviderGuid);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteProvider_WhenIdIsInvalid_ReturnsBadRequestObjectResultAsync()
        {
            // Arrange
            var guid = Guid.NewGuid();
            providerService.Setup(x => x.Delete(guid)).ThrowsAsync(new ArgumentNullException());

            // Act
            var result = await providerController.Delete(guid).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        private static void AssertTwoCollectionsEqualByValues(IEnumerable<ProviderDto> expected, IEnumerable<ProviderDto> actual)
        {
            var expectedArray = expected.ToArray();
            var actualArray = actual.ToArray();
            Assert.Multiple(() =>
            {
                for (var i = 0; i < expectedArray.Length; i++)
                {
                    AssertProviderDtosAreEqual(expectedArray[i], actualArray[i]);
                }
            }
            );
        }

        private static void AssertProviderDtosAreEqual(ProviderDto expected, ProviderDto result)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected.FullTitle, result.FullTitle);
                Assert.AreEqual(expected.ShortTitle, result.ShortTitle);
                Assert.AreEqual(expected.PhoneNumber, result.PhoneNumber);
                Assert.AreEqual(expected.Website, result.Website);
                Assert.AreEqual(expected.Facebook, result.Facebook);
                Assert.AreEqual(expected.Email, result.Email);
                Assert.AreEqual(expected.Instagram, result.Instagram);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.Director, result.Director);
                Assert.AreEqual(expected.DirectorDateOfBirth, result.DirectorDateOfBirth);
                Assert.AreEqual(expected.EdrpouIpn, result.EdrpouIpn);
                Assert.AreEqual(expected.Founder, result.Founder);
                Assert.AreEqual(expected.Ownership, result.Ownership);
                Assert.AreEqual(expected.Type, result.Type);
                Assert.AreEqual(expected.Status, result.Status);
                Assert.AreEqual(expected.UserId, result.UserId);
                Assert.AreEqual(expected.Rating, result.Rating);
                Assert.AreEqual(expected.NumberOfRatings, result.NumberOfRatings);
                Assert.AreEqual(expected.ActualAddress, result.ActualAddress);
                Assert.AreEqual(expected.LegalAddress, result.LegalAddress);
                Assert.AreEqual(expected.InstitutionStatusId, result.InstitutionStatusId);
            });
        }
    }
}

