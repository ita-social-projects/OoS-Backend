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
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.SocialGroup;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class AdminControllerTests
{
    private AdminController controller;
    private Mock<ISensitiveMinistryAdminService> sensitiveMinistryAdminService;
    private Mock<ISensitiveDirectionService> sensitiveDirectionService;
    private Mock<ISensitiveProviderService> sensitiveProviderService;
    private Mock<ISensitiveApplicationService> sensitiveApplicationService;
    private Mock<ILogger<AdminController>> logger;
    private Mock<IStringLocalizer<SharedResource>> localizer;

    private string userId;
    private Guid providerId;
    private Mock<HttpContext> httpContext;

    private List<ApplicationDto> applications;
    private IEnumerable<ChildDto> children;
    private List<WorkshopCard> workshopCards;
    private ParentDTO parent;
    private ProviderDto provider;
    private WorkshopV2Dto workshopDto;
    private List<MinistryAdminDto> ministryAdminDtos;
    private List<Provider> providers;
    private DirectionDto direction;
    private IMapper mapper;
    private HttpContext fakeHttpContext;
    private string userRole;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        sensitiveMinistryAdminService = new Mock<ISensitiveMinistryAdminService>();
        sensitiveDirectionService = new Mock<ISensitiveDirectionService>();
        sensitiveProviderService = new Mock<ISensitiveProviderService>();
        sensitiveApplicationService = new Mock<ISensitiveApplicationService>();
        logger = new Mock<ILogger<AdminController>>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();

        userId = Guid.NewGuid().ToString();
        ministryAdminDtos = AdminGenerator.GenerateMinistryAdminsDtos(10);

        httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));


        controller = new AdminController(
            logger.Object,
            sensitiveMinistryAdminService.Object,
            sensitiveApplicationService.Object,
            sensitiveDirectionService.Object,
            sensitiveProviderService.Object,
            localizer.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContext.Object },
        };
        providerId = Guid.NewGuid();

        workshopCards = WorkshopCardGenerator.Generate(10);

        workshopDto = WorkshopV2DtoGenerator.Generate();
        workshopDto.ProviderId = providerId;
        children = ChildDtoGenerator.Generate(2).WithSocial(new SocialGroupDto { Id = 1 });
        providers = ProvidersGenerator.Generate(10);
        parent = ParentDtoGenerator.Generate().WithUserId(userId);
        provider = ProviderDtoGenerator.Generate();
        provider.UserId = userId;
        provider.Id = providerId;
        applications = ApplicationDTOsGenerator.Generate(2).WithWorkshopCard(workshopCards.First()).WithParent(parent);

        fakeHttpContext = GetFakeHttpContext();
        controller.ControllerContext.HttpContext = fakeHttpContext;
    }

    [Test]
    public async Task GetByFilterMinistryAdmin_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        var expected = new SearchResult<MinistryAdminDto>
        {
            TotalAmount = 10,
            Entities = ministryAdminDtos.Select(x => mapper.Map<MinistryAdminDto>(x)).ToList(),
        };

        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        sensitiveMinistryAdminService.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetByFilerMinistryAdmin_WhenCalled_ReturnsNoContentResultObject()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;

        var expected = new SearchResult<MinistryAdminDto>
        {
            TotalAmount = 0,
            Entities = ministryAdminDtos.Select(x => mapper.Map<MinistryAdminDto>(x)).ToList(),
        };

        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task GetByFilterMinistryAdmin_ReturnsObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Provider).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;

        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>())).ReturnsAsync(new SearchResult<MinistryAdminDto> { TotalAmount = 0, Entities = new List<MinistryAdminDto>() });

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false);

        // Assert
        sensitiveMinistryAdminService.Verify(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()), Times.Never);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task GetApplications_WhenCalledByAdmin_ShouldReturnOkResultObject()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveApplicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ReturnsAsync(new SearchResult<ApplicationDto>
        {
            Entities = applications,
            TotalAmount = applications.Count,
        });

        // Act
        var result = await controller.GetApplications(new ApplicationFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GetApplications_WhenCollectionIsEmpty_ShouldReturnNoContent()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveApplicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ReturnsAsync(new SearchResult<ApplicationDto>()
        {
            Entities = new List<ApplicationDto>(),
            TotalAmount = 0,
        });

        // Act
        var result = await controller.GetApplications(new ApplicationFilter()).ConfigureAwait(false) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public async Task GetApplications_WhenCalledParentOrProvider_RetursObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Parent).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveApplicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await controller.GetApplications(new ApplicationFilter()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task UpdateDirections_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        var changedDirection = new DirectionDto()
        {
            Id = 1,
            Title = "ChangedTitle",
        };
        sensitiveDirectionService.Setup(x => x.Update(changedDirection)).ReturnsAsync(changedDirection);

        // Act
        var result = await controller.UpdateDirections(changedDirection).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(200, result.StatusCode);
    }

    [Test]
    public async Task UpdateDirections_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        controller.ModelState.AddModelError("UpdateDirection", "Invalid model state.");

        // Act
        var result = await controller.UpdateDirections(direction).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That(((BadRequestObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateDirections_WhenUserIsNotTechAdmin_ReturnsObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.MinistryAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        var changedDirection = new DirectionDto()
        {
            Id = 1,
            Title = "ChangedTitle",
        };
        sensitiveDirectionService.Setup(x => x.Update(changedDirection)).ReturnsAsync(changedDirection);

        // Act
        var result = await controller.UpdateDirections(changedDirection).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    [TestCase(1)]
    public async Task DeleteDirectionById_WhenIdIsValid_ReturnsNoContentResult(long id)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Success(direction));

        // Act
        var result = await controller.DeleteDirectionById(id) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(204, result.StatusCode);
    }

    [Test]
    [TestCase(0)]
    public void DeleteDirectionById_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveDirectionService.Setup(x => x.Delete(id));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await controller.DeleteDirectionById(id).ConfigureAwait(false));
    }

    [Test]
    [TestCase(10)]
    public async Task DeleteDirectionById_WhenIdIsInvalid_ReturnsNull(long id)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Success(direction));

        // Act
        var result = await controller.DeleteDirectionById(id) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    [TestCase(10)]
    public async Task DeleteDirectionById_WhenThereAreRelatedWorkshops_ReturnsBadRequestObjectResult(long id)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Failed(new OperationError
        {
            Code = "400",
            Description = "Some workshops assosiated with this direction. Deletion prohibited.",
        }));

        // Act
        var result = await controller.DeleteDirectionById(id);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
    }

    [Test]
    [TestCase(1)]
    public async Task DeleteDirectionById_WhenIdIsValid_ReturnsObjectResult(long id)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.MinistryAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Success(direction));

        // Act
        var result = await controller.DeleteDirectionById(id);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task GetProviderByFilter_ReturnsOkResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        var expected = new SearchResult<ProviderDto> { TotalAmount = 1, Entities = new List<ProviderDto>() };
        sensitiveProviderService.Setup(x => x.GetByFilter(It.IsAny<ProviderFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await controller.GetProviderByFilter(new ProviderFilter()).ConfigureAwait(false) as ActionResult;

        // Assert
        sensitiveProviderService.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetProviderByFilter_ReturnsObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.MinistryAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        var expected = new SearchResult<ProviderDto> { TotalAmount = 1, Entities = new List<ProviderDto>() };
        sensitiveProviderService.Setup(x => x.GetByFilter(It.IsAny<ProviderFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await controller.GetProviderByFilter(new ProviderFilter()).ConfigureAwait(false) as ActionResult;

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task BlockProvider_WithValidIdAndBlocked_ReturnsOkResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = true, Message = "test"});

        // Act
        var result = await controller.BlockProvider(new ProviderBlockDto{BlockReason = "str", Id = new Guid("4c617c71-c131-4ad6-9eac-bb16288f5322"), IsBlocked = false, BlockPhoneNumber = "-"});

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task BlockProvider_WithValidIdAndBlocked_ReturnsForbidResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;

        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.Forbidden, IsSuccess = false, Message = "test"});

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task BlockProvider_WithValidIdAndBlocked_ReturnsNotFoundResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.NotFound, IsSuccess = false, Message = "test"});

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task BlockProvider_WithValidIdAndBlocked_DefaultCase_ReturnsNotFoundResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.TechAdmin).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = false, Message = "test"});

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task BlockProvider_UserIsNotTechAdmin_ReturnsObjectResult()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Parent).ToLower()),
            }));
        controller.ControllerContext.HttpContext.User = user;
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = false, Message = "test"});

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    private HttpContext GetFakeHttpContext()
    {
        var authProps = new AuthenticationProperties();

        authProps.StoreTokens(new List<AuthenticationToken>
        {
            new AuthenticationToken{ Name = "access_token", Value = "accessTokenValue"},
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

        var context = new DefaultHttpContext()
        {
            RequestServices = serviceProviderMock.Object,
        };

        return context;
    }

    [Test]
    [Ignore("Until mock HttpContext.GetTokenAsync()")]
    public async Task Block_ReturnsProviderBlockDto_IfProviderExist()
    {
        // TODO: it's nessesary to mock HttpContext.GetTokenAsync() to run this test.

        // Arrange
        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        var responseDto = new ResponseDto()
        {
            Result = providerBlockDto,
            Message = It.IsAny<string>(),
            HttpStatusCode = HttpStatusCode.OK,
            IsSuccess = true,
        };

        sensitiveProviderService.Setup(x => x.Block(providerBlockDto, It.IsAny<string>()))
            .ReturnsAsync(responseDto);

        // Act
        var result = await controller.BlockProvider(providerBlockDto).ConfigureAwait(false);

        // Assert
        result.AssertResponseOkResultAndValidateValue(responseDto.Result);
    }
}
