using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Individual;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
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
    private Mock<ICurrentUserService> currentUserService;
    private List<Provider> providers;
    private Provider provider;
    private string userId;

    [SetUp]
    public void Setup()
    {
        userId = Guid.NewGuid().ToString();

        providerService = new Mock<IProviderService>();
        currentUserService = new Mock<ICurrentUserService>();
        var logger = new Mock<ILogger<ProviderController>>();

        // Setup current user service with default values
        currentUserService.Setup(x => x.UserId).Returns(userId);
        currentUserService.Setup(x => x.IsInRole(Role.Employee)).Returns(false);

        providerController = new ProviderController(
            providerService.Object,
            currentUserService.Object,
            logger.Object);

        providerController.ControllerContext.HttpContext = this.GetFakeHttpContext();

        providers = ProvidersGenerator.Generate(10);
        provider = ProvidersGenerator.Generate();
    }

    [Test]
    public async Task GetProfile_WhenNoProviderWithSuchUserId_ReturnsNoContent()
    {
        // Arrange
        providerService.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(null as ProviderDto);

        // Act
        var result = await providerController.GetProfile().ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task GetProfile_WhenProviderForUserIdExists_ReturnsOkObjectResult_WithExpectedValue()
    {
        // Arrange
        var expected = provider.ToDto();
        providerService.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(provider.ToDto());

        // Act
        var result = await providerController.GetProfile().ConfigureAwait(false);

        // Assert
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetProfile_WhenUserIdIsNull_ReturnsNoContentResult()
    {
        // Arrange
        currentUserService.Setup(x => x.UserId).Returns(string.Empty);

        // Act
        var result = await providerController.GetProfile().ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task GetProviderById_WhenProviderWithIdExistsInDb_ReturnsOkObjectResult_WithExpectedValue()
    {
        // Arrange
        var expectedDto = providers.RandomItem().ToDto();
        var existingId = expectedDto.Id;
        providerService.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providers.SingleOrDefault(x => x.Id == existingId).ToDto());

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
        var createDto = ProviderCreateDtoGenerator.Generate();
        var expectedCreated = createDto.ToModel().ToDto();
        var expectedResponse = new CreatedAtActionResult(
            nameof(providerController.GetById),
            nameof(ProviderController),
            new { providerId = expectedCreated.Id, },
            expectedCreated
        );
        providerService.Setup(x => x.Create(It.IsAny<ProviderCreateDto>())).ReturnsAsync(expectedCreated);

        // Act
        var result = await providerController.Create(createDto).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<CreatedAtActionResult>(expectedResponse);
    }

    [Test]
    public async Task CreateProvider_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var createDto = ProviderCreateDtoGenerator.Generate();
        var dictionary = new ModelStateDictionary();
        dictionary.AddModelError("CreateProvider", "Invalid model state.");
        var expected = new BadRequestObjectResult(new ModelStateDictionary(dictionary));
        providerController.ModelState.AddModelError("CreateProvider", "Invalid model state.");

        // Act
        var result = await providerController.Create(createDto).ConfigureAwait(false);

        // Assert
        result.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
    }

    [Test]
    public async Task UpdateProvider_WhenModelIsValidAndProviderExists_ReturnsOkObjectResult()
    {
        // Arrange
        var providerToUpdate = providers.FirstOrDefault();
        providerToUpdate.FullTitle = TestDataHelper.GetRandomWords();
        var providerDto = GenerateProviderUpdateDto();
        providerService.Setup(x => x.Update(providerDto, userId))
            .ReturnsAsync(providerToUpdate.ToDto());

        // Act
        var result = await providerController.Update(providerDto).ConfigureAwait(false);
        var value = (result as ObjectResult).Value as ProviderDto;

        // Assert
        result.AssertResponseOkResultAndValidateValue(providerToUpdate.ToDto());
    }

    [Test]
    public async Task UpdateProvider_WhenModelWithErrorsReceived_BadRequest_And_ModelsIsValid_False()
    {
        // Arrange
        var providerToUpdateDto = GenerateProviderUpdateDto();
        var dictionary = new ModelStateDictionary();
        dictionary.AddModelError("UpdateError", "bad model state");
        var expected = new BadRequestObjectResult(new ModelStateDictionary(dictionary));
        providerController.ModelState.AddModelError("UpdateError", "bad model state");

        providerService.Setup(x => x.Update(providerToUpdateDto, userId))
            .ReturnsAsync(provider.ToDto());

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
        var providerToUpdateDto = GenerateProviderUpdateDto();
        providerToUpdateDto.FullTitle = TestDataHelper.GetRandomWords();
        var expected = new BadRequestObjectResult("Can't change Provider with such parameters.\n" +
                                                  "Please check that information are valid.");
        providerService.Setup(x => x.Update(providerToUpdateDto, userId)).ReturnsAsync(null as ProviderDto);

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
        var providerToUpdateDto = GenerateProviderUpdateDto();
        providerToUpdateDto.FullTitle = TestDataHelper.GetRandomWords();
        var expected = new BadRequestObjectResult(new DbUpdateConcurrencyException());
        providerService.Setup(x => x.Update(providerToUpdateDto, userId)).ThrowsAsync(new DbUpdateConcurrencyException());

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
        providerService.Setup(x => x.Delete(existingProviderGuid)).ReturnsAsync(true);

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
        var errorMessage = TestDataHelper.GetRandomWords();
        providerService.Setup(x => x.Delete(guid)).ReturnsAsync(new ErrorResponse { HttpStatusCode = HttpStatusCode.NotFound, Message = errorMessage });

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
        var data = new UploadEmployeesRequestDto
        {
            Employees = [.. UploadEmployeeDtoGenerator.Generate(entitiesCount)]
        };
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
        var data = (UploadEmployeesRequestDto)null;

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
        var data = new UploadEmployeesRequestDto
        {
            Employees = [.. UploadEmployeeDtoGenerator.Generate(entitiesCount)]
        };
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
    [TestCase(Constants.MaxNumberOfEmployeesToUpload + 1)]
    public async Task Upload_WhenServisThrowsArgumentOutOfRangeException_ReturnsBadRequestObjectResult(int entitiesCount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new UploadEmployeesRequestDto
        {
            Employees = [.. UploadEmployeeDtoGenerator.Generate(entitiesCount)]
        };
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
        var data = new UploadEmployeesRequestDto
        {
            Employees = [.. UploadEmployeeDtoGenerator.Generate(entitiesCount)]
        };
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

    [Test]
    public async Task Update_WhenModelIsValid_PassesUserIdFromCurrentUser()
    {
        // Arrange
        var providerToUpdate = GenerateProviderUpdateDto();
        providerService.Setup(x => x.Update(providerToUpdate, userId))
            .ReturnsAsync(provider.ToDto());

        // Act
        await providerController.Update(providerToUpdate).ConfigureAwait(false);

        // Assert
        providerService.Verify(x => x.Update(providerToUpdate, userId), Times.Once);
    }

    private ProviderCreateDto GenerateProviderCreateDto()
        => new Faker<ProviderCreateDto>()
            .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
            .RuleFor(x => x.ShortTitle, f => f.Company.CompanySuffix())
            .RuleFor(x => x.Ownership, f => f.Random.ArrayElement((OwnershipType[])Enum.GetValues(typeof(OwnershipType))))
            .RuleFor(x => x.TypeId, _ => 1)
            .RuleFor(x => x.Status, f => f.Random.ArrayElement((ProviderStatus[])Enum.GetValues(typeof(ProviderStatus))))
            .RuleFor(x => x.License, f => f.Random.AlphaNumeric(15))
            .RuleFor(x => x.InstitutionType, f => f.PickRandom<InstitutionType>())
            .Generate();

    private ProviderUpdateDto GenerateProviderUpdateDto()
        => new Faker<ProviderUpdateDto>()
            .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
            .RuleFor(x => x.ShortTitle, f => f.Company.CompanySuffix())
            .RuleFor(x => x.TypeId, _ => 1)
            .RuleFor(x => x.Status, f => f.Random.ArrayElement((ProviderStatus[])Enum.GetValues(typeof(ProviderStatus))))
            .RuleFor(x => x.License, f => f.Random.AlphaNumeric(15))
            .RuleFor(x => x.InstitutionType, f => f.PickRandom<InstitutionType>())
            .Generate();
}