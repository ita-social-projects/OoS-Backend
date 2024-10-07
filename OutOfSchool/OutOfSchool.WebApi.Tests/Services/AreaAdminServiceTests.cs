using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using User = OutOfSchool.Services.Models.User;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class AreaAdminServiceTests
{
    private readonly string email = "email@gmail.com";
    private readonly string includeProperties = "Institution,User,CATOTTG.Parent.Parent";

    private Mock<ICodeficatorRepository> codeficatorRepositoryMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IAreaAdminRepository> areaAdminRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> institutionAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> apiErrorServiceUserRepositoryMock;
    private Mock<ISearchStringService> searchStringServiceMock;

    private AreaAdminService areaAdminService;
    private AreaAdmin areaAdmin;
    private List<AreaAdmin> areaAdmins;
    private AreaAdminDto areaAdminDto;
    private List<AreaAdminDto> areaAdminsDtos;
    private ErrorResponse emailAlreadyTakenErrorResponse;
    private ApiErrorResponse badRequestApiErrorResponse;
    private ApiErrorService apiErrorService;

    [SetUp]
    public void SetUp()
    {
        areaAdmin = AdminGenerator.GenerateAreaAdmin().WithUserAndInstitution();
        areaAdmins = AdminGenerator.GenerateAreaAdmins(5).WithUserAndInstitution();

        codeficatorRepositoryMock = new Mock<ICodeficatorRepository>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        httpClientFactory = new Mock<IHttpClientFactory>();
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();

        badRequestApiErrorResponse = new ApiErrorResponse();
        badRequestApiErrorResponse.AddApiError(
            ApiErrorsTypes.Common.EmailAlreadyTaken("AreaAdmin", email));
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

        areaAdminRepositoryMock = new Mock<IAreaAdminRepository>();
        var logger = new Mock<ILogger<AreaAdminService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        institutionAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        apiErrorServiceUserRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        var apiErrorServiceLogger = new Mock<ILogger<ApiErrorService>>();
        apiErrorService = new ApiErrorService(apiErrorServiceUserRepositoryMock.Object, apiErrorServiceLogger.Object);
        searchStringServiceMock = new Mock<ISearchStringService>();

        areaAdminService = new AreaAdminService(
            codeficatorRepositoryMock.Object,
            codeficatorServiceMock.Object,
            httpClientFactory.Object,
            identityServerConfig.Object,
            communicationConfig.Object,
            areaAdminRepositoryMock.Object,
            logger.Object,
            userRepositoryMock.Object,
            mapper,
            currentUserServiceMock.Object,
            institutionAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            apiErrorService,
            searchStringServiceMock.Object);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<AreaAdminDto>(areaAdmin);
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(areaAdmin);

        // Act
        var result = await areaAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        areaAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Act
        var result = await areaAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUserId_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<AreaAdminDto>(areaAdmin);
        areaAdminRepositoryMock
            .Setup(x => x
                .GetByFilter(It.IsAny<Expression<Func<AreaAdmin, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<AreaAdmin>>(new List<AreaAdmin> { areaAdmin }));

        // Act
        var result = await areaAdminService.GetByUserId(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        areaAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void GetByUserId_WhenCalled_ReturnsException()
    {
        // Arrange
        areaAdminRepositoryMock
            .Setup(x => x
                .GetByFilter(It.IsAny<Expression<Func<AreaAdmin, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<AreaAdmin>>(new List<AreaAdmin>()));

        // Act, Assert
        Assert.That(() => areaAdminService.GetByUserId(It.IsAny<string>()), Throws.ArgumentException);
    }

    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // Arrange
        var expected = areaAdmins
            .Select(p => mapper.Map<AreaAdminDto>(p))
            .ToList();

        var filter = new AreaAdminFilter()
        {
            From = 1,
            Size = 5,
        };

        var areaAdminsMock = areaAdmins.AsQueryable().BuildMock();
        areaAdminRepositoryMock.Setup(repo => repo.Count(It.IsAny<Expression<Func<AreaAdmin, bool>>>()))
            .ReturnsAsync(5);
        areaAdminRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<AreaAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<AreaAdmin, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(areaAdminsMock);

        // Act
        var result = await areaAdminService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        areaAdminRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.That(result.TotalAmount, Is.EqualTo(expected.Count));
    }

    [Test]
    public void GetByFilter_WhenFilterIsInvalid_ReturnsException()
    {
        // Act
        areaAdminService.Invoking(x => x.GetByFilter(new AreaAdminFilter())).Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void Update_WhenNullModel_ReturnsException()
    {
        // Act
        areaAdminService
            .Invoking(x => x
                .UpdateAreaAdminAsync(It.IsAny<string>(), It.IsAny<AreaAdminDto>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void Subordinate_WhenNullIds_ReturnsException()
    {
        // Act
        areaAdminService
            .Invoking(x => x
                .IsAreaAdminSubordinateMinistryAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Subordinate_WhenIsSubordinate_ReturnsTrue()
    {
        // Arrange
        MinistryAdminDto institutionAdmin = AdminGenerator.GenerateMinistryAdminDto();
        institutionAdmin.InstitutionId = areaAdmin.InstitutionId;
        institutionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(institutionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateMinistryAsync(institutionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Subordinate_WhenIsNotSubordinate_ReturnsFalse()
    {
        // Arrange
        MinistryAdminDto institutionAdmin = AdminGenerator.GenerateMinistryAdminDto();
        institutionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(institutionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateMinistryAsync(institutionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Subordinate_WhenIsSubordinateToRegionAdmin_ReturnsTrue()
    {
        // Arrange
        RegionAdminDto regionAdmin = AdminGenerator.GenerateRegionAdminDto();
        regionAdmin.InstitutionId = areaAdmin.InstitutionId;
        regionAdmin.CATOTTGId = long.MinValue;
        areaAdmin.CATOTTG = new CATOTTG { Parent = new CATOTTG { Parent = new CATOTTG { Id = long.MinValue } } };
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(regionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateRegionAsync(regionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Subordinate_WhenIsNotSubordinateToRegionAdmin_ReturnsFalse()
    {
        // Arrange
        RegionAdminDto regionAdmin = AdminGenerator.GenerateRegionAdminDto();
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(regionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateRegionAsync(regionAdmin.Id, areaAdmin.UserId);

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

        var areaAdminBaseDto = new AreaAdminBaseDto();
        areaAdminBaseDto.Email = email;

        // Act
        var response = await areaAdminService.CreateAreaAdminAsync(It.IsAny<string>(), areaAdminBaseDto, It.IsAny<string>()).ConfigureAwait(false);

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
        var filter = new AreaAdminFilter()
        {
            SearchString = "admin@, Адміністратор",
        };

        areaAdmins[0].User.FirstName = "Адміністратор";
        areaAdmins[1].User.Email = "admin@";

        var filteredAreaAdmins = new List<AreaAdmin>() { areaAdmins[0], areaAdmins[1] };
        var expectedAdminDtos = mapper.Map<List<AreaAdminDto>>(filteredAreaAdmins);

        SetupCommonMocks(filter, filteredAreaAdmins, ["Адміністратор,admin@"]);

        var expectedResult = new SearchResult<AreaAdminDto>()
        {
            TotalAmount = expectedAdminDtos.Count,
            Entities = expectedAdminDtos,
        };

        // Act
        var result = await areaAdminService.GetByFilter(filter)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedResult);

        searchStringServiceMock.VerifyAll();
        currentUserServiceMock.VerifyAll();
        areaAdminRepositoryMock.VerifyAll();
    }

    private void SetupCommonMocks(
        AreaAdminFilter filter = null,
        List<AreaAdmin> filteredAreaAdmins = null,
        string[] searchWords = null,
        bool isRegionAdmin = false,
        bool isMinistryAdmin = false)
    {
        searchStringServiceMock.Setup(s => s.SplitSearchString(
            It.Is<string>(x => x == filter.SearchString)))
            .Returns(searchWords);

        currentUserServiceMock.Setup(s => s.IsMinistryAdmin())
            .Returns(isMinistryAdmin);

        currentUserServiceMock.Setup(s => s.IsRegionAdmin())
            .Returns(isRegionAdmin);

        areaAdminRepositoryMock.Setup(
            r => r.Count(It.IsAny<Expression<Func<AreaAdmin, bool>>>()))
            .ReturnsAsync(filteredAreaAdmins.Count);

        areaAdminRepositoryMock.Setup(repo => repo.Get(
                It.Is<int>(x => x == filter.From),
                It.Is<int>(x => x == filter.Size),
                It.Is<string>(x => x == includeProperties),
                It.IsAny<Expression<Func<AreaAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<AreaAdmin, dynamic>>, SortDirection>>(),
                It.Is<bool>(x => x)))
            .Returns(filteredAreaAdmins.AsQueryable()
            .BuildMock());
    }
}