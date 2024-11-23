using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V2;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopControllerV2Tests
{
    private const int Ok = 200;
    private const int NoContent = 204;
    private const int Create = 201;
    private const int BadRequest = 400;
    private const int Forbidden = 403;

    private static List<WorkshopV2Dto> workshops;
    private static List<WorkshopCard> workshopCards;
    private static WorkshopResultDto workshopResultDto;
    private static WorkshopV2Dto workshopCreateDto;
    private static WorkshopV2CreateRequestDto workshopV2CreateRequestDto;
    private static ProviderDto provider;
    private static Mock<IOptions<AppDefaultsConfig>> options;

    private WorkshopController controller;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceMoq;
    private Mock<IProviderServiceV2> providerServiceMoq;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<IUserService> userServiceMoq;
    private Mock<ILogger<WorkshopController>> loggerMoq;
    private Mock<HttpContext> httpContextMoq;
    private IMapper mapper;

    private string userId;
    private List<WorkshopBaseCard> workshopBaseCards;
    private List<ShortEntityDto> workshopShortEntitiesList;
    private List<WorkshopProviderViewCard> workshopProviderViewCardList;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        userId = "someUserId";
        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        httpContextMoq.Setup(x => x.User.IsInRole("provider"))
            .Returns(true);
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        workshops = WorkshopV2DtoGenerator.Generate(5);
        provider = ProviderDtoGenerator.Generate();
        workshopCreateDto = WorkshopV2DtoGenerator.Generate();
        workshopV2CreateRequestDto = mapper.Map<WorkshopV2CreateRequestDto>(WorkshopGenerator.Generate());
        workshopCreateDto.Address = AddressDtoGenerator.Generate();
        workshopCreateDto.DateTimeRanges = DateTimeRangeDtoGenerator.Generate(5);
        workshopCreateDto.ProviderId = provider.Id;
        workshopResultDto = new WorkshopResultDto()
        {
            Workshop = workshopCreateDto,
        };
        workshopCards = WorkshopCardGenerator.Generate(5);
        workshopBaseCards = WorkshopBaseCardGenerator.Generate(5);
        workshopShortEntitiesList = ShortEntityDtoGenerator.Generate(10);
        workshopProviderViewCardList = WorkshopProviderViewCardGenerator.Generate(5);

        workshopV2CreateRequestDto.ProviderId = provider.Id;
        var config = new AppDefaultsConfig();
        config.City = "Київ";
        options = new Mock<IOptions<AppDefaultsConfig>>();
        options.Setup(x => x.Value).Returns(config);
    }

    [SetUp]
    public void Setup()
    {
        workshopServiceMoq = new Mock<IWorkshopServicesCombinerV2>();
        providerServiceMoq = new Mock<IProviderServiceV2>();
        providerAdminService = new Mock<IProviderAdminService>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        userServiceMoq = new Mock<IUserService>();
        loggerMoq = new Mock<ILogger<WorkshopController>>();

        controller = new WorkshopController(
            workshopServiceMoq.Object,
            providerServiceMoq.Object,
            localizer.Object,
            loggerMoq.Object,
            providerAdminService.Object,
            userServiceMoq.Object,
            options.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
        };
    }

    #region CreateWorkshop
    [Test]
    public async Task CreateWorkshop_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
    {
        // Arrange
        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(provider).Verifiable(Times.Exactly(2));
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
            .ReturnsAsync(workshopResultDto).Verifiable(Times.Once);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Create, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenDtoIsNull_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var workshopCreateDto = (WorkshopV2CreateRequestDto)null;

        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Never);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(provider).Verifiable(Times.Never);
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
            .ReturnsAsync(workshopResultDto).Verifiable(Times.Never);

        // Act
        var result = await controller.Create(workshopCreateDto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenProviderIsBlocked_ShouldReturn403ObjectResult()
    {
        // Arrange
        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(true).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(provider).Verifiable(Times.Never);
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
            .ReturnsAsync(workshopResultDto).Verifiable(Times.Never);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto) as ObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenCurrentUserIsBlocked_ShouldReturn403ObjectResult()
    {
        // Arrange
        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(true).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(provider).Verifiable(Times.Never);
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
            .ReturnsAsync(workshopResultDto).Verifiable(Times.Never);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto) as ObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(provider).Verifiable(Times.Never);
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
            .ReturnsAsync(workshopResultDto).Verifiable(Times.Never);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenProviderHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        var notAuthorProvider = new ProviderDto() { Id = It.IsAny<Guid>(), UserId = userId };

        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(notAuthorProvider).Verifiable(Times.Once);
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
            .ReturnsAsync(workshopResultDto).Verifiable(Times.Never);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto) as ObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenModelHasInvalidMemberOfWorkshopId_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(provider).Verifiable(Times.Once);
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto)).
            ThrowsAsync(new InvalidOperationException(It.IsAny<string>())).Verifiable(Times.Once);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenUserProvidersOwnerOrAdmin_ShouldCallGiveAssistantAccessToWorkshopOnce()
    {
        // Arrange
        providerServiceMoq.Setup(x => x.GetProviderIdForWorkshopById(It.IsAny<Guid>()))
            .ReturnsAsync(provider.Id).Verifiable(Times.Never);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        userServiceMoq.Setup(x => x.IsBlocked(It.IsAny<string>()))
            .ReturnsAsync(false).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((ProviderDto)null).Verifiable(Times.Exactly(2));
        workshopServiceMoq.Setup(x => x.Create(workshopV2CreateRequestDto))
                    .ReturnsAsync(workshopResultDto).Verifiable(Times.Once);

        int n = 0;
        providerAdminService.Setup(x => x.CheckUserIsRelatedProviderAdmin(userId, provider.Id, Guid.Empty))
            .ReturnsAsync(() => n++ <= 0).Verifiable(Times.Exactly(2));
        providerAdminService.Setup(x => x.GiveAssistantAccessToWorkshop(userId, workshopResultDto.Workshop.Id))
            .Verifiable(Times.Once);

        // Act
        var result = await controller.Create(workshopV2CreateRequestDto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        providerAdminService.VerifyAll();
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        userServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Create, result.StatusCode);
    }
    #endregion
}