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
public class PublicProviderControllerTests
{
    private PublicProviderController providerController;
    private Mock<IPublicProviderService> publicProviderService;
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

        publicProviderService = new Mock<IPublicProviderService>();
        providerController = new PublicProviderController(publicProviderService.Object);

        providerController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        providers = ProvidersGenerator.Generate(10);
        provider = ProvidersGenerator.Generate();
    }

    [Test]
    public async Task StatusUpdate_WhenIdIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var provider = providers.FirstOrDefault();

        var updateRequest = new ProviderStatusDto
        {
            ProviderId = provider.Id,
            Status = ProviderStatus.Approved,
        };
        publicProviderService.Setup(x => x.UpdateStatus(updateRequest, userId))
            .ReturnsAsync(updateRequest);

        // Act
        var result = await providerController.StatusUpdate(updateRequest).ConfigureAwait(false);

        // Assert
        result.AssertResponseOkResultAndValidateValue(updateRequest);
    }

    [Test]
    public async Task StatusUpdate_WhenIdDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var nonExistentProviderId = Guid.NewGuid();
        var expected = new NotFoundObjectResult($"There is no Provider in DB with Id - {nonExistentProviderId}");
        var updateRequest = new ProviderStatusDto
        {
            ProviderId = Guid.NewGuid(),
            Status = ProviderStatus.Approved,
        };
        publicProviderService.Setup(x => x.UpdateStatus(updateRequest, userId))
            .ReturnsAsync(null as ProviderStatusDto);

        // Act
        var result = await providerController.StatusUpdate(updateRequest).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<NotFoundObjectResult>(expected);
    }
}
