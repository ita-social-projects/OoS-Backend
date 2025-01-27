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
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

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
    private Mock<ISensitiveWorkshopsService> sensitiveWorkshopServices;
    private Mock<ISensitiveWorkshopDraftService> sensitiveWorkshopDraftService;

    private string userId;
    private Guid providerId;
    private Mock<HttpContext> httpContext;

    private List<ApplicationDto> applications;
    private List<WorkshopCard> workshopCards;
    private ParentDTO parent;
    private ProviderDto provider;
    private WorkshopV2Dto workshopDto;
    private List<MinistryAdminDto> ministryAdminDtos;
    private DirectionDto direction;
    private List<WorkshopDto> listWorkshopDto;
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
        sensitiveWorkshopDraftService = new Mock<ISensitiveWorkshopDraftService>();
        logger = new Mock<ILogger<AdminController>>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        sensitiveWorkshopServices = new Mock<ISensitiveWorkshopsService>();

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
            sensitiveWorkshopServices.Object,
            localizer.Object,
            sensitiveWorkshopDraftService.Object
            )
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContext.Object },
        };
        providerId = Guid.NewGuid();

        workshopCards = WorkshopCardGenerator.Generate(10);

        workshopDto = WorkshopV2DtoGenerator.Generate();
        workshopDto.ProviderId = providerId;
        parent = ParentDtoGenerator.Generate().WithUserId(userId);
        provider = ProviderDtoGenerator.Generate();
        provider.UserId = userId;
        provider.Id = providerId;
        applications = ApplicationDTOsGenerator.Generate(2).WithWorkshopCard(workshopCards.First()).WithParent(parent);
        listWorkshopDto = WorkshopDtoGenerator.Generate(5);

        fakeHttpContext = GetFakeHttpContext();
        controller.ControllerContext.HttpContext = fakeHttpContext;
    }

    [Test]
    public async Task GetByFilterMinistryAdmin_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);

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
    public async Task GetByFilterMinistryAdmin_ReturnsNoContentResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;

        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>())).ReturnsAsync(new SearchResult<MinistryAdminDto> { TotalAmount = 0, Entities = new List<MinistryAdminDto>() });

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false);

        // Assert
        sensitiveMinistryAdminService.Verify(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()), Times.Once);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task GetApplications_WhenCalledByAdmin_ShouldReturnOkResultObject()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
    public async Task GetApplications_ShouldReturnOkResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
    public async Task UpdateDirections_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.MinistryAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Success(direction));

        // Act
        var result = await controller.DeleteDirectionById(id) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(204, result.StatusCode);
    }

    [Test]
    [TestCase(0)]
    public void DeleteDirectionById_WhenIdIsInvalid_ShouldThrowException(long id)
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.MinistryAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.Parent);

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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                   new ResponseDto() { Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = true, Message = "test" });

        // Act
        var result = await controller.BlockProvider(new ProviderBlockDto { BlockReason = "str", Id = new Guid("4c617c71-c131-4ad6-9eac-bb16288f5322"), IsBlocked = false, BlockPhoneNumber = "-" });

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task BlockProvider_WithValidIdAndBlocked_ReturnsForbidResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);

        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto() { Result = new object(), HttpStatusCode = HttpStatusCode.Forbidden, IsSuccess = false, Message = "test" });

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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto() { Result = new object(), HttpStatusCode = HttpStatusCode.NotFound, IsSuccess = false, Message = "test" });

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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto() { Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = false, Message = "test" });

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
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.Parent);
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto() { Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = false, Message = "test" });

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task ValidateImportData_CheckData_ReturnsOkResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        sensitiveProviderService
            .Setup(x => x.ValidateImportData(It.IsAny<ImportDataValidateRequest>())).ReturnsAsync(
                new ImportDataValidateResponse());

        // Act
        var result = await controller.ValidateImportData(It.IsAny<ImportDataValidateRequest>());

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
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

    [Test]
    public async Task ExportProviders_WhenNoProvidersInDb_ReturnsNoContentResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);

        sensitiveProviderService
            .Setup(s => s.GetCsvExportData())
            .ReturnsAsync([]);

        // Act
        var result = await controller.ExportProviders().ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task ExportProviders_WhenSomeProvidersInDb_ReturnsFileContentResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);

        sensitiveProviderService
            .Setup(s => s.GetCsvExportData())
            .ReturnsAsync([1, 2, 3, 4, 5]);

        // Act
        var result = await controller.ExportProviders().ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<FileContentResult>(result);
    }

    [Test]
    public async Task GetWorkshopsByFilter_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        var filter = new WorkshopFilterAdministration();
        var expected = new SearchResult<WorkshopDto>
        {
            TotalAmount = 5,
            Entities = listWorkshopDto.ToList(),
        };

        sensitiveWorkshopServices.Setup(x => x.FetchByFilterForAdmins(filter)).ReturnsAsync(expected);

        // Act
        var result = await controller.GetWorkshopsByFilter(filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        sensitiveWorkshopServices.Verify(x => x.FetchByFilterForAdmins(filter), Times.Once);
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetWorkshopsByFilter_WhenServiceReturnEmptyCollectionOfEntities_ShouldReturnsNoContentResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.Moderator);
        var filter = new WorkshopFilterAdministration();
        var expected = new SearchResult<WorkshopDto>
        {
            TotalAmount = 0,
            Entities = new List<WorkshopDto>(),
        };

        sensitiveWorkshopServices.Setup(x => x.FetchByFilterForAdmins(filter)).ReturnsAsync(expected);

        // Act
        var result = await controller.GetWorkshopsByFilter(filter).ConfigureAwait(false);

        // Assert
        sensitiveWorkshopServices.Verify(x => x.FetchByFilterForAdmins(filter), Times.Once);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task GetWorkshopDraftsByFilter_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.TechAdmin);
        var filter = new WorkshopDraftFilterAdministration();
        var expected = new SearchResult<WorkshopV2Dto>
        {
            TotalAmount = 5,
            Entities = WorkshopV2DtoGenerator.Generate(5),
        };

        sensitiveWorkshopDraftService.Setup(x => x.FetchByFilterForAdmins(filter)).ReturnsAsync(expected);

        // Act
        var result = await controller.GetWorkshopDraftsByFilter(filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        sensitiveWorkshopDraftService.Verify(x => x.FetchByFilterForAdmins(filter), Times.Once);
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetWorkshopDraftsByFilter_WhenServiceReturnEmptyCollectionOfEntities_ShouldReturnsNoContentResult()
    {
        // Arrange
        controller.ControllerContext.HttpContext = fakeHttpContext;
        controller.ControllerContext.HttpContext.SetContextUser(Role.Moderator);
        var filter = new WorkshopDraftFilterAdministration();
        var expected = new SearchResult<WorkshopV2Dto>
        {
            TotalAmount = 0,
            Entities = new List<WorkshopV2Dto>(),
        };

        sensitiveWorkshopDraftService.Setup(x => x.FetchByFilterForAdmins(filter)).ReturnsAsync(expected);

        // Act
        var result = await controller.GetWorkshopDraftsByFilter(filter).ConfigureAwait(false);

        // Assert
        sensitiveWorkshopDraftService.Verify(x => x.FetchByFilterForAdmins(filter), Times.Once);
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}
