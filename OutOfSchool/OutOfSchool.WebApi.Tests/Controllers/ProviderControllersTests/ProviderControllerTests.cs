using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models.Individual;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers.ProviderControllersTests;

[TestFixture]
public class ProviderControllerTests
{
    private ProviderController providerController;
    private Mock<IProviderService> providerService;
    private List<Provider> providers;
    private Provider provider;
    private IMapper mapper;
    private string userId;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        userId = Guid.NewGuid().ToString();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();

        providerService = new Mock<IProviderService>();
        providerController = new ProviderController(providerService.Object, localizer.Object, new Mock<ILogger<ProviderController>>().Object);

        providerController.ControllerContext.HttpContext = GetFakeHttpContext();
        providers = ProvidersGenerator.Generate(10);
        provider = ProvidersGenerator.Generate();
    }

    [Test]
    public async Task GetProfile_WhenNoProviderWithSuchUserId_ReturnsNoContent()
    {
        // Arrange
        providerService.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null as ProviderDto);

        // Act
        var result = await providerController.GetProfile().ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task GetProfile_WhenProviderForUserIdExists_ReturnsOkObjectResult_WithExpectedValue()
    {
        // Arrange
        var expected = mapper.Map<ProviderDto>(provider);
        providerService.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(mapper.Map<ProviderDto>(provider));

        // Act
        var result = await providerController.GetProfile().ConfigureAwait(false);

        // Assert
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetProviderById_WhenProviderWithIdExistsInDb_ReturnsOkObjectResult_WithExpectedValue()
    {
        // Arrange
        var expectedDto = mapper.Map<ProviderDto>(providers.RandomItem());
        var existingId = expectedDto.Id;
        providerService.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(mapper.Map<ProviderDto>(providers.SingleOrDefault(x => x.Id == existingId)));

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
        var expectedCreated = mapper.Map<ProviderDto>(provider);
        var expectedResponse = new CreatedAtActionResult(
            nameof(providerController.GetById),
            nameof(ProviderController),
            new { providerId = expectedCreated.Id, },
            expectedCreated);
        providerService.Setup(x => x.Create(It.IsAny<ProviderCreateDto>())).ReturnsAsync(mapper.Map<ProviderDto>(provider));

        // Act
        var result = await providerController.Create(mapper.Map<ProviderCreateDto>(provider)).ConfigureAwait(false);

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
        var result = await providerController.Create(mapper.Map<ProviderCreateDto>(provider)).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
    }

    [Test]
    public async Task UpdateProvider_WhenModelIsValidAndProviderExists_ReturnsOkObjectResult()
    {
        // Arrange
        var providerToUpdate = providers.FirstOrDefault();
        providerToUpdate.FullTitle = TestDataHelper.GetRandomWords();
        var providerDto = mapper.Map<ProviderUpdateDto>(providerToUpdate);
        providerService.Setup(x => x.Update(providerDto, It.IsAny<string>()))
            .ReturnsAsync(mapper.Map<ProviderDto>(providerToUpdate));

        // Act
        var result = await providerController.Update(providerDto).ConfigureAwait(false);
        var value = (result as ObjectResult).Value as ProviderDto;

        // Assert
        result.AssertResponseOkResultAndValidateValue(mapper.Map<ProviderDto>(providerToUpdate));
    }

    [Test]
    public async Task UpdateProvider_WhenModelWithErrorsReceived_BadRequest_And_ModelsIsValid_False()
    {
        // Arrange
        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        var dictionary = new ModelStateDictionary();
        dictionary.AddModelError("UpdateError", "bad model state");
        var expected = new BadRequestObjectResult(new ModelStateDictionary(dictionary));
        providerController.ModelState.AddModelError("UpdateError", "bad model state");

        providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>()))
            .ReturnsAsync(mapper.Map<ProviderDto>(provider));

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
        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(providers.FirstOrDefault());
        providerToUpdateDto.FullTitle = TestDataHelper.GetRandomWords();
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
        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(ProviderDtoGenerator.Generate());
        providerToUpdateDto.FullTitle = TestDataHelper.GetRandomWords();
        var expected = new BadRequestObjectResult(new DbUpdateConcurrencyException());
        providerService.Setup(x => x.Update(providerToUpdateDto, It.IsAny<string>())).ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await providerController.Update(providerToUpdateDto).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
    }

    [Test]
    public async Task DeleteProvider_WhenIdIsValid_ReturnsOkResult()
    {
        // Arrange
        var existingProviderGuid = providers.Select(p => p.Id).FirstOrDefault();
        providerService.Setup(x => x.Delete(existingProviderGuid, It.IsAny<string>())).ReturnsAsync(new ObjectResult(null));

        // Act
        var result = await providerController.Delete(existingProviderGuid);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
    }

    [Test]
    public async Task DeleteProvider_WhenIdIsInvalid_ReturnsBadRequestObjectResultAsync()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var errorMessage = TestDataHelper.GetRandomWords();
        providerService.Setup(x => x.Delete(guid, It.IsAny<string>())).ReturnsAsync(new ErrorResponse { HttpStatusCode = HttpStatusCode.NotFound, Message = errorMessage });

        // Act
        var result = await providerController.Delete(guid).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        Assert.AreEqual((int)HttpStatusCode.NotFound, (result as ObjectResult).StatusCode);
        Assert.AreEqual(errorMessage, (result as ObjectResult).Value);
    }

    private HttpContext GetFakeHttpContext()
    {
        var authProps = new AuthenticationProperties();

        authProps.StoreTokens(new List<AuthenticationToken>
        {
            new() { Name = "access_token", Value = "accessTokenValue"},
        });

        var authResult = AuthenticateResult
            .Success(new AuthenticationTicket(new ClaimsPrincipal(), authProps, It.IsAny<string>()));

        var authenticationServiceMock = new Mock<IAuthenticationService>();

        authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResult);

        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock
            .Setup(s => s.GetService(typeof(IAuthenticationService)))
            .Returns(authenticationServiceMock.Object);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new(IdentityResourceClaimsTypes.Sub, userId),
                    new(IdentityResourceClaimsTypes.Role, Role.Provider.ToString()),
                },
                IdentityResourceClaimsTypes.Sub));

        var context = new DefaultHttpContext()
        {
            RequestServices = serviceProviderMock.Object,
            User = user,
        };

        return context;
    }

    [Test]
    [TestCase(4)]
    [TestCase(10)]
    public async Task Upload_WhenModelIsValid_ReturnsOkObjectResult(int entitiesCount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = UploadEmployeeDtoGenerator.Generate(entitiesCount).ToArray();
        providerService.Setup(ps => ps.UploadEmployeesForProvider(It.IsAny<Guid>(), It.IsAny<UploadEmployeeRequestDto[]>()))
            .ReturnsAsync(It.IsAny<UploadEmployeeResponse>())
            .Verifiable(Times.Once);

        // Act
        var result = await providerController.Upload(id, data)
            .ConfigureAwait(false);

        //Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        providerService.VerifyAll();
    }

    [Test]
    public void Upload_WhenModelIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = (UploadEmployeeRequestDto[])null;
        providerService.Setup(ps => ps.UploadEmployeesForProvider(It.IsAny<Guid>(), It.IsAny<UploadEmployeeRequestDto[]>()))
            .ReturnsAsync(It.IsAny<UploadEmployeeResponse>())
            .Verifiable(Times.Never);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await providerController.Upload(id, data).ConfigureAwait(false));
        providerService.VerifyAll();
    }

    [Test]
    [TestCase(0)]
    public async Task Upload_WhenServisThrowsInvalidOperationException_ReturnsBadRequestObjectResult(int entitiesCount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = UploadEmployeeDtoGenerator.Generate(entitiesCount).ToArray();
        var errorMessage = "Error message";
        providerService.Setup(ps => ps.UploadEmployeesForProvider(It.IsAny<Guid>(), It.IsAny<UploadEmployeeRequestDto[]>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage))
            .Verifiable(Times.Once);

        // Act
        var result = await providerController.Upload(id, data)
           .ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status400BadRequest);
        providerService.VerifyAll();
    }

    [Test]
    [TestCase(101)]
    public async Task Upload_WhenServisThrowsArgumentOutOfRangeException_ReturnsBadRequestObjectResult(int entitiesCount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = UploadEmployeeDtoGenerator.Generate(entitiesCount).ToArray();
        var errorMessage = "Error message";
        providerService.Setup(ps => ps.UploadEmployeesForProvider(It.IsAny<Guid>(), It.IsAny<UploadEmployeeRequestDto[]>()))
            .ThrowsAsync(new ArgumentOutOfRangeException(errorMessage))
            .Verifiable(Times.Once);

        // Act
        var result = await providerController.Upload(id, data)
           .ConfigureAwait(false);

        // Assert
        result.Should()
              .BeOfType<BadRequestObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status400BadRequest);
        providerService.VerifyAll();
    }

    [Test]
    [TestCase(4)]
    [TestCase(10)]
    public async Task Upload_WhenModelIsInvalid_ReturnsBadRequestObjectResult(int entitiesCount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = UploadEmployeeDtoGenerator.Generate(entitiesCount).ToArray();
        var dictionary = new ModelStateDictionary();
        dictionary.AddModelError("CreateProvider", "Invalid model state.");
        var expected = new BadRequestObjectResult(new ModelStateDictionary(dictionary));
        providerController.ModelState.AddModelError("CreateProvider", "Invalid model state.");
        providerService.Setup(ps => ps.UploadEmployeesForProvider(It.IsAny<Guid>(), It.IsAny<UploadEmployeeRequestDto[]>()))
            .ReturnsAsync(It.IsAny<UploadEmployeeResponse>())
            .Verifiable(Times.Never);

        // Act
        var result = await providerController.Upload(id, data)
            .ConfigureAwait(false);

        //Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
        providerService.VerifyAll();
    }
}