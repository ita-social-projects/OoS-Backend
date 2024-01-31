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
    private Mock<ICurrentUserService> currentUserService;
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
    private IEnumerable<WorkshopCard> workshops;
    private ParentDTO parent;
    private ProviderDto provider;
    private WorkshopV2Dto workshopDto;
    private List<MinistryAdminDto> ministryAdminDtos;
    private List<Provider> providers;
    private DirectionDto direction;
    private IMapper mapper;
    private HttpContext fakeHttpContext;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        currentUserService = new Mock<ICurrentUserService>();
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
            currentUserService.Object,
            sensitiveMinistryAdminService.Object,
            sensitiveApplicationService.Object,
            sensitiveDirectionService.Object,
            sensitiveProviderService.Object,
            localizer.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContext.Object },
        };
        providerId = Guid.NewGuid();
        workshops = FakeWorkshopCards();
        workshopDto = FakeWorkshop();
        workshopDto.ProviderId = providerId;
        children = ChildDtoGenerator.Generate(2).WithSocial(new SocialGroupDto { Id = 1 });
        providers = ProvidersGenerator.Generate(10);
        parent = ParentDtoGenerator.Generate().WithUserId(userId);
        provider = ProviderDtoGenerator.Generate();
        provider.UserId = userId;
        provider.Id = providerId;
        applications = ApplicationDTOsGenerator.Generate(2).WithWorkshopCard(workshops.First()).WithParent(parent);
        fakeHttpContext = GetFakeHttpContext();
        controller.ControllerContext.HttpContext = fakeHttpContext;
    }

    [Test]
    public async Task GetApplications_WhenCalledByAdmin_ShouldReturnOkResultObject()
    {
        // Arrange
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
    public async Task GetProviders_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        sensitiveProviderService.Setup(x => x.GetByFilter(It.IsAny<ProviderFilter>()))
            .ReturnsAsync(new SearchResult<ProviderDto> { TotalAmount = 0, Entities = new List<ProviderDto>() });

        currentUserService.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await controller.GetByFilterProvider(new ProviderFilter()).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    [TestCase(0)]
    public void Delete_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
    {
        // Arrange
        sensitiveDirectionService.Setup(x => x.Delete(id));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await controller.DeleteDirectionById(id).ConfigureAwait(false));
    }

    [Test]
    public void GetApplications_WhenCalledParentOrProvider_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        sensitiveApplicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.GetApplications(new ApplicationFilter()));
    }

    [Test]
    [TestCase(10)]
    public async Task Delete_WhenIdIsInvalid_ReturnsNull(long id)
    {
        // Arrange
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Success(direction));

        // Act
        var result = await controller.DeleteDirectionById(id) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetApplications_WhenCollectionIsEmpty_ShouldReturnNoContent()
    {
        // Arrange
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
    public async Task GetMinistryAdmins_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<MinistryAdminDto>
        {
            TotalAmount = 10,
            Entities = ministryAdminDtos.Select(x => mapper.Map<MinistryAdminDto>(x)).ToList(),
        };

        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(expected);

        currentUserService.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        sensitiveMinistryAdminService.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetMinistryAdmins_WhenCalled_ReturnsForbidResultObject()
    {
        // Arrange
        var expected = new SearchResult<MinistryAdminDto>
        {
            TotalAmount = 10,
            Entities = ministryAdminDtos.Select(x => mapper.Map<MinistryAdminDto>(x)).ToList(),
        };

        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(expected);

        currentUserService.Setup(x => x.IsAdmin()).Returns(false);

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false) as ForbidResult;

        // Assert
        Assert.That(result is ForbidResult);
    }

    [Test]
    [TestCase(10)]
    public async Task Delete_WhenThereAreRelatedWorkshops_ReturnsBadRequestObjectResult(long id)
    {
        // Arrange
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
    public async Task GetProviders_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<ProviderDto>
        {
            TotalAmount = 10,
            Entities = providers.Select(x => mapper.Map<ProviderDto>(x)).ToList(),
        };

        sensitiveProviderService.Setup(x => x.GetByFilter(It.IsAny<ProviderFilter>()))
            .ReturnsAsync(expected);

        currentUserService.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await controller.GetByFilterProvider(new ProviderFilter()).ConfigureAwait(false);

        // Assert
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetProviders_WhenCalled_ReturnsForbidObject()
    {
        // Arrange
        var expected = new SearchResult<ProviderDto>
        {
            TotalAmount = 10,
            Entities = providers.Select(x => mapper.Map<ProviderDto>(x)).ToList(),
        };

        sensitiveProviderService.Setup(x => x.GetByFilter(It.IsAny<ProviderFilter>()))
            .ReturnsAsync(expected);

        currentUserService.Setup(x => x.IsAdmin()).Returns(false);

        // Act
        var result = await controller.GetByFilterProvider(new ProviderFilter()).ConfigureAwait(false) as ForbidResult;

        // Assert
        Assert.That(result is ForbidResult);
    }

    [Test]
    public async Task GetByFilter_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        sensitiveMinistryAdminService.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>())).ReturnsAsync(new SearchResult<MinistryAdminDto> { TotalAmount = 0, Entities = new List<MinistryAdminDto>() });
        currentUserService.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await controller.GetByFilterMinistryAdmin(new MinistryAdminFilter()).ConfigureAwait(false);

        // Assert
        sensitiveMinistryAdminService.Verify(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()), Times.Once);
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task Update_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
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
    [TestCase(1)]
    public async Task Delete_WhenIdIsValid_ReturnsNoContentResult(long id)
    {
        // Arrange
        sensitiveDirectionService.Setup(x => x.Delete(id)).ReturnsAsync(Result<DirectionDto>.Success(direction));

        // Act
        var result = await controller.DeleteDirectionById(id) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(204, result.StatusCode);
    }

    [Test]
    public async Task Update_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        controller.ModelState.AddModelError("UpdateDirection", "Invalid model state.");

        // Act
        var result = await controller.UpdateDirections(direction).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That(((BadRequestObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetProviderByFilter_ReturnsOkResult()
    {
        // Arrange
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
    public async Task BlockProvider_WithValidIdAndBlocked_ReturnsOkResult()
    {
        // Arrange
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = true, Message = "test"});

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task BlockProvider_WithValidIdAndBlocked_ReturnsForbidResult()
    {
        // Arrange
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
        sensitiveProviderService
            .Setup(x => x.Block(It.IsAny<ProviderBlockDto>(), It.IsAny<string>())).ReturnsAsync(
                new ResponseDto(){Result = new object(), HttpStatusCode = HttpStatusCode.Accepted, IsSuccess = false, Message = "test"});

        // Act
        var result = await controller.BlockProvider(It.IsAny<ProviderBlockDto>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
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

    private List<WorkshopCard> FakeWorkshopCards()
    {
        return FakeWorkshops().Select(w => new WorkshopCard
        {
            WorkshopId = w.Id,
            ProviderTitle = w.ProviderTitle,
            ProviderOwnership = w.ProviderOwnership,
            Title = w.Title,
            PayRate = (PayRateType)w.PayRate,
            CoverImageId = w.CoverImageId,
            MinAge = w.MinAge,
            MaxAge = w.MaxAge,
            Price = (decimal)w.Price,
            DirectionIds = w.DirectionIds,
            ProviderId = w.ProviderId,
            Address = w.Address,
            WithDisabilityOptions = w.WithDisabilityOptions,
            Rating = w.Rating,
            ProviderLicenseStatus = w.ProviderLicenseStatus,
            InstitutionHierarchyId = w.InstitutionHierarchyId,
            InstitutionId = w.InstitutionId,
            Institution = w.Institution,
            AvailableSeats = w.AvailableSeats ?? uint.MaxValue,
            TakenSeats = w.TakenSeats,
        }).ToList();
    }

    private WorkshopDescriptionItemDto FakeWorkshopDescriptionItem()
    {
        var id = Guid.NewGuid();
        return new WorkshopDescriptionItemDto
        {
            Id = id,
            SectionName = "test heading",
            Description = $"test description text sentence for id: {id.ToString()}",
        };
    }

    private WorkshopV2Dto FakeWorkshop()
    {
        return new WorkshopV2Dto()
        {
            Id = Guid.NewGuid(),
            Title = "Title6",
            Phone = "1111111111",
            WorkshopDescriptionItems = new[]
            {
                FakeWorkshopDescriptionItem(),
                FakeWorkshopDescriptionItem(),
                FakeWorkshopDescriptionItem(),
            },
            Price = 6000,
            WithDisabilityOptions = true,
            ProviderTitle = "ProviderTitle",
            DisabilityOptionsDesc = "Desc6",
            Website = "website6",
            Instagram = "insta6",
            Facebook = "facebook6",
            Email = "email6@gmail.com",
            MaxAge = 10,
            MinAge = 4,
            CoverImageId = "image6",
            ProviderId = Guid.NewGuid(),
            InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            AddressId = 55,
            Address = new AddressDto
            {
                Id = 55,
                CATOTTGId = 4970,
                Street = "Street55",
                BuildingNumber = "BuildingNumber55",
                Latitude = 0,
                Longitude = 0,
            },
            Teachers = new List<TeacherDTO>
            {
                new TeacherDTO
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alex",
                    LastName = "Brown",
                    MiddleName = "SomeMiddleName",
                    Description = "Description",
                    CoverImageId = "Image",
                    DateOfBirth = DateTime.Parse("2000-01-01"),
                    WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
                },
                new TeacherDTO
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Snow",
                    MiddleName = "SomeMiddleName",
                    Description = "Description",
                    CoverImageId = "Image",
                    DateOfBirth = DateTime.Parse("1990-01-01"),
                    WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
                },
            },
        };
    }
    private List<WorkshopV2Dto> FakeWorkshops()
    {
        return new List<WorkshopV2Dto>()
        {
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title1",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                },
                Price = 1000,
                WithDisabilityOptions = true,
                ProviderId = Guid.NewGuid(),
                ProviderTitle = "ProviderTitle",
                DisabilityOptionsDesc = "Desc1",
                Website = "website1",
                Instagram = "insta1",
                Facebook = "facebook1",
                Email = "email1@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                CoverImageId = "image1",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                Address = new AddressDto
                {
                    CATOTTGId = 4970,
                },
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title2",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                },
                Price = 2000,
                WithDisabilityOptions = true,
                ProviderId = Guid.NewGuid(),
                ProviderTitle = "ProviderTitle",
                DisabilityOptionsDesc = "Desc2",
                Website = "website2",
                Instagram = "insta2",
                Facebook = "facebook2",
                Email = "email2@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                CoverImageId = "image2",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                Address = new AddressDto
                {
                    CATOTTGId = 4970,
                },
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title3",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                },
                Price = 3000,
                WithDisabilityOptions = true,
                ProviderId = Guid.NewGuid(),
                ProviderTitle = "ProviderTitleNew",
                DisabilityOptionsDesc = "Desc3",
                Website = "website3",
                Instagram = "insta3",
                Facebook = "facebook3",
                Email = "email3@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                CoverImageId = "image3",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title4",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                },
                Price = 4000,
                WithDisabilityOptions = true,
                ProviderId = Guid.NewGuid(),
                ProviderTitle = "ProviderTitleNew",
                DisabilityOptionsDesc = "Desc4",
                Website = "website4",
                Instagram = "insta4",
                Facebook = "facebook4",
                Email = "email4@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                CoverImageId = "image4",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title5",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                },
                Price = 5000,
                WithDisabilityOptions = true,
                ProviderId = Guid.NewGuid(),
                ProviderTitle = "ProviderTitleNew",
                DisabilityOptionsDesc = "Desc5",
                Website = "website5",
                Instagram = "insta5",
                Facebook = "facebook5",
                Email = "email5@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                CoverImageId = "image5",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                Address = new AddressDto
                {
                    CATOTTGId = 4970,
                },
            },
        };
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
