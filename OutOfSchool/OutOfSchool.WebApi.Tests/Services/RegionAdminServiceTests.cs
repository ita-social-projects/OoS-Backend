﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class RegionAdminServiceTests
{
    private readonly string email = "email@gmail.com";
    private readonly string includeProperties = "Institution,User,CATOTTG";

    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IRegionAdminRepository> regionAdminRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> apiErrorServiceUserRepositoryMock;
    private Mock<ISearchStringService> searchStringServiceMock;

    private RegionAdminService regionAdminService;
    private RegionAdmin regionAdmin;
    private List<RegionAdmin> regionAdmins;
    private RegionAdminDto regionAdminDto;
    private List<RegionAdminDto> regionAdminsDtos;
    private ErrorResponse emailAlreadyTakenErrorResponse;
    private ApiErrorResponse badRequestApiErrorResponse;
    private ApiErrorService apiErrorService;

    [SetUp]
    public void SetUp()
    {
        regionAdmin = AdminGenerator.GenerateRegionAdmin().WithUserAndInstitution();
        regionAdmins = AdminGenerator.GenerateRegionAdmins(5).WithUserAndInstitution();
        regionAdminDto = AdminGenerator.GenerateRegionAdminDto();
        regionAdminsDtos = AdminGenerator.GenerateRegionAdminsDtos(5);

        httpClientFactory = new Mock<IHttpClientFactory>();
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();

        badRequestApiErrorResponse = new ApiErrorResponse();
        badRequestApiErrorResponse.AddApiError(
            ApiErrorsTypes.Common.EmailAlreadyTaken("RegionAdmin", email));
        emailAlreadyTakenErrorResponse = ErrorResponse.BadRequest(badRequestApiErrorResponse);

        communicationConfig.Setup(x => x.Value)
            .Returns(new CommunicationConfig()
            {
                ClientName = "test",
                TimeoutInSeconds = 2,
                MaxNumberOfRetries = 7,
            });
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient()
            {
                Timeout = new TimeSpan(2),
                BaseAddress = It.IsAny<Uri>(),
            });

        regionAdminRepositoryMock = new Mock<IRegionAdminRepository>();
        var logger = new Mock<ILogger<RegionAdminService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        apiErrorServiceUserRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        var apiErrorServiceLogger = new Mock<ILogger<ApiErrorService>>();
        apiErrorService = new ApiErrorService(apiErrorServiceUserRepositoryMock.Object, apiErrorServiceLogger.Object);
        searchStringServiceMock = new Mock<ISearchStringService>();

        regionAdminService = new RegionAdminService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            communicationConfig.Object,
            regionAdminRepositoryMock.Object,
            logger.Object,
            userRepositoryMock.Object,
            mapper,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            apiErrorService,
            searchStringServiceMock.Object);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<RegionAdminDto>(regionAdmin);
        regionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(regionAdmin);

        // Act
        var result = await regionAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        regionAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Act
        var result = await regionAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUserId_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<RegionAdminDto>(regionAdmin);
        regionAdminRepositoryMock
            .Setup(x => x
                .GetByFilter(It.IsAny<Expression<Func<RegionAdmin, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<RegionAdmin>>(new List<RegionAdmin> { regionAdmin }));

        // Act
        var result = await regionAdminService.GetByUserId(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        regionAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void GetByUserId_WhenCalled_ReturnsException()
    {
        // Arrange
        var expected = mapper.Map<RegionAdminDto>(regionAdmin);
        regionAdminRepositoryMock
            .Setup(x => x
                .GetByFilter(It.IsAny<Expression<Func<RegionAdmin, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<RegionAdmin>>(new List<RegionAdmin>()));

        // Act, Assert
        Assert.That(() => regionAdminService.GetByUserId(It.IsAny<string>()), Throws.ArgumentException);
    }

    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // Arrange
        var expected = regionAdmins
            .Select(p => mapper.Map<RegionAdminDto>(p))
            .ToList();

        var filter = new RegionAdminFilter()
        {
            From = 1,
            Size = 5,
        };

        var regionAdminsMock = regionAdmins.AsQueryable().BuildMock();
        regionAdminRepositoryMock.Setup(repo => repo.Count(It.IsAny<Expression<Func<RegionAdmin, bool>>>()))
            .ReturnsAsync(5);
        regionAdminRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<RegionAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<RegionAdmin, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(regionAdminsMock);

        // Act
        var result = await regionAdminService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        regionAdminRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.That(result.TotalAmount, Is.EqualTo(expected.Count));
    }

    [Test]
    public void GetByFilter_WhenFilterIsInvalid_ReturnsException()
    {
        // Act
        regionAdminService.Invoking(x => x.GetByFilter(new RegionAdminFilter())).Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void Update_WhenNullModel_ReturnsException()
    {
        // Act
        regionAdminService
            .Invoking(x => x
                .UpdateRegionAdminAsync(It.IsAny<string>(), It.IsAny<BaseUserDto>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Update_WhenAdminNotExist_ReturnsErrorResponse()
    {
        // Arrange
        regionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as RegionAdmin);

        // Act
        var result = await regionAdminService.UpdateRegionAdminAsync(It.IsAny<string>(), new BaseUserDto(), It.IsAny<string>());

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.Match(error => error.HttpStatusCode, null));
    }

    [Test]
    public void Subordinate_WhenNullIds_ReturnsException()
    {
        // Act
        regionAdminService
            .Invoking(x => x
                .IsRegionAdminSubordinateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Subordinate_WhenIsSubordinate_ReturnsTrue()
    {
        // Arrange
        MinistryAdminDto ministryAdmin = AdminGenerator.GenerateMinistryAdminDto();
        ministryAdmin.InstitutionId = regionAdmin.InstitutionId;
        ministryAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(ministryAdmin));
        regionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(regionAdmin));

        // Act
        var result = await regionAdminService.IsRegionAdminSubordinateAsync(ministryAdmin.Id, regionAdmin.UserId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Subordinate_WhenIsNotSubordinate_ReturnsFalse()
    {
        // Arrange
        MinistryAdminDto ministryAdmin = AdminGenerator.GenerateMinistryAdminDto();
        ministryAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(ministryAdmin));
        regionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(regionAdmin));

        // Act
        var result = await regionAdminService.IsRegionAdminSubordinateAsync(ministryAdmin.Id, regionAdmin.UserId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Create_EmailIsAlreadyTaken_ReturnsErrorResponse()
    {
        // Arrange
        var expected = emailAlreadyTakenErrorResponse
            .ApiErrorResponse
            .ApiErrors
            .First();
        apiErrorServiceUserRepositoryMock.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<User> { new User() });

        var regionAdminBaseDto = new RegionAdminBaseDto();
        regionAdminBaseDto.Email = email;

        // Act
        var response = await regionAdminService.CreateRegionAdminAsync(It.IsAny<string>(), regionAdminBaseDto, It.IsAny<string>()).ConfigureAwait(false);

        ErrorResponse errorResponse = default;
        response.Match<ErrorResponse>(
            actionResult => errorResponse = actionResult,
            succeed => errorResponse = new ErrorResponse());

        var result = errorResponse.ApiErrorResponse.ApiErrors.First();

        // Assert
        Assert.AreEqual(expected.Group, result.Group);
        Assert.AreEqual(expected.Code, result.Code);
        Assert.AreEqual(expected.Message, result.Message);
    }

    [Test]
    public async Task GetByFilter_WhenFilteredBySearchString_ShouldReturnEntities()
    {
        // Arrange
        var filter = new RegionAdminFilter()
        {
            SearchString = "Хмельницька,Київська,  ",
        };

        regionAdmins[0].CATOTTG = new CATOTTG() { Name = "Хмельницька область" };
        regionAdmins[1].CATOTTG = new CATOTTG() { Name = "Київська область" };

        var filteredRegionAdmins = new List<RegionAdmin>() { regionAdmins[0], regionAdmins[1] };
        var expectedDtos = mapper.Map<List<RegionAdminDto>>(filteredRegionAdmins);

        SetupCommonMocks(filteredRegionAdmins, filter, ["Київська", "Хмельницька"]);

        // Act
        var result = await regionAdminService.GetByFilter(filter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should()
            .BeEquivalentTo(expectedDtos);

        searchStringServiceMock.VerifyAll();
        regionAdminRepositoryMock.VerifyAll();
        currentUserServiceMock.VerifyAll();
    }

    private void SetupCommonMocks(
    List<RegionAdmin> filteredRegionAdmins = null,
    RegionAdminFilter filter = null,
    string[] splitResults = null,
    bool isMinistryAdmin = false,
    bool isRegionAdmin = false)
    {
        searchStringServiceMock
            .Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(splitResults);

        currentUserServiceMock.Setup(s => s.IsMinistryAdmin())
            .Returns(isMinistryAdmin);
        currentUserServiceMock.Setup(s => s.IsRegionAdmin())
            .Returns(isRegionAdmin);

        regionAdminRepositoryMock
            .Setup(r => r.Count(It.IsAny<Expression<Func<RegionAdmin, bool>>>()))
            .ReturnsAsync(filteredRegionAdmins.Count);

        regionAdminRepositoryMock
            .Setup(r =>
                r.Get(
                    It.Is<int>(x => x == filter.From),
                    It.Is<int>(x => x == filter.Size),
                    It.Is<string>(x => x == includeProperties),
                    It.IsAny<Expression<Func<RegionAdmin, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<RegionAdmin, dynamic>>, SortDirection>>(),
                    It.Is<bool>(x => x)))
            .Returns(filteredRegionAdmins.AsQueryable()
            .BuildMock());
    }
}