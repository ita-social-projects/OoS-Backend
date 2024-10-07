using System;
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
public class MinistryAdminServiceTests
{
    private readonly string email = "email@gmail.com";
    private readonly string includeProperties = "Institution,User";

    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IInstitutionAdminRepository> institutionAdminRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, OutOfSchool.Services.Models.User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> apiErrorServiceUserRepositoryMock;
    private Mock<ISearchStringService> searchStringServiceMock;

    private MinistryAdminService ministryAdminService;
    private InstitutionAdmin institutionAdmin;
    private List<InstitutionAdmin> institutionAdmins;
    private ErrorResponse emailAlreadyTakenErrorResponse;
    private ApiErrorResponse badRequestApiErrorResponse;
    private ApiErrorService apiErrorService;

    [SetUp]
    public void SetUp()
    {
        institutionAdmin = AdminGenerator.GenerateInstitutionAdmin().WithUserAndInstitution();
        institutionAdmins = AdminGenerator.GenerateInstitutionAdmins(5).WithUserAndInstitution();

        httpClientFactory = new Mock<IHttpClientFactory>();
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();

        badRequestApiErrorResponse = new ApiErrorResponse();
        badRequestApiErrorResponse.AddApiError(
            ApiErrorsTypes.Common.EmailAlreadyTaken("MinistryAdmin", email));
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

        institutionAdminRepositoryMock = new Mock<IInstitutionAdminRepository>();
        var logger = new Mock<ILogger<MinistryAdminService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        apiErrorServiceUserRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        var apiErrorServiceLogger = new Mock<ILogger<ApiErrorService>>();
        apiErrorService = new ApiErrorService(apiErrorServiceUserRepositoryMock.Object, apiErrorServiceLogger.Object);
        searchStringServiceMock = new Mock<ISearchStringService>();

        ministryAdminService = new MinistryAdminService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            communicationConfig.Object,
            institutionAdminRepositoryMock.Object,
            logger.Object,
            userRepositoryMock.Object,
            mapper,
            currentUserServiceMock.Object,
            apiErrorService,
            searchStringServiceMock.Object);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = institutionAdmin;
        institutionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(institutionAdmin);

        // Act
        var result = await ministryAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        institutionAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(mapper.Map<MinistryAdminDto>(expected), result);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Act
        var result = await ministryAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // Arrange
        var expected = institutionAdmins
            .Select(p => mapper.Map<MinistryAdminDto>(p))
            .ToList();

        var filter = new MinistryAdminFilter()
        {
            From = 1,
            Size = 5,
        };

        var institutionAdminsMock = institutionAdmins.AsQueryable().BuildMock();
        institutionAdminRepositoryMock.Setup(repo => repo.Count(It.IsAny<Expression<Func<InstitutionAdmin, bool>>>()))
            .ReturnsAsync(5);
        institutionAdminRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<InstitutionAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionAdmin, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(institutionAdminsMock);

        // Act
        var result = await ministryAdminService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        institutionAdminRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.AreEqual(expected.Count, result.TotalAmount);
    }

    [Test]
    public void GetByFilter_WhenFilterIsInvalid_ReturnsException()
    {
        // Act
        ministryAdminService.Invoking(x => x.GetByFilter(new MinistryAdminFilter())).Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void Update_WhenNullModel_ReturnsException()
    {
        // Act
        ministryAdminService
            .Invoking(x => x
                .UpdateMinistryAdminAsync(It.IsAny<string>(), It.IsAny<BaseUserDto>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Update_WhenAdminNotExist_ReturnsErrorResponse()
    {
        // Arrange
        institutionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as InstitutionAdmin);

        // Act
        var result = await ministryAdminService.UpdateMinistryAdminAsync(It.IsAny<string>(), new BaseUserDto(), It.IsAny<string>());

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.Match(error => error.HttpStatusCode, null));
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

        var ministryAdminBaseDto = new MinistryAdminBaseDto();
        ministryAdminBaseDto.Email = email;

        // Act
        var response = await ministryAdminService.CreateMinistryAdminAsync(It.IsAny<string>(), ministryAdminBaseDto, It.IsAny<string>()).ConfigureAwait(false);

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
        var filter = new MinistryAdminFilter()
        {
            SearchString = "ministry@ Адміністратор міністерства",
        };

        institutionAdmin.User.Email = "ministry@admin.com";
        institutionAdmin.User.FirstName = "Адміністратор";

        var filteredMinistryAdmins = new List<InstitutionAdmin>() { institutionAdmin };
        var expectedDtos = mapper.Map<List<MinistryAdminDto>>(filteredMinistryAdmins);

        SetupCommonMocks(
            filter,
            filteredMinistryAdmins,
            ["ministry@", "Адміністратор", "міністерства"]);

        // Act
        var result = await ministryAdminService.GetByFilter(filter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should()
            .BeEquivalentTo(expectedDtos);

        searchStringServiceMock.VerifyAll();
        currentUserServiceMock.VerifyAll();
        institutionAdminRepositoryMock.VerifyAll();
    }

    private void SetupCommonMocks(
        MinistryAdminFilter filter = null,
        List<InstitutionAdmin> filteredMinistryAdmins = null,
        string[] searchWords = null,
        bool isMinistryAdmin = false)
    {

        searchStringServiceMock.Setup(s => s.SplitSearchString(
            It.Is<string>(x => x == filter.SearchString)))
            .Returns(searchWords);

        currentUserServiceMock.Setup(s => s.IsMinistryAdmin())
            .Returns(isMinistryAdmin);

        institutionAdminRepositoryMock.Setup(r =>
            r.Get(
                It.Is<int>(x => x == filter.From),
                It.Is<int>(x => x == filter.Size),
                It.Is<string>(x => x == includeProperties),
                It.IsAny<Expression<Func<InstitutionAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionAdmin, object>>, SortDirection>>(),
                It.Is<bool>(x => x)
            ))
            .Returns(filteredMinistryAdmins.AsQueryable()
            .BuildMock());

        institutionAdminRepositoryMock.Setup(r =>
            r.Count(It.IsAny<Expression<Func<InstitutionAdmin, bool>>>()))
            .ReturnsAsync(filteredMinistryAdmins.Count);
    }
}
