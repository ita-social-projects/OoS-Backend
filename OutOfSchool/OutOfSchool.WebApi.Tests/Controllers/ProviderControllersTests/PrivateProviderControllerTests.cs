using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services.ProviderServices;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Controllers.ProviderControllersTests;

[TestFixture]
public class PrivateProviderControllerTests
{
    private PrivateProviderController privateProviderController;
    private Mock<IPrivateProviderService> privateProviderService;
    private List<Provider> providers;
    private Provider provider;
    private IMapper mapper;
    private string userId;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        userId = Guid.NewGuid().ToString();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var user = new ClaimsPrincipal
        (new ClaimsIdentity(
            new Claim[]
            {
                new Claim(IdentityResourceClaimsTypes.Sub, userId),
                new Claim(IdentityResourceClaimsTypes.Role, Role.Provider.ToString()),
            },
            IdentityResourceClaimsTypes.Sub));

        privateProviderService = new Mock<IPrivateProviderService>();
        privateProviderController = new PrivateProviderController(privateProviderService.Object);

        privateProviderController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        providers = ProvidersGenerator.Generate(10);
        provider = ProvidersGenerator.Generate();
    }

    [Test]
    public async Task LicenseStatusUpdate_WhenIdIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var provider = providers.FirstOrDefault();

        var updateRequest = new ProviderLicenseStatusDto
        {
            ProviderId = provider.Id,
            LicenseStatus = ProviderLicenseStatus.Approved,
        };
        privateProviderService.Setup(x => x.UpdateLicenseStatus(updateRequest, userId))
            .ReturnsAsync(updateRequest);

        // Act
        var result = await privateProviderController.LicenseStatusUpdate(updateRequest).ConfigureAwait(false);

        // Assert
        result.AssertResponseOkResultAndValidateValue(updateRequest);
    }

    [Test]
    public async Task LicenseStatusUpdate_WhenInvalidRequest_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var errorMessage = TestDataHelper.GetRandomWords();
        var expected = new BadRequestObjectResult(errorMessage);
        var updateRequest = new ProviderLicenseStatusDto
        {
            ProviderId = Guid.NewGuid(),
            LicenseStatus = ProviderLicenseStatus.Approved,
        };
        privateProviderService.Setup(x => x.UpdateLicenseStatus(updateRequest, userId))
            .ThrowsAsync(new ArgumentException(errorMessage));

        // Act
        var result = await privateProviderController.LicenseStatusUpdate(updateRequest).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
    }

    [Test]
    public async Task LicenseStatusUpdate_WhenIdDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var nonExistentProviderId = Guid.NewGuid();
        var expected = new NotFoundObjectResult($"There is no Provider in DB with Id - {nonExistentProviderId}");
        var updateRequest = new ProviderLicenseStatusDto
        {
            ProviderId = Guid.NewGuid(),
            LicenseStatus = ProviderLicenseStatus.Approved,
        };
        privateProviderService.Setup(x => x.UpdateLicenseStatus(updateRequest, userId))
            .ReturnsAsync(null as ProviderLicenseStatusDto);

        // Act
        var result = await privateProviderController.LicenseStatusUpdate(updateRequest).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<NotFoundObjectResult>(expected);
    }
}
