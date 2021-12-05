﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public async Task GetProfile_WhenProviderForUserIdExists_ReturnsOkObjectResult_WithExpectedValue()
        {
            // Arrange
            var expected = provider.ToModel();
            providerService.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider.ToModel());

            // Act
            var result = await providerController.GetProfile().ConfigureAwait(false);

            // Assert
            result.AssertResponseOkResultAndValidateValue(expected);
        }


        [Test]
        public async Task GetProviders_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
        {
            // Arrange
            var expected = providers.Select(x => x.ToModel());
            providerService.Setup(x => x.GetAll()).ReturnsAsync(providers.Select(p => p.ToModel()));

            // Act
            var result = await providerController.Get().ConfigureAwait(false);

            // Assert
            result.AssertResponseOkResultAndValidateValue(expected);
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
        public async Task GetProviderById_WhenProviderWithIdExistsInDb_ReturnsOkObjectResult_WithExpectedValue()
        {
            // Arrange
            var expectedDto = providers.RandomItem().ToModel();
            var existingId = expectedDto.Id;
            providerService.Setup(x => x.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(providers.SingleOrDefault(x => x.Id == existingId).ToModel());

            // Act
            var result = await providerController.GetById(existingId).ConfigureAwait(false);

            // Assert
            result.AssertResponseOkResultAndValidateValue(expectedDto);
        }

        [Test]
        public async Task GetProviderById_WhenIdDoesntExistsInDb_ReturnsNotFoundResult()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid();
            var expected = new NotFoundObjectResult($"There is no Provider in DB with {nameof(invalidUserId)} - {invalidUserId}");
            providerService.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(null as ProviderDto);

            // Act
            var result = await providerController.GetById(invalidUserId).ConfigureAwait(false);

            // Assert
            result.AssertExpectedResponseTypeAndCheckDataInside<NotFoundObjectResult>(expected);
        }

        [Test]
        public async Task CreateProvider_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var expectedCreated = provider.ToModel();
            var expectedResponse = new CreatedAtActionResult(
                nameof(providerController.GetById),
                nameof(ProviderController),
                new { providerId = expectedCreated.Id, },
                expectedCreated);
            providerService.Setup(x => x.Create(It.IsAny<ProviderDto>())).ReturnsAsync(provider.ToModel());

            // Act
            var result = await providerController.Create(provider.ToModel()).ConfigureAwait(false);

            // Assert
            result.AssertExpectedResponseTypeAndCheckDataInside<CreatedAtActionResult>(expectedResponse);
        }

        [Test]
        public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("CreateProvider", "Invalid model state.");
            var expected = new BadRequestObjectResult(new ModelStateDictionary(dictionary));
            providerController.ModelState.AddModelError("CreateProvider", "Invalid model state.");

            // Act
            var result = await providerController.Create(provider.ToModel()).ConfigureAwait(false);

            // Assert
            result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
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
            result.AssertResponseOkResultAndValidateValue(providerDto);
        }

        [Test]
        public async Task UpdateProvider_WhenModelWithErrorsReceived_BadRequest_And_ModelsIsValid_False()
        {
            // Arrange
            var providerToUpdateDto = provider.ToModel();
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("UpdateError", "bad model state");
            var expected = new BadRequestObjectResult(new ModelStateDictionary(dictionary));
            providerController.ModelState.AddModelError("UpdateError", "bad model state");

            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>()))
                .ReturnsAsync(provider.ToModel());

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
            Assert.That(!providerController.ModelState.IsValid);
        }

        [Test]
        public async Task UpdateProvider_WhenCorrectData_AND_WrongUserId_ModelIsValid_But_BadRequest()
        {
            // Arrange
            var providerToUpdateDto = providers.FirstOrDefault().ToModel();
            providerToUpdateDto.FullTitle = "New Title for changed provider";
            var expected = new BadRequestObjectResult("Can't change Provider with such parameters.\n" +
                        "Please check that information are valid.");
            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>())).ReturnsAsync(null as ProviderDto);

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            Assert.That(providerController.ModelState.IsValid);
            result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
        }

        [Test]
        public async Task UpdateProvider_ServiceCantGetRequestedProvider_BadRequest_WithExceptionAsValue()
        {
            // Arrange
            var providerToUpdateDto = ProviderDtoGenerator.Generate();
            providerToUpdateDto.FullTitle = "New Title for changed provider";
            var expected = new BadRequestObjectResult(new DbUpdateConcurrencyException());
            providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>())).ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

            // Assert
            result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
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
            var expected = new BadRequestObjectResult("string exception");
            providerService.Setup(x => x.Delete(guid)).ThrowsAsync(new ArgumentNullException());

            // Act
            var result = await providerController.Delete(guid).ConfigureAwait(false);

            // Assert
            result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
        }




    }
}

