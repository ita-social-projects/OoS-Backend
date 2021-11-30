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
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;
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
        private List<ProviderDto> providerDtos;
        private ProviderDto providerDto;

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
            providerDtos = ProviderDtoGenerator.Generate(10);
            providerDto = ProviderDtoGenerator.Generate();
        }

        [Test]
        public async Task GetProfile_WhenNoProviderWithSuchUserId_ReturnsNoContent()
        {
            // Arrange
            ProviderDto nullProvider = null;
            providerService.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(nullProvider);

            // Act
            var result = await providerController.GetProfile().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task GetProfile_WhenProviderForUserIdExists_ReturnsOkObjectResult()
        {
            // Arrange
            providerService.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(providerDto);

            // Act
            var result = await providerController.GetProfile().ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            providerService.Setup(x => x.GetAll()).ReturnsAsync(providerDtos);

            // Act
            var result = await providerController.Get().ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<IEnumerable<ProviderDto>>();
        }


        // write test to compare collection returned
        [Test]
        public async Task GetProviders_ReturnsExpectedCollectionOfDtos()
        {

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
            var existingId = providerDtos.Select(x => x.Id).FirstOrDefault();
            providerService.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(providerDtos.SingleOrDefault(x => x.Id == existingId));

            // Act
            var result = await providerController.GetById(existingId).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
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
            providerService.Setup(x => x.Create(It.IsAny<ProviderDto>())).ReturnsAsync(providerDto);

            // Act
            var result = await providerController.Create(providerDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<CreatedAtActionResult>();
        }

        [Test]
        public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            providerController.ModelState.AddModelError("CreateProvider", "Invalid model state.");

            // Act
            var result = await providerController.Create(providerDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        /// TO DO ????
        /// Since we have a mock here - we also must check that controller received a proper argument when updating the provider.
        [Test]
        public async Task UpdateProvider_WhenModelIsValidAndProviderExists_ReturnsOkObjectResult()
        {
            // Arrange
            var providerToUpdateDto = providerDtos.FirstOrDefault();
            providerToUpdateDto.FullTitle = "New Title for changed provider";
            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>()))
                .ReturnsAsync(providerToUpdateDto);

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseOkAndValidValue<ProviderDto>();
        }

        [Test]
        public async Task UpdateProvider_WhenUserUpdateNotOwnProvider_ReturnsBadRequestResult()
        {
            // Arrange
            var providerToUpdateDto = providerDtos.FirstOrDefault();
            providerToUpdateDto.FullTitle = "New Title for changed provider";
            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>())).ReturnsAsync(null as ProviderDto);

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            providerController.ModelState.AddModelError("UpdateProvider", "Invalid model state.");

            // Act
            var result = await providerController.Update(providerDto).ConfigureAwait(false);

            // Assert
            result.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task DeleteProvider_WhenIdIsValid_ReturnsNoContentResult()
        {
            // Arrange
            var existingProviderGuid = providerDtos.Select(p => p.Id).FirstOrDefault();
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
    }
}
